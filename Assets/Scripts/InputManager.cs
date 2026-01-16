using UnityEngine;

public static class InputManager
{
    public static PlayerInputAction _inputActions;

    public static PlayerInputAction InputActions
    {
        get
        {
            if (_inputActions == null)
            {
                _inputActions = new PlayerInputAction();
                _inputActions.Player.Enable();
            }

            return _inputActions;
        }
    }

    private static void Cleanup()
    {
        if (_inputActions != null)
        {
            _inputActions.Player.Disable();
            _inputActions.Dispose();
            _inputActions = null;
        }
    }
}