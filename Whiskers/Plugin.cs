/*
 * Copyright(c) 2023 GiR-Zippo, Meowchestra
 * Licensed under the GPL v3 license. See https://github.com/GiR-Zippo/LightAmp/blob/main/LICENSE for full license information.
 */

using Dalamud.Game;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Whiskers.Offsets;
using static Whiskers.Offsets.GameSettings;

namespace Whiskers;

public class Whiskers : IDalamudPlugin
{
    //public static XivCommonBase CBase;
    public string Name => "Whiskers";

    private const string CommandName = "/purr";

    private DalamudPluginInterface PluginInterface { get; }
    private ICommandManager CommandManager { get; }
    private Configuration Configuration { get; }
    private PluginUi PluginUi { get; }
    internal static AgentConfigSystem? AgentConfigSystem { get; private set; }
    internal static AgentPerformance? AgentPerformance { get; private set; }
    internal static EnsembleManager? EnsembleManager { get; set; }

    [PluginService] 
    private static ISigScanner? SigScanner { get; set; }

    public Whiskers(DalamudPluginInterface pluginInterface, IDataManager? data, ICommandManager commandManager, IClientState? clientState, IPartyList? partyList)
    {
        Api.Initialize(this, pluginInterface);
        PluginInterface = pluginInterface;
        CommandManager  = commandManager;

        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Configuration.Initialize(PluginInterface);
        OffsetManager.Setup(SigScanner);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Open the Whiskers settings menu."
        });

        AgentConfigSystem = new AgentConfigSystem(AgentManager.Instance.FindAgentInterfaceByVtable(Offsets.Offsets.AgentConfigSystem));
        AgentPerformance  = new AgentPerformance(AgentManager.Instance.FindAgentInterfaceByVtable(Offsets.Offsets.AgentPerformance));
        EnsembleManager   = new EnsembleManager();

        Collector.Instance.Initialize(data, clientState, partyList);

        AgentConfigSystem.GetSettings();

        //NetworkReader.Initialize();

        // you might normally want to embed resources and load them from the manifest stream
        PluginUi = new PluginUi(Configuration);

        PluginInterface.UiBuilder.Draw         += DrawUi;
        PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUi;
    }

    public void Dispose()
    {
        //NetworkReader.Dispose();
        AgentConfigSystem.RestoreSettings();
        AgentConfigSystem?.ApplyGraphicSettings();
        EnsembleManager?.Dispose();
        Collector.Instance.Dispose();

        PluginUi.Dispose();
        CommandManager.RemoveHandler(CommandName);
    }

    private void OnCommand(string command, string args)
    {
        // in response to the slash command, just display our main ui
        PluginUi.Visible = true;
    }

    private void DrawUi()
    {
        PluginUi.Draw();
    }

    private void DrawConfigUi()
    {
        PluginUi.Visible = true;
    }
}