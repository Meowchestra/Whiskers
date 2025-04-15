/*
 * Copyright(c) 2025 Meowchestra, GiR-Zippo
 * Licensed under the GPL v3 license. See https://github.com/Meowchestra/MeowMusic/blob/main/LICENSE for full license information.
 */

using System.Runtime.InteropServices;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Whiskers.GameFunctions;
using Whiskers.Offsets;

namespace Whiskers.Utils;
public static class AutoSelect
{
    public class AutoSelectYes : IDisposable
    {
        public AutoSelectYes()
        {
            Api.AddonLifecycle?.RegisterListener(AddonEvent.PostSetup, "SelectYesNo", AddonSetup);
        }

        public void Dispose()
        {
            Api.AddonLifecycle?.UnregisterListener(AddonSetup);
        }

        public void Enable()
        {
            Listen = true;
        }

        public void Disable()
        {
            Listen = false;
        }

        private bool Listen { get; set; }

        private unsafe void AddonSetup(AddonEvent eventType, AddonArgs addonInfo)
        {
            if (!Listen)
                return;

            var addon = (AtkUnitBase*)addonInfo.Addon;
            var text = addon->AtkValues[0].GetValueAsString();
            if (LangStrings.LfgPatterns.Any(r => r.IsMatch(text)) ||
                LangStrings.LeavePartyPatterns.Any(r => r.IsMatch(text)) ||
                LangStrings.PromotePatterns.Any(r => r.IsMatch(text)) ||
                LangStrings.ConfirmHouseEntrance.Any(r => r.IsMatch(text)) ||
                LangStrings.ConfirmGroupTeleport.Any(r => r.IsMatch(text)) ||
                LangStrings.ConfirmLogout.Any(r => r.IsMatch(text)) ||
                LangStrings.ConfirmShutdown.Any(r => r.IsMatch(text)))
            {
                PerformActions.ClickYes();
                Party.Instance.AcceptDisable();
            }
        }

        [StructLayout(LayoutKind.Explicit, Size = 0x10)]
        private struct AddonSelectYesNoOnSetupData
        {
            [FieldOffset(0x8)]
            public nint TextPtr;
        }
    }
}