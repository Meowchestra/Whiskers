/*
 * Copyright(c) 2025 Meowchestra, GiR-Zippo
 * Licensed under the GPL v3 license. See https://github.com/Meowchestra/MeowMusic/blob/main/LICENSE for full license information.
 */

using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using System.Timers;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using H.Pipes.Args;
using Whiskers.Config;
using Whiskers.GameFunctions;
using Whiskers.Offsets;
using Whiskers.Utils;
using Chat = Whiskers.GameFunctions.Chat;
using Timer = System.Timers.Timer;

namespace Whiskers.Windows;

public class MainWindow : Window, IDisposable
{
    private Timer ReconnectTimer { get; set; } = new();
    private Queue<IpcMessage> Qt { get; set; } = new();
    private Configuration Configuration { get; init; }

    // this extra bool exists for ImGui, since you can't ref a property
    private bool _visible;
    public bool Visible
    {
        get => _visible;
        set => _visible = value;
    }

    private bool ManuallyDisconnected { get; set; }
    private bool? LastKnownGfxState { get; set; } // Track GFX state in memory for plugin session

    public MainWindow(Whiskers plugin, Configuration configuration) : base(
        "Whiskers", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        Configuration = configuration;

        Pipe.Initialize();
        if (Pipe.Client != null)
        {
            Pipe.Client.Connected       += pipeClient_Connected;
            Pipe.Client.MessageReceived += pipeClient_MessageReceived;
            Pipe.Client.Disconnected    += pipeClient_Disconnected;
        }

        ReconnectTimer.Elapsed += reconnectTimer_Elapsed;

        ReconnectTimer.Interval = 2000;
        ReconnectTimer.Enabled  = configuration.AutoConnect;

        Visible = false;
    }

    private void pipeClient_Connected(object? sender, ConnectionEventArgs<IpcMessage> e)
    {
        Pipe.Client?.WriteAsync(new IpcMessage
        {
            MsgType    = MessageType.Handshake,
            MsgChannel = 0,
            Message    = Environment.ProcessId.ToString()
        });

        Pipe.Client?.WriteAsync(new IpcMessage
        {
            MsgType    = MessageType.Version,
            MsgChannel = 0,
            Message    = Environment.ProcessId + ":" + Assembly.GetExecutingAssembly().GetName().Version
        });
        
        GameSettings.AgentConfigSystem.GetSettings(GameSettingsTables.Instance.CustomTable);
        var currentGfxState = GameSettings.AgentConfigSystem.CheckLowSettings(GameSettingsTables.Instance.CustomTable);
        LastKnownGfxState = currentGfxState;
        Pipe.Write(MessageType.SetGfx, 0, currentGfxState);
        Pipe.Write(MessageType.BackgroundFpsState, 0, GameSettings.AgentConfigSystem.GetBackgroundFpsEnable());
        Pipe.Write(MessageType.BackgroundAudioState, 0, GameSettings.AgentConfigSystem.GetBackgroundAudioEnable());
        Pipe.Write(MessageType.MasterSoundState, 0, GameSettings.AgentConfigSystem.GetMasterSoundEnable());
        Pipe.Write(MessageType.MasterVolume, 0, GameSettings.AgentConfigSystem.GetMasterSoundVolume());
        Pipe.Write(MessageType.BgmSoundState, 0, GameSettings.AgentConfigSystem.GetBgmSoundEnable());
        Pipe.Write(MessageType.BgmVolume, 0, GameSettings.AgentConfigSystem.GetBgmSoundVolume());
        Pipe.Write(MessageType.EffectsSoundState, 0, GameSettings.AgentConfigSystem.GetEffectsSoundEnable());
        Pipe.Write(MessageType.EffectsVolume, 0, GameSettings.AgentConfigSystem.GetEffectsSoundVolume());
        Pipe.Write(MessageType.VoiceSoundState, 0, GameSettings.AgentConfigSystem.GetVoiceSoundEnable());
        Pipe.Write(MessageType.VoiceVolume, 0, GameSettings.AgentConfigSystem.GetVoiceSoundVolume());
        Pipe.Write(MessageType.SystemSoundState, 0, GameSettings.AgentConfigSystem.GetSystemSoundEnable());
        Pipe.Write(MessageType.SystemVolume, 0, GameSettings.AgentConfigSystem.GetSystemSoundVolume());
        Pipe.Write(MessageType.AmbientSoundState, 0, GameSettings.AgentConfigSystem.GetAmbientSoundEnable());
        Pipe.Write(MessageType.AmbientVolume, 0, GameSettings.AgentConfigSystem.GetAmbientSoundVolume());
        Pipe.Write(MessageType.PerformanceSoundState, 0, GameSettings.AgentConfigSystem.GetPerformanceSoundEnable());
        Pipe.Write(MessageType.PerformanceVolume, 0, GameSettings.AgentConfigSystem.GetPerformanceSoundVolume());

        Collector.Instance.UpdateClientStats();
    }

    private void pipeClient_Disconnected(object? sender, ConnectionEventArgs<IpcMessage> e)
    {
        if (!Configuration.AutoConnect)
            return;

        ReconnectTimer.Interval = 2000;
        ReconnectTimer.Enabled  = Configuration.AutoConnect;
    }

    private void reconnectTimer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        if (ManuallyDisconnected)
            return;

        switch (Pipe.Client)
        {
            case { IsConnected: true }:
                ReconnectTimer.Enabled = false;
                return;
            case { IsConnecting: false }:
                Pipe.Client.ConnectAsync();
                break;
        }
    }

    private void pipeClient_MessageReceived(object? sender, ConnectionMessageEventArgs<IpcMessage?> e)
    {
        var inMsg = e.Message;
        if (inMsg == null)
            return;

        switch (inMsg.MsgType)
        {
            case MessageType.Version:
                /*if (new Version(inMsg.Message) > Assembly.GetEntryAssembly()?.GetName().Version)
                {
                    ManuallyDisconnected = true;
                    Pipe.Client?.DisconnectAsync();
                    Api.PluginLog?.Error("Whiskers is out of date and cannot work with the running bard program.");
                }*/
                break;
            case MessageType.NoteOn:
                PerformActions.PlayNote(Convert.ToInt16(inMsg.Message), true);
                break;
            case MessageType.NoteOff:
                PerformActions.PlayNote(Convert.ToInt16(inMsg.Message), false);
                break;
            case MessageType.ProgramChange:
                PerformActions.GuitarSwitchTone(Convert.ToInt32(inMsg.Message));
                break;
            case MessageType.Instrument:
            case MessageType.StartEnsemble:
            case MessageType.AcceptReply:
            case MessageType.PartyInvite:
            case MessageType.PartyInviteAccept:
            case MessageType.PartyPromote:
            case MessageType.PartyEnterHouse:
            case MessageType.PartyTeleport:
            case MessageType.PartyFollow:
            case MessageType.SetGfx:
            case MessageType.SetWindowRenderSize:
            case MessageType.BackgroundFpsState:
            case MessageType.BackgroundAudioState:
            case MessageType.MasterSoundState:
            case MessageType.MasterVolume:
            case MessageType.BgmSoundState:
            case MessageType.BgmVolume:
            case MessageType.EffectsSoundState:
            case MessageType.EffectsVolume:
            case MessageType.VoiceSoundState:
            case MessageType.VoiceVolume:
            case MessageType.SystemSoundState:
            case MessageType.SystemVolume:
            case MessageType.AmbientSoundState:
            case MessageType.AmbientVolume:
            case MessageType.PerformanceSoundState:
            case MessageType.PerformanceVolume:
            case MessageType.Chat:
            case MessageType.ClientLogout:
            case MessageType.GameShutdown:
            case MessageType.ExitGame:
                Qt.Enqueue(inMsg);
                break;
        }
    }

    public void Dispose()
    {
        ManuallyDisconnected = true;

        if (Pipe.Client != null)
        {
            Pipe.Client.Connected       -= pipeClient_Connected;
            Pipe.Client.MessageReceived -= pipeClient_MessageReceived;
            Pipe.Client.Disconnected    -= pipeClient_Disconnected;
            ReconnectTimer.Elapsed      -= reconnectTimer_Elapsed;

            Pipe.Client.DisconnectAsync();
            Pipe.Client.DisposeAsync();
        }

        Pipe.Dispose();
    }

    public override void Update()
    {
        //Do the in queue
        while (Qt.Count > 0)
        {
            try
            {
                var msg = Qt.Dequeue();
                switch (msg.MsgType)
                {
                    case MessageType.Instrument:
                        PerformActions.DoPerformActionOnTick(Convert.ToUInt32(msg.Message));
                        break;
                    case MessageType.StartEnsemble:
                        PerformActions.BeginReadyCheck();
                        PerformActions.ConfirmBeginReadyCheck();
                        break;
                    case MessageType.AcceptReply:
                        PerformActions.ConfirmReceiveReadyCheck();
                        break;
                    case MessageType.PartyInvite:
                        Party.Instance.PartyInvite(msg.Message);
                        break;
                    case MessageType.PartyInviteAccept:
                        Party.Instance.AcceptPartyInviteEnable();
                        break;
                    case MessageType.PartyPromote:
                        Party.Instance.PromoteCharacter(msg.Message);
                        break;
                    case MessageType.PartyEnterHouse:
                        FollowSystem.StopFollow();
                        Party.Instance.EnterHouse();
                        break;
                    case MessageType.PartyTeleport:
                        FollowSystem.StopFollow();
                        Party.Instance.Teleport(Convert.ToBoolean(msg.Message));
                        break;
                    case MessageType.PartyFollow:
                        if (msg.Message == "")
                            FollowSystem.StopFollow();
                        else
                            FollowSystem.FollowCharacter(msg.Message.Split(';')[0], Convert.ToUInt16(msg.Message.Split(';')[1]));
                        break;
                    case MessageType.SetGfx:
                        if (msg.Message == "query")
                        {
                            GameSettings.AgentConfigSystem.GetSettings(GameSettingsTables.Instance.CustomTable);
                            var currentState = GameSettings.AgentConfigSystem.CheckLowSettings(GameSettingsTables.Instance.CustomTable);
                            LastKnownGfxState = currentState;
                            Pipe.Write(MessageType.SetGfx, 0, currentState);
                        }
                        else
                        {
                            var requestedState = Convert.ToBoolean(msg.Message);
                            GameSettings.AgentConfigSystem.SetGfx(requestedState);
                            LastKnownGfxState = requestedState;
                            Pipe.Write(MessageType.SetGfx, 0, requestedState);
                        }
                        break;
                    case MessageType.SetWindowRenderSize:
                        Misc.SetGameRenderSize(Convert.ToUInt32(msg.Message.Split(';')[0]), Convert.ToUInt32(msg.Message.Split(';')[1]));
                        break;
                    case MessageType.BackgroundFpsState:
                        if (msg.Message == "query")
                        {
                            Pipe.Write(MessageType.BackgroundFpsState, 0, GameSettings.AgentConfigSystem.GetBackgroundFpsEnable());
                        }
                        else
                        {
                            var requestedState = Convert.ToBoolean(msg.Message);
                            GameSettings.AgentConfigSystem.SetBackgroundFpsEnable(requestedState);
                            Pipe.Write(MessageType.BackgroundFpsState, 0, requestedState);
                        }
                        break;
                    case MessageType.BackgroundAudioState:
                        if (msg.Message == "query")
                        {
                            Pipe.Write(MessageType.BackgroundAudioState, 0, GameSettings.AgentConfigSystem.GetBackgroundAudioEnable());
                        }
                        else
                        {
                            var requestedAudioState = Convert.ToBoolean(msg.Message);
                            GameSettings.AgentConfigSystem.SetBackgroundAudioEnable(requestedAudioState);
                            Pipe.Write(MessageType.BackgroundAudioState, 0, requestedAudioState);
                        }
                        break;
                    case MessageType.MasterSoundState:
                        var requestedSoundState = Convert.ToBoolean(msg.Message);
                        GameSettings.AgentConfigSystem.SetMasterSoundEnable(requestedSoundState);
                        Pipe.Write(MessageType.MasterSoundState, 0, requestedSoundState);
                        break;
                    case MessageType.MasterVolume:
                        if (Convert.ToInt16(msg.Message) == -1)
                        {
                            Pipe.Write(MessageType.MasterVolume, 0, GameSettings.AgentConfigSystem.GetMasterSoundVolume());
                            Pipe.Write(MessageType.MasterSoundState, 0, GameSettings.AgentConfigSystem.GetMasterSoundEnable());
                        }
                        else
                            GameSettings.AgentConfigSystem.SetMasterSoundVolume(Convert.ToInt16(msg.Message));
                        break;
                    case MessageType.BgmSoundState:
                        var requestedBgmState = Convert.ToBoolean(msg.Message);
                        GameSettings.AgentConfigSystem.SetBgmSoundEnable(requestedBgmState);
                        Pipe.Write(MessageType.BgmSoundState, 0, requestedBgmState);
                        break;
                    case MessageType.BgmVolume:
                        if (Convert.ToInt16(msg.Message) == -1)
                        {
                            Pipe.Write(MessageType.BgmVolume, 0, GameSettings.AgentConfigSystem.GetBgmSoundVolume());
                            Pipe.Write(MessageType.BgmSoundState, 0, GameSettings.AgentConfigSystem.GetBgmSoundEnable());
                        }
                        else
                            GameSettings.AgentConfigSystem.SetBgmSoundVolume(Convert.ToInt16(msg.Message));
                        break;
                    case MessageType.EffectsSoundState:
                        var requestedEffectsState = Convert.ToBoolean(msg.Message);
                        GameSettings.AgentConfigSystem.SetEffectsSoundEnable(requestedEffectsState);
                        Pipe.Write(MessageType.EffectsSoundState, 0, requestedEffectsState);
                        break;
                    case MessageType.EffectsVolume:
                        if (Convert.ToInt16(msg.Message) == -1)
                        {
                            Pipe.Write(MessageType.EffectsVolume, 0, GameSettings.AgentConfigSystem.GetEffectsSoundVolume());
                            Pipe.Write(MessageType.EffectsSoundState, 0, GameSettings.AgentConfigSystem.GetEffectsSoundEnable());
                        }
                        else
                            GameSettings.AgentConfigSystem.SetEffectsSoundVolume(Convert.ToInt16(msg.Message));
                        break;
                    case MessageType.VoiceSoundState:
                        var requestedVoiceState = Convert.ToBoolean(msg.Message);
                        GameSettings.AgentConfigSystem.SetVoiceSoundEnable(requestedVoiceState);
                        Pipe.Write(MessageType.VoiceSoundState, 0, requestedVoiceState);
                        break;
                    case MessageType.VoiceVolume:
                        if (Convert.ToInt16(msg.Message) == -1)
                        {
                            Pipe.Write(MessageType.VoiceVolume, 0, GameSettings.AgentConfigSystem.GetVoiceSoundVolume());
                            Pipe.Write(MessageType.VoiceSoundState, 0, GameSettings.AgentConfigSystem.GetVoiceSoundEnable());
                        }
                        else
                            GameSettings.AgentConfigSystem.SetVoiceSoundVolume(Convert.ToInt16(msg.Message));
                        break;
                    case MessageType.SystemSoundState:
                        var requestedSystemState = Convert.ToBoolean(msg.Message);
                        GameSettings.AgentConfigSystem.SetSystemSoundEnable(requestedSystemState);
                        Pipe.Write(MessageType.SystemSoundState, 0, requestedSystemState);
                        break;
                    case MessageType.SystemVolume:
                        if (Convert.ToInt16(msg.Message) == -1)
                        {
                            Pipe.Write(MessageType.SystemVolume, 0, GameSettings.AgentConfigSystem.GetSystemSoundVolume());
                            Pipe.Write(MessageType.SystemSoundState, 0, GameSettings.AgentConfigSystem.GetSystemSoundEnable());
                        }
                        else
                            GameSettings.AgentConfigSystem.SetSystemSoundVolume(Convert.ToInt16(msg.Message));
                        break;
                    case MessageType.AmbientSoundState:
                        var requestedAmbientState = Convert.ToBoolean(msg.Message);
                        GameSettings.AgentConfigSystem.SetAmbientSoundEnable(requestedAmbientState);
                        Pipe.Write(MessageType.AmbientSoundState, 0, requestedAmbientState);
                        break;
                    case MessageType.AmbientVolume:
                        if (Convert.ToInt16(msg.Message) == -1)
                        {
                            Pipe.Write(MessageType.AmbientVolume, 0, GameSettings.AgentConfigSystem.GetAmbientSoundVolume());
                            Pipe.Write(MessageType.AmbientSoundState, 0, GameSettings.AgentConfigSystem.GetAmbientSoundEnable());
                        }
                        else
                            GameSettings.AgentConfigSystem.SetAmbientSoundVolume(Convert.ToInt16(msg.Message));
                        break;
                    case MessageType.PerformanceSoundState:
                        var requestedPerformanceState = Convert.ToBoolean(msg.Message);
                        GameSettings.AgentConfigSystem.SetPerformanceSoundEnable(requestedPerformanceState);
                        Pipe.Write(MessageType.PerformanceSoundState, 0, requestedPerformanceState);
                        break;
                    case MessageType.PerformanceVolume:
                        if (Convert.ToInt16(msg.Message) == -1)
                        {
                            Pipe.Write(MessageType.PerformanceVolume, 0, GameSettings.AgentConfigSystem.GetPerformanceSoundVolume());
                            Pipe.Write(MessageType.PerformanceSoundState, 0, GameSettings.AgentConfigSystem.GetPerformanceSoundEnable());
                        }
                        else
                            GameSettings.AgentConfigSystem.SetPerformanceSoundVolume(Convert.ToInt16(msg.Message));
                        break;
                    case MessageType.Chat:
                        var chatMessageChannelType = ChatMessageChannelType.ParseByChannelCode(msg.MsgChannel);
                        if (chatMessageChannelType.Equals(ChatMessageChannelType.None))
                            Chat.SendMessage(msg.Message);
                        else
                            Chat.SendMessage(chatMessageChannelType.ChannelShortCut + " " + msg.Message);
                        break;
                    case MessageType.ClientLogout:
                        MiscGameFunctions.CharacterLogout();
                        break;
                    case MessageType.GameShutdown:
                        MiscGameFunctions.GameShutdown();
                        break;

                    case MessageType.ExitGame:
                        Process.GetCurrentProcess().Kill();
                        break;
                }
            }
            catch (Exception ex)
            {
                Api.PluginLog?.Error($"exception: {ex}");
            }
        }
    }

    public override void Draw()
    {
        ImGui.SetNextWindowSize(new Vector2(300, 250));
        ImGui.SetNextWindowSizeConstraints(new Vector2(300, 250), new Vector2(float.MaxValue, float.MaxValue));
        if (ImGui.Begin("Whiskers", ref _visible))
        {
            if (ImGui.BeginTabBar("WhiskersTabs"))
            {
                if (ImGui.BeginTabItem("Connection"))
                {
                    DrawConnectionTab();
                    ImGui.EndTabItem();
                }
                if (ImGui.BeginTabItem("Settings"))
                {
                    DrawSettingsTab();
                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }
            ImGui.End();
        }
    }

    private void DrawConnectionTab()
    {
        // can't ref a property, so use a local copy
        var configValue = Configuration.AutoConnect;
        if (ImGui.Checkbox("Auto Connect", ref configValue))
        {
            Configuration.AutoConnect = configValue;
            // can save immediately on change, if you don't want to provide a "Save and Close" button
            Configuration.Save();
        }

        if (ImGui.Button("Connect"))
        {
            if (Configuration.AutoConnect)
                ManuallyDisconnected = false;
            ReconnectTimer.Interval = 500;
            ReconnectTimer.Enabled  = true;
        }
        ImGui.SameLine();
        if (ImGui.Button("Disconnect"))
        {
            if (Pipe.Client is { IsConnected: false })
                return;

            Pipe.Client?.DisconnectAsync();
            ManuallyDisconnected = true;
        }
        ImGui.Text($"Is connected: {Pipe.Client is { IsConnected: true }}");
    }

    private void DrawSettingsTab()
    {
        ImGui.Text("Player Configuration");
        ImGui.BeginGroup();
        if (ImGui.Button("Save"))
        {
            GameSettings.AgentConfigSystem.SaveConfig();
        }
        ImGui.SameLine();
        if (ImGui.Button("Load"))
        {
            GameSettings.AgentConfigSystem.LoadConfig();
        }
        ImGui.SameLine();
        if (ImGui.Button("Erase"))
        {
            File.Delete($"{Api.PluginInterface?.GetPluginConfigDirectory()}\\{Api.ClientState?.LocalPlayer?.Name}-({Api.ClientState?.LocalPlayer?.HomeWorld.ValueNullable?.Name.ToDalamudString().TextValue}).json");
        }
        ImGui.SameLine();
        if (ImGui.Button("Restore"))
        {
            GameSettings.AgentConfigSystem.RestoreSettings(GameSettingsTables.Instance.StartupTable, true, Configuration.PreserveFullscreenResolution);
        }
        ImGui.EndGroup();

        ImGui.Separator();
        ImGui.Text("Graphics Settings");
        
        var loadOnLogin = Configuration.LoadGraphicsOnLogin;
        if (ImGui.Checkbox("Load graphics on login", ref loadOnLogin))
        {
            Configuration.LoadGraphicsOnLogin = loadOnLogin;
            Configuration.Save();
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("When enabled, saved graphics settings will automatically be loaded when logging in.");
        }

        var restoreOnLogout = Configuration.RestoreGraphicsOnLogout;
        if (ImGui.Checkbox("Restore graphics on logout", ref restoreOnLogout))
        {
            Configuration.RestoreGraphicsOnLogout = restoreOnLogout;
            Configuration.Save();
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("When enabled, graphics settings will be restored to startup values when logging out.\nKeep disabled if causing crashes.");
        }

        var restoreOnPluginUnload = Configuration.RestoreGraphicsOnPluginUnload;
        if (ImGui.Checkbox("Restore graphics on plugin shutdown", ref restoreOnPluginUnload))
        {
            Configuration.RestoreGraphicsOnPluginUnload = restoreOnPluginUnload;
            Configuration.Save();
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("When enabled, graphics settings will be restored to startup values when the plugin is disabled or unloaded.");
        }

        var skipWindowPositioning = Configuration.PreserveFullscreenResolution;
        if (ImGui.Checkbox("Preserve fullscreen resolution", ref skipWindowPositioning))
        {
            Configuration.PreserveFullscreenResolution = skipWindowPositioning;
            Configuration.Save();
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("When enabled, window position, size, & resolution will be skipped during graphics handling if the game is fullscreen.\nThis prevents the window from being moved out of the top-snapped state.");
        }
    }
}