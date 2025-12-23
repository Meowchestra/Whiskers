/*
 * Copyright(c) 2025 Meowchestra, GiR-Zippo
 * Licensed under the GPL v3 license. See https://github.com/Meowchestra/MeowMusic/blob/main/LICENSE for full license information.
 */

using System.Runtime.CompilerServices;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace Whiskers.Offsets;

public class Api
{
    [PluginService] public static IAddonLifecycle? AddonLifecycle { get; private set; }
    [PluginService] public static IClientState? ClientState { get; private set; }
    [PluginService] public static ICommandManager? CommandManager { get; private set; }
    [PluginService] public static ICondition? Condition { get; private set; }
    [PluginService] public static IDalamudPluginInterface? PluginInterface { get; private set; }
    [PluginService] public static IFramework? Framework { get; private set; }
    [PluginService] public static IGameConfig? GameConfig { get; private set; }
    [PluginService] public static IGameInteropProvider? GameInteropProvider { get; private set; }
    [PluginService] public static IObjectTable? Objects { get; private set; }
    [PluginService] public static IPlayerState? PlayerState { get; private set; }
    [PluginService] public static IPluginLog? PluginLog { get; private set; }
    [PluginService] public static ISigScanner? SigScanner { get; private set; }

    internal static bool IsInitialized;

    /// <summary>
    /// Get the local player - has to run in tick
    /// </summary>
    /// <returns></returns>
    public static IPlayerCharacter? GetLocalPlayer()
    {
        return Framework?.RunOnTick(() => Objects?.LocalPlayer).Result;
    }

    public static void Init(IDalamudPluginInterface? pi)
    {
        if (IsInitialized)
        {
            PluginLog?.Debug("Services already initialized, skipping");
        }
        IsInitialized = true;
        try
        {
            pi?.Create<Api>();
        }
        catch
        {
            PluginLog?.Error("Services already initialized, skipping");
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Safe(Action a, bool suppressErrors = false)
    {
        try
        {
            a();
        }
        catch (Exception e)
        {
            if (!suppressErrors) PluginLog?.Error($"{e.Message}\n{e.StackTrace ?? ""}");
        }
    }
}