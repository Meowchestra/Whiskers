/*
 * Copyright(c) 2024 Meowchestra, GiR-Zippo, Ori @MidiBard2
 * Licensed under the GPL v3 license. See https://github.com/Meowchestra/MeowMusic/blob/main/LICENSE for full license information.
 */

using Dalamud.Game.Config;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Newtonsoft.Json;

namespace Whiskers.Offsets;

[Serializable]
public class GameSettingsVarTable
{
    public uint Fps { get; set; }
    public uint FpsInActive { get; set; }
    public uint DisplayObjectLimitType { get; set; }

    //DX11
    public uint AntiAliasingDx11 { get; set; }
    public uint TextureFilterQualityDx11 { get; set; }
    public uint TextureAnisotropicQualityDx11 { get; set; }
    public uint SsaoDx11 { get; set; }
    public uint GlareDx11 { get; set; }
    public uint DistortionWaterDx11 { get; set; }
    public uint VignettingDx11 { get; set; }
    public uint RadialBlurDx11 { get; set; }
    public uint DepthOfFieldDx11 { get; set; }
    public uint GrassQualityDx11 { get; set; }
    public uint TranslucentQualityDx11 { get; set; }
    public uint ShadowSoftShadowTypeDx11 { get; set; }
    public uint ShadowTextureSizeTypeDx11 { get; set; }
    public uint ShadowCascadeCountTypeDx11 { get; set; }
    public uint LodTypeDx11 { get; set; }
    public uint OcclusionCullingDx11 { get; set; }
    public uint ShadowLodDx11 { get; set; }
    public uint MapResolutionDx11 { get; set; }
    public uint ShadowVisibilityTypeSelfDx11 { get; set; }
    public uint ShadowVisibilityTypePartyDx11 { get; set; }
    public uint ShadowVisibilityTypeOtherDx11 { get; set; }
    public uint ShadowVisibilityTypeEnemyDx11 { get; set; }
    public uint PhysicsTypeSelfDx11 { get; set; }
    public uint PhysicsTypePartyDx11 { get; set; }
    public uint PhysicsTypeOtherDx11 { get; set; }
    public uint PhysicsTypeEnemyDx11 { get; set; }
    public uint ReflectionTypeDx11 { get; set; }
    public uint ParallaxOcclusionDx11 { get; set; }
    public uint TessellationDx11 { get; set; }
    public uint GlareRepresentationDx11 { get; set; }
    public uint UiAssetType { get; set; }
    public uint ScreenWidth { get; set; }
    public uint ScreenHeight { get; set; }
    //Sound
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

    public static GameSettingsTables Instance
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
    internal class AgentConfigSystem
    {
        #region Get/Restore config
        /// <summary>
        /// Get the gfx settings and save them
        /// </summary>
        public static unsafe void GetSettings(GameSettingsVarTable? varTable)
        {
            var configEntry = Framework.Instance()->SystemConfig.SystemConfigBase.ConfigBase.ConfigEntry;

            if (varTable != null)
            {
                varTable.Fps                    = configEntry[(int)ConfigOption.Fps].Value.UInt;
                varTable.FpsInActive            = configEntry[(int)ConfigOption.FPSInActive].Value.UInt;
                varTable.DisplayObjectLimitType = configEntry[(int)ConfigOption.DisplayObjectLimitType].Value.UInt;

                varTable.AntiAliasingDx11              = configEntry[(int)ConfigOption.AntiAliasing_DX11].Value.UInt;
                varTable.TextureFilterQualityDx11      = configEntry[(int)ConfigOption.TextureFilterQuality_DX11].Value.UInt;
                varTable.TextureAnisotropicQualityDx11 = configEntry[(int)ConfigOption.TextureAnisotropicQuality_DX11].Value.UInt;
                varTable.SsaoDx11                      = configEntry[(int)ConfigOption.SSAO_DX11].Value.UInt;
                varTable.GlareDx11                     = configEntry[(int)ConfigOption.Glare_DX11].Value.UInt;
                varTable.DistortionWaterDx11           = configEntry[(int)ConfigOption.DistortionWater_DX11].Value.UInt;
                varTable.VignettingDx11                = configEntry[(int)ConfigOption.Vignetting_DX11].Value.UInt;
                varTable.RadialBlurDx11                = configEntry[(int)ConfigOption.RadialBlur_DX11].Value.UInt;
                varTable.DepthOfFieldDx11              = configEntry[(int)ConfigOption.DepthOfField_DX11].Value.UInt;
                varTable.GrassQualityDx11              = configEntry[(int)ConfigOption.GrassQuality_DX11].Value.UInt;
                varTable.TranslucentQualityDx11        = configEntry[(int)ConfigOption.TranslucentQuality_DX11].Value.UInt;
                varTable.ShadowSoftShadowTypeDx11      = configEntry[(int)ConfigOption.ShadowSoftShadowType_DX11].Value.UInt;
                varTable.ShadowTextureSizeTypeDx11     = configEntry[(int)ConfigOption.ShadowTextureSizeType_DX11].Value.UInt;
                varTable.ShadowCascadeCountTypeDx11    = configEntry[(int)ConfigOption.ShadowCascadeCountType_DX11].Value.UInt;
                varTable.LodTypeDx11                   = configEntry[(int)ConfigOption.LodType_DX11].Value.UInt;
                varTable.OcclusionCullingDx11          = configEntry[(int)ConfigOption.OcclusionCulling_DX11].Value.UInt;
                varTable.ShadowLodDx11                 = configEntry[(int)ConfigOption.ShadowLOD_DX11].Value.UInt;
                varTable.MapResolutionDx11             = configEntry[(int)ConfigOption.MapResolution_DX11].Value.UInt;
                varTable.ShadowVisibilityTypeSelfDx11  = configEntry[(int)ConfigOption.ShadowVisibilityTypeSelf_DX11].Value.UInt;
                varTable.ShadowVisibilityTypePartyDx11 = configEntry[(int)ConfigOption.ShadowVisibilityTypeParty_DX11].Value.UInt;
                varTable.ShadowVisibilityTypeOtherDx11 = configEntry[(int)ConfigOption.ShadowVisibilityTypeOther_DX11].Value.UInt;
                varTable.ShadowVisibilityTypeEnemyDx11 = configEntry[(int)ConfigOption.ShadowVisibilityTypeEnemy_DX11].Value.UInt;
                varTable.PhysicsTypeSelfDx11           = configEntry[(int)ConfigOption.PhysicsTypeSelf_DX11].Value.UInt;
                varTable.PhysicsTypePartyDx11          = configEntry[(int)ConfigOption.PhysicsTypeParty_DX11].Value.UInt;
                varTable.PhysicsTypeOtherDx11          = configEntry[(int)ConfigOption.PhysicsTypeOther_DX11].Value.UInt;
                varTable.PhysicsTypeEnemyDx11          = configEntry[(int)ConfigOption.PhysicsTypeEnemy_DX11].Value.UInt;
                varTable.ReflectionTypeDx11            = configEntry[(int)ConfigOption.ReflectionType_DX11].Value.UInt;
                varTable.ParallaxOcclusionDx11         = configEntry[(int)ConfigOption.ParallaxOcclusion_DX11].Value.UInt;
                varTable.TessellationDx11              = configEntry[(int)ConfigOption.Tessellation_DX11].Value.UInt;
                varTable.GlareRepresentationDx11       = configEntry[(int)ConfigOption.GlareRepresentation_DX11].Value.UInt;
                varTable.UiAssetType                   = configEntry[(int)ConfigOption.UiAssetType].Value.UInt;
                varTable.ScreenWidth                   = configEntry[(int)ConfigOption.ScreenWidth].Value.UInt;
                varTable.ScreenHeight                  = configEntry[(int)ConfigOption.ScreenHeight].Value.UInt;

                varTable.SoundEnabled                  = configEntry[(int)ConfigOption.IsSndMaster].Value.UInt;
            }
        }

        /// <summary>
        /// Restore the GFX settings
        /// </summary>
        public static unsafe void RestoreSettings(GameSettingsVarTable? varTable)
        {
            var configEntry = Framework.Instance()->SystemConfig.SystemConfigBase.ConfigBase.ConfigEntry;

            if (varTable != null)
            {
                configEntry[(int)ConfigOption.Fps].SetValueUInt(varTable.Fps);
                configEntry[(int)ConfigOption.FPSInActive].SetValueUInt(varTable.FpsInActive);
                configEntry[(int)ConfigOption.DisplayObjectLimitType].SetValueUInt(varTable.DisplayObjectLimitType);

                configEntry[(int)ConfigOption.AntiAliasing_DX11].SetValueUInt(varTable.AntiAliasingDx11);
                configEntry[(int)ConfigOption.TextureFilterQuality_DX11].SetValueUInt(varTable.TextureFilterQualityDx11);
                configEntry[(int)ConfigOption.TextureAnisotropicQuality_DX11].SetValueUInt(varTable.TextureAnisotropicQualityDx11);
                configEntry[(int)ConfigOption.SSAO_DX11].SetValueUInt(varTable.SsaoDx11);
                configEntry[(int)ConfigOption.Glare_DX11].SetValueUInt(varTable.GlareDx11);
                configEntry[(int)ConfigOption.DistortionWater_DX11].SetValueUInt(varTable.DistortionWaterDx11);
                configEntry[(int)ConfigOption.Vignetting_DX11].SetValueUInt(varTable.VignettingDx11);
                configEntry[(int)ConfigOption.RadialBlur_DX11].SetValueUInt(varTable.RadialBlurDx11);
                configEntry[(int)ConfigOption.DepthOfField_DX11].SetValueUInt(varTable.DepthOfFieldDx11);
                configEntry[(int)ConfigOption.GrassQuality_DX11].SetValueUInt(varTable.GrassQualityDx11);
                configEntry[(int)ConfigOption.TranslucentQuality_DX11].SetValueUInt(varTable.TranslucentQualityDx11);
                configEntry[(int)ConfigOption.ShadowSoftShadowType_DX11].SetValueUInt(varTable.ShadowSoftShadowTypeDx11);
                configEntry[(int)ConfigOption.ShadowTextureSizeType_DX11].SetValueUInt(varTable.ShadowTextureSizeTypeDx11);
                configEntry[(int)ConfigOption.ShadowCascadeCountType_DX11].SetValueUInt(varTable.ShadowCascadeCountTypeDx11);
                configEntry[(int)ConfigOption.LodType_DX11].SetValueUInt(varTable.LodTypeDx11);
                configEntry[(int)ConfigOption.OcclusionCulling_DX11].SetValueUInt(varTable.OcclusionCullingDx11);
                configEntry[(int)ConfigOption.ShadowLOD_DX11].SetValueUInt(varTable.ShadowLodDx11);
                configEntry[(int)ConfigOption.MapResolution_DX11].SetValueUInt(varTable.MapResolutionDx11);
                configEntry[(int)ConfigOption.ShadowVisibilityTypeSelf_DX11].SetValueUInt(varTable.ShadowVisibilityTypeSelfDx11);
                configEntry[(int)ConfigOption.ShadowVisibilityTypeParty_DX11].SetValueUInt(varTable.ShadowVisibilityTypePartyDx11);
                configEntry[(int)ConfigOption.ShadowVisibilityTypeOther_DX11].SetValueUInt(varTable.ShadowVisibilityTypeOtherDx11);
                configEntry[(int)ConfigOption.ShadowVisibilityTypeEnemy_DX11].SetValueUInt(varTable.ShadowVisibilityTypeEnemyDx11);
                configEntry[(int)ConfigOption.PhysicsTypeSelf_DX11].SetValueUInt(varTable.PhysicsTypeSelfDx11);
                configEntry[(int)ConfigOption.PhysicsTypeParty_DX11].SetValueUInt(varTable.PhysicsTypePartyDx11);
                configEntry[(int)ConfigOption.PhysicsTypeOther_DX11].SetValueUInt(varTable.PhysicsTypeOtherDx11);
                configEntry[(int)ConfigOption.PhysicsTypeEnemy_DX11].SetValueUInt(varTable.PhysicsTypeEnemyDx11);
                configEntry[(int)ConfigOption.ReflectionType_DX11].SetValueUInt(varTable.ReflectionTypeDx11);
                configEntry[(int)ConfigOption.ParallaxOcclusion_DX11].SetValueUInt(varTable.ParallaxOcclusionDx11);
                configEntry[(int)ConfigOption.Tessellation_DX11].SetValueUInt(varTable.TessellationDx11);
                configEntry[(int)ConfigOption.GlareRepresentation_DX11].SetValueUInt(varTable.GlareRepresentationDx11);
                configEntry[(int)ConfigOption.UiAssetType].SetValueUInt(varTable.UiAssetType);
                configEntry[(int)ConfigOption.ScreenWidth].SetValueUInt(varTable.ScreenWidth);
                configEntry[(int)ConfigOption.ScreenHeight].SetValueUInt(varTable.ScreenHeight);
                
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
            return varTable is { DisplayObjectLimitType: 4, OcclusionCullingDx11: 1, ReflectionTypeDx11: 0, GrassQualityDx11: 0, SsaoDx11: 0 };
        }

        /// <summary>
        /// Set the GFX to minimal
        /// </summary>
        public static unsafe void SetMinimalGfx()
        {
            var configEntry = Framework.Instance()->SystemConfig.SystemConfigBase.ConfigBase.ConfigEntry;

            configEntry[(int)ConfigOption.Fps].SetValueUInt(2);
            configEntry[(int)ConfigOption.FPSInActive].SetValueUInt(0);
            configEntry[(int)ConfigOption.DisplayObjectLimitType].SetValueUInt(4);

            configEntry[(int)ConfigOption.AntiAliasing_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.TextureFilterQuality_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.TextureAnisotropicQuality_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.SSAO_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.Glare_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.DistortionWater_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.Vignetting_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.RadialBlur_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.DepthOfField_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.GrassQuality_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.TranslucentQuality_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.ShadowSoftShadowType_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.ShadowTextureSizeType_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.ShadowCascadeCountType_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.LodType_DX11].SetValueUInt(1);
            configEntry[(int)ConfigOption.OcclusionCulling_DX11].SetValueUInt(1);
            configEntry[(int)ConfigOption.ShadowLOD_DX11].SetValueUInt(1);
            configEntry[(int)ConfigOption.MapResolution_DX11].SetValueUInt(2);
            configEntry[(int)ConfigOption.ShadowVisibilityTypeSelf_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.ShadowVisibilityTypeParty_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.ShadowVisibilityTypeOther_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.ShadowVisibilityTypeEnemy_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.PhysicsTypeSelf_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.PhysicsTypeParty_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.PhysicsTypeOther_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.PhysicsTypeEnemy_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.ReflectionType_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.ParallaxOcclusion_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.Tessellation_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.GlareRepresentation_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.UiAssetType].SetValueUInt(0);
            configEntry[(int)ConfigOption.ScreenWidth].SetValueUInt(1024);
            configEntry[(int)ConfigOption.ScreenHeight].SetValueUInt(720);
        }
        #endregion

        #region Mute/Unmute Sound
        /// <summary>
        /// Mutes/Unmutes the master volume
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
        /// Gets/Sets the master volume
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
        /// Mutes/Unmutes the BGM
        /// </summary>
        /// <param name="enabled"></param>
        public static void SetBgmSoundEnable(bool enabled)
        {
            Api.GameConfig?.Set(SystemConfigOption.IsSndBgm, !enabled);
        }
        public static bool GetBgmSoundEnable()
        {
            return Api.GameConfig != null && Api.GameConfig.TryGet(SystemConfigOption.IsSndBgm, out bool isSndBgm) && isSndBgm;
        }

        /// <summary>
        /// Gets/Sets the BGM volume
        /// </summary>
        /// <param name="value"></param>
        public static void SetBgmSoundVolume(short value)
        {
            Api.GameConfig?.Set(SystemConfigOption.SoundBgm, (uint)value);
        }
        public static int GetBgmSoundVolume()
        {
            return Api.GameConfig != null && Api.GameConfig.TryGet(SystemConfigOption.SoundBgm, out uint isSndBgm) ? (int)isSndBgm : -1;
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
            return Api.GameConfig != null && Api.GameConfig.TryGet(SystemConfigOption.IsSndSe, out bool isSndSe) && isSndSe;
        }

        /// <summary>
        /// Gets/Sets the sound effects volume
        /// </summary>
        /// <param name="value"></param>
        public static void SetEffectsSoundVolume(short value)
        {
            Api.GameConfig?.Set(SystemConfigOption.SoundSe, (uint)value);
        }
        public static int GetEffectsSoundVolume()
        {
            return Api.GameConfig != null && Api.GameConfig.TryGet(SystemConfigOption.SoundSe, out uint isSndSe) ? (int)isSndSe : -1;
        }

        /// <summary>
        /// Mutes/Unmutes the voice sounds
        /// </summary>
        /// <param name="enabled"></param>
        public static void SetVoiceSoundEnable(bool enabled)
        {
            Api.GameConfig?.Set(SystemConfigOption.IsSndVoice, !enabled);
        }
        public static bool GetVoiceSoundEnable()
        {
            return Api.GameConfig != null && Api.GameConfig.TryGet(SystemConfigOption.IsSndVoice, out bool isSndVoice) && isSndVoice;
        }

        /// <summary>
        /// Gets/Sets the voice volume
        /// </summary>
        /// <param name="value"></param>
        public static void SetVoiceSoundVolume(short value)
        {
            Api.GameConfig?.Set(SystemConfigOption.SoundVoice, (uint)value);
        }
        public static int GetVoiceSoundVolume()
        {
            return Api.GameConfig != null && Api.GameConfig.TryGet(SystemConfigOption.SoundVoice, out uint isSndVoice) ? (int)isSndVoice : -1;
        }

        /// <summary>
        /// Mutes/Unmutes the ambient sounds
        /// </summary>
        /// <param name="enabled"></param>
        public static void SetAmbientSoundEnable(bool enabled)
        {
            Api.GameConfig?.Set(SystemConfigOption.IsSndEnv, !enabled);
        }
        public static bool GetAmbientSoundEnable()
        {
            return Api.GameConfig != null && Api.GameConfig.TryGet(SystemConfigOption.IsSndEnv, out bool isSndEnv) && isSndEnv;
        }

        /// <summary>
        /// Gets/Sets the ambient volume
        /// </summary>
        /// <param name="value"></param>
        public static void SetAmbientSoundVolume(short value)
        {
            Api.GameConfig?.Set(SystemConfigOption.SoundEnv, (uint)value);
        }
        public static int GetAmbientSoundVolume()
        {
            return Api.GameConfig != null && Api.GameConfig.TryGet(SystemConfigOption.SoundEnv, out uint isSndEnv) ? (int)isSndEnv : -1;
        }

        /// <summary>
        /// Mutes/Unmutes the system sounds
        /// </summary>
        /// <param name="enabled"></param>
        public static void SetSystemSoundEnable(bool enabled)
        {
            Api.GameConfig?.Set(SystemConfigOption.IsSndSystem, !enabled);
        }
        public static bool GetSystemSoundEnable()
        {
            return Api.GameConfig != null && Api.GameConfig.TryGet(SystemConfigOption.IsSndSystem, out bool isSndSystem) && isSndSystem;
        }

        /// <summary>
        /// Gets/Sets the system volume
        /// </summary>
        /// <param name="value"></param>
        public static void SetSystemSoundVolume(short value)
        {
            Api.GameConfig?.Set(SystemConfigOption.SoundSystem, (uint)value);
        }
        public static int GetSystemSoundVolume()
        {
            return Api.GameConfig != null && Api.GameConfig.TryGet(SystemConfigOption.SoundSystem, out uint isSndSystem) ? (int)isSndSystem : -1;
        }

        /// <summary>
        /// Mutes/Unmutes the performance
        /// </summary>
        /// <param name="enabled"></param>
        public static void SetPerformanceSoundEnable(bool enabled)
        {
            Api.GameConfig?.Set(SystemConfigOption.IsSndPerform, !enabled);
        }
        public static bool GetPerformanceSoundEnable()
        {
            return Api.GameConfig != null && Api.GameConfig.TryGet(SystemConfigOption.IsSndPerform, out bool isSndPerform) && isSndPerform;
        }

        /// <summary>
        /// Gets/Sets the performance volume
        /// </summary>
        /// <param name="value"></param>
        public static void SetPerformanceSoundVolume(short value)
        {
            Api.GameConfig?.Set(SystemConfigOption.SoundPerform, (uint)value);
        }
        public static int GetPerformanceSoundVolume()
        {
            return Api.GameConfig != null && Api.GameConfig.TryGet(SystemConfigOption.SoundPerform, out uint isSndPerform) ? (int)isSndPerform : -1;
        }
        #endregion

        private static string GetCharConfigFilename()
        {
            if (Api.ClientState != null && !Api.ClientState.IsLoggedIn) return "";

            if (Api.ClientState?.LocalPlayer is null) return "";

            var player = Api.ClientState.LocalPlayer;
            if (player == null)
                return "";

            var world = player.HomeWorld.GameData;
            return world == null ? "" : $"{Api.PluginInterface?.GetPluginConfigDirectory()}\\{player.Name.TextValue}-({world.Name.RawString}).json";
        }

        public static void LoadConfig()
        {
            var file = GetCharConfigFilename();
            if (file == "")
                return;
            if (!File.Exists(file))
                return;

            GameSettingsTables.Instance.CustomTable =
                JsonConvert.DeserializeObject<GameSettingsVarTable>(File.ReadAllText(file));
            RestoreSettings(GameSettingsTables.Instance.CustomTable);
        }

        public static void SaveConfig()
        {
            var file = GetCharConfigFilename();
            if (file == "")
                return;

            //Save the config
            GetSettings(GameSettingsTables.Instance?.CustomTable);
            var jsonString = JsonConvert.SerializeObject(GameSettingsTables.Instance?.CustomTable);
            File.WriteAllText(file, JsonConvert.SerializeObject(GameSettingsTables.Instance?.CustomTable));
        }
    }
}