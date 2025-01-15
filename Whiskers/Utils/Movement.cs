/*
 * Copyright(c) 2025 Meowchestra, GiR-Zippo, 2024 awgil
 * Licensed under the GPL v3 license. See https://github.com/Meowchestra/MeowMusic/blob/main/LICENSE for full license information.
 * Licensed under the BSD 3-Clause license. See https://github.com/awgil/ffxiv_bossmod/blob/master/LICENSE for full license information.
 */

using System.Numerics;
using System.Runtime.InteropServices;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Config;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using Whiskers.Offsets;

namespace Whiskers.Utils;

[StructLayout(LayoutKind.Explicit, Size = 0x18)]
public struct PlayerMoveControllerFlyInput
{
    [FieldOffset(0x0)] public float Forward;
    [FieldOffset(0x4)] public float Left;
    [FieldOffset(0x8)] public float Up;
    [FieldOffset(0xC)] public float Turn;
    [FieldOffset(0x10)] public float u10;
    [FieldOffset(0x14)] public byte DirMode;
    [FieldOffset(0x15)] public byte HaveBackwardOrStrafe;
}

public unsafe class OverrideMovement : IDisposable
{
    public bool Enabled
    {
        get => _rmiWalkHook is { IsEnabled: true };
        set
        {
            if (value)
            {
                _rmiWalkHook?.Enable();
                _rmiFlyHook?.Enable();
            }
            else
            {
                _rmiWalkHook?.Disable();
                _rmiFlyHook?.Disable();
            }
        }
    }

    public bool IgnoreUserInput; // if true - override even if user tries to change camera orientation, otherwise override only if user does nothing
    public Vector3 DesiredPosition;
    public float Precision = 0.01f;

    private bool _legacyMode;

    private delegate bool RmiWalkIsInputEnabled(void* self);
    private RmiWalkIsInputEnabled? _rmiWalkIsInputEnabled1;
    private RmiWalkIsInputEnabled? _rmiWalkIsInputEnabled2;

    private delegate void RmiWalkDelegate(void* self, float* sumLeft, float* sumForward, float* sumTurnLeft, byte* haveBackwardOrStrafe, byte* a6, byte bAdditiveUnk);
    [Signature("E8 ?? ?? ?? ?? 80 7B 3E 00 48 8D 3D")]
    private Hook<RmiWalkDelegate>? _rmiWalkHook = null;

    private delegate void RmiFlyDelegate(void* self, PlayerMoveControllerFlyInput* result);
    [Signature("E8 ?? ?? ?? ?? 0F B6 0D ?? ?? ?? ?? B8")]
    private Hook<RmiFlyDelegate>? _rmiFlyHook = null;

    public OverrideMovement()
    {
        if (Api.SigScanner != null)
        {
            var rmiWalkIsInputEnabled1Addr = Api.SigScanner.ScanText("E8 ?? ?? ?? ?? 84 C0 75 10 38 43 3C");
            var rmiWalkIsInputEnabled2Addr = Api.SigScanner.ScanText("E8 ?? ?? ?? ?? 84 C0 75 03 88 47 3F");
            Api.PluginLog?.Information($"RMIWalkIsInputEnabled1 address: 0x{rmiWalkIsInputEnabled1Addr:X}");
            Api.PluginLog?.Information($"RMIWalkIsInputEnabled2 address: 0x{rmiWalkIsInputEnabled2Addr:X}");
            _rmiWalkIsInputEnabled1 = Marshal.GetDelegateForFunctionPointer<RmiWalkIsInputEnabled>(rmiWalkIsInputEnabled1Addr);
            _rmiWalkIsInputEnabled2 = Marshal.GetDelegateForFunctionPointer<RmiWalkIsInputEnabled>(rmiWalkIsInputEnabled2Addr);
        }

        Api.GameInteropProvider?.InitializeFromAttributes(this);
        if (_rmiWalkHook != null) 
            Api.PluginLog?.Information($"RMIWalk address: 0x{_rmiWalkHook.Address:X}");
        if (_rmiFlyHook != null) 
            Api.PluginLog?.Information($"RMIFly address: 0x{_rmiFlyHook.Address:X}");
        if (Api.GameConfig != null) 
            Api.GameConfig.UiControlChanged += OnConfigChanged;
        UpdateLegacyMode();
    }

    public void Dispose()
    {
        if (Api.GameConfig != null)
            Api.GameConfig.UiControlChanged -= OnConfigChanged;
        _rmiWalkHook?.Dispose();
        _rmiFlyHook?.Dispose();
    }

    private void RmiWalkDetour(void* self, float* sumLeft, float* sumForward, float* sumTurnLeft, byte* haveBackwardOrStrafe, byte* a6, byte bAdditiveUnk)
    {
        _rmiWalkHook?.Original(self, sumLeft, sumForward, sumTurnLeft, haveBackwardOrStrafe, a6, bAdditiveUnk);

        // TODO: we really need to introduce some extra checks that PlayerMoveController::readInput does - sometimes it skips reading input, and returning something non-zero breaks stuff...
        var movementAllowed = bAdditiveUnk == 0 && _rmiWalkIsInputEnabled1 != null && _rmiWalkIsInputEnabled1(self) && _rmiWalkIsInputEnabled2 != null && _rmiWalkIsInputEnabled2(self) && Api.Condition != null && !Api.Condition[ConditionFlag.BeingMoved];
        if (movementAllowed && (IgnoreUserInput || *sumLeft == 0 && *sumForward == 0) && DirectionToDestination(false) is var relDir && relDir != null)
        {
            var dir = relDir.Value.h.ToDirection();
            *sumLeft    = dir.X;
            *sumForward = dir.Y;
        }
    }

    private void RmiFlyDetour(void* self, PlayerMoveControllerFlyInput* result)
    {
        _rmiFlyHook?.Original(self, result);

        // TODO: we really need to introduce some extra checks that PlayerMoveController::readInput does - sometimes it skips reading input, and returning something non-zero breaks stuff...
        if ((IgnoreUserInput || result->Forward == 0 && result->Left == 0 && result->Up == 0) && DirectionToDestination(true) is var relDir && relDir != null)
        {
            var dir = relDir.Value.h.ToDirection();
            result->Forward = dir.Y;
            result->Left    = dir.X;
            result->Up      = relDir.Value.v.Rad;
        }
    }

    private (Angle h, Angle v)? DirectionToDestination(bool allowVertical)
    {
        var player = Api.ClientState?.LocalPlayer;
        if (player == null)
            return null;

        var dist = DesiredPosition - player.Position;
        if (dist.LengthSquared() <= Precision * Precision)
            return null;

        var dirH = Angle.FromDirectionXZ(dist);
        var dirV = allowVertical ? Angle.FromDirection(new(dist.Y, new Vector2(dist.X, dist.Z).Length())) : default;

        var refDir = _legacyMode
            ? ((CameraEx*)CameraManager.Instance()->GetActiveCamera())->DirH.Radians() + 180.Degrees()
            : player.Rotation.Radians();
        return (dirH - refDir, dirV);
    }

    private void OnConfigChanged(object? sender, ConfigChangeEvent evt) => UpdateLegacyMode();
    private void UpdateLegacyMode()
    {
        if (Api.GameConfig != null)
            _legacyMode = Api.GameConfig.UiControl.TryGetUInt("MoveMode", out var mode) && mode == 1;
        Api.PluginLog?.Info($"Legacy mode is now {(_legacyMode ? "enabled" : "disabled")}");
    }
}