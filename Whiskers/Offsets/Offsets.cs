/*
 * Copyright(c) 2024 Meowchestra, GiR-Zippo, akira0245 @MidiBard, Ori @MidiBard2
 * Licensed under the GPL v3 license. See https://github.com/Meowchestra/MeowMusic/blob/main/LICENSE for full license information.
 */

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Dalamud.Hooking;

namespace Whiskers.Offsets;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]

public static partial class Chat
{
    private static class Signatures
    {
        internal const string SendChat = "48895C24??574883EC20488BFA488BD94584C9";
        internal const string SanitiseString = "E8????????EB0A488D4C24??E8????????488D8D";
    }
}

public static class Offsets
{
    [StaticAddress("488D05????????488B??488901488D05????????48894128488B4948")]
    public static nint AgentPerformance { get; private set; }

    [StaticAddress("488D05????????C783E0000000????????")]
    public static nint AgentConfigSystem { get; private set; }

    [StaticAddress("488B15????????F6C2??")]
    public static nint PerformanceStructPtr { get; private set; }

    [Function("48896C24104889742418574883EC??48833D??????????418BE8")]
    public static nint DoPerformAction { get; private set; }

    [Function("488BC4564881EC????????48895810")]
    public static nint ApplyGraphicConfigsFunc { get; private set; }

    [Function("40534883EC20488BD94883C178E8????????488D8B????????E8????????488D5320")]
    public static IntPtr NetworkEnsembleStart { get; private set; }
}

public sealed unsafe class AgentPerformance(AgentInterface agentInterface)
    : AgentInterface(agentInterface.Pointer, agentInterface.Id)
{
    public static AgentPerformance? Instance => Whiskers.AgentPerformance;
    public new AgentPerformanceStruct* Struct => (AgentPerformanceStruct*)Pointer;

    [StructLayout(LayoutKind.Explicit)]
    public struct AgentPerformanceStruct
    {
        [FieldOffset(0)] private readonly FFXIVClientStructs.FFXIV.Component.GUI.AgentInterface AgentInterface;
        [FieldOffset(0x20)] public readonly byte InPerformanceMode;
        [FieldOffset(0x1F)] public byte Instrument;
        [FieldOffset(0x38)] public readonly long PerformanceTimer1;
        [FieldOffset(0x40)] public readonly long PerformanceTimer2;
        [FieldOffset(0x5C)] public int NoteOffset;
        [FieldOffset(0x60)] public int CurrentPressingNote;
        [FieldOffset(0xFC)] public int OctaveOffset;
        [FieldOffset(0x1B0)] public readonly int GroupTone;
    }

    internal int CurrentGroupTone => Struct->GroupTone;
    internal bool InPerformanceMode => Struct->InPerformanceMode != 0;
    internal bool NotePressed => Struct->CurrentPressingNote != -100;
    internal int NoteNumber => Struct->CurrentPressingNote;
    internal long PerformanceTimer1 => Struct->PerformanceTimer1;
    internal long PerformanceTimer2 => Struct->PerformanceTimer2;
    internal byte Instrument => Struct->Instrument;
}

internal class EnsembleManager : IDisposable
{
    private delegate long SubNetworkEnsemble(IntPtr a1, IntPtr a2);
    private readonly Hook<SubNetworkEnsemble>? _networkEnsembleHook;
    internal EnsembleManager()
    {
        //Get the ensemble start
        _networkEnsembleHook = Api.GameInteropProvider?.HookFromAddress<SubNetworkEnsemble>(Offsets.NetworkEnsembleStart, (a1, a2) =>
        {
            //and pipe it
            if (Pipe.Client != null && Pipe.Client.IsConnected)
            {
                Pipe.Client.WriteAsync(new IpcMessage
                {
                    MsgType = MessageType.StartEnsemble,
                    Message = Environment.ProcessId + ":1"
                });
            }

            return _networkEnsembleHook != null ? _networkEnsembleHook.Original(a1, a2) : 0;
        });
        _networkEnsembleHook?.Enable();
    }

    public void Dispose()
    {
        _networkEnsembleHook?.Dispose();
    }
}