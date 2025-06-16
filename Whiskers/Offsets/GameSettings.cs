/*
 * Copyright(c) 2025 Meowchestra, GiR-Zippo, Ori @MidiBard2
 * Licensed under the GPL v3 license. See https://github.com/Meowchestra/MeowMusic/blob/main/LICENSE for full license information.
 */

using Dalamud.Game.Config;
using Dalamud.Utility;
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
    public uint ShadowLOD_DX11 { get; set; }
    public uint ShadowVisibilityTypeSelf_DX11 { get; set; }
    public uint ShadowVisibilityTypeParty_DX11 { get; set; }
    public uint ShadowVisibilityTypeOther_DX11 { get; set; }
    public uint ShadowVisibilityTypeEnemy_DX11 { get; set; }
    public uint PhysicsTypeSelf_DX11 { get; set; }
    public uint PhysicsTypeParty_DX11 { get; set; }
    public uint PhysicsTypeOther_DX11 { get; set; }
    public uint PhysicsTypeEnemy_DX11 { get; set; }
    public uint ReflectionType_DX11 { get; set; }
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
        public static void GetSettings(GameSettingsVarTable? varTable)
        {
            if (varTable == null || Api.GameConfig == null) return;

            Api.GameConfig.TryGet(SystemConfigOption.Fps, out uint fps); varTable.Fps = fps;
            Api.GameConfig.TryGet(SystemConfigOption.FPSInActive, out uint fpsInActive); varTable.FPSInActive = fpsInActive;
            Api.GameConfig.TryGet(SystemConfigOption.DisplayObjectLimitType, out uint displayObjectLimitType); varTable.DisplayObjectLimitType = displayObjectLimitType;

            Api.GameConfig.TryGet(SystemConfigOption.AntiAliasing_DX11, out uint antiAliasingDx11); varTable.AntiAliasing_DX11 = antiAliasingDx11;
            Api.GameConfig.TryGet(SystemConfigOption.TextureAnisotropicQuality_DX11, out uint textureAnisotropicQualityDx11); varTable.TextureAnisotropicQuality_DX11 = textureAnisotropicQualityDx11;
            Api.GameConfig.TryGet(SystemConfigOption.SSAO_DX11, out uint ssaoDx11); varTable.SSAO_DX11 = ssaoDx11;
            Api.GameConfig.TryGet(SystemConfigOption.Glare_DX11, out uint glareDx11); varTable.Glare_DX11 = glareDx11;
            Api.GameConfig.TryGet(SystemConfigOption.DistortionWater_DX11, out uint distortionWaterDx11); varTable.DistortionWater_DX11 = distortionWaterDx11;
            Api.GameConfig.TryGet(SystemConfigOption.DepthOfField_DX11, out uint depthOfFieldDx11); varTable.DepthOfField_DX11 = depthOfFieldDx11;
            Api.GameConfig.TryGet(SystemConfigOption.RadialBlur_DX11, out uint radialBlurDx11); varTable.RadialBlur_DX11 = radialBlurDx11;
            Api.GameConfig.TryGet(SystemConfigOption.Vignetting_DX11, out uint vignettingDx11); varTable.Vignetting_DX11 = vignettingDx11;
            Api.GameConfig.TryGet(SystemConfigOption.GrassQuality_DX11, out uint grassQualityDx11); varTable.GrassQuality_DX11 = grassQualityDx11;
            Api.GameConfig.TryGet(SystemConfigOption.TranslucentQuality_DX11, out uint translucentQualityDx11); varTable.TranslucentQuality_DX11 = translucentQualityDx11;
            Api.GameConfig.TryGet(SystemConfigOption.ShadowSoftShadowType_DX11, out uint shadowSoftShadowTypeDx11); varTable.ShadowSoftShadowType_DX11 = shadowSoftShadowTypeDx11;
            Api.GameConfig.TryGet(SystemConfigOption.ShadowTextureSizeType_DX11, out uint shadowTextureSizeTypeDx11); varTable.ShadowTextureSizeType_DX11 = shadowTextureSizeTypeDx11;
            Api.GameConfig.TryGet(SystemConfigOption.ShadowCascadeCountType_DX11, out uint shadowCascadeCountTypeDx11); varTable.ShadowCascadeCountType_DX11 = shadowCascadeCountTypeDx11;
            Api.GameConfig.TryGet(SystemConfigOption.LodType_DX11, out uint lodTypeDx11); varTable.LodType_DX11 = lodTypeDx11;
            Api.GameConfig.TryGet(SystemConfigOption.ShadowLOD_DX11, out uint shadowLodDx11); varTable.ShadowLOD_DX11 = shadowLodDx11;
            Api.GameConfig.TryGet(SystemConfigOption.ShadowVisibilityTypeSelf_DX11, out uint shadowVisibilityTypeSelfDx11); varTable.ShadowVisibilityTypeSelf_DX11 = shadowVisibilityTypeSelfDx11;
            Api.GameConfig.TryGet(SystemConfigOption.ShadowVisibilityTypeParty_DX11, out uint shadowVisibilityTypePartyDx11); varTable.ShadowVisibilityTypeParty_DX11 = shadowVisibilityTypePartyDx11;
            Api.GameConfig.TryGet(SystemConfigOption.ShadowVisibilityTypeOther_DX11, out uint shadowVisibilityTypeOtherDx11); varTable.ShadowVisibilityTypeOther_DX11 = shadowVisibilityTypeOtherDx11;
            Api.GameConfig.TryGet(SystemConfigOption.ShadowVisibilityTypeEnemy_DX11, out uint shadowVisibilityTypeEnemyDx11); varTable.ShadowVisibilityTypeEnemy_DX11 = shadowVisibilityTypeEnemyDx11;
            Api.GameConfig.TryGet(SystemConfigOption.PhysicsTypeSelf_DX11, out uint physicsTypeSelfDx11); varTable.PhysicsTypeSelf_DX11 = physicsTypeSelfDx11;
            Api.GameConfig.TryGet(SystemConfigOption.PhysicsTypeParty_DX11, out uint physicsTypePartyDx11); varTable.PhysicsTypeParty_DX11 = physicsTypePartyDx11;
            Api.GameConfig.TryGet(SystemConfigOption.PhysicsTypeOther_DX11, out uint physicsTypeOtherDx11); varTable.PhysicsTypeOther_DX11 = physicsTypeOtherDx11;
            Api.GameConfig.TryGet(SystemConfigOption.PhysicsTypeEnemy_DX11, out uint physicsTypeEnemyDx11); varTable.PhysicsTypeEnemy_DX11 = physicsTypeEnemyDx11;
            Api.GameConfig.TryGet(SystemConfigOption.ReflectionType_DX11, out uint reflectionTypeDx11); varTable.ReflectionType_DX11 = reflectionTypeDx11;
            Api.GameConfig.TryGet(SystemConfigOption.ParallaxOcclusion_DX11, out uint parallaxOcclusionDx11); varTable.ParallaxOcclusion_DX11 = parallaxOcclusionDx11;
            Api.GameConfig.TryGet(SystemConfigOption.Tessellation_DX11, out uint tessellationDx11); varTable.Tessellation_DX11 = tessellationDx11;
            Api.GameConfig.TryGet(SystemConfigOption.GlareRepresentation_DX11, out uint glareRepresentationDx11); varTable.GlareRepresentation_DX11 = glareRepresentationDx11;
            Api.GameConfig.TryGet(SystemConfigOption.DynamicRezoThreshold, out uint dynamicRezoThreshold); varTable.DynamicRezoThreshold = dynamicRezoThreshold;
            Api.GameConfig.TryGet(SystemConfigOption.GraphicsRezoScale, out uint graphicsRezoScale); varTable.GraphicsRezoScale = graphicsRezoScale;
            Api.GameConfig.TryGet(SystemConfigOption.GraphicsRezoUpscaleType, out uint graphicsRezoUpscaleType); varTable.GraphicsRezoUpscaleType = graphicsRezoUpscaleType;
            Api.GameConfig.TryGet(SystemConfigOption.GrassEnableDynamicInterference, out uint grassEnableDynamicInterference); varTable.GrassEnableDynamicInterference = grassEnableDynamicInterference;
            Api.GameConfig.TryGet(SystemConfigOption.ShadowBgLOD, out uint shadowBgLod); varTable.ShadowBgLOD = shadowBgLod;
            Api.GameConfig.TryGet(SystemConfigOption.TextureRezoType, out uint textureRezoType); varTable.TextureRezoType = textureRezoType;
            Api.GameConfig.TryGet(SystemConfigOption.ShadowLightValidType, out uint shadowLightValidType); varTable.ShadowLightValidType = shadowLightValidType;
            Api.GameConfig.TryGet(SystemConfigOption.DynamicRezoType, out uint dynamicRezoType); varTable.DynamicRezoType = dynamicRezoType;
            Api.GameConfig.TryGet(SystemConfigOption.UiAssetType, out uint uiAssetType); varTable.UiAssetType = uiAssetType;
            Api.GameConfig.TryGet(SystemConfigOption.ScreenLeft, out uint screenLeft); varTable.ScreenLeft = screenLeft;
            Api.GameConfig.TryGet(SystemConfigOption.ScreenTop, out uint screenTop); varTable.ScreenTop = screenTop;
            Api.GameConfig.TryGet(SystemConfigOption.ScreenWidth, out uint screenWidth); varTable.ScreenWidth = screenWidth;
            Api.GameConfig.TryGet(SystemConfigOption.ScreenHeight, out uint screenHeight); varTable.ScreenHeight = screenHeight;

            Api.GameConfig.TryGet(SystemConfigOption.IsSndMaster, out uint soundEnabled); varTable.SoundEnabled = soundEnabled;
        }

        /// <summary>
        /// Restore the GFX settings
        /// </summary>
        public static void RestoreSettings(GameSettingsVarTable? varTable)
        {
            if (varTable == null || Api.GameConfig == null) return;

            Api.GameConfig.Set(SystemConfigOption.Fps, varTable.Fps);
            Api.GameConfig.Set(SystemConfigOption.FPSInActive, varTable.FPSInActive);
            Api.GameConfig.Set(SystemConfigOption.DisplayObjectLimitType, varTable.DisplayObjectLimitType);

            Api.GameConfig.Set(SystemConfigOption.AntiAliasing_DX11, varTable.AntiAliasing_DX11);
            Api.GameConfig.Set(SystemConfigOption.TextureAnisotropicQuality_DX11, varTable.TextureAnisotropicQuality_DX11);
            Api.GameConfig.Set(SystemConfigOption.SSAO_DX11, varTable.SSAO_DX11);
            Api.GameConfig.Set(SystemConfigOption.Glare_DX11, varTable.Glare_DX11);
            Api.GameConfig.Set(SystemConfigOption.DistortionWater_DX11, varTable.DistortionWater_DX11);
            Api.GameConfig.Set(SystemConfigOption.DepthOfField_DX11, varTable.DepthOfField_DX11);
            Api.GameConfig.Set(SystemConfigOption.RadialBlur_DX11, varTable.RadialBlur_DX11);
            Api.GameConfig.Set(SystemConfigOption.Vignetting_DX11, varTable.Vignetting_DX11);
            Api.GameConfig.Set(SystemConfigOption.GrassQuality_DX11, varTable.GrassQuality_DX11);
            Api.GameConfig.Set(SystemConfigOption.TranslucentQuality_DX11, varTable.TranslucentQuality_DX11);
            Api.GameConfig.Set(SystemConfigOption.ShadowSoftShadowType_DX11, varTable.ShadowSoftShadowType_DX11);
            Api.GameConfig.Set(SystemConfigOption.ShadowTextureSizeType_DX11, varTable.ShadowTextureSizeType_DX11);
            Api.GameConfig.Set(SystemConfigOption.ShadowCascadeCountType_DX11, varTable.ShadowCascadeCountType_DX11);
            Api.GameConfig.Set(SystemConfigOption.LodType_DX11, varTable.LodType_DX11);
            Api.GameConfig.Set(SystemConfigOption.ShadowLOD_DX11, varTable.ShadowLOD_DX11);
            Api.GameConfig.Set(SystemConfigOption.ShadowVisibilityTypeSelf_DX11, varTable.ShadowVisibilityTypeSelf_DX11);
            Api.GameConfig.Set(SystemConfigOption.ShadowVisibilityTypeParty_DX11, varTable.ShadowVisibilityTypeParty_DX11);
            Api.GameConfig.Set(SystemConfigOption.ShadowVisibilityTypeOther_DX11, varTable.ShadowVisibilityTypeOther_DX11);
            Api.GameConfig.Set(SystemConfigOption.ShadowVisibilityTypeEnemy_DX11, varTable.ShadowVisibilityTypeEnemy_DX11);
            Api.GameConfig.Set(SystemConfigOption.PhysicsTypeSelf_DX11, varTable.PhysicsTypeSelf_DX11);
            Api.GameConfig.Set(SystemConfigOption.PhysicsTypeParty_DX11, varTable.PhysicsTypeParty_DX11);
            Api.GameConfig.Set(SystemConfigOption.PhysicsTypeOther_DX11, varTable.PhysicsTypeOther_DX11);
            Api.GameConfig.Set(SystemConfigOption.PhysicsTypeEnemy_DX11, varTable.PhysicsTypeEnemy_DX11);
            Api.GameConfig.Set(SystemConfigOption.ReflectionType_DX11, varTable.ReflectionType_DX11);
            Api.GameConfig.Set(SystemConfigOption.ParallaxOcclusion_DX11, varTable.ParallaxOcclusion_DX11);
            Api.GameConfig.Set(SystemConfigOption.Tessellation_DX11, varTable.Tessellation_DX11);
            Api.GameConfig.Set(SystemConfigOption.GlareRepresentation_DX11, varTable.GlareRepresentation_DX11);
            Api.GameConfig.Set(SystemConfigOption.DynamicRezoThreshold, varTable.DynamicRezoThreshold);
            Api.GameConfig.Set(SystemConfigOption.GraphicsRezoScale, varTable.GraphicsRezoScale);
            Api.GameConfig.Set(SystemConfigOption.GraphicsRezoUpscaleType, varTable.GraphicsRezoUpscaleType);
            Api.GameConfig.Set(SystemConfigOption.GrassEnableDynamicInterference, varTable.GrassEnableDynamicInterference);
            Api.GameConfig.Set(SystemConfigOption.ShadowBgLOD, varTable.ShadowBgLOD);
            Api.GameConfig.Set(SystemConfigOption.TextureRezoType, varTable.TextureRezoType);
            Api.GameConfig.Set(SystemConfigOption.ShadowLightValidType, varTable.ShadowLightValidType);
            Api.GameConfig.Set(SystemConfigOption.DynamicRezoType, varTable.DynamicRezoType);
            Api.GameConfig.Set(SystemConfigOption.UiAssetType, varTable.UiAssetType);
            Api.GameConfig.Set(SystemConfigOption.ScreenLeft, varTable.ScreenLeft);
            Api.GameConfig.Set(SystemConfigOption.ScreenTop, varTable.ScreenTop);
            Api.GameConfig.Set(SystemConfigOption.ScreenWidth, varTable.ScreenWidth);
            Api.GameConfig.Set(SystemConfigOption.ScreenHeight, varTable.ScreenHeight);

            Api.GameConfig.Set(SystemConfigOption.IsSndMaster, varTable.SoundEnabled);

            Misc.SetGameRenderSize(varTable.ScreenWidth, varTable.ScreenHeight, varTable.ScreenLeft, varTable.ScreenTop);
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
        public static void SetMinimalGfx()
        {
            if (Api.GameConfig == null) return;
            
            Api.GameConfig.Set(SystemConfigOption.Fps, 2u);
            Api.GameConfig.Set(SystemConfigOption.FPSInActive, 0u);
            Api.GameConfig.Set(SystemConfigOption.DisplayObjectLimitType, 4u);

            Api.GameConfig.Set(SystemConfigOption.AntiAliasing_DX11, 0u);
            Api.GameConfig.Set(SystemConfigOption.TextureAnisotropicQuality_DX11, 0u);
            Api.GameConfig.Set(SystemConfigOption.SSAO_DX11, 0u);
            Api.GameConfig.Set(SystemConfigOption.Glare_DX11, 1u);
            Api.GameConfig.Set(SystemConfigOption.DistortionWater_DX11, 0u);
            Api.GameConfig.Set(SystemConfigOption.DepthOfField_DX11, 0u);
            Api.GameConfig.Set(SystemConfigOption.RadialBlur_DX11, 0u);
            Api.GameConfig.Set(SystemConfigOption.Vignetting_DX11, 0u);
            Api.GameConfig.Set(SystemConfigOption.GrassQuality_DX11, 0u);
            Api.GameConfig.Set(SystemConfigOption.TranslucentQuality_DX11, 0u);
            Api.GameConfig.Set(SystemConfigOption.ShadowSoftShadowType_DX11, 0u);
            Api.GameConfig.Set(SystemConfigOption.ShadowTextureSizeType_DX11, 0u);
            Api.GameConfig.Set(SystemConfigOption.ShadowCascadeCountType_DX11, 0u);
            Api.GameConfig.Set(SystemConfigOption.LodType_DX11, 1u);
            Api.GameConfig.Set(SystemConfigOption.ShadowLOD_DX11, 1u);
            Api.GameConfig.Set(SystemConfigOption.ShadowVisibilityTypeSelf_DX11, 0u);
            Api.GameConfig.Set(SystemConfigOption.ShadowVisibilityTypeParty_DX11, 0u);
            Api.GameConfig.Set(SystemConfigOption.ShadowVisibilityTypeOther_DX11, 0u);
            Api.GameConfig.Set(SystemConfigOption.ShadowVisibilityTypeEnemy_DX11, 0u);
            Api.GameConfig.Set(SystemConfigOption.PhysicsTypeSelf_DX11, 0u);
            Api.GameConfig.Set(SystemConfigOption.PhysicsTypeParty_DX11, 0u);
            Api.GameConfig.Set(SystemConfigOption.PhysicsTypeOther_DX11, 0u);
            Api.GameConfig.Set(SystemConfigOption.PhysicsTypeEnemy_DX11, 0u);
            Api.GameConfig.Set(SystemConfigOption.ReflectionType_DX11, 0u);
            Api.GameConfig.Set(SystemConfigOption.ParallaxOcclusion_DX11, 0u);
            Api.GameConfig.Set(SystemConfigOption.Tessellation_DX11, 0u);
            Api.GameConfig.Set(SystemConfigOption.GlareRepresentation_DX11, 0u);
            Api.GameConfig.Set(SystemConfigOption.DynamicRezoThreshold, 1u);
            Api.GameConfig.Set(SystemConfigOption.GraphicsRezoScale, 50u);
            Api.GameConfig.Set(SystemConfigOption.GraphicsRezoUpscaleType, 0u);
            Api.GameConfig.Set(SystemConfigOption.GrassEnableDynamicInterference, 0u);
            Api.GameConfig.Set(SystemConfigOption.ShadowBgLOD, 1u);
            Api.GameConfig.Set(SystemConfigOption.TextureRezoType, 0u);
            Api.GameConfig.Set(SystemConfigOption.ShadowLightValidType, 0u);
            Api.GameConfig.Set(SystemConfigOption.DynamicRezoType, 0u);
            Api.GameConfig.Set(SystemConfigOption.UiAssetType, 0u);
            Api.GameConfig.Set(SystemConfigOption.ScreenWidth, 1024u);
            Api.GameConfig.Set(SystemConfigOption.ScreenHeight, 720u);

            //Misc.SetGameRenderSize(1024, 720); // Causes crash, disabled for now?
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
            if (string.IsNullOrEmpty(file) || !File.Exists(file))
                return;

            GameSettingsTables.Instance.CustomTable = JsonConvert.DeserializeObject<GameSettingsVarTable>(File.ReadAllText(file));
            RestoreSettings(GameSettingsTables.Instance.CustomTable);
        }

        public static void SaveConfig()
        {
            var file = GetCharConfigFilename();
            if (string.IsNullOrEmpty(file))
                return;

            //Save the config
            GetSettings(GameSettingsTables.Instance.CustomTable);
            File.WriteAllText(file, JsonConvert.SerializeObject(GameSettingsTables.Instance.CustomTable, Formatting.Indented));
        }
    }
}