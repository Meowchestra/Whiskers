using Whiskers.GameFunctions;
using Whiskers.Offsets;
using Chat = Whiskers.GameFunctions.Chat;

namespace Whiskers.IPC;

internal sealed class IpcProvider : IDisposable
{
    private Action? DisposeActions { get; set; }

    public IpcProvider(Whiskers plugin)
    {
        Register("SendChat", (string msg) =>
        {
            Api.Framework?.RunOnTick(delegate
            {
                Chat.SendMessage(msg);
            });
        });

        Register("SetGfxLow", (bool state) => { GameSettings.AgentConfigSystem.SetGfx(state); });

        Register("PartyInvite", (string character, ushort homeWorldId) => Party.PartyInvite(character, homeWorldId));
        Register("PartyInviteAccept", () => Party.Instance.AcceptPartyInviteEnable());
        Register("PartySetLead", (string data) => Party.Instance.PromoteCharacter(data));
        Register("PartyLeave", () => Party.Instance.PartyLeave());
        Register("PartyEnterHouse", () => Party.Instance.EnterHouse());
        Register("PartyTeleport", (bool showMenu) => Party.Instance.Teleport(showMenu));
        Register("PartyFollow", (ulong goId, string name, ushort worldId) => FollowSystem.FollowCharacter(goId, name, worldId));
        Register("PartyUnFollow", FollowSystem.StopFollow);
        Register("MoveTo", (float x, float y, float z, float rot) => MovementFactory.Instance.MoveTo(x, y, z, rot));
        Register("MoveStop", () => MovementFactory.Instance.StopMovement());
        Register("CharacterLogout", MiscGameFunctions.CharacterLogout);
        Register("GameShutdown", MiscGameFunctions.GameShutdown);

    }

    public void Dispose() => DisposeActions?.Invoke();

    private void Register<TRet>(string name, Func<TRet> func)
    {
        var p = Api.PluginInterface?.GetIpcProvider<TRet>("Whiskers." + name);
        p?.RegisterFunc(func);
        if (p != null)
            DisposeActions += p.UnregisterFunc;
    }

    private void Register<TRet, T1>(string name, Func<TRet, T1> func)
    {
        var p = Api.PluginInterface?.GetIpcProvider<TRet, T1>("Whiskers." + name);
        p?.RegisterFunc(func);
        if (p != null)
            DisposeActions += p.UnregisterFunc;
    }

    private void Register(string name, Action func)
    {
        var p = Api.PluginInterface?.GetIpcProvider<object>("Whiskers." + name);
        p?.RegisterAction(func);
        if (p != null)
            DisposeActions += p.UnregisterAction;
    }

    private void Register<T1>(string name, Action<T1> func)
    {
        var p = Api.PluginInterface?.GetIpcProvider<T1, object>("Whiskers." + name);
        p?.RegisterAction(func);
        if (p != null)
            DisposeActions += p.UnregisterAction;
    }
    
    private void Register<T1, T2>(string name, Action<T1, T2> func)
    {
        var p = Api.PluginInterface?.GetIpcProvider<T1, T2, object>("Whiskers." + name);
        p?.RegisterAction(func);
        if (p != null)
            DisposeActions += p.UnregisterAction;
    }

    private void Register<T1, T2, T3>(string name, Action<T1, T2, T3> func)
    {
        var p = Api.PluginInterface?.GetIpcProvider<T1, T2, T3, object>("Whiskers." + name);
        p?.RegisterAction(func);
        if (p != null)
            DisposeActions += p.UnregisterAction;
    }

    private void Register<T1, T2, T3, T4>(string name, Action<T1, T2, T3, T4> func)
    {
        var p = Api.PluginInterface?.GetIpcProvider<T1, T2, T3, T4, object>("Whiskers." + name);
        p?.RegisterAction(func);
        if (p != null)
            DisposeActions += p.UnregisterAction;
    }

}