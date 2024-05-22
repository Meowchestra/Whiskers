/*
 * Copyright(c) 2024 Meowchestra, GiR-Zippo
 * Licensed under the GPL v3 license. See https://github.com/Meowchestra/MeowMusic/blob/main/LICENSE for full license information.
 */

using Dalamud.Interface.Windowing;
using ImGuiNET;
using Whiskers.Offsets;

namespace Whiskers.Windows;

public class ConfigWindow : Window, IDisposable
{
    public ConfigWindow(Whiskers plugin) : base(
        "A Wonderful Configuration Window",
        ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
    }

    public void Dispose() 
    { 

    }

    public override void Draw()
    {
        if (ImGui.Button("Connect"))
        {
            Api.PluginLog?.Debug("config");
        }
    }
}