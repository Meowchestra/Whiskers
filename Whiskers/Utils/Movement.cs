/*
 * Copyright(c) 2024 Meowchestra, GiR-Zippo
 * Licensed under the GPL v3 license. See https://github.com/Meowchestra/MeowMusic/blob/main/LICENSE for full license information.
 */

using System.Numerics;
using System.Runtime.InteropServices;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using Whiskers.Offsets;

namespace Whiskers.Utils;

[StructLayout(LayoutKind.Explicit, Size = 0x18)]
internal struct PlayerMoveControllerFlyInput
{
    [FieldOffset(0x0)] public float Forward;
    [FieldOffset(0x4)] public float Left;
    [FieldOffset(0x8)] public float Up;
    [FieldOffset(0xC)] public float Turn;
    [FieldOffset(0x10)] public float u10;
    [FieldOffset(0x14)] public byte DirMode;
    [FieldOffset(0x15)] public byte HaveBackwardOrStrafe;
}

internal unsafe class OverrideMovement : IDisposable
{
    public bool IgnoreUserInput { get; set; }
    public Action? ActionIfUserInput { get; set; } = null;
    public Vector3? DesiredPosition { get; set; }
    public float Precision { get; set; } = 0.5f;

    private delegate void RmiWalkDelegate(void* self, float* sumLeft, float* sumForward, float* sumTurnLeft, byte* haveBackwardOrStrafe, byte* a6, byte bAdditiveUnk);
    [Signature("E8 ?? ?? ?? ?? 80 7B 3E 00 48 8D 3D", DetourName = nameof(RmiWalkDetour))]
    private Hook<RmiWalkDelegate>? _rmiWalkHook = null;

    private delegate void RmiFlyDelegate(void* self, PlayerMoveControllerFlyInput* result);
    [Signature("E8 ?? ?? ?? ?? 0F B6 0D ?? ?? ?? ?? B8", DetourName = nameof(RmiFlyDetour))]
    private Hook<RmiFlyDelegate>? _rmiFlyHook = null;

    public OverrideMovement()
    {
        Api.GameInteropProvider?.InitializeFromAttributes(this);
        _rmiWalkHook?.Enable();
        _rmiFlyHook?.Enable();
    }

    public void Dispose()
    {
        _rmiWalkHook?.Dispose();
        _rmiFlyHook?.Dispose();
    }

    private void RmiWalkDetour(void* self, float* sumLeft, float* sumForward, float* sumTurnLeft, byte* haveBackwardOrStrafe, byte* a6, byte bAdditiveUnk)
    {
        _rmiWalkHook?.Original(self, sumLeft, sumForward, sumTurnLeft, haveBackwardOrStrafe, a6, bAdditiveUnk);

        if (!CanOverride(out var relDir)) return;

        var noInput = *sumLeft == 0 && *sumForward == 0;
        if (!IgnoreUserInput && !noInput)
        {
            ActionIfUserInput?.Invoke();
        }
        else if (bAdditiveUnk == 0)
        {
            var dir = GetMoveDir(relDir);
            *sumLeft    = dir.X;
            *sumForward = dir.Y;
        }
    }

    private void RmiFlyDetour(void* self, PlayerMoveControllerFlyInput* result)
    {
        _rmiFlyHook?.Original(self, result);

        if (!CanOverride(out var relDir)) return;

        var noInput = result->Forward == 0 && result->Left == 0 && result->Up == 0;
        if (!IgnoreUserInput && !noInput)
        {
            ActionIfUserInput?.Invoke();
        }
        else
        {
            var dir = GetMoveDir(relDir);
            result->Left    = dir.X;
            result->Forward = dir.Y;
            result->Up      = MathF.Atan2(relDir.Y, new Vector2(relDir.X, relDir.Z).Length());
        }
    }

    private bool CanOverride(out Vector3 dir)
    {
        dir = default;

        if (Api.Condition != null && (Api.Condition[ConditionFlag.BetweenAreas]
                                      || Api.Condition[ConditionFlag.BetweenAreas51])) return false;

        if (Api.Condition != null && (Api.Condition[ConditionFlag.WatchingCutscene]
                                      || Api.Condition[ConditionFlag.WatchingCutscene78]
                                      || Api.Condition[ConditionFlag.OccupiedInCutSceneEvent])) return false;

        if (Api.Condition != null && (Api.Condition[ConditionFlag.OccupiedInQuestEvent]
                                      || Api.Condition[ConditionFlag.OccupiedInEvent]
                                      || Api.Condition[ConditionFlag.OccupiedSummoningBell])) return false;

        if (Api.Condition != null && Api.Condition[ConditionFlag.Unknown57]) return false;

        // TODO: we really need to introduce some extra checks that PlayerMoveController::readInput does - sometimes it skips reading input, and returning something non-zero breaks stuff...
        if (Api.ClientState?.LocalPlayer == null) return false;
        if (!DesiredPosition.HasValue) return false;

        dir = DesiredPosition.Value - Api.ClientState.LocalPlayer.Position;

        return dir.Length() > Precision - 0.01f;
    }

    private static Vector2 GetMoveDir(in Vector3 dir)
    {
        var angle = MathF.Atan2(dir.X, dir.Z);
        angle -= *(float*)((nint)CameraManager.Instance()->GetActiveCamera() + 0x130) + MathF.PI;
        return new Vector2(MathF.Sin(angle), MathF.Cos(angle));
    }
}