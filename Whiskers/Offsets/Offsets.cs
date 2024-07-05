/*
 * Copyright(c) 2024 Meowchestra, GiR-Zippo, akira0245 @MidiBard, Ori @MidiBard2
 * Licensed under the GPL v3 license. See https://github.com/Meowchestra/MeowMusic/blob/main/LICENSE for full license information.
 */

using System.Diagnostics.CodeAnalysis;
using Dalamud.Hooking;

namespace Whiskers.Offsets;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]

public static partial class Chat
{
    private static class Signatures
    {
        internal const string SendChat = "48895C24??574883EC20488BFA488BD94584C9";
        internal const string SanitiseString = "E8????????488D4C24??0FB6F0E8????????488D4DC0";
    }
}

public static class Offsets
{
    [StaticAddress("488BC20FB615????????F6C201")]
    public static nint PerformanceStructPtr { get; private set; }

    [Function("48896C24104889742418574883EC??48833D??????????418BE8")]
    public static nint DoPerformAction { get; private set; }

    [Function("40534883EC20488BD94883C178E8????488D8B????E8????488D5320")]
    public static IntPtr NetworkEnsembleStart { get; private set; }
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