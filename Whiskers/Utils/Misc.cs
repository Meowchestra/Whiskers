/*
 * Copyright(c) 2025 Meowchestra, GiR-Zippo
 * Licensed under the GPL v3 license. See https://github.com/Meowchestra/MeowMusic/blob/main/LICENSE for full license information.
 */

using System.Runtime.InteropServices;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using FFXIVClientStructs.FFXIV.Common.Math;
using Whiskers.Offsets;

// ReSharper disable InconsistentNaming

namespace Whiskers.Utils;

internal static class Misc
{
    #region Win32 P/Invoke
    [StructLayout(LayoutKind.Sequential)]
    private struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    private static extern bool AdjustWindowRectEx(ref Rect lpRect, uint dwStyle, bool bMenu, uint dwExStyle);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
    private static extern nint GetWindowLongPtr(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsZoomed(IntPtr hWnd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    // ShowWindow constants
    private const int SW_RESTORE = 9;

    // Window styles from WinUser.h
    private const int GWL_STYLE = -16;
    private const int GWL_EXSTYLE = -20;

    // SetWindowPos Flags
    //private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOZORDER = 0x0004;
    private const uint SWP_NOACTIVATE = 0x0010;
    //private const uint SWP_FRAMECHANGED = 0x0020;
    #endregion

    /// <summary>
    /// Safely sets the game's internal render resolution with option to skip DirectX operations.
    /// </summary>
    internal static unsafe void SetGameRenderSize(uint width, uint height, uint left = 0, uint top = 0, bool skipDirectXOperations = false, bool skipWindowPositioning = false, bool preserveFullscreenResolution = false)
    {
        try
        {
            var dev = Device.Instance();
            if (dev == null) return;

            var hWnd = (IntPtr)dev->hWnd;
            if (hWnd == IntPtr.Zero) return;

            // Check if window is currently fullscreen/snapped
            var isCurrentlyFullscreen = IsZoomed(hWnd);
            var shouldPreserveFullscreen = preserveFullscreenResolution && isCurrentlyFullscreen;
            
            // Handle maximized/snapped windows by restoring them first
            // This allows us to set position/resolution even when snapped fullscreen at top
            // But only if we're not preserving fullscreen state
            if (isCurrentlyFullscreen && !shouldPreserveFullscreen)
            {
                // Restore the window from maximized/snapped state
                ShowWindow(hWnd, SW_RESTORE);
                
                // Give the window manager a moment to process the restore
                Thread.Sleep(50);
            }

            // 2. Get the current window style.
            var style = (uint)GetWindowLongPtr(hWnd, GWL_STYLE);
            var exStyle = (uint)GetWindowLongPtr(hWnd, GWL_EXSTYLE);

            // 3. Calculate the required total window size for the desired client area size.
            var rect = new Rect
            {
                Left   = 0,
                Top    = 0,
                Right  = (int)width,
                Bottom = (int)height
            };
            AdjustWindowRectEx(ref rect, style, false, exStyle);

            var windowWidth = rect.Right - rect.Left;
            var windowHeight = rect.Bottom - rect.Top;
            
            // 4. Determine the final position and flags.
            const uint flags = SWP_NOZORDER | SWP_NOACTIVATE;

            var windowX = (int)left + rect.Left;
            var windowY = (int)top + rect.Top;

            // 5. Apply the final size and position FIRST (Win32 operations are safer and should precede DirectX).
            // Handle window positioning and sizing separately
            if (!shouldPreserveFullscreen)
            {
                if (skipWindowPositioning)
                {
                    // Only set size, skip position (use SWP_NOMOVE flag)
                    SetWindowPos(hWnd, IntPtr.Zero, 0, 0, windowWidth, windowHeight, flags | SWP_NOMOVE);
                }
                else
                {
                    // Set both position and size
                    SetWindowPos(hWnd, IntPtr.Zero, windowX, windowY, windowWidth, windowHeight, flags);
                }
            }

            // 6. Apply DirectX operations AFTER window sizing to trigger internal resolution changes
            // This prevents race conditions between window operations and DirectX state changes
            // Skip DirectX operations if explicitly requested OR if preserving fullscreen state
            if (!skipDirectXOperations && !shouldPreserveFullscreen)
            {
                // Set the game's internal resolution - this triggers FFXIV to apply pending config changes
                dev->NewWidth                = width;
                dev->NewHeight               = height;
                dev->RequestResolutionChange = 1;
            }
        }
        catch (Exception ex)
        {
            // Log the error but don't crash the game
            Api.PluginLog?.Warning($"SetGameRenderSize failed: {ex.Message}");
        }
    }

    internal static IGameObject? GetNearestEntrance(out float distance)
    {
        var currentDistance = float.MaxValue;
        IGameObject? currentObject = null;

        if (Api.Objects != null)
        {
            foreach (var x in Api.Objects)
            {
                if (x.IsTargetable && LangStrings.Entrance.Any(r => r.IsMatch(x.Name.TextValue)))
                {
                    if (Api.ClientState?.LocalPlayer != null)
                    {
                        var position = Vector3.Distance(Api.ClientState.LocalPlayer.Position, x.Position);
                        if (position < currentDistance)
                        {
                            currentDistance = position;
                            currentObject   = x;

                            distance = currentDistance;
                            return currentObject;
                        }
                    }
                }
            }
        }

        distance = currentDistance;
        return currentObject;
    }
}