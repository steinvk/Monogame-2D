using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MonoGameLibrary.Input;

public class GamePadInfo
{
    private TimeSpan _vibrationTimeRemaining = TimeSpan.Zero;

    /// <summary>
    /// Gets the index of the player this gamepad is for.
    /// </summary>
    public PlayerIndex PlayerIndex { get; }

    /// <summary>
    /// Gets the state of input for this gamepad during the previous update cycle.
    /// </summary>
    public GamePadState PreviousState { get; private set; }


    /// <summary>
    /// Gets the state of input for this gamepad during the current update cycle.
    /// </summary>
    public GamePadState CurrentState { get; private set; }

    /// <summary>
    /// Gets a value that indicates if this gamepad is currently connected
    /// </summary>
    public bool IsConnected => CurrentState.IsConnected;

    /// <summary>
    /// Gets the value of the left thumbstick of this gamepad
    /// </summary>
    public Vector2 LeftThumbStick => CurrentState.ThumbSticks.Left;

    /// <summary>
    /// Gets the value of the right thumbstick of this gamepad
    /// </summary>
    public Vector2 RightThumbStick => CurrentState.ThumbSticks.Right;

    /// <summary>
    /// Gets the value of the left trigger of this gamepad
    /// </summary>
    public float LeftTrigger => CurrentState.Triggers.Left;

    /// <summary>
    /// Gets the value of the right trigger of this gamepad
    /// </summary>
    public float RightTrigger => CurrentState.Triggers.Right;


    public GamePadInfo(PlayerIndex playerIndex)
    {
        PlayerIndex = playerIndex;
        PreviousState = new GamePadState();
        CurrentState = GamePad.GetState(playerIndex);
    }

    public void Update(GameTime gameTime)
    {
        PreviousState = CurrentState;
        CurrentState = GamePad.GetState(PlayerIndex);

        if (_vibrationTimeRemaining > TimeSpan.Zero)
        {
            _vibrationTimeRemaining -= gameTime.ElapsedGameTime;

            if (_vibrationTimeRemaining <= TimeSpan.Zero)
            {
                StopVibration();
            }
        }
    }

    public bool IsButtonDown(Buttons button)
    {
        return CurrentState.IsButtonDown(button);
    }

    public bool IsButtonUp(Buttons button)
    {
        return CurrentState.IsButtonUp(button);
    }

    public bool WasButtonJustPressed(Buttons button)
    {
        return CurrentState.IsButtonDown(button) && PreviousState.IsButtonUp(button);
    }

    public bool WasButtonJustReleased(Buttons button)
    {
        return CurrentState.IsButtonUp(button) && PreviousState.IsButtonDown(button);
    }


    public void SetVibration(float strength, TimeSpan time)
    {
        _vibrationTimeRemaining = time;
        GamePad.SetVibration(PlayerIndex, strength, strength);
    }
    
    public void StopVibration()
    {
        GamePad.SetVibration(PlayerIndex, 0.0f, 0.0f);
    }
}
