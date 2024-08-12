/*
 * Copyright(c) 2024 Meowchestra, GiR-Zippo
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
        var newPos = new Vector3() { X = x, Y = y, Z = z };
        if (Api.ClientState != null && Api.ClientState.LocalPlayer != null && newPos == Api.ClientState.LocalPlayer.Position)
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
        if (Cam != null)
            Cam.Enabled = false;

        if (Movement != null)
            Movement.Enabled = false;
    }

    private Vector3 _lastPos;
    private byte _round;
    private async Task RunMoveTask(CancellationToken token)
    {
        _round    = 4; //4 rounds until we give up
        if (Api.ClientState != null)
        {
            if (Api.ClientState.LocalPlayer != null)
            {
                _lastPos = Api.ClientState.LocalPlayer.Position;

                if (Movement is { Enabled: false })
                    Movement.Enabled = true;

                while (!token.IsCancellationRequested)
                {
                    if (token.IsCancellationRequested)
                        break;

                    if (Movement is { Enabled: true })
                    {
                        // check if we stuck
                        var ldist = _lastPos - Api.ClientState.LocalPlayer.Position;
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
                                    await Task.Delay(300, token).ContinueWith(static tsk => { }, token);
                                    Cam.Enabled = false;
                                }

                                break;
                            }

                            _round -= 1;
                        }
                        else
                            _round = 4; //4 rounds until we give up

                        //check if we reached our position
                        var dist = Movement.DesiredPosition - Api.ClientState.LocalPlayer.Position;
                        dist.Y = 0.0f;
                        if (dist.LengthSquared() <= Movement.Precision * Movement.Precision)
                        {
                            if (Cam != null)
                            {
                                Cam.DesiredAzimuth = _desiredRotation;
                                Cam.Enabled        = true;
                                Movement.Enabled   = false;

                                await Task.Delay(800, token).ContinueWith(static tsk => { }, token);
                                Cam.Enabled = false;
                            }

                            break;
                        }
                    }

                    _lastPos = Api.ClientState.LocalPlayer.Position;
                    await Task.Delay(50, token).ContinueWith(static tsk => { }, token);
                }
            }
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