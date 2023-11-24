/*
 * Copyright(c) 2023 GiR-Zippo, Meowchestra
 * Licensed under the GPL v3 license. See https://github.com/GiR-Zippo/LightAmp/blob/main/LICENSE for full license information.
 */

using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using System.Timers;
using H.Pipes.Args;
using ImGuiNET;
using Whiskers.Offsets;
using Timer = System.Timers.Timer;

namespace Whiskers;

public class PayloadMessage
{
    public MessageType MsgType { get; init; } = MessageType.None;
    public int MsgChannel { get; init; }
    public string Message { get; init; } = "";
}

// It is good to have this be disposable in general, in case you ever need it
// to do any cleanup
internal class PluginUi : IDisposable
{
    private Timer ReconnectTimer { get; set; } = new();
    private readonly Queue<PayloadMessage> _qt = new();
    private readonly Configuration _configuration;

    // this extra bool exists for ImGui, since you can't ref a property
    private bool _visible;
    public bool Visible
    {
        get => _visible;
        set => _visible = value;
    }

    private bool ManuallyDisconnected { get; set; }

    public PluginUi(Configuration configuration)
    {
        _configuration = configuration;

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
        
        GameSettings.AgentConfigSystem.LoadConfig();
    }

    private static void pipeClient_Connected(object? sender, ConnectionEventArgs<PayloadMessage> e)
    {
        Pipe.Client?.WriteAsync(new PayloadMessage
        {
            MsgType    = MessageType.Handshake,
            MsgChannel = 0,
            Message    = Environment.ProcessId.ToString()
        });


        Pipe.Client?.WriteAsync(new PayloadMessage
        {
            MsgType    = MessageType.Version,
            MsgChannel = 0,
            Message    = Environment.ProcessId + ":" + Assembly.GetExecutingAssembly().GetName().Version
        });

        Pipe.Write(MessageType.SetGfx, 0, GameSettings.AgentConfigSystem.CheckLowSettings(GameSettingsTables.Instance?.CustomTable));
        Pipe.Write(MessageType.MasterSoundState, 0, GameSettings.AgentConfigSystem.GetMasterSoundEnable());
        Pipe.Write(MessageType.MasterVolume, 0, GameSettings.AgentConfigSystem.GetMasterSoundVolume());

        Collector.Instance.UpdateClientStats();
    }

    private void pipeClient_Disconnected(object? sender, ConnectionEventArgs<PayloadMessage> e)
    {
        if (!_configuration.AutoConnect)
            return;

        ReconnectTimer.Interval = 2000;
        ReconnectTimer.Enabled  = _configuration.AutoConnect;
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

    private void pipeClient_MessageReceived(object? sender, ConnectionMessageEventArgs<PayloadMessage?> e)
    {
        var inMsg = e.Message;
        if (inMsg == null)
            return;
            
        switch (inMsg.MsgType)
        {
            case MessageType.Version:
                if (new Version(inMsg.Message) > Assembly.GetEntryAssembly()?.GetName().Version)
                {
                    ManuallyDisconnected = true;
                    Pipe.Client?.DisconnectAsync();
                    Api.PluginLog?.Error("Whiskers is out of date and cannot work with the running bard program.");
                }
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
            case MessageType.Chat:
            case MessageType.Instrument:
            case MessageType.AcceptReply:
            case MessageType.SetGfx:
            case MessageType.MasterSoundState:
            case MessageType.MasterVolume:
            case MessageType.VoiceSoundState:
            case MessageType.EffectsSoundState:
            case MessageType.StartEnsemble:
            case MessageType.ExitGame:
                _qt.Enqueue(inMsg);
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

    public void Draw()
    {
        // This is our only draw handler attached to UIBuilder, so it needs to be
        // able to draw any windows we might have open.
        // Each method checks its own visibility/state to ensure it only draws when
        // it actually makes sense.
        // There are other ways to do this, but it is generally best to keep the number of
        // draw delegates as low as possible.

        DrawMainWindow();
    }

    private void DrawMainWindow()
    {
        if (Visible)
        {
            ImGui.SetNextWindowSize(new Vector2(300, 180), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSizeConstraints(new Vector2(300, 180), new Vector2(float.MaxValue, float.MaxValue));
            if (ImGui.Begin("Whiskers", ref _visible, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                // can't ref a property, so use a local copy
                var configValue = _configuration.AutoConnect;
                if (ImGui.Checkbox("AutoConnect", ref configValue))
                {
                    _configuration.AutoConnect = configValue;
                    // can save immediately on change, if you don't want to provide a "Save and Close" button
                    _configuration.Save();
                }

                //The connect Button
                if (ImGui.Button("Connect"))
                {
                    if (_configuration.AutoConnect)
                        ManuallyDisconnected = false;
                    ReconnectTimer.Interval = 500;
                    ReconnectTimer.Enabled  = true;
                }
                ImGui.SameLine();
                //The disconnect Button
                if (ImGui.Button("Disconnect"))
                {
                    if (Pipe.Client is { IsConnected: false })
                        return;

                    Pipe.Client?.DisconnectAsync();

                    ManuallyDisconnected = true;
                }
                ImGui.Text($"Is connected: {Pipe.Client is { IsConnected: true }}");

                //PlayerConfig Save/Erase
                ImGui.NewLine();
                ImGui.Text($"Player configuration");
                ImGui.BeginGroup();
                if (ImGui.Button("Save"))
                {
                    GameSettings.AgentConfigSystem.SaveConfig();
                }
                ImGui.SameLine();
                if (ImGui.Button("Erase"))
                {
                    File.Delete($"{Api.PluginInterface?.GetPluginConfigDirectory()}\\{Api.ClientState?.LocalPlayer?.Name}-({Api.ClientState?.LocalPlayer?.HomeWorld.GameData?.Name}).json");
                }
                ImGui.EndGroup();
            }
            ImGui.End();
        }

        //Check performance state
        /*if (Whiskers.AgentPerformance != null && Whiskers.AgentPerformance.InPerformanceMode != performanceModeOpen)
        {
            performanceModeOpen = Whiskers.AgentPerformance.InPerformanceMode;
            if (Pipe.Client != null && Pipe.Client.IsConnected)
            {
                Pipe.Client.WriteAsync(new PayloadMessage()
                {
                    MsgType = MessageType.PerformanceModeState,
                    Message = Environment.ProcessId + ":" + Whiskers.AgentPerformance.Instrument.ToString()
                });
            }
        }*/

        //Do the in queue
        while (_qt.Count > 0)
        {
            try
            {
                var msg = _qt.Dequeue();
                switch (msg.MsgType)
                {
                    case MessageType.Chat:
                        var chatMessageChannelType = ChatMessageChannelType.ParseByChannelCode(msg.MsgChannel);
                        if (chatMessageChannelType.Equals(ChatMessageChannelType.None))
                            Chat.SendMessage(msg.Message);
                        else
                            Chat.SendMessage(chatMessageChannelType.ChannelShortCut + " " + msg.Message);
                        break;
                    case MessageType.Instrument:
                        PerformActions.PerformAction(Convert.ToUInt32(msg.Message));
                        break;
                    case MessageType.AcceptReply:
                        PerformActions.ConfirmReceiveReadyCheck();
                        break;
                    case MessageType.SetGfx:
                        if (Convert.ToBoolean(msg.Message))
                        {
                            GameSettings.AgentConfigSystem.GetSettings(GameSettingsTables.Instance?.CustomTable);
                            GameSettings.AgentConfigSystem.SetMinimalGfx();
                            Whiskers.AgentConfigSystem?.ApplyGraphicSettings();
                        }
                        else
                        {
                            GameSettings.AgentConfigSystem.RestoreSettings(GameSettingsTables.Instance?.CustomTable);
                            Whiskers.AgentConfigSystem?.ApplyGraphicSettings();
                        }
                        break;
                    case MessageType.MasterSoundState:
                        GameSettings.AgentConfigSystem.SetMasterSoundEnable(Convert.ToBoolean(msg.Message));
                        break;
                    case MessageType.MasterVolume:
                        if (Convert.ToInt16(msg.Message) == -1)
                        {
                            Pipe.Write(MessageType.MasterVolume, 0, GameSettings.AgentConfigSystem.GetMasterSoundVolume());
                            Pipe.Write(MessageType.MasterSoundState, 0, GameSettings.AgentConfigSystem.GetMasterSoundEnable());
                            Pipe.Write(MessageType.VoiceSoundState, 0, GameSettings.AgentConfigSystem.GetVoiceSoundEnable());
                            Pipe.Write(MessageType.EffectsSoundState, 0, GameSettings.AgentConfigSystem.GetEffectsSoundEnable());
                        }
                        else
                            GameSettings.AgentConfigSystem.SetMasterSoundVolume(Convert.ToInt16(msg.Message));
                        break;
                    case MessageType.VoiceSoundState:
                        GameSettings.AgentConfigSystem.SetVoiceSoundEnable(Convert.ToBoolean(msg.Message));
                        break;
                    case MessageType.EffectsSoundState:
                        GameSettings.AgentConfigSystem.SetEffectsSoundEnable(Convert.ToBoolean(msg.Message));
                        break;
                    case MessageType.ExitGame:
                        Process.GetCurrentProcess().Kill();
                        break;
                    case MessageType.StartEnsemble:
                        PerformActions.BeginReadyCheck();
                        PerformActions.ConfirmBeginReadyCheck();
                        break;
                }
            }
            catch (Exception ex)
            {
                Api.PluginLog?.Error($"exception: {ex}");
            }
        }
    }
}