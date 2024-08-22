/*
 * Copyright(c) 2024 Meowchestra, GiR-Zippo
 * Licensed under the GPL v3 license. See https://github.com/Meowchestra/MeowMusic/blob/main/LICENSE for full license information.
 */

using Dalamud.Game;
using Dalamud.Game.ClientState.Objects;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace Whiskers.Offsets;

public class Api
{
    [PluginService]
    public static IDalamudPluginInterface? PluginInterface { get; private set; }

    [PluginService] 
    public static IAddonLifecycle? AddonLifecycle { get; private set; }

    [PluginService]
    public static IBuddyList? BuddyList { get; private set; }

    [PluginService]
    public static IChatGui? ChatGui { get; private set; }

    [PluginService]
    public static IClientState? ClientState { get; private set; }

    [PluginService]
    public static ICommandManager? CommandManager { get; private set; }

    [PluginService]
    public static ICondition? Condition { get; private set; }

    [PluginService] 
    public static IGameInteropProvider? GameInteropProvider { get; private set; }

    [PluginService]
    public static IDataManager? DataManager { get; private set; }

    [PluginService]
    public static IFateTable? FateTable { get; private set; }

    [PluginService]
    public static IFlyTextGui? FlyTextGui { get; private set; }

    [PluginService]
    public static IFramework? Framework { get; private set; }

    [PluginService]
    public static IGameGui? GameGui { get; private set; }

    [PluginService]
    public static IGameNetwork? GameNetwork { get; private set; }

    [PluginService]
    public static IJobGauges? JobGauges { get; private set; }

    [PluginService]
    public static IKeyState? KeyState { get; private set; }

    [PluginService]
    public static IPartyFinderGui? PartyFinderGui { get; private set; }

    [PluginService]
    public static IPartyList? PartyList { get; private set; }

    [PluginService]
    public static ISigScanner? SigScanner { get; private set; }

    [PluginService]
    public static ITargetManager? TargetManager { get; private set; }

    [PluginService]
    public static IToastGui? ToastGui { get; private set; }

    [PluginService]
    public static IPluginLog? PluginLog { get; private set; }
    
    [PluginService] 
    public static IGameConfig? GameConfig { get; private set; }

    [PluginService]
    public static IGameLifecycle? GameLifecycle { get; private set; }

    [PluginService]
    public static IObjectTable? Objects { get; private set; }

    [PluginService]
    public static ITargetManager? Targets { get; private set; }
}
