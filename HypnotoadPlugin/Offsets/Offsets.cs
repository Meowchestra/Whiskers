/*
 * Copyright(c) 2022 akira0245 @MidiBard, Ori @MidiBard2, GiR-Zippo
 *
 * Contains all signatures, which are used by this plugin
 * 
 */

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Dalamud.Hooking;

namespace HypnotoadPlugin.Offsets;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]

public static partial class Chat
{
    private static class Signatures
    {
        internal const string SendChat = "48 89 5C 24 ?? 57 48 83 EC 20 48 8B FA 48 8B D9 45 84 C9";
        internal const string SanitiseString = "E8 ?? ?? ?? ?? EB 0A 48 8D 4C 24 ?? E8 ?? ?? ?? ?? 48 8D 8D";
    }
}

public static class Offsets
{
    [StaticAddress("48 8D 05 ?? ?? ?? ?? 48 8B ?? 48 89 01 48 8D 05 ?? ?? ?? ?? 48 89 41 28 48 8B 49 48")]
    public static nint AgentPerformance { get; private set; }

    [StaticAddress("48 8D 05 ?? ?? ?? ?? 48 89 03 48 8D 4B 40")]
    public static nint AgentMetronome { get; private set; }

    [StaticAddress("48 8D 05 ?? ?? ?? ?? C7 83 E0 00 00 00 ?? ?? ?? ??")]
    public static nint AgentConfigSystem { get; private set; }

    [StaticAddress("48 8B 15 ?? ?? ?? ?? F6 C2 ??")]
    public static nint PerformanceStructPtr { get; private set; }

    [Function("48 89 6C 24 10 48 89 74 24 18 57 48 83 EC ?? 48 83 3D ?? ?? ?? ?? ?? 41 8B E8")]
    public static nint DoPerformAction { get; private set; }

    [Offset("40 88 ?? ?? 66 89 ?? ?? 40 84", +3)]
    public static byte InstrumentOffset { get; private set; }

    [Function("48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 0F B6 FA 48 8B D9 84 D2 ")]
    public static nint UpdateMetronome { get; private set; }

    [Function("48 8B C4 56 48 81 EC ?? ?? ?? ?? 48 89 58 10 ")]
    public static nint ApplyGraphicConfigsFunc { get; private set; }

    [Function("48 89 ? ? ? 48 89 ? ? ? 57 48 83 EC ? 8B FA 41 0F ? ? 03 79")]
    public static nint PressNote { get; private set; }

    [Function("40 53 48 83 EC 20 48 8B D9 48 83 C1 78 E8 ? ? ? ? 48 8D 8B ? ? ? ? E8 ? ? ? ? 48 8D 53 20 ")]
    public static IntPtr NetworkEnsembleStart { get; private set; }

}

public sealed unsafe class AgentPerformance : AgentInterface
{
    public AgentPerformance(AgentInterface agentInterface) : base(agentInterface.Pointer, agentInterface.Id) { }
    public static AgentPerformance? Instance => Hypnotoad.AgentPerformance;
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
    private delegate long sub_1410F4EC0(IntPtr a1, IntPtr a2);
    private readonly Hook<sub_1410F4EC0> _networkEnsembleHook;
    internal EnsembleManager()
    {
        //Get the ensemble start
        _networkEnsembleHook = Hook<sub_1410F4EC0>.FromAddress(Offsets.NetworkEnsembleStart, (a1, a2) =>
        {
            //and pipe it
            if (Pipe.Client != null && Pipe.Client.IsConnected)
            {
                Pipe.Client.WriteAsync(new PayloadMessage
                {
                    MsgType = MessageType.StartEnsemble,
                    Message = Environment.ProcessId + ":1"
                });
            }

            return _networkEnsembleHook != null ? _networkEnsembleHook.Original(a1, a2) : 0;
        });
        _networkEnsembleHook.Enable();
    }

    public void Dispose()
    {
        _networkEnsembleHook.Dispose();
    }
}