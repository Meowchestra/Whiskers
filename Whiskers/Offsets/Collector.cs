/*
 * Copyright(c) 2025 Meowchestra, GiR-Zippo
 * Licensed under the GPL v3 license. See https://github.com/Meowchestra/MeowMusic/blob/main/LICENSE for full license information.
 */

using Dalamud.Plugin.Services;

namespace Whiskers.Offsets;

public readonly struct PlayerInfo(string? name, string? world, string? region)
{
    // Struct for holding the playerInfo relevant to our purposes. These values
    // are fetched using the various methods throughout the PartyHandler class

    private string? PlayerName { get; } = name;

    private string? PlayerWorld { get; } = world;

    private string? PlayerRegion { get; } = region;

    public override string ToString() => $"{PlayerName} [{PlayerWorld}, {PlayerRegion}]";
}

public class Collector
{
    #region Const/Dest
    private static readonly Lazy<Collector> LazyInstance = new(() => new Collector());

    private Collector()
    {
    }

    public static Collector Instance => LazyInstance.Value;

       
    public void Initialize(IDataManager? data, IClientState? clientState, IPartyList? partyList)
    {
        Data        = data;
        ClientState = clientState;
        PartyList   = partyList;
        if (ClientState != null)
        {
            ClientState.Login  += ClientState_Login;
            ClientState.Logout += ClientState_Logout;
        }
    }

    ~Collector() => Dispose();
    public void Dispose()
    {
        if (ClientState != null)
        {
            ClientState.Login  -= ClientState_Login;
            ClientState.Logout -= ClientState_Logout;
        }

        GC.SuppressFinalize(this);
    }
    #endregion

    internal IDataManager? Data;
    internal IClientState? ClientState;
    internal IPartyList? PartyList;

    /// <summary>
    /// Only called when the plugin is started
    /// </summary>
    public void UpdateClientStats()
    {
        ClientState_Login();
    }

    /// <summary>
    /// Triggered by ClientState_Login
    /// Send the Name and WorldId to the LA
    /// </summary>
    private void ClientState_Login()
    {
        if (ClientState?.LocalPlayer != null)
        {
            var name = ClientState?.LocalPlayer?.Name.TextValue;
            var homeWorld = ClientState?.LocalPlayer?.HomeWorld.ValueNullable?.RowId;
            if (Pipe.Client != null && Pipe.Client.IsConnected)
            {
                Pipe.Client.WriteAsync(new IpcMessage
                {
                    MsgType = MessageType.NameAndHomeWorld,
                    Message = Environment.ProcessId + ":" + name + ":" + homeWorld
                });
            }
        }
    }

    private static void ClientState_Logout(int type, int code)
    {
    }

    private List<PlayerInfo> _getInfoFromNormalParty()
    {
        // Generates a list of playerInfo objects from the game's memory
        // assuming the party is a normal party (light/full/etc.)
        var output = new List<PlayerInfo>();
        if (PartyList != null)
        {
            var pCount = PartyList.Length;

            for (var i = 0; i < pCount; i++)
            {
                var memberPtr = PartyList.GetPartyMemberAddress(i);
                var member = PartyList.CreatePartyMemberReference(memberPtr);
                var tempName = member?.Name.ToString();
                const string tempWorld = "";
                const string tempRegion = "";
                output.Add(new PlayerInfo(tempName, tempWorld, tempRegion));
            }
        }

        return output;
    }
}