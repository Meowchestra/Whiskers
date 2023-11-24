using H.Formatters;
using H.Pipes;

namespace Whiskers;

internal static class Pipe
{
    internal static PipeClient<PayloadMessage>? Client { get; private set; }

    internal static void Initialize()
    {
        Client = new PipeClient<PayloadMessage>("Whiskers", formatter: new NewtonsoftJsonFormatter());
    }

    internal static void Write(MessageType messageType, int channel, bool msg)
    {
        Client?.WriteAsync(new PayloadMessage
        {
            MsgType    = messageType,
            MsgChannel = channel,
            Message    = Environment.ProcessId + ":" + msg
        });
    }

    internal static void Write(MessageType messageType, int channel, float msg)
    {
        Client?.WriteAsync(new PayloadMessage
        {
            MsgType    = messageType,
            MsgChannel = channel,
            Message    = Environment.ProcessId + ":" + msg
        });
    }

    internal static void Write(MessageType messageType, int channel, int msg)
    {
        Client?.WriteAsync(new PayloadMessage
        {
            MsgType    = messageType,
            MsgChannel = channel,
            Message    = Environment.ProcessId + ":" + msg
        });
    }

    internal static void Dispose()
    {

    }
}