/*
 * Copyright(c) 2025 Meowchestra, GiR-Zippo
 * Licensed under the GPL v3 license. See https://github.com/Meowchestra/MeowMusic/blob/main/LICENSE for full license information.
 */

using System.Runtime.InteropServices;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using FFXIVClientStructs.FFXIV.Common.Math;
using Whiskers.Offsets;

namespace Whiskers.Utils;

internal static class Misc
{
    /*
    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOZORDER = 0x0004;
    */

    internal static unsafe void SetGameRenderSize(uint width, uint height)
    {
        var dev = Device.Instance();
        dev->NewWidth                = width;
        dev->NewHeight               = height;
        dev->RequestResolutionChange = 1;

        /*if (dev->hWnd != null)
        {
            SetWindowPos(
                (IntPtr)dev->hWnd,
                IntPtr.Zero,
                (int)left, (int)top,
                (int)width, (int)height, 
                SWP_NOZORDER);
        }*/
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