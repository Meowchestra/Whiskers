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

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool AdjustWindowRectEx(ref Rect lpRect, uint dwStyle, bool bMenu, uint dwExStyle);

    // Use GetWindowLongPtr for 64-bit compatibility
    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW", SetLastError = true)]
    private static extern nint GetWindowLongPtr(IntPtr hWnd, int nIndex);

    // Window styles from WinUser.h
    private const int GWL_STYLE = -16;
    private const int GWL_EXSTYLE = -20;

    // SetWindowPos Flags
    private const uint SWP_NOZORDER = 0x0004;
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOSIZE = 0x0001;
    #endregion

    /// <summary>
    /// Sets the game's internal render resolution and adjusts the window position and size
    /// to match the desired client area, compensating for window borders and title bar.
    /// </summary>
    internal static unsafe void SetGameRenderSize(uint width, uint height, uint left = 0, uint top = 0, bool keepCurrentPosition = false)
    {
        var dev = Device.Instance();
        if (dev == null) return;

        // 1. Set the game's internal resolution. FFXIV will use this for rendering.
        dev->NewWidth                = width;
        dev->NewHeight               = height;
        dev->RequestResolutionChange = 1;

        var hWnd = (IntPtr)dev->hWnd;
        if (hWnd == IntPtr.Zero) return;

        // 2. Calculate the correct window size.
        var flags = SWP_NOZORDER;
        int windowX = 0, windowY = 0; // These will be unused if keepCurrentPosition is true

        // Define the desired client rectangle based on the provided parameters.
        var rect = new Rect
        {
            Left   = 0,
            Top    = 0,
            Right  = (int)width,
            Bottom = (int)height
        };

        // Get the current window styles. This is necessary for AdjustWindowRectEx to be accurate.
        var style = (uint)GetWindowLongPtr(hWnd, GWL_STYLE);
        var exStyle = (uint)GetWindowLongPtr(hWnd, GWL_EXSTYLE);

        // Ask Windows to calculate the required window rectangle for our desired client rectangle.
        // The function modifies the RECT in-place.
        AdjustWindowRectEx(ref rect, style, false, exStyle);
    
        var windowWidth = rect.Right - rect.Left;
        var windowHeight = rect.Bottom - rect.Top;

        // 3. Determine the final position and flags
        if (keepCurrentPosition)
        {
            // We only want to set the size, not the position.
            flags |= SWP_NOMOVE;
        }
        else
        {
            // We want to set a specific position. The final window position needs to be
            // offset by the size of the top-left corner of the window frame.
            // `rect.Left` will be a negative number representing the width of the left border.
            // `rect.Top` will be a negative number representing the height of the title bar and top border.
            windowX = (int)left + rect.Left;
            windowY = (int)top + rect.Top;
        }

        // 4. Set the window's actual position and size on the screen.
        SetWindowPos(
            hWnd,
            IntPtr.Zero,
            windowX, 
            windowY,
            windowWidth,
            windowHeight,
            flags);
    }

    internal static IGameObject? GetNearestEntrance(out float distance, bool bypassPredefined = false)
    {
        var currentDistance = float.MaxValue;
        IGameObject? currentObject = null;

        if (Api.Objects != null)
        {
            foreach (var x in Api.Objects)
            {
                if (x.IsTargetable && LangStrings.Entrance.Any(r => r.IsMatch(x.Name.TextValue)))
                {
                    if (Api.ClientState != null)
                    {
                        if (Api.ClientState.LocalPlayer != null)
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
        }

        distance = currentDistance;
        return currentObject;
    }
}