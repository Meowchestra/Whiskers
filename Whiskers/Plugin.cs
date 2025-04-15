/*
 * Copyright(c) 2025 Meowchestra, GiR-Zippo
 * Licensed under the GPL v3 license. See https://github.com/Meowchestra/MeowMusic/blob/main/LICENSE for full license information.
 */

using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Whiskers.Config;
using Whiskers.GameFunctions;
using Whiskers.IPC;
using Whiskers.Offsets;
using Whiskers.Windows;
using static Whiskers.Offsets.GameSettings;

namespace Whiskers;

public class Whiskers : IDalamudPlugin
{
    public static string Name => "Whiskers";

    private WindowSystem WindowSystem { get; init; } = new("Whiskers");
    private MainWindow PluginUi { get; init; }

    private const string CommandName = "/purr";
    private static IDalamudPluginInterface? PluginInterface { get; set; }

    private Configuration Configuration { get; }
    internal static AgentPerformance? AgentPerformance { get; private set; }
    internal static EnsembleManager? EnsembleManager { get; set; }

    private readonly IpcProvider _ipc;

    public Whiskers(IDalamudPluginInterface? pluginInterface, IDataManager? data, ICommandManager commandManager, IClientState? clientState, IPartyList? partyList)
    {
        Api.Safe(() => Api.Init(pluginInterface));
        OffsetManager.Setup(Api.SigScanner);

        PluginInterface = pluginInterface;
        Configuration   = PluginInterface?.GetPluginConfig() as Configuration ?? new Configuration();
        Configuration.Initialize(PluginInterface);

        Api.CommandManager?.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Open the Whiskers settings menu."
        });

        AgentPerformance = new AgentPerformance(AgentId.PerformanceMode);
        EnsembleManager  = new EnsembleManager();
        Party.Instance.Initialize();

        Collector.Instance.Initialize(data, clientState, partyList);

        AgentConfigSystem.GetSettings(GameSettingsTables.Instance.StartupTable);
        AgentConfigSystem.GetSettings(GameSettingsTables.Instance.CustomTable);

        PluginUi = new MainWindow(this, Configuration);
        WindowSystem.AddWindow(PluginUi);

        if (PluginInterface != null)
        {
            PluginInterface.UiBuilder.Draw         += DrawUi;
            //PluginInterface.UiBuilder.OpenMainUi   += OpenMainUi;
            PluginInterface.UiBuilder.OpenConfigUi += OpenMainUi;
        }

        AgentConfigSystem.LoadConfig();
        if (Api.ClientState != null)
        {
            Api.ClientState.Login  += OnLogin;
            Api.ClientState.Logout += OnLogout;
        }

        _ipc = new IpcProvider(this);
    }

    private static void OnLogin()
    {
        AgentConfigSystem.LoadConfig();
    }
    
    private static void OnLogout(int type, int code)
    {
        AgentConfigSystem.RestoreSettings(GameSettingsTables.Instance.StartupTable);
    }

    public void Dispose()
    {
        _ipc.Dispose();
        MovementFactory.Instance.Dispose();
        Party.Instance.Dispose();
        if (Api.ClientState != null)
        {
            Api.ClientState.Login  -= OnLogin;
            Api.ClientState.Logout -= OnLogout;
        }

        AgentConfigSystem.RestoreSettings(GameSettingsTables.Instance.StartupTable);

        // Force dispose the EnsembleManager even if null
        if (EnsembleManager != null)
        {
            EnsembleManager.Dispose();
            EnsembleManager = null;
        }

        Collector.Instance.Dispose();

        WindowSystem.RemoveAllWindows();
        PluginUi.Dispose();

        Api.CommandManager?.RemoveHandler(CommandName);
    }

    private void DrawUi()
    {
        WindowSystem.Draw();
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