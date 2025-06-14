/*
 * Copyright(c) 2025 Meowchestra, GiR-Zippo, Ori @MidiBard2
 * Licensed under the GPL v3 license. See https://github.com/Meowchestra/MeowMusic/blob/main/LICENSE for full license information.
 */

using Dalamud.Game.Config;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Newtonsoft.Json;
using Whiskers.Utils;

namespace Whiskers.Offsets;

// ReSharper disable InconsistentNaming
[Serializable]
public class GameSettingsVarTable
{
    public uint Fps { get; set; }
    public uint FPSInActive { get; set; }
    public uint DisplayObjectLimitType { get; set; }

    //DX11
    public uint AntiAliasing_DX11 { get; set; }
    //public uint TextureFilterQuality_DX11 { get; set; } // Hidden / Removed Setting?
    public uint TextureAnisotropicQuality_DX11 { get; set; }
    public uint SSAO_DX11 { get; set; }
    public uint Glare_DX11 { get; set; }
    public uint DistortionWater_DX11 { get; set; }
    public uint DepthOfField_DX11 { get; set; }
    public uint RadialBlur_DX11 { get; set; }
    public uint Vignetting_DX11 { get; set; }
    public uint GrassQuality_DX11 { get; set; }
    public uint TranslucentQuality_DX11 { get; set; }
    public uint ShadowSoftShadowType_DX11 { get; set; }
    public uint ShadowTextureSizeType_DX11 { get; set; }
    public uint ShadowCascadeCountType_DX11 { get; set; }
    public uint LodType_DX11 { get; set; }
    //public uint OcclusionCulling_DX11 { get; set; } // Hidden / Removed Setting?
    public uint ShadowLOD_DX11 { get; set; }
    //public uint MapResolution_DX11 { get; set; } // Hidden / Removed Setting?
    public uint ShadowVisibilityTypeSelf_DX11 { get; set; }
    public uint ShadowVisibilityTypeParty_DX11 { get; set; }
    public uint ShadowVisibilityTypeOther_DX11 { get; set; }
    public uint ShadowVisibilityTypeEnemy_DX11 { get; set; }
    public uint PhysicsTypeSelf_DX11 { get; set; }
    public uint PhysicsTypeParty_DX11 { get; set; }
    public uint PhysicsTypeOther_DX11 { get; set; }
    public uint PhysicsTypeEnemy_DX11 { get; set; }
    public uint ReflectionType_DX11 { get; set; }
    //public uint WaterWet_DX11 { get; set; } // Hidden / Removed Setting?
    public uint ParallaxOcclusion_DX11 { get; set; }
    public uint Tessellation_DX11 { get; set; }
    public uint GlareRepresentation_DX11 { get; set; }
    public uint DynamicRezoThreshold { get; set; }
    public uint GraphicsRezoScale { get; set; }
    public uint GraphicsRezoUpscaleType { get; set; }
    public uint GrassEnableDynamicInterference { get; set; }
    public uint ShadowBgLOD { get; set; }
    public uint TextureRezoType { get; set; }
    public uint ShadowLightValidType { get; set; }
    public uint DynamicRezoType { get; set; }
    public uint UiAssetType { get; set; }
    public uint ScreenLeft { get; set; }
    public uint ScreenTop { get; set; }
    public uint ScreenWidth { get; set; }
    public uint ScreenHeight { get; set; }

    //Sound
    public uint SoundEnabled { get; set; }
}
// ReSharper restore InconsistentNaming

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
        public static void SetGfx(bool low)
        {
            if (low)
            {
                GetSettings(GameSettingsTables.Instance.CustomTable);
                SetMinimalGfx();
            }
            else
            {
                RestoreSettings(GameSettingsTables.Instance.CustomTable);
            }
        }

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
                varTable.FPSInActive            = configEntry[(int)ConfigOption.FPSInActive].Value.UInt;
                varTable.DisplayObjectLimitType = configEntry[(int)ConfigOption.DisplayObjectLimitType].Value.UInt;

                varTable.AntiAliasing_DX11              = configEntry[(int)ConfigOption.AntiAliasing_DX11].Value.UInt;
                //varTable.TextureFilterQuality_DX11      = configEntry[(int)ConfigOption.TextureFilterQuality_DX11].Value.UInt; // Hidden / Removed Setting?
                varTable.TextureAnisotropicQuality_DX11 = configEntry[(int)ConfigOption.TextureAnisotropicQuality_DX11].Value.UInt;
                varTable.SSAO_DX11                      = configEntry[(int)ConfigOption.SSAO_DX11].Value.UInt;
                varTable.Glare_DX11                     = configEntry[(int)ConfigOption.Glare_DX11].Value.UInt;
                varTable.DistortionWater_DX11           = configEntry[(int)ConfigOption.DistortionWater_DX11].Value.UInt;
                varTable.DepthOfField_DX11              = configEntry[(int)ConfigOption.DepthOfField_DX11].Value.UInt;
                varTable.RadialBlur_DX11                = configEntry[(int)ConfigOption.RadialBlur_DX11].Value.UInt;
                varTable.Vignetting_DX11                = configEntry[(int)ConfigOption.Vignetting_DX11].Value.UInt;
                varTable.GrassQuality_DX11              = configEntry[(int)ConfigOption.GrassQuality_DX11].Value.UInt;
                varTable.TranslucentQuality_DX11        = configEntry[(int)ConfigOption.TranslucentQuality_DX11].Value.UInt;
                varTable.ShadowSoftShadowType_DX11      = configEntry[(int)ConfigOption.ShadowSoftShadowType_DX11].Value.UInt;
                varTable.ShadowTextureSizeType_DX11     = configEntry[(int)ConfigOption.ShadowTextureSizeType_DX11].Value.UInt;
                varTable.ShadowCascadeCountType_DX11    = configEntry[(int)ConfigOption.ShadowCascadeCountType_DX11].Value.UInt;
                varTable.LodType_DX11                   = configEntry[(int)ConfigOption.LodType_DX11].Value.UInt;
                //varTable.OcclusionCulling_DX11          = configEntry[(int)ConfigOption.OcclusionCulling_DX11].Value.UInt; // Hidden / Removed Setting?
                varTable.ShadowLOD_DX11                 = configEntry[(int)ConfigOption.ShadowLOD_DX11].Value.UInt;
                //varTable.MapResolution_DX11             = configEntry[(int)ConfigOption.MapResolution_DX11].Value.UInt; // Hidden / Removed Setting?
                varTable.ShadowVisibilityTypeSelf_DX11  = configEntry[(int)ConfigOption.ShadowVisibilityTypeSelf_DX11].Value.UInt;
                varTable.ShadowVisibilityTypeParty_DX11 = configEntry[(int)ConfigOption.ShadowVisibilityTypeParty_DX11].Value.UInt;
                varTable.ShadowVisibilityTypeOther_DX11 = configEntry[(int)ConfigOption.ShadowVisibilityTypeOther_DX11].Value.UInt;
                varTable.ShadowVisibilityTypeEnemy_DX11 = configEntry[(int)ConfigOption.ShadowVisibilityTypeEnemy_DX11].Value.UInt;
                varTable.PhysicsTypeSelf_DX11           = configEntry[(int)ConfigOption.PhysicsTypeSelf_DX11].Value.UInt;
                varTable.PhysicsTypeParty_DX11          = configEntry[(int)ConfigOption.PhysicsTypeParty_DX11].Value.UInt;
                varTable.PhysicsTypeOther_DX11          = configEntry[(int)ConfigOption.PhysicsTypeOther_DX11].Value.UInt;
                varTable.PhysicsTypeEnemy_DX11          = configEntry[(int)ConfigOption.PhysicsTypeEnemy_DX11].Value.UInt;
                varTable.ReflectionType_DX11            = configEntry[(int)ConfigOption.ReflectionType_DX11].Value.UInt;
                //varTable.WaterWet_DX11                  = configEntry[(int)ConfigOption.WaterWet_DX11].Value.UInt; // Hidden / Removed Setting?
                varTable.ParallaxOcclusion_DX11         = configEntry[(int)ConfigOption.ParallaxOcclusion_DX11].Value.UInt;
                varTable.Tessellation_DX11              = configEntry[(int)ConfigOption.Tessellation_DX11].Value.UInt;
                varTable.GlareRepresentation_DX11       = configEntry[(int)ConfigOption.GlareRepresentation_DX11].Value.UInt;
                varTable.DynamicRezoThreshold           = configEntry[(int)ConfigOption.DynamicRezoThreshold].Value.UInt;
                varTable.GraphicsRezoScale              = configEntry[(int)ConfigOption.GraphicsRezoScale].Value.UInt;
                varTable.GraphicsRezoUpscaleType        = configEntry[(int)ConfigOption.GraphicsRezoUpscaleType].Value.UInt;
                varTable.GrassEnableDynamicInterference = configEntry[(int)ConfigOption.GrassEnableDynamicInterference].Value.UInt;
                varTable.ShadowBgLOD                    = configEntry[(int)ConfigOption.ShadowBgLOD].Value.UInt;
                varTable.TextureRezoType                = configEntry[(int)ConfigOption.TextureRezoType].Value.UInt;
                varTable.ShadowLightValidType           = configEntry[(int)ConfigOption.ShadowLightValidType].Value.UInt;
                varTable.DynamicRezoType                = configEntry[(int)ConfigOption.DynamicRezoType].Value.UInt;
                varTable.UiAssetType                    = configEntry[(int)ConfigOption.UiAssetType].Value.UInt;
                varTable.ScreenLeft                     = configEntry[(int)ConfigOption.ScreenLeft].Value.UInt;
                varTable.ScreenTop                      = configEntry[(int)ConfigOption.ScreenTop].Value.UInt;
                varTable.ScreenWidth                    = configEntry[(int)ConfigOption.ScreenWidth].Value.UInt;
                varTable.ScreenHeight                   = configEntry[(int)ConfigOption.ScreenHeight].Value.UInt;

                varTable.SoundEnabled                   = configEntry[(int)ConfigOption.IsSndMaster].Value.UInt;
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
                configEntry[(int)ConfigOption.FPSInActive].SetValueUInt(varTable.FPSInActive);
                configEntry[(int)ConfigOption.DisplayObjectLimitType].SetValueUInt(varTable.DisplayObjectLimitType);

                configEntry[(int)ConfigOption.AntiAliasing_DX11].SetValueUInt(varTable.AntiAliasing_DX11);
                //configEntry[(int)ConfigOption.TextureFilterQuality_DX11].SetValueUInt(varTable.TextureFilterQuality_DX11); // Hidden / Removed Setting?
                configEntry[(int)ConfigOption.TextureAnisotropicQuality_DX11].SetValueUInt(varTable.TextureAnisotropicQuality_DX11);
                configEntry[(int)ConfigOption.SSAO_DX11].SetValueUInt(varTable.SSAO_DX11);
                configEntry[(int)ConfigOption.Glare_DX11].SetValueUInt(varTable.Glare_DX11);
                configEntry[(int)ConfigOption.DistortionWater_DX11].SetValueUInt(varTable.DistortionWater_DX11);
                configEntry[(int)ConfigOption.DepthOfField_DX11].SetValueUInt(varTable.DepthOfField_DX11);
                configEntry[(int)ConfigOption.RadialBlur_DX11].SetValueUInt(varTable.RadialBlur_DX11);
                configEntry[(int)ConfigOption.Vignetting_DX11].SetValueUInt(varTable.Vignetting_DX11);
                configEntry[(int)ConfigOption.GrassQuality_DX11].SetValueUInt(varTable.GrassQuality_DX11);
                configEntry[(int)ConfigOption.TranslucentQuality_DX11].SetValueUInt(varTable.TranslucentQuality_DX11);
                configEntry[(int)ConfigOption.ShadowSoftShadowType_DX11].SetValueUInt(varTable.ShadowSoftShadowType_DX11);
                configEntry[(int)ConfigOption.ShadowTextureSizeType_DX11].SetValueUInt(varTable.ShadowTextureSizeType_DX11);
                configEntry[(int)ConfigOption.ShadowCascadeCountType_DX11].SetValueUInt(varTable.ShadowCascadeCountType_DX11);
                configEntry[(int)ConfigOption.LodType_DX11].SetValueUInt(varTable.LodType_DX11);
                //configEntry[(int)ConfigOption.OcclusionCulling_DX11].SetValueUInt(varTable.OcclusionCulling_DX11); // Hidden / Removed Setting?
                configEntry[(int)ConfigOption.ShadowLOD_DX11].SetValueUInt(varTable.ShadowLOD_DX11);
                //configEntry[(int)ConfigOption.MapResolution_DX11].SetValueUInt(varTable.MapResolution_DX11); // Hidden / Removed Setting?
                configEntry[(int)ConfigOption.ShadowVisibilityTypeSelf_DX11].SetValueUInt(varTable.ShadowVisibilityTypeSelf_DX11);
                configEntry[(int)ConfigOption.ShadowVisibilityTypeParty_DX11].SetValueUInt(varTable.ShadowVisibilityTypeParty_DX11);
                configEntry[(int)ConfigOption.ShadowVisibilityTypeOther_DX11].SetValueUInt(varTable.ShadowVisibilityTypeOther_DX11);
                configEntry[(int)ConfigOption.ShadowVisibilityTypeEnemy_DX11].SetValueUInt(varTable.ShadowVisibilityTypeEnemy_DX11);
                configEntry[(int)ConfigOption.PhysicsTypeSelf_DX11].SetValueUInt(varTable.PhysicsTypeSelf_DX11);
                configEntry[(int)ConfigOption.PhysicsTypeParty_DX11].SetValueUInt(varTable.PhysicsTypeParty_DX11);
                configEntry[(int)ConfigOption.PhysicsTypeOther_DX11].SetValueUInt(varTable.PhysicsTypeOther_DX11);
                configEntry[(int)ConfigOption.PhysicsTypeEnemy_DX11].SetValueUInt(varTable.PhysicsTypeEnemy_DX11);
                configEntry[(int)ConfigOption.ReflectionType_DX11].SetValueUInt(varTable.ReflectionType_DX11);
                //configEntry[(int)ConfigOption.WaterWet_DX11].SetValueUInt(varTable.WaterWet_DX11); // Hidden / Removed Setting?
                configEntry[(int)ConfigOption.ParallaxOcclusion_DX11].SetValueUInt(varTable.ParallaxOcclusion_DX11);
                configEntry[(int)ConfigOption.Tessellation_DX11].SetValueUInt(varTable.Tessellation_DX11);
                configEntry[(int)ConfigOption.GlareRepresentation_DX11].SetValueUInt(varTable.GlareRepresentation_DX11);
                configEntry[(int)ConfigOption.DynamicRezoThreshold].SetValueUInt(varTable.DynamicRezoThreshold);
                configEntry[(int)ConfigOption.GraphicsRezoScale].SetValueUInt(varTable.GraphicsRezoScale);
                configEntry[(int)ConfigOption.GraphicsRezoUpscaleType].SetValueUInt(varTable.GraphicsRezoUpscaleType);
                configEntry[(int)ConfigOption.GrassEnableDynamicInterference].SetValueUInt(varTable.GrassEnableDynamicInterference);
                configEntry[(int)ConfigOption.ShadowBgLOD].SetValueUInt(varTable.ShadowBgLOD);
                configEntry[(int)ConfigOption.TextureRezoType].SetValueUInt(varTable.TextureRezoType);
                configEntry[(int)ConfigOption.ShadowLightValidType].SetValueUInt(varTable.ShadowLightValidType);
                configEntry[(int)ConfigOption.DynamicRezoType].SetValueUInt(varTable.DynamicRezoType);
                configEntry[(int)ConfigOption.UiAssetType].SetValueUInt(varTable.UiAssetType);
                configEntry[(int)ConfigOption.ScreenLeft].SetValueUInt(varTable.ScreenLeft);
                configEntry[(int)ConfigOption.ScreenTop].SetValueUInt(varTable.ScreenTop);
                configEntry[(int)ConfigOption.ScreenWidth].SetValueUInt(varTable.ScreenWidth);
                configEntry[(int)ConfigOption.ScreenHeight].SetValueUInt(varTable.ScreenHeight);

                configEntry[(int)ConfigOption.IsSndMaster].SetValueUInt(varTable.SoundEnabled);

                Misc.SetGameRenderSize(varTable.ScreenWidth, varTable.ScreenHeight, varTable.ScreenLeft, varTable.ScreenTop);
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
            return varTable is { DisplayObjectLimitType: 4, ReflectionType_DX11: 0, GrassQuality_DX11: 0, SSAO_DX11: 0 };
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
            //configEntry[(int)ConfigOption.TextureFilterQuality_DX11].SetValueUInt(2); // Hidden / Removed Setting?
            configEntry[(int)ConfigOption.TextureAnisotropicQuality_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.SSAO_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.Glare_DX11].SetValueUInt(1);
            configEntry[(int)ConfigOption.DistortionWater_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.DepthOfField_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.RadialBlur_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.Vignetting_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.GrassQuality_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.TranslucentQuality_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.ShadowSoftShadowType_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.ShadowTextureSizeType_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.ShadowCascadeCountType_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.LodType_DX11].SetValueUInt(1);
            //configEntry[(int)ConfigOption.OcclusionCulling_DX11].SetValueUInt(1); // Hidden / Removed Setting?
            configEntry[(int)ConfigOption.ShadowLOD_DX11].SetValueUInt(1);
            //configEntry[(int)ConfigOption.MapResolution_DX11].SetValueUInt(1); // Hidden / Removed Setting?
            configEntry[(int)ConfigOption.ShadowVisibilityTypeSelf_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.ShadowVisibilityTypeParty_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.ShadowVisibilityTypeOther_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.ShadowVisibilityTypeEnemy_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.PhysicsTypeSelf_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.PhysicsTypeParty_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.PhysicsTypeOther_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.PhysicsTypeEnemy_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.ReflectionType_DX11].SetValueUInt(0);
            //configEntry[(int)ConfigOption.WaterWet_DX11].SetValueUInt(1); // Hidden / Removed Setting?
            configEntry[(int)ConfigOption.ParallaxOcclusion_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.Tessellation_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.GlareRepresentation_DX11].SetValueUInt(0);
            configEntry[(int)ConfigOption.DynamicRezoThreshold].SetValueUInt(1);
            configEntry[(int)ConfigOption.GraphicsRezoScale].SetValueUInt(50);
            configEntry[(int)ConfigOption.GraphicsRezoUpscaleType].SetValueUInt(0);
            configEntry[(int)ConfigOption.GrassEnableDynamicInterference].SetValueUInt(0);
            configEntry[(int)ConfigOption.ShadowBgLOD].SetValueUInt(1);
            configEntry[(int)ConfigOption.TextureRezoType].SetValueUInt(0);
            configEntry[(int)ConfigOption.ShadowLightValidType].SetValueUInt(0);
            configEntry[(int)ConfigOption.DynamicRezoType].SetValueUInt(0);
            configEntry[(int)ConfigOption.UiAssetType].SetValueUInt(0);
            configEntry[(int)ConfigOption.ScreenWidth].SetValueUInt(1024);
            configEntry[(int)ConfigOption.ScreenHeight].SetValueUInt(720);
            Misc.SetGameRenderSize(1024, 720, keepCurrentPosition: true);
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
            if (Api.ClientState is not { IsLoggedIn: true }) return "";
            if (Api.GetLocalPlayer() == null) return "";

            var player = Api.GetLocalPlayer();
            var world = player?.HomeWorld.ValueNullable;

            if (player == null || world == null) 
                return "";

            return $"{Api.PluginInterface?.GetPluginConfigDirectory()}\\{player.Name.TextValue}-({world.Value.Name.ToDalamudString().TextValue}).json";
        }

        public static void LoadConfig()
        {
            var file = GetCharConfigFilename();
            if (file == "")
                return;
            if (!File.Exists(file))
                return;

            GameSettingsTables.Instance.CustomTable = JsonConvert.DeserializeObject<GameSettingsVarTable>(File.ReadAllText(file));
            RestoreSettings(GameSettingsTables.Instance.CustomTable);
        }

        public static void SaveConfig()
        {
            var file = GetCharConfigFilename();
            if (file == "")
                return;

            //Save the config
            GetSettings(GameSettingsTables.Instance.CustomTable);
            var jsonString = JsonConvert.SerializeObject(GameSettingsTables.Instance.CustomTable);
            File.WriteAllText(file, JsonConvert.SerializeObject(GameSettingsTables.Instance.CustomTable));
        }
    }
}