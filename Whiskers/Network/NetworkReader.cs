/*
 * Copyright(c) 2025 Meowchestra, GiR-Zippo
 * Licensed under the GPL v3 license. See https://github.com/Meowchestra/MeowMusic/blob/main/LICENSE for full license information.
 */

using System.Globalization;
using System.Runtime.InteropServices;
using Dalamud.Game.Network;
using Whiskers.Offsets;

namespace Whiskers.Network;

public static unsafe class NetworkReader
{
    private const string EnsembleStart = "?? ?? ?? ?? ?? 00 ?? ?? ?? ?? ?? ?? 00 00 00 00 00 00 ?? ?? 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00";
    private const string EnsembleRequest = "?? ?? ?? ?? ?? 00 ?? 00 ?? ?? ?? 10 00 00 00 00 00 00 ?? ?? ?? 00 00 00";
    private const string EnsembleEquip = "02 00 00 00 10 00 00 00 ?? 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00";
    private const string EnsembleUnequip = "02 00 00 00 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00";
    private const string EnsembleStop = "?? ?? ?? ?? ?? 00 ?? 00 ?? ?? ?? ?? 00 00 00 00";

    private static byte[] GenerateHeader(uint size, IReadOnlyList<byte> time, ushort opCode, uint sourceActorId, uint targetActorId)
    {
        var src = BitConverter.GetBytes(sourceActorId);
        var tar = BitConverter.GetBytes(targetActorId);
        var opcode = BitConverter.GetBytes(opCode);
        var sz = BitConverter.GetBytes(size);

        var outPacketArray = new byte[size];
        outPacketArray[0] = sz[0];
        outPacketArray[1] = sz[1];
        outPacketArray[2] = sz[2];
        outPacketArray[3] = sz[3];

        outPacketArray[4] = tar[0];
        outPacketArray[5] = tar[1];
        outPacketArray[6] = tar[2];
        outPacketArray[7] = tar[3];

        outPacketArray[8]  = src[0];
        outPacketArray[9]  = src[1];
        outPacketArray[10] = src[2];
        outPacketArray[11] = src[3];

        outPacketArray[18] = opcode[0];
        outPacketArray[19] = opcode[1];

        outPacketArray[24] = time[0];
        outPacketArray[25] = time[1];
        outPacketArray[26] = time[2];
        outPacketArray[27] = time[3];
        return outPacketArray;
    }

    private static byte[]? Packet48(IReadOnlyList<byte> time, ushort opCode, uint sourceActorId, uint targetActorId, IReadOnlyList<byte> trunk)
    {
        var outPacketArray = GenerateHeader(48, time, opCode, sourceActorId, targetActorId);

        for (var i = 0; i < 15; i++)
            if (outPacketArray != null)
                outPacketArray[i + 32] = trunk[i];
        return outPacketArray;
    }

    private static byte[]? Packet56(IReadOnlyList<byte> time, ushort opCode, uint sourceActorId, uint targetActorId, IReadOnlyList<byte> trunk)
    {
        var outPacketArray = GenerateHeader(56, time, opCode, sourceActorId, targetActorId);

        for (var i = 0; i < 23; i++)
            if (outPacketArray != null)
                outPacketArray[i + 32] = trunk[i];
        return outPacketArray;
    }

    private static byte[]? Packet88(IReadOnlyList<byte> time, ushort opCode, uint sourceActorId, uint targetActorId, IReadOnlyList<byte> trunk)
    {
        var outPacketArray = GenerateHeader(88, time, opCode, sourceActorId, targetActorId);

        for (var i = 0; i < 54; i++)
            if (outPacketArray != null)
                outPacketArray[i + 32] = trunk[i];
        return outPacketArray;
    }

    private static void NetworkMessage(nint dataPtr, ushort opCode, uint sourceActorId, uint targetActorId, NetworkMessageDirection direction)
    {
        if (direction == NetworkMessageDirection.ZoneDown && Pipe.Client != null && Pipe.Client.IsConnected)
        {
            var inPacket = GetPacket(dataPtr);
            var ti = new byte[5];
            Marshal.Copy(dataPtr - 8, ti, 0, 4);
            var hexString = BitConverter.ToString(inPacket);
            byte[]? packet = null;

            /*if (opCode == 1234)
            {
                Api.PluginLog?.Debug("NET: " + Convert.ToString(opCode));
                Api.PluginLog?.Debug(BitConverter.ToString(ti));
                Api.PluginLog?.Debug(hexString);
                Api.PluginLog?.Debug(sourceActorId.ToString());
                Api.PluginLog?.Debug(targetActorId.ToString());
            }*/

            if (CheckPattern(EnsembleStart, inPacket))
                packet = Packet88(ti, opCode, sourceActorId, targetActorId, inPacket);
            else if (CheckPattern(EnsembleEquip, inPacket))
                packet = Packet56(ti, opCode, sourceActorId, targetActorId, inPacket);
            else if (CheckPattern(EnsembleUnequip, inPacket))
                packet = Packet56(ti, opCode, sourceActorId, targetActorId, inPacket);
            else if (CheckPattern(EnsembleRequest, inPacket))
                packet = Packet56(ti, opCode, sourceActorId, targetActorId, inPacket);
            else if (CheckPattern(EnsembleStop, inPacket))
                packet = Packet48(ti, opCode, sourceActorId, targetActorId, inPacket);

            if (packet != null)
            {
                Pipe.Client.WriteAsync(new IpcMessage
                {
                    MsgType = MessageType.NetworkPacket,
                    Message = Environment.ProcessId + ":" + Convert.ToBase64String(packet)
                });
            }
        }
    }

    internal static void Initialize()
    {
        if (Api.GameNetwork != null) Api.GameNetwork.NetworkMessage += NetworkMessage;
    }

    internal static void Dispose()
    {
        if (Api.GameNetwork != null) Api.GameNetwork.NetworkMessage -= NetworkMessage;
    }

    private static byte[] GetPacket(IntPtr dataPtr)
    {
        using var memoryStream = new MemoryStream();
        using var unmanagedMemoryStream = new UnmanagedMemoryStream((byte*)(dataPtr /*- 0x20*/).ToPointer(), 4096);
        unmanagedMemoryStream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }

    private static bool CheckPattern(string pattern, IReadOnlyList<byte> array2Check)
    {
        var strBytes = pattern.Split(' ');
        return !strBytes.Where((t, i) => t != "?" && t != "??" && byte.Parse(t, NumberStyles.HexNumber) != array2Check[i]).Any();
    }
}