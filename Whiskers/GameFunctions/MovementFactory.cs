/*
 * Copyright(c) 2025 Meowchestra, GiR-Zippo
 * Licensed under the GPL v3 license. See https://github.com/Meowchestra/MeowMusic/blob/main/LICENSE for full license information.
 */

using System.Numerics;
using Whiskers.Offsets;
using Whiskers.Utils;

/*********************************************************/
/***                 CLEAN UP THIS MESS                ***/
/*********************************************************/


namespace Whiskers.GameFunctions;

public class MovementFactory : IDisposable
{
    private static readonly Lazy<MovementFactory> LazyInstance = new(static () => new MovementFactory());

    private CancellationTokenSource _cancelMovementToken = new();

    private MovementFactory()
    {
        Movement = new OverrideMovement();
        Cam      = new CameraUtil();
    }

    public static MovementFactory Instance => LazyInstance.Value;

    private CameraUtil? Cam { get; set; }

    private OverrideMovement? Movement { get; set; }

    private Vector3 DesiredPosition { get; set; } = new(0.0f, 0.0f, 0.0f);

    private Angle _desiredRotation;

    public void Initialize(Vector3 position, float rotation)
    {
        DesiredPosition  = position;
        _desiredRotation = new Angle(rotation);
    }

    public void MoveTo(float x, float y, float z, float rot)
    {
        var player = Api.GetLocalPlayer();
        if (player == null)
            return;

        var newPos = new Vector3 { X = x, Y = y, Z = z };

        if (newPos == player.Position)
            return;

        DesiredPosition  = newPos;
        _desiredRotation = new Angle(rot);
        Move();
    }

    public void Move()
    {
        FollowSystem.StopFollow();

        if (Movement != null)
        {
            Movement.Precision       = 0.05f;
            Movement.DesiredPosition = DesiredPosition;
        }

        Api.Framework?.RunOnTick(delegate
        {
            _cancelMovementToken = new CancellationTokenSource();
            Task.Factory.StartNew(() => RunMoveTask(_cancelMovementToken.Token), TaskCreationOptions.LongRunning);
        });
    }

    public void StopMovement()
    {
        _cancelMovementToken.Cancel();
        Cleanup();
    }

    public void Cleanup()
    {
        Cam?.Enabled      = false;
        Movement?.Enabled = false;
    }

    private Vector3 _lastPos;
    private byte _round;
    private async Task RunMoveTask(CancellationToken token)
    {
        _round = 4; // 4 rounds until we give up

        if (Api.ClientState == null)
        {
            Cleanup();
            return;
        }

        var player = Api.GetLocalPlayer();
        if (player == null)
        {
            Cleanup();
            return;
        }

        _lastPos = player.Position;

        if (Movement is { Enabled: false })
            Movement.Enabled = true;

        while (!token.IsCancellationRequested)
        {
            if (Movement is { Enabled: true })
            {
                // Refresh player reference each tick (safer in case it changes)
                player = Api.GetLocalPlayer();
                if (player == null)
                    break;

                // Check if stuck
                var ldist = _lastPos - player.Position;
                ldist.Y = 0.0f;

                if (ldist.LengthSquared() <= 0.2f * 0.2f)
                {
                    if (_round == 0)
                    {
                        if (Cam != null)
                        {
                            Cam.DesiredAzimuth = _desiredRotation;
                            Cam.Enabled        = true;
                            Movement.Enabled   = false;

                            await Task.Delay(300, token).ContinueWith(static _ => { }, token);

                            Cam.Enabled = false;
                        }

                        break;
                    }

                    _round--;
                }
                else
                {
                    _round = 4;
                }

                // Check if we reached our position
                var dist = Movement.DesiredPosition - player.Position;
                dist.Y = 0.0f;

                if (dist.LengthSquared() <= Movement.Precision * Movement.Precision)
                {
                    if (Cam != null)
                    {
                        Cam.DesiredAzimuth = _desiredRotation;
                        Cam.Enabled        = true;
                        Movement.Enabled   = false;

                        await Task.Delay(800, token).ContinueWith(static _ => { }, token);

                        Cam.Enabled = false;
                    }

                    break;
                }

                _lastPos = player.Position;
            }

            await Task.Delay(50, token).ContinueWith(static _ => { }, token);
        }

        Cleanup();
    }

    public void Dispose()
    {
        if (Cam != null)
        {
            Cam.Enabled = false;
            Cam.Dispose();
            Cam = null;
        }
        if (Movement != null)
        {
            Movement.Enabled = false;
            Movement.Dispose();
            Movement = null;
        }

    }
}