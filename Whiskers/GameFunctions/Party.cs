/*
 * Copyright(c) 2024 Meowchestra, GiR-Zippo
 * Licensed under the GPL v3 license. See https://github.com/Meowchestra/MeowMusic/blob/main/LICENSE for full license information.
 */

using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using Whiskers.Offsets;
using Whiskers.Utils;

namespace Whiskers.GameFunctions;

public class Party : IDisposable
{
    public enum AcceptFlags
    { 
        Accept_Teleport = 0b00000001,
        Accept_GroupInv = 0b00000010,
    }

    private static readonly Lazy<Party> LazyInstance = new(static () => new Party());

    private Party()
    {}

    public static Party Instance => LazyInstance.Value;

    private AutoSelect.AutoSelectYes? YesNoAddon { get; set; }

    private byte AcceptLock { get; set; } = 0;

    public void Initialize()
    {
        YesNoAddon = new AutoSelect.AutoSelectYes();
    }

    public void Dispose()
    {
        YesNoAddon?.Dispose();
        YesNoAddon = null;
    }

    public bool IsAcceptFlagSet(AcceptFlags flag) => ((AcceptFlags)AcceptLock & flag) == flag;

    public void ClearFlags() => AcceptLock = 0;

    public void SetFlag(AcceptFlags flag) => AcceptLock |= (byte)flag;

    public unsafe void PartyInvite(string message)
    {
        if (message == "")
        {
            YesNoAddon?.Enable();
            return;
        }
        var character = message.Split(';')[0];
        var homeWorldId = Convert.ToUInt16(message.Split(';')[1]);
        PartyInvite(character, homeWorldId);
    }

    public unsafe void PartyInvite(string character, ushort homeWorldId)
    {
        InfoProxyPartyInvite.Instance()->InviteToParty(0, character, homeWorldId);
    }

    public void AcceptPartyInviteEnable()
    {
        YesNoAddon?.Enable();
    }

    public unsafe void PromoteCharacter(string message)
    {
        YesNoAddon?.Enable();

        foreach (var i in GroupManager.Instance()->GetGroup()->PartyMembers)
        {
            if (i.NameString.StartsWith(message) || i.NameString == message)
            {
                AgentPartyMember.Instance()->Promote(message, 0, i.ContentId);
                return;
            }
        }
    }

    public unsafe void EnterHouse()
    {
        var entrance = Misc.GetNearestEntrance(out var distance);
        if (entrance != null && distance < 4.8f)
            TargetSystem.Instance()->InteractWithObject((GameObject*)entrance.Address, false);

        YesNoAddon?.Enable();
    }

    public unsafe void Teleport(bool showMenu)
    {
        if (showMenu)
            AgentTeleport.Instance()->Show();
        else
            YesNoAddon?.Enable();
    }

    public unsafe void PartyLeave()
    {
        YesNoAddon?.Enable();

        Api.Framework?.RunOnTick(delegate
        {
            Chat.SendMessage("/leave");
        }, default(TimeSpan), 10, default(CancellationToken));
    }

    public void AcceptDisable()
    {
        //if (AcceptLock == 0)
            YesNoAddon?.Disable();
    }
}