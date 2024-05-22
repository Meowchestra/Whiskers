/*
 * Copyright(c) 2024 Meowchestra, GiR-Zippo
 * Licensed under the GPL v3 license. See https://github.com/Meowchestra/MeowMusic/blob/main/LICENSE for full license information.
 */

using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Whiskers.Offsets;
using Whiskers.Windows;
using static Whiskers.Offsets.GameSettings;

namespace Whiskers;

public class Whiskers : IDalamudPlugin
{
    public static string Name => "Whiskers";

    //The windows
    public WindowSystem WindowSystem = new("Whiskers");
    private MainWindow PluginUi { get; init; }
    private ConfigWindow ConfigUi { get; set; }

    private const string CommandName = "/purr";

    private DalamudPluginInterface PluginInterface { get; }
    private Configuration Configuration { get; }
    internal static AgentConfigSystem? AgentConfigSystem { get; private set; }
    internal static AgentPerformance? AgentPerformance { get; private set; }
    internal static EnsembleManager? EnsembleManager { get; set; }

    public Api? Api { get; set; }

    public Whiskers(DalamudPluginInterface pluginInterface, IDataManager? data, ICommandManager commandManager, IClientState? clientState, IPartyList? partyList)
    {
        Api             = pluginInterface.Create<Api>();
        PluginInterface = pluginInterface;

        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Configuration.Initialize(PluginInterface);
        OffsetManager.Setup(Api.SigScanner);

        Api.CommandManager?.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Open the Whiskers settings menu."
        });

        AgentConfigSystem = new AgentConfigSystem(AgentManager.Instance.FindAgentInterfaceByVtable(Offsets.Offsets.AgentConfigSystem));
        AgentPerformance  = new AgentPerformance(AgentManager.Instance.FindAgentInterfaceByVtable(Offsets.Offsets.AgentPerformance));
        EnsembleManager   = new EnsembleManager();

        Collector.Instance.Initialize(data, clientState, partyList);

        AgentConfigSystem.GetSettings(GameSettingsTables.Instance?.StartupTable);
        AgentConfigSystem.GetSettings(GameSettingsTables.Instance?.CustomTable);

        //NetworkReader.Initialize();

        // you might normally want to embed resources and load them from the manifest stream
        PluginUi = new MainWindow(this, Configuration);
        ConfigUi = new ConfigWindow(this);

        WindowSystem.AddWindow(PluginUi);
        WindowSystem.AddWindow(ConfigUi);

        PluginInterface.UiBuilder.Draw         += DrawUi;
        PluginInterface.UiBuilder.OpenConfigUi += UiBuilder_DrawConfigUI;
        PluginInterface.UiBuilder.OpenMainUi   += UiBuilder_OpenMainUi;

        AgentConfigSystem.LoadConfig();
        if (Api.ClientState != null)
        {
            Api.ClientState.Login  += OnLogin;
            Api.ClientState.Logout += OnLogout;
        }
    }

    private static void OnLogin()
    {
        AgentConfigSystem.LoadConfig();
    }

    private static void OnLogout()
    {
        AgentConfigSystem.RestoreSettings(GameSettingsTables.Instance?.StartupTable);
        AgentConfigSystem?.ApplyGraphicSettings();
    }

    public void Dispose()
    {
        if (Api.ClientState != null)
        {
            Api.ClientState.Login  -= OnLogin;
            Api.ClientState.Logout -= OnLogout;
        }

        //NetworkReader.Dispose();
        AgentConfigSystem.RestoreSettings(GameSettingsTables.Instance?.StartupTable);
        AgentConfigSystem?.ApplyGraphicSettings();
        EnsembleManager?.Dispose();
        Collector.Instance.Dispose();

        WindowSystem.RemoveAllWindows();
        PluginUi.Dispose();
        ConfigUi.Dispose();

        Api.CommandManager?.RemoveHandler(CommandName);
    }

    private void OnCommand(string command, string args)
    {
        // in response to the slash command, just display our main ui
        PluginUi.IsOpen = !PluginUi.IsOpen;
    }

    private void DrawUi()
    {
        WindowSystem.Draw();
        PluginUi.Update(); //update the main window... for the msg queue
    }

    private void UiBuilder_OpenMainUi()
    {
        PluginUi.IsOpen = !PluginUi.IsOpen;
    }

    private void UiBuilder_DrawConfigUI()
    {
        ConfigUi.IsOpen = !ConfigUi.IsOpen;
    }
}