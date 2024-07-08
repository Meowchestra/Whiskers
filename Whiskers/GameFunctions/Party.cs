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

public static class Party
{
    private static AutoSelect.AutoSelectYes? YesNoAddon { get; set; }
    public static unsafe void PartyInvite(string message)
    {
        if (message == "")
        {
            AcceptPartyInviteEnable();
            return;
        }
        var character = message.Split(';')[0];
        var homeWorldId = Convert.ToUInt16(message.Split(';')[1]);
        InfoProxyPartyInvite.Instance()->InviteToParty(0, character, homeWorldId);
    }

    public static void AcceptPartyInviteEnable()
    {
        if (YesNoAddon != null)
            return;
        Api.PluginLog?.Debug("Create new AcceptPartyInviteEnable");
        YesNoAddon = new AutoSelect.AutoSelectYes();
        AutoSelect.AutoSelectYes.Enable();
    }

    public static unsafe void PromoteCharacter(string message)
    {
        Api.PluginLog?.Debug(message);
        if (YesNoAddon != null)
            return;
        Api.PluginLog?.Debug("Create new AcceptPromote");
        YesNoAddon = new AutoSelect.AutoSelectYes();
        AutoSelect.AutoSelectYes.Enable();

        Api.PluginLog?.Debug(message);
        foreach (var i in GroupManager.Instance()->GetGroup()->PartyMembers)
        {
            if (i.NameString.StartsWith(message))
            {
                AgentPartyMember.Instance()->Promote(message, 0, i.ContentId);
                return;
            }
        }
    }

    public static unsafe void EnterHouse()
    {
        var entrance = Misc.GetNearestEntrance(out var distance);
        if (entrance != null && distance < 4.8f)
            TargetSystem.Instance()->InteractWithObject((GameObject*)entrance.Address, false);

        if (YesNoAddon != null)
            return;
        Api.PluginLog?.Debug("Create new AcceptPartyInviteEnable");
        YesNoAddon = new AutoSelect.AutoSelectYes();
        AutoSelect.AutoSelectYes.Enable();
    }

    public static void AcceptDisable()
    {
        if (YesNoAddon == null)
            return;
        AutoSelect.AutoSelectYes.Disable();
        YesNoAddon = null;
    }
}