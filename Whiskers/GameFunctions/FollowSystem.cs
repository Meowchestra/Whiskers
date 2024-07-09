/*
 * Copyright(c) 2024 Meowchestra, GiR-Zippo
 * Licensed under the GPL v3 license. See https://github.com/Meowchestra/MeowMusic/blob/main/LICENSE for full license information.
 */

using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Common.Math;
using Whiskers.Offsets;
using Whiskers.Utils;

namespace Whiskers.GameFunctions;

public class FollowSystem : IDisposable
{
    internal bool Follow;
    internal bool Following;
    internal int FollowDistance = 2;
    internal string FollowTarget = "";
    internal uint HomeWorldId { get; set; }
    internal IGameObject? FollowTargetObject;
    internal IPlayerCharacter? TChar = null;
    private readonly OverrideMovement _overrideMovement;

    public FollowSystem(string targetName, uint homeWorldId)
    {
        FollowTarget      = targetName;
        HomeWorldId       = homeWorldId;
        _overrideMovement = new OverrideMovement();
        if (Api.Framework != null) Api.Framework.Update += OnGameFrameworkUpdate;
    }

    private static IGameObject? GetGameObjectFromName(string objectName, uint worldId)
    {
        if (Api.Objects != null)
        {
            var obj = Api.Objects.AsEnumerable().FirstOrDefault(s => s.Name.ToString().Equals(objectName));
            if (obj is not IPlayerCharacter f)
                return null;
            if (f.HomeWorld.Id == worldId)
                return obj;
        }

        return null;
    }

    private bool GetFollowTargetObject()
    {
        var ftarget = GetGameObjectFromName(FollowTarget, HomeWorldId);
        if (ftarget == null)
        {
            FollowTargetObject = null;
            if (Following)
                Stop();
            return false;
        }

        FollowTargetObject = ftarget;
        return true;
    }

    private void MoveTo(Vector3 position, float precision = 0.1f)
    {
        if (_overrideMovement.Precision != precision)
            _overrideMovement.Precision = precision;

        _overrideMovement.DesiredPosition = position;
    }

    private void OnGameFrameworkUpdate(IFramework framework)
    {
        //If follow is not enabled clear TextColored's and return
        if (!Follow)
        {
            if (Following)
            {
                Stop();
                Following = false;
            }
            return;
        }

        //If LocalPlayer object is null return (we are not logged in or between zones etc..)
        if (Api.ClientState?.LocalPlayer == null) return;

        //If followTarget is not empty GetFollowTargetObject then set our player variable and calculate the distance
        //between player and followTargetObject and if distance > followDistance move to the followTargetObject
        if (!string.IsNullOrEmpty(FollowTarget))
        {
            try
            {
                var player = Api.ClientState.LocalPlayer;
                if (!GetFollowTargetObject())
                    return;

                if (FollowTargetObject == null || player == null) return;

                if (!Follow)
                {
                    if (Following)
                    {
                        Stop();
                        Following = false;
                    }
                    return;
                }

                var distance = Vector3.Distance(player.Position, FollowTargetObject.Position);
                if (distance > FollowDistance + .1f && distance < 100)
                {
                    Following = true;
                    MoveTo(FollowTargetObject.Position, FollowDistance + .1f);
                }
                else if (Following)
                {
                    Following = false;
                    Stop();
                }
            }
            catch (Exception e)
            {
                Api.PluginLog?.Error(e.ToString());
                if (Following)
                {
                    Following = false;
                    Stop();
                }
            }
        }
        else
        {
            if (Following)
            {
                Following = false;
                Stop();
            }
        }
    }

    private void StopAllMovement()
    {
        Follow    = false;
        Following = false;
        Stop();
    }

    private void Stop()
    {
        if (_overrideMovement.DesiredPosition != null)
            _overrideMovement.DesiredPosition = null;
    }

    public void Dispose()
    {
        StopAllMovement();
        if (Api.Framework != null) Api.Framework.Update -= OnGameFrameworkUpdate;
        _overrideMovement.Dispose();
    }

}