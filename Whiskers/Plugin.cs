/*
 * Copyright(c) 2024 Meowchestra, GiR-Zippo
 * Licensed under the GPL v3 license. See https://github.com/Meowchestra/MeowMusic/blob/main/LICENSE for full license information.
 */

using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Whiskers.Offsets;
using Whiskers.Windows;
using static Whiskers.Offsets.GameSettings;

namespace Whiskers;

public class Whiskers : IDalamudPlugin
{
    public static string Name => "Whiskers";

    private readonly WindowSystem _windowSystem = new("Whiskers");
    private MainWindow PluginUi { get; init; }

    private const string CommandName = "/purr";

    private static IDalamudPluginInterface? PluginInterface { get; set; }

    private Configuration Configuration { get; }
    internal static AgentPerformance? AgentPerformance { get; private set; }
    internal static EnsembleManager? EnsembleManager { get; set; }

    public Api? Api { get; set; }

    public Whiskers(IDalamudPluginInterface? pluginInterface, IDataManager? data, ICommandManager commandManager, IClientState? clientState, IPartyList? partyList)
    {
        Api             = pluginInterface?.Create<Api>();
        PluginInterface = pluginInterface;

        Configuration = PluginInterface?.GetPluginConfig() as Configuration ?? new Configuration();
        Configuration.Initialize(PluginInterface);
        OffsetManager.Setup(Api.SigScanner);

        Api.CommandManager?.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Open the Whiskers settings menu."
        });

        AgentPerformance = new AgentPerformance(AgentId.PerformanceMode);
        EnsembleManager  = new EnsembleManager();

        Collector.Instance.Initialize(data, clientState, partyList);

        AgentConfigSystem.GetSettings(GameSettingsTables.Instance.StartupTable);
        AgentConfigSystem.GetSettings(GameSettingsTables.Instance.CustomTable);

        //NetworkReader.Initialize();

        PluginUi = new MainWindow(this, Configuration);
        _windowSystem.AddWindow(PluginUi);

        if (PluginInterface != null)
        {
            PluginInterface.UiBuilder.Draw         += DrawUi;
            PluginInterface.UiBuilder.OpenConfigUi += OpenMainUi;
        }

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
        AgentConfigSystem.RestoreSettings(GameSettingsTables.Instance.StartupTable);
    }

    public void Dispose()
    {
        if (Api.ClientState != null)
        {
            Api.ClientState.Login  -= OnLogin;
            Api.ClientState.Logout -= OnLogout;
        }

        //NetworkReader.Dispose();
        AgentConfigSystem.RestoreSettings(GameSettingsTables.Instance.StartupTable);
        EnsembleManager?.Dispose();
        Collector.Instance.Dispose();

        _windowSystem.RemoveAllWindows();
        PluginUi.Dispose();

        Api.CommandManager?.RemoveHandler(CommandName);
    }

    private void DrawUi()
    {
        _windowSystem.Draw();
        PluginUi.Update(); //update the main window... for the msg queue
    }

    private void OpenMainUi()
    {
        PluginUi.IsOpen = !PluginUi.IsOpen;
    }

    private void OnCommand(string command, string args)
    {
        PluginUi.IsOpen = !PluginUi.IsOpen;
    }
}