/*
 * Copyright(c) 2024 Meowchestra, GiR-Zippo
 * Licensed under the GPL v3 license. See https://github.com/Meowchestra/MeowMusic/blob/main/LICENSE for full license information.
 */

using H.Pipes;

namespace Whiskers;


public class IpcMessage
{
    public MessageType MsgType { get; init; } = MessageType.None;
    public int MsgChannel { get; init; }
    public string Message { get; init; } = "";
}

internal static class Pipe
{
    internal static PipeClient<IpcMessage>? Client { get; private set; }

    internal static void Initialize()
    {
        Client = new PipeClient<IpcMessage>("Whiskers", formatter: new NewtonsoftJsonFormatter());
    }

    internal static void Write(MessageType messageType, int channel, bool msg)
    {
        Client?.WriteAsync(new IpcMessage
        {
            MsgType    = messageType,
            MsgChannel = channel,
            Message    = Environment.ProcessId + ":" + msg
        });
    }

    internal static void Write(MessageType messageType, int channel, float msg)
    {
        Client?.WriteAsync(new IpcMessage
        {
            MsgType    = messageType,
            MsgChannel = channel,
            Message    = Environment.ProcessId + ":" + msg
        });
    }

    internal static void Write(MessageType messageType, int channel, int msg)
    {
        Client?.WriteAsync(new IpcMessage
        {
            MsgType    = messageType,
            MsgChannel = channel,
            Message    = Environment.ProcessId + ":" + msg
        });
    }

    internal static void Dispose()
    {
        Client?.DisconnectAsync();
    }
}