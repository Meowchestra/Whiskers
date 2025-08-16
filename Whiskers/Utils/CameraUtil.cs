/*
 * Copyright(c) 2025 Meowchestra, GiR-Zippo
 * Licensed under the GPL v3 license. See https://github.com/Meowchestra/MeowMusic/blob/main/LICENSE for full license information.
 */

using System.Runtime.InteropServices;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using Whiskers.Offsets;

namespace Whiskers.Utils;

[StructLayout(LayoutKind.Explicit, Size = 0x2B0)]
public struct CameraEx
{
    [FieldOffset(0x130)] public float DirH; // 0 is north, increases CW
    [FieldOffset(0x134)] public float DirV; // 0 is horizontal, positive is looking up, negative looking down
    [FieldOffset(0x138)] public float InputDeltaHAdjusted;
    [FieldOffset(0x13C)] public float InputDeltaVAdjusted;
    [FieldOffset(0x140)] public float InputDeltaH;
    [FieldOffset(0x144)] public float InputDeltaV;
    [FieldOffset(0x148)] public float DirVMin; // -85deg by default
    [FieldOffset(0x14C)] public float DirVMax; // +45deg by default
}

public unsafe class CameraUtil : IDisposable
{
    public bool Enabled
    {
        get => _rmiCameraHook is { IsEnabled: true };
        set
        {
            if (value)
                _rmiCameraHook?.Enable();
            else
                _rmiCameraHook?.Disable();
        }
    }

    public bool IgnoreUserInput; // if true - override even if user tries to change camera orientation, otherwise override only if user does nothing
    public Angle DesiredAzimuth;
    public Angle DesiredAltitude;
    public Angle SpeedH = 360.Degrees(); // per second
    public Angle SpeedV = 360.Degrees(); // per second

    private delegate void RmiCameraDelegate(CameraEx* self, int inputMode, float speedH, float speedV);
    [Signature("40 53 48 83 EC 70 44 0F 29 44 24 ?? 48 8B D9")]
    private Hook<RmiCameraDelegate>? _rmiCameraHook = null;

    public CameraUtil()
    {
        Api.GameInteropProvider?.InitializeFromAttributes(this);
        if (_rmiCameraHook != null)
            Api.PluginLog?.Debug($"RMICamera address: 0x{_rmiCameraHook.Address:X}");
    }

    public void Dispose()
    {
        _rmiCameraHook?.Dispose();
    }

    private void RmiCameraDetour(CameraEx* self, int inputMode, float speedH, float speedV)
    {
        _rmiCameraHook?.Original(self, inputMode, speedH, speedV);
        if (IgnoreUserInput || inputMode == 0) // let user override...
        {
            var dt = Framework.Instance()->FrameDeltaTime;
            var deltaH = (DesiredAzimuth - self->DirH.Radians()).Normalized();
            var deltaV = (DesiredAltitude - self->DirV.Radians()).Normalized();
            var maxH = SpeedH.Rad * dt;
            var maxV = SpeedV.Rad * dt;
            self->InputDeltaH = Math.Clamp(deltaH.Rad, -maxH, maxH);
            self->InputDeltaV = Math.Clamp(deltaV.Rad, -maxV, maxV);
        }
    }
}