/*
 * Copyright(c) 2024 Meowchestra, GiR-Zippo, akira0245 @MidiBard, Ori @MidiBard2
 * Licensed under the GPL v3 license. See https://github.com/Meowchestra/MeowMusic/blob/main/LICENSE for full license information.
 */

using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace Whiskers.Offsets;

/// <summary>
/// AgentInterface Base
/// </summary>
public unsafe class AgentInterface
{
    public nint Pointer { get; }
    public nint VTable { get; }
    public int Id { get; }
    public FFXIVClientStructs.FFXIV.Client.UI.Agent.AgentInterface* Struct => (FFXIVClientStructs.FFXIV.Client.UI.Agent.AgentInterface*)Pointer;
    
    public AgentInterface(nint pointer, int id)
    {
        Pointer = pointer;
        Id      = id;
        VTable  = Marshal.ReadIntPtr(Pointer);
    }

    public AgentInterface(AgentId id)
    {
        Pointer = (nint)AgentModule.Instance()->GetAgentByInternalId(id);
        Id      = (int)id;
        VTable  = (nint)AgentModule.Instance()->GetAgentByInternalId(id)->VirtualTable;
    }

    public override string ToString()
    {
        return $"{Id} {(long)Pointer:X} {(long)VTable:X}";
    }
}

/// <summary>
/// Key injection manager
/// </summary>
public sealed unsafe class AgentPerformance(AgentId id) : AgentInterface(id)
{
    public static AgentPerformance? Instance => Whiskers.AgentPerformance;
    public new AgentPerformanceStruct* Struct => (AgentPerformanceStruct*)Pointer;

    [StructLayout(LayoutKind.Explicit)]
    public struct AgentPerformanceStruct
    {
        [FieldOffset(0)] public FFXIVClientStructs.FFXIV.Client.UI.Agent.AgentInterface AgentInterface;
        [FieldOffset(0x20)] public byte InPerformanceMode;
        [FieldOffset(0x1F)] public byte Instrument;
        [FieldOffset(0x38)] public long PerformanceTimer1;
        [FieldOffset(0x40)] public long PerformanceTimer2;
        [FieldOffset(0x5C)] public int NoteOffset;
        [FieldOffset(0x60)] public int CurrentPressingNote;
        [FieldOffset(0xFC)] public int OctaveOffset;
        [FieldOffset(0x1B0)] public int GroupTone;
    }

    internal int CurrentGroupTone => Struct->GroupTone;
    internal bool InPerformanceMode => Struct->InPerformanceMode != 0;
    internal bool NotePressed => Struct->CurrentPressingNote != -100;
    internal int NoteNumber => Struct->CurrentPressingNote;
    internal long PerformanceTimer1 => Struct->PerformanceTimer1;
    internal long PerformanceTimer2 => Struct->PerformanceTimer2;
    internal byte Instrument => Struct->Instrument;
}