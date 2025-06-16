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
    /// Sets the game's internal render resolution and adjusts the window position and size.
    /// </summary>
    internal static unsafe void SetGameRenderSize(uint width, uint height, uint left = 0, uint top = 0, bool keepCurrentPosition = false)
    {
        var dev = Device.Instance();
        if (dev == null) return;

        var hWnd = (IntPtr)dev->hWnd;
        if (hWnd == IntPtr.Zero) return;

        // Gracefully handle maximized windows.
        // If the window is maximized (top snapped), we abort the entire operation
        // to avoid conflicts with the window manager and respect the user's current layout.
        if (IsZoomed(hWnd))
        {
            return;
        }

        // 1. Set the game's internal resolution.
        dev->NewWidth                = width;
        dev->NewHeight               = height;
        dev->RequestResolutionChange = 1;

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

        // 5. Apply the final size and position.
        SetWindowPos(hWnd, IntPtr.Zero, windowX, windowY, windowWidth, windowHeight, flags);
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