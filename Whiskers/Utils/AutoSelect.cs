/*
 * Copyright(c) 2024 Meowchestra, GiR-Zippo
 * Licensed under the GPL v3 license. See https://github.com/Meowchestra/MeowMusic/blob/main/LICENSE for full license information.
 */

using System.Runtime.InteropServices;
using System.Text;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Whiskers.GameFunctions;
using Whiskers.Offsets;
using ValueType = FFXIVClientStructs.FFXIV.Component.GUI.ValueType;

namespace Whiskers.Utils;

public abstract class AutoSelect
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
            var dataPtr = (AddonSelectYesNoOnSetupData*)addon;
            if (dataPtr == null)
                return;

            var text = GetSeStringText(MemoryHelper.ReadSeStringNullTerminated(new nint(addon->AtkValues[0].String)));
            if (LangStrings.LfgPatterns.Any(r => r.IsMatch(text)))
            {
                SelectYes(addon);
                Party.Instance.AcceptDisable();
                return;
            }
            if (LangStrings.LeavePartyPatterns.Any(r => r.IsMatch(text)))
            {
                SelectYes(addon);
                Party.Instance.AcceptDisable();
                return;
            }
            if (LangStrings.PromotePatterns.Any(r => r.IsMatch(text)))
            {
                SelectYes(addon);
                Party.Instance.AcceptDisable();
                return;
            }
            if (LangStrings.ConfirmHouseEntrance.Any(r => r.IsMatch(text)))
            {
                SelectYes(addon);
                Party.Instance.AcceptDisable();
                return;
            }
            if (LangStrings.ConfirmGroupTeleport.Any(r => r.IsMatch(text)))
            {
                SelectYes(addon);
                Party.Instance.AcceptDisable();
                return;
            }
            if (LangStrings.ConfirmLogout.Any(r => r.IsMatch(text)))
            {
                SelectYes(addon);
                Party.Instance.AcceptDisable();
                return;
            }
            if (LangStrings.ConfirmShutdown.Any(r => r.IsMatch(text)))
            {
                SelectYes(addon);
                Party.Instance.AcceptDisable();
            }
        }

        private static unsafe bool SelectYes(AtkUnitBase* addon)
        {
            if (addon == null) return false;
            GenerateCallback(addon, 0);
            addon->Close(false);
            return true;
        }

        private static unsafe void GenerateCallback(AtkUnitBase* unitBase, params object[] values)
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
                        Marshal.FreeHGlobal(new nint(atkValues[i].String));
                    }
                }
                Marshal.FreeHGlobal(new nint(atkValues));
            }
        }

        private static string GetSeStringText(SeString seString)
        {
            var pieces = seString.Payloads.OfType<TextPayload>().Select(t => t.Text);
            var text = string.Join(string.Empty, pieces).Replace('\n', ' ').Trim();
            return text;
        }

        [StructLayout(LayoutKind.Explicit, Size = 0x10)]
        private struct AddonSelectYesNoOnSetupData
        {
            [FieldOffset(0x8)]
            public nint TextPtr;
        }
    }
}