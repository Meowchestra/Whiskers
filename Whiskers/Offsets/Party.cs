/*
 * Copyright(c) 2024 Meowchestra, GiR-Zippo, akira0245 @MidiBard, Ori @MidiBard2
 * Licensed under the GPL v3 license. See https://github.com/Meowchestra/MeowMusic/blob/main/LICENSE for full license information.
 */

using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ValueType = FFXIVClientStructs.FFXIV.Component.GUI.ValueType;

namespace Whiskers.Offsets;

public static class Party
{
    public static AutoSelectYesNo? YesNoAddon { get; set; }
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
        YesNoAddon = new AutoSelectYesNo();
        AutoSelectYesNo.Enable();
    }

    public static unsafe void PromoteCharacter(string message)
    {
        Api.PluginLog?.Debug(message);
        if (YesNoAddon != null)
            return;
        Api.PluginLog?.Debug("Create new AcceptPromote");
        YesNoAddon = new AutoSelectYesNo();
        AutoSelectYesNo.Enable();

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

    public static void AcceptDisable()
    {
        if (YesNoAddon == null)
            return;
        AutoSelectYesNo.Disable();
        YesNoAddon = null;
    }

    public static readonly List<Regex> LfgPatterns =
    [
        new Regex(@"Join .* party\?"),
        new Regex(@".*のパーティに参加します。よろしいですか？"),
        new Regex(@"Der Gruppe von .* beitreten\?"),
        new Regex(@"Rejoindre l'équipe de .*\?")
    ];

    public static readonly List<Regex> PromotePatterns =
    [
        new Regex(@"Promote .* to party leader\?"),
        new Regex(@".* zum Gruppenanführer machen\?")
    ];
}

public class AutoSelectYesNo
{
    public static void Enable()
    {
        Api.AddonLifecycle?.RegisterListener(AddonEvent.PostSetup, "SelectYesNo", AddonSetup);
    }

    public static void Disable()
    {
        Api.AddonLifecycle?.UnregisterListener(AddonSetup);
    }

    protected static unsafe void AddonSetup(AddonEvent eventType, AddonArgs addonInfo)
    {
        var addon = (AtkUnitBase*)addonInfo.Addon;
        var dataPtr = (AddonSelectYesNoOnSetupData*)addon;
        if (dataPtr == null)
            return;

        var text = GetSeStringText(MemoryHelper.ReadSeStringNullTerminated(new nint(addon->AtkValues[0].String)));
        if (Party.LfgPatterns.Any(r => r.IsMatch(text)))
        {
            SelectYes(addon);
            Party.AcceptDisable();
            return;
        }

        if (Party.PromotePatterns.Any(r => r.IsMatch(text)))
        {
            SelectYes(addon);
            Party.AcceptDisable();
        }
    }

    public static unsafe bool SelectYes(AtkUnitBase* addon)
    {
        if (addon == null) return false;
        GenerateCallback(addon, 0);
        addon->Close(false);
        return true;
    }

    public static unsafe void GenerateCallback(AtkUnitBase* unitBase, params object[] values)
    {
        if (unitBase == null) throw new Exception("Null UnitBase");
        var atkValues = (AtkValue*)Marshal.AllocHGlobal(values.Length * sizeof(AtkValue));
        if (atkValues == null) return;
        try
        {
            for (var i = 0; i < values.Length; i++)
            {
                var v = values[i];
                switch (v)
                {
                    case uint uintValue:
                        atkValues[i].Type = ValueType.UInt;
                        atkValues[i].UInt = uintValue;
                        break;
                    case int intValue:
                        atkValues[i].Type = ValueType.Int;
                        atkValues[i].Int  = intValue;
                        break;
                    case float floatValue:
                        atkValues[i].Type  = ValueType.Float;
                        atkValues[i].Float = floatValue;
                        break;
                    case bool boolValue:
                        atkValues[i].Type = ValueType.Bool;
                        atkValues[i].Byte = (byte)(boolValue ? 1 : 0);
                        break;
                    case string stringValue:
                    {
                        atkValues[i].Type = ValueType.String;
                        var stringBytes = Encoding.UTF8.GetBytes(stringValue);
                        var stringAlloc = Marshal.AllocHGlobal(stringBytes.Length + 1);
                        Marshal.Copy(stringBytes, 0, stringAlloc, stringBytes.Length);
                        Marshal.WriteByte(stringAlloc, stringBytes.Length, 0);
                        atkValues[i].String = (byte*)stringAlloc;
                        break;
                    }
                    default:
                        throw new ArgumentException($"Unable to convert type {v.GetType()} to AtkValue");
                }
            }
            unitBase->FireCallback((ushort)values.Length, atkValues);
        }
        finally
        {
            for (var i = 0; i < values.Length; i++)
            {
                if (atkValues[i].Type == ValueType.String)
                {
                    Marshal.FreeHGlobal(new IntPtr(atkValues[i].String));
                }
            }
            Marshal.FreeHGlobal(new IntPtr(atkValues));
        }
    }

    internal static string GetSeStringText(SeString seString)
    {
        var pieces = seString.Payloads.OfType< TextPayload >().Select(t => t.Text);
        var text = string.Join(string.Empty, pieces).Replace('\n', ' ').Trim();
        return text;
    }

    [StructLayout(LayoutKind.Explicit, Size = 0x10)]
    private struct AddonSelectYesNoOnSetupData
    {
        [FieldOffset(0x8)]
        public IntPtr TextPtr;
    }
}