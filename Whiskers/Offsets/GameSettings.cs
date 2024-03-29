/*
 * Copyright(c) 2023 GiR-Zippo, Ori@MidiBard2
 * Licensed under the GPL v3 license. See https://github.com/GiR-Zippo/LightAmp/blob/main/LICENSE for full license information.
 */

using System.Runtime.InteropServices;
using Dalamud.Game.Config;
using Dalamud.Game.Gui.Toast;
using Dalamud.Game.Text.SeStringHandling;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Newtonsoft.Json;

namespace Whiskers.Offsets;

[Serializable]
public class GameSettingsVarTable
{
    public uint FpsInActive { get; set; }
    public uint OriginalObjQuantity { get; set; }
    public uint WaterWetDx11 { get; set; }
    public uint OcclusionCullingDx11 { get; set; }
    public uint LodTypeDx11 { get; set; }
    public uint ReflectionTypeDx11 { get; set; }
    public uint AntiAliasingDx11 { get; set; }
    public uint TranslucentQualityDx11 { get; set; }
    public uint GrassQualityDx11 { get; set; }
    public uint ParallaxOcclusionDx11 { get; set; }
    public uint TessellationDx11 { get; set; }
    public uint GlareRepresentationDx11 { get; set; }
    public uint MapResolutionDx11 { get; set; }
    public uint ShadowVisibilityTypeSelfDx11 { get; set; }
    public uint ShadowVisibilityTypePartyDx11 { get; set; }
    public uint ShadowVisibilityTypeOtherDx11 { get; set; }
    public uint ShadowVisibilityTypeEnemyDx11 { get; set; }
    public uint ShadowLodDx11 { get; set; }
    public uint ShadowTextureSizeTypeDx11 { get; set; }
    public uint ShadowCascadeCountTypeDx11 { get; set; }
    public uint ShadowSoftShadowTypeDx11 { get; set; }
    public uint TextureFilterQualityDx11 { get; set; }
    public uint TextureAnisotropicQualityDx11 { get; set; }
    public uint PhysicsTypeSelfDx11 { get; set; }
    public uint PhysicsTypePartyDx11 { get; set; }
    public uint PhysicsTypeOtherDx11 { get; set; }
    public uint PhysicsTypeEnemyDx11 { get; set; }
    public uint RadialBlurDx11 { get; set; }
    public uint SsaoDx11 { get; set; }
    public uint GlareDx11 { get; set; }
    public uint DistortionWaterDx11 { get; set; }
    public uint SoundEnabled { get; set; }
}

public sealed class GameSettingsTables
{
    private static GameSettingsTables? _instance;
    private static readonly object Padlock = new();
    public GameSettingsVarTable? StartupTable { get; set; } = new();
    public GameSettingsVarTable? CustomTable { get; set; } = new();

    private GameSettingsTables()
    {
    }

    public static GameSettingsTables? Instance
    {
        get
        {
            lock (Padlock)
            {
                return _instance ??= new GameSettingsTables();
            }
        }
    }
}

internal static class GameSettings
{
    /// <summary>
    /// The agent for the client config
    /// </summary>
    internal class AgentConfigSystem(AgentInterface agentInterface)
        : AgentInterface(agentInterface.Pointer, agentInterface.Id)
    {
        /// <summary>
        /// Apply Settings
        /// </summary>
        public unsafe void ApplyGraphicSettings()
        {
            var refreshConfigGraphicState = (delegate* unmanaged<nint, long>)Offsets.ApplyGraphicConfigsFunc;
            var _ = refreshConfigGraphicState(Pointer);
            if (Api.ToastGui != null) Api.ToastGui.Toast += OnToast;

            void OnToast(ref SeString message, ref ToastOptions options, ref bool handled)
            {
                if (Api.ToastGui != null) Api.ToastGui.Toast -= OnToast;
            }
        }
        #region Get/Restore config

        /// <summary>
        /// Get the gfx settings and save them
        /// </summary>
        public static unsafe void GetSettings(GameSettingsVarTable? varTable)
        {
            var configEntry = Framework.Instance()->SystemConfig.CommonSystemConfig.ConfigBase.ConfigEntry;

            if (varTable != null)
            {
                varTable.FpsInActive             = configEntry[(int)ConfigOption.FPSInActive].Value.UInt;
                varTable.OriginalObjQuantity     = configEntry[(int)ConfigOption.DisplayObjectLimitType].Value.UInt;
                varTable.WaterWetDx11            = configEntry[(int)ConfigOption.WaterWet_DX11].Value.UInt;
                varTable.OcclusionCullingDx11    = configEntry[(int)ConfigOption.OcclusionCulling_DX11].Value.UInt;
                varTable.LodTypeDx11             = configEntry[(int)ConfigOption.LodType_DX11].Value.UInt;
                varTable.ReflectionTypeDx11      = configEntry[(int)ConfigOption.ReflectionType_DX11].Value.UInt;
                varTable.AntiAliasingDx11        = configEntry[(int)ConfigOption.AntiAliasing_DX11].Value.UInt;
                varTable.TranslucentQualityDx11  = configEntry[(int)ConfigOption.TranslucentQuality_DX11].Value.UInt;
                varTable.GrassQualityDx11        = configEntry[(int)ConfigOption.GrassQuality_DX11].Value.UInt;
                varTable.ParallaxOcclusionDx11   = configEntry[(int)ConfigOption.ParallaxOcclusion_DX11].Value.UInt;
                varTable.TessellationDx11        = configEntry[(int)ConfigOption.Tessellation_DX11].Value.UInt;
                varTable.GlareRepresentationDx11 = configEntry[(int)ConfigOption.GlareRepresentation_DX11].Value.UInt;
                varTable.MapResolutionDx11       = configEntry[(int)ConfigOption.MapResolution_DX11].Value.UInt;
                varTable.ShadowVisibilityTypeSelfDx11 =
                    configEntry[(int)ConfigOption.ShadowVisibilityTypeSelf_DX11].Value.UInt;
                varTable.ShadowVisibilityTypePartyDx11 =
                    configEntry[(int)ConfigOption.ShadowVisibilityTypeParty_DX11].Value.UInt;
                varTable.ShadowVisibilityTypeOtherDx11 =
                    configEntry[(int)ConfigOption.ShadowVisibilityTypeOther_DX11].Value.UInt;
                varTable.ShadowVisibilityTypeEnemyDx11 =
                    configEntry[(int)ConfigOption.ShadowVisibilityTypeEnemy_DX11].Value.UInt;
                varTable.ShadowLodDx11 = configEntry[(int)ConfigOption.ShadowLOD_DX11].Value.UInt;
                varTable.ShadowTextureSizeTypeDx11 =
                    configEntry[(int)ConfigOption.ShadowTextureSizeType_DX11].Value.UInt;
                varTable.ShadowCascadeCountTypeDx11 =
                    configEntry[(int)ConfigOption.ShadowCascadeCountType_DX11].Value.UInt;
                varTable.ShadowSoftShadowTypeDx11 = configEntry[(int)ConfigOption.ShadowSoftShadowType_DX11].Value.UInt;
                varTable.TextureFilterQualityDx11 = configEntry[(int)ConfigOption.TextureFilterQuality_DX11].Value.UInt;
                varTable.TextureAnisotropicQualityDx11 =
                    configEntry[(int)ConfigOption.TextureAnisotropicQuality_DX11].Value.UInt;
                varTable.PhysicsTypeSelfDx11  = configEntry[(int)ConfigOption.PhysicsTypeSelf_DX11].Value.UInt;
                varTable.PhysicsTypePartyDx11 = configEntry[(int)ConfigOption.PhysicsTypeParty_DX11].Value.UInt;
                varTable.PhysicsTypeOtherDx11 = configEntry[(int)ConfigOption.PhysicsTypeOther_DX11].Value.UInt;
                varTable.PhysicsTypeEnemyDx11 = configEntry[(int)ConfigOption.PhysicsTypeEnemy_DX11].Value.UInt;
                varTable.RadialBlurDx11       = configEntry[(int)ConfigOption.RadialBlur_DX11].Value.UInt;
                varTable.SsaoDx11             = configEntry[(int)ConfigOption.SSAO_DX11].Value.UInt;
                varTable.GlareDx11            = configEntry[(int)ConfigOption.Glare_DX11].Value.UInt;
                varTable.DistortionWaterDx11  = configEntry[(int)ConfigOption.DistortionWater_DX11].Value.UInt;
                varTable.SoundEnabled         = configEntry[(int)ConfigOption.IsSndMaster].Value.UInt;
            }
        }

        /// <summary>
        /// Restore the GFX settings
        /// </summary>
        public static unsafe void RestoreSettings(GameSettingsVarTable? varTable)
        {
            var configEntry = Framework.Instance()->SystemConfig.CommonSystemConfig.ConfigBase.ConfigEntry;

            if (varTable != null)
            {
                configEntry[(int)ConfigOption.FPSInActive].SetValueUInt(varTable.FpsInActive);
                configEntry[(int)ConfigOption.DisplayObjectLimitType].SetValueUInt(varTable.OriginalObjQuantity);
                configEntry[(int)ConfigOption.WaterWet_DX11].SetValueUInt(varTable.WaterWetDx11);
                configEntry[(int)ConfigOption.OcclusionCulling_DX11].SetValueUInt(varTable.OcclusionCullingDx11);
                configEntry[(int)ConfigOption.LodType_DX11].SetValueUInt(varTable.LodTypeDx11);
                configEntry[(int)ConfigOption.ReflectionType_DX11].SetValueUInt(varTable.ReflectionTypeDx11);
                configEntry[(int)ConfigOption.AntiAliasing_DX11].SetValueUInt(varTable.AntiAliasingDx11);
                configEntry[(int)ConfigOption.TranslucentQuality_DX11].SetValueUInt(varTable.TranslucentQualityDx11);
                configEntry[(int)ConfigOption.GrassQuality_DX11].SetValueUInt(varTable.GrassQualityDx11);
                configEntry[(int)ConfigOption.ParallaxOcclusion_DX11].SetValueUInt(varTable.ParallaxOcclusionDx11);
                configEntry[(int)ConfigOption.Tessellation_DX11].SetValueUInt(varTable.TessellationDx11);
                configEntry[(int)ConfigOption.GlareRepresentation_DX11].SetValueUInt(varTable.GlareRepresentationDx11);
                configEntry[(int)ConfigOption.MapResolution_DX11].SetValueUInt(varTable.MapResolutionDx11);
                configEntry[(int)ConfigOption.ShadowVisibilityTypeSelf_DX11]
                    .SetValueUInt(varTable.ShadowVisibilityTypeSelfDx11);
                configEntry[(int)ConfigOption.ShadowVisibilityTypeParty_DX11]
                    .SetValueUInt(varTable.ShadowVisibilityTypePartyDx11);
                configEntry[(int)ConfigOption.ShadowVisibilityTypeOther_DX11]
                    .SetValueUInt(varTable.ShadowVisibilityTypeOtherDx11);
                configEntry[(int)ConfigOption.ShadowVisibilityTypeEnemy_DX11]
                    .SetValueUInt(varTable.ShadowVisibilityTypeEnemyDx11);
                configEntry[(int)ConfigOption.ShadowLOD_DX11].SetValueUInt(varTable.ShadowLodDx11);
                configEntry[(int)ConfigOption.ShadowTextureSizeType_DX11]
                    .SetValueUInt(varTable.ShadowTextureSizeTypeDx11);
                configEntry[(int)ConfigOption.ShadowCascadeCountType_DX11]
                    .SetValueUInt(varTable.ShadowCascadeCountTypeDx11);
                configEntry[(int)ConfigOption.ShadowSoftShadowType_DX11]
                    .SetValueUInt(varTable.ShadowSoftShadowTypeDx11);
                configEntry[(int)ConfigOption.TextureFilterQuality_DX11]
                    .SetValueUInt(varTable.TextureFilterQualityDx11);
                configEntry[(int)ConfigOption.TextureAnisotropicQuality_DX11]
                    .SetValueUInt(varTable.TextureAnisotropicQualityDx11);
                configEntry[(int)ConfigOption.PhysicsTypeSelf_DX11].SetValueUInt(varTable.PhysicsTypeSelfDx11);
                configEntry[(int)ConfigOption.PhysicsTypeParty_DX11].SetValueUInt(varTable.PhysicsTypePartyDx11);
                configEntry[(int)ConfigOption.PhysicsTypeOther_DX11].SetValueUInt(varTable.PhysicsTypeOtherDx11);
                configEntry[(int)ConfigOption.PhysicsTypeEnemy_DX11].SetValueUInt(varTable.PhysicsTypeEnemyDx11);
                configEntry[(int)ConfigOption.RadialBlur_DX11].SetValueUInt(varTable.RadialBlurDx11);
                configEntry[(int)ConfigOption.SSAO_DX11].SetValueUInt(varTable.SsaoDx11);
                configEntry[(int)ConfigOption.Glare_DX11].SetValueUInt(varTable.GlareDx11);
                configEntry[(int)ConfigOption.DistortionWater_DX11].SetValueUInt(varTable.DistortionWaterDx11);
                configEntry[(int)ConfigOption.IsSndMaster].SetValueUInt(varTable.SoundEnabled);
            }
        }
        #endregion

        #region GfxConfig
        /// <summary>
        /// Basic check if GFX settings are low
        /// </summary>
        /// <returns></returns>
        public static bool CheckLowSettings(GameSettingsVarTable? varTable)
        {
            return varTable is { OriginalObjQuantity: 4, WaterWetDx11: 0, OcclusionCullingDx11: 1, ReflectionTypeDx11: 3, GrassQualityDx11: 3, SsaoDx11: 4 };
        }

        /// <summary>
        /// Set the GFX to minimal
        /// </summary>
        public static unsafe void SetMinimalGfx()
        {
            var configEntry = Framework.Instance()->SystemConfig.CommonSystemConfig.ConfigBase.ConfigEntry;

            configEntry[(int)ConfigOption.FPSInActive].SetValueUInt(0);
            configEntry[(int)ConfigOption.DisplayObjectLimitType].SetValueUInt(4);
            configEntry[(int)ConfigOption.WaterWet_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.OcclusionCulling_DX11].SetValueUInt(1);
            configEntry[(int)ConfigOption.LodType_DX11].SetValueUInt(1);
            configEntry[(int)ConfigOption.ReflectionType_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.AntiAliasing_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.TranslucentQuality_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.GrassQuality_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.ParallaxOcclusion_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.Tessellation_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.GlareRepresentation_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.MapResolution_DX11].SetValueUInt(2);
            configEntry[(int)ConfigOption.ShadowVisibilityTypeSelf_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.ShadowVisibilityTypeParty_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.ShadowVisibilityTypeOther_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.ShadowVisibilityTypeEnemy_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.ShadowLOD_DX11].SetValueUInt(1);
            configEntry[(int)ConfigOption.ShadowTextureSizeType_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.ShadowCascadeCountType_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.ShadowSoftShadowType_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.TextureFilterQuality_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.TextureAnisotropicQuality_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.PhysicsTypeSelf_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.PhysicsTypeParty_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.PhysicsTypeOther_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.PhysicsTypeEnemy_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.RadialBlur_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.SSAO_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.Glare_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.DistortionWater_DX11].SetValueUInt(0);
        }
        #endregion

        #region Mute/Unmute MasterSound

        /// <summary>
        /// Mutes/Unmutes the sound
        /// </summary>
        /// <param name="enabled"></param>
        public static void SetMasterSoundEnable(bool enabled)
        {
            Api.GameConfig?.Set(SystemConfigOption.IsSndMaster, !enabled);
        }

        public static bool GetMasterSoundEnable()
        {
            return Api.GameConfig != null && Api.GameConfig.TryGet(SystemConfigOption.IsSndMaster, out bool isSndMaster) && isSndMaster;
        }

        /// <summary>
        /// Gets/Sets the master-volume
        /// </summary>
        /// <param name="value"></param>
        public static void SetMasterSoundVolume(short value)
        {
            Api.GameConfig?.Set(SystemConfigOption.SoundMaster, (uint)value);
        }

        public static int GetMasterSoundVolume()
        {
            return Api.GameConfig != null && Api.GameConfig.TryGet(SystemConfigOption.SoundMaster, out uint isSndMaster) ? (int)isSndMaster : -1;
        }

        /// <summary>
        /// Mutes/Unmutes the voices
        /// </summary>
        /// <param name="enabled"></param>
        public static void SetVoiceSoundEnable(bool enabled)
        {
            Api.GameConfig?.Set(SystemConfigOption.IsSndVoice, !enabled);
        }

        public static bool GetVoiceSoundEnable()
        {
            return Api.GameConfig != null && Api.GameConfig.TryGet(SystemConfigOption.IsSndVoice, out bool isSndMaster) && isSndMaster;
        }

        /// <summary>
        /// Mutes/Unmutes the sound effects
        /// </summary>
        /// <param name="enabled"></param>
        public static void SetEffectsSoundEnable(bool enabled)
        {
            Api.GameConfig?.Set(SystemConfigOption.IsSndSe, !enabled);
        }

        public static bool GetEffectsSoundEnable()
        {
            return Api.GameConfig != null && Api.GameConfig.TryGet(SystemConfigOption.IsSndSe, out bool isSndMaster) && isSndMaster;
        }
        #endregion

        public static void LoadConfig()
        {
            if (Api.ClientState != null && !Api.ClientState.IsLoggedIn)
                return;

            var file = $"{Api.PluginInterface?.GetPluginConfigDirectory()}\\{Api.ClientState?.LocalPlayer?.Name}-({Api.ClientState?.LocalPlayer?.HomeWorld.GameData?.Name}).json";
            if (!File.Exists(file))
                return;

            if (GameSettingsTables.Instance != null)
            {
                GameSettingsTables.Instance.CustomTable =
                    JsonConvert.DeserializeObject<GameSettingsVarTable>(File.ReadAllText(file));
                RestoreSettings(GameSettingsTables.Instance.CustomTable);
            }

            Whiskers.AgentConfigSystem?.ApplyGraphicSettings();
        }

        public static void SaveConfig()
        {
            if (Api.ClientState != null && !Api.ClientState.IsLoggedIn)
                return;

            //Save the config
            GetSettings(GameSettingsTables.Instance?.CustomTable);
            var jsonString = JsonConvert.SerializeObject(GameSettingsTables.Instance?.CustomTable);
            File.WriteAllText($"{Api.PluginInterface?.GetPluginConfigDirectory()}\\{Api.ClientState?.LocalPlayer?.Name}-({Api.ClientState?.LocalPlayer?.HomeWorld.GameData?.Name}).json",
                JsonConvert.SerializeObject(GameSettingsTables.Instance?.CustomTable));
        }
    }
}

public unsafe class AgentInterface
{
    public nint Pointer { get; }
    public nint VTable { get; }
    public int Id { get; }
    public FFXIVClientStructs.FFXIV.Component.GUI.AgentInterface* Struct => (FFXIVClientStructs.FFXIV.Component.GUI.AgentInterface*)Pointer;

    public AgentInterface(nint pointer, int id)
    {
        Pointer = pointer;
        Id      = id;
        VTable  = Marshal.ReadIntPtr(Pointer);
    }

    public override string ToString()
    {
        return $"{Id} {(long)Pointer:X} {(long)VTable:X}";
    }
}

internal unsafe class AgentManager
{
    private List<AgentInterface> AgentTable { get; } = new(400);

    private AgentManager()
    {
        try
        {
            var instance = Framework.Instance();
            var agentModule = instance->UIModule->GetAgentModule();
            var i = 0;
            foreach (var pointer in agentModule->AgentsSpan)
                AgentTable.Add(new AgentInterface((nint)pointer.Value, i++));
        }
        catch (Exception e)
        {
            Api.PluginLog?.Error(e.ToString());
        }
    }

    public static AgentManager Instance { get; } = new();

    internal AgentInterface FindAgentInterfaceByVtable(nint vtbl) => AgentTable.First(i => i.VTable == vtbl);
}
