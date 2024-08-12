using Whiskers.Offsets;
using Whiskers.Utils;
using System.Numerics;

/*********************************************************/
/***                 CLEAN UP THIS MESS                ***/
/*********************************************************/


namespace Whiskers.GameFunctions;

public class MovementFactory : IDisposable
{
    private static readonly Lazy<MovementFactory> LazyInstance = new(static () => new MovementFactory());

    private CancellationTokenSource cancelMovementToken = new CancellationTokenSource();

    private MovementFactory()
    {
        move = new OverrideMovement();
        cam = new CameraUtil();
    }

    public static MovementFactory Instance => LazyInstance.Value;

    CameraUtil cam { get; set; } = null;
    OverrideMovement move { get; set; } = null;

    Vector3 DesiredPosition { get; set; } = new Vector3(0.0f, 0.0f, 0.0f);
    Angle DesiredRotation = new Angle();

    public void Initialize(Vector3 position, float rotation)
    {
        DesiredPosition = position;
        DesiredRotation = new Angle(rotation);
    }

    public void MoveTo(float x, float y, float z, float rot)
    {
        Vector3 newPos = new Vector3() { X = x, Y = y, Z = z };
        if (newPos == Api.ClientState.LocalPlayer.Position)
            return;
        DesiredPosition = newPos;
        DesiredRotation = new Angle(rot);
        Move();
    }

    public void Move()
    {
        FollowSystem.StopFollow();

        move.Precision = 0.05f;
        move.DesiredPosition = DesiredPosition;

        Api.Framework.RunOnTick(delegate
        {
            cancelMovementToken = new CancellationTokenSource();
            Task.Factory.StartNew(() => RunMoveTask(cancelMovementToken.Token), TaskCreationOptions.LongRunning);
        }, default(TimeSpan), 0, default(CancellationToken));
    }

    public void StopMovement()
    {
        cancelMovementToken.Cancel();
        cleanup();
    }

    public void cleanup()
    {
        if (cam != null)
            cam.Enabled = false;

        if (move != null)
            move.Enabled = false;
    }

    Vector3 last_pos;
    byte round = 0;
    private async Task RunMoveTask(CancellationToken token)
    {
        round = 4; //4 rounds until we give up
        last_pos = Api.ClientState.LocalPlayer.Position;

        if (!move.Enabled)
            move.Enabled = true;

        while (!token.IsCancellationRequested)
        {
            if (token.IsCancellationRequested)
                break;

            if (move != null)
                if (move.Enabled)
                {
                    // check if we stuck
                    var ldist = last_pos - Api.ClientState.LocalPlayer.Position;
                    ldist.Y = 0.0f;
                    if (ldist.LengthSquared() <= 0.2f * 0.2f)
                    {
                        if (round == 0)
                        {
                            cam.DesiredAzimuth = DesiredRotation;
                            cam.Enabled        = true;
                            move.Enabled       = false;
                            await Task.Delay(300, token).ContinueWith(static tsk => { }, token);
                            cam.Enabled = false;
                            break;
                        }
                        round -= 1;
                    }
                    else
                        round = 4; //4 rounds until we give up

                    //check if we reached our position
                    var dist = move.DesiredPosition - Api.ClientState.LocalPlayer.Position;
                    dist.Y = 0.0f;
                    if (dist.LengthSquared() <= move.Precision * move.Precision)
                    {
                        cam.DesiredAzimuth = DesiredRotation;
                        cam.Enabled        = true;
                        move.Enabled = false;

                        await Task.Delay(800, token).ContinueWith(static tsk => { }, token);
                        cam.Enabled = false;
                        break;
                    }
                }

            last_pos = Api.ClientState.LocalPlayer.Position;
            await Task.Delay(50, token).ContinueWith(static tsk => { }, token);
        }
        cleanup();
    }

    public void Dispose()
    {
        if (cam != null)
        {
            cam.Enabled = false;
            cam.Dispose();
            cam = null;
        }
        if (move != null)
        {
            move.Enabled = false;
            move.Dispose();
            move = null;
        }

    }
}