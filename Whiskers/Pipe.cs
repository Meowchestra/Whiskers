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

    internal static void Dispose()
    {

    }
}