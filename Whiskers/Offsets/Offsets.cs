/*
 * Copyright(c) 2025 Meowchestra, GiR-Zippo, akira0245 @MidiBard, Ori @MidiBard2
 * Licensed under the GPL v3 license. See https://github.com/Meowchestra/MeowMusic/blob/main/LICENSE for full license information.
 */

using System.Diagnostics.CodeAnalysis;
using Dalamud.Hooking;

namespace Whiskers.Offsets;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]

public static class Chat
{
    internal static class Signatures
    {
        internal const string SendChat = "48 89 5C 24 ?? 48 89 74 24 10 57 48 83 EC 20 48 8B F2 48 8B F9 45 84 C9";
        internal const string SanitiseString = "E8 ?? ?? ?? ?? 48 8D 4C 24 ?? 0F B6 F8 E8 ?? ?? ?? ?? 48 8D 4D D0";
    }
}

public static class Offsets
{
    [StaticAddress("48 8B C2 0F B6 15 ?? ?? ?? ?? F6 C2 01")]
    public static nint PerformanceStructPtr { get; private set; }

    [Function("48 89 6C 24 10 48 89 74 24 18 57 48 83 EC ?? 48 83 3D ?? ?? ?? ?? ?? 41 8B E8")]
    public static nint DoPerformAction { get; private set; }

    [Function("40 53 48 83 EC 20 48 8B D9 48 83 C1 78 E8 ?? ?? ?? ?? 48 8D 8B ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8D 53 20")]
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
        if (_networkEnsembleHook != null)
        {
            _networkEnsembleHook.Disable();
            _networkEnsembleHook.Dispose();
        }
    }
}