using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Linq;

namespace Owlicity
{
  public struct GameInput
  {
    public bool WantsInteraction;
    public bool WantsAttack;
    public bool WantsPause;

    public void Reset()
    {
      this = new GameInput();
    }
  }
  
  public struct DebugInput
  {
    public float SpeedMultiplier;
    public bool ToggleMainDrawing;
    public bool ToggleDebugDrawing;
    public bool TogglePhysicsDebugView;
    public bool ToggleCameraVisibilityBounds;
    public bool ResetCameraPosition;

    public void Reset()
    {
      float preservedSpeedMultiplier = SpeedMultiplier;
      this = new DebugInput();
      SpeedMultiplier = preservedSpeedMultiplier;
    }
  }

  public struct PlatformInput
  {
    public bool WantsExit;
    public bool ToggleFullscreen;

    public void Reset()
    {
      this = new PlatformInput();
    }
  }

  public class InputHandler
  {
    public const int NUM_SUPPORTED_GAMEPADS = 2;

    private KeyboardState _prevKeyboard;
    private MouseState _prevMouse;
    private GamePadState[] _prevGamepad = new GamePadState[NUM_SUPPORTED_GAMEPADS];

    public Vector2 MouseSensitivity = Vector2.One;
    public Vector2[] LeftThumbstickSensitivity = Enumerable.Repeat(new Vector2(1, -1), NUM_SUPPORTED_GAMEPADS).ToArray();
    public Vector2[] RightThumbstickSensitivity = Enumerable.Repeat(new Vector2(1, -1), NUM_SUPPORTED_GAMEPADS).ToArray();

    public GameInput CharacterInput;
    public Vector2 CharacterMovement;

    public GameInput CompanionInput;
    public Vector2 CompanionMovement;

    public PlatformInput PlatformInput;

    public DebugInput DebugInput = new DebugInput { SpeedMultiplier = 1.0f };
    public Vector2 DebugMovement;


    public void Update(float deltaSeconds)
    {
      KeyboardState newKeyboard = Keyboard.GetState();
      MouseState newMouse = Mouse.GetState();
      GamePadState[] newGamepad = new GamePadState[NUM_SUPPORTED_GAMEPADS];
      for(int gamepadIndex = 0; gamepadIndex < NUM_SUPPORTED_GAMEPADS; gamepadIndex++)
      {
        newGamepad[gamepadIndex] = GamePad.GetState(gamepadIndex);
      }

      Vector2 mouseDelta = Vector2.Zero;
      Vector2 timelessMouseDelta = Vector2.Zero;
      if(deltaSeconds > 0)
      {
        mouseDelta = (newMouse.Position - _prevMouse.Position).ToVector2() * MouseSensitivity;
        timelessMouseDelta = mouseDelta / deltaSeconds;
      }

      //
      // Character input
      //
      {
        // Reset
        CharacterInput.Reset();
        CharacterMovement = Vector2.Zero;

        // Mouse
        Vector2 mouseMovement = Vector2.Zero;

        // Keyboard
        Vector2 keyboardMovement = new Vector2();
        if(newKeyboard.IsKeyDown(Keys.Left)) keyboardMovement.X -= 1.0f;
        if(newKeyboard.IsKeyDown(Keys.Right)) keyboardMovement.X += 1.0f;
        if(newKeyboard.IsKeyDown(Keys.Up)) keyboardMovement.Y -= 1.0f;
        if(newKeyboard.IsKeyDown(Keys.Down)) keyboardMovement.Y += 1.0f;
        if(newKeyboard.WasKeyPressed(Keys.Space, ref _prevKeyboard)) CharacterInput.WantsAttack = true;
        if(newKeyboard.WasKeyPressed(Keys.Enter, ref _prevKeyboard)) CharacterInput.WantsInteraction = true;
        if(newKeyboard.WasKeyPressed(Keys.Escape, ref _prevKeyboard)) CharacterInput.WantsPause = true;

        // Gamepad
        const int padIndex = 0;
        Vector2 gamepadMovement = newGamepad[padIndex].ThumbSticks.Left * LeftThumbstickSensitivity[padIndex];
        if(newGamepad[padIndex].WasButtonPressed(Buttons.X, ref _prevGamepad[padIndex])) CharacterInput.WantsAttack = true;
        if(newGamepad[padIndex].WasButtonPressed(Buttons.A, ref _prevGamepad[padIndex])) CharacterInput.WantsInteraction = true;
        if(newGamepad[padIndex].WasButtonPressed(Buttons.Start, ref _prevGamepad[padIndex])) CharacterInput.WantsInteraction = true;

        // Finalize
        CharacterMovement = (keyboardMovement + gamepadMovement).GetClampedTo(1.0f) + mouseMovement;
      }

      //
      // Companion input
      //
      {
        CompanionInput.Reset();
        CompanionMovement = Vector2.Zero;

        // Mouse
        Vector2 mouseMovement = Vector2.Zero;

        // Keyboard
        Vector2 keyboardMovement = new Vector2();

        // Gamepad
        const int padIndex = 1;
        Vector2 gamepadMovement = newGamepad[padIndex].ThumbSticks.Left * LeftThumbstickSensitivity[padIndex];
        if(newGamepad[padIndex].IsButtonDown(Buttons.Y) && _prevGamepad[padIndex].IsButtonUp(Buttons.Y)) CompanionInput.WantsAttack = true;
        if(newGamepad[padIndex].IsButtonDown(Buttons.A) && _prevGamepad[padIndex].IsButtonUp(Buttons.A)) CompanionInput.WantsInteraction = true;
        if(newGamepad[padIndex].IsButtonDown(Buttons.Start) && _prevGamepad[padIndex].IsButtonUp(Buttons.Start)) CompanionInput.WantsInteraction = true;

        // Finalize
        CompanionMovement = (keyboardMovement + gamepadMovement).GetClampedTo(1.0f) + mouseMovement;
      }

      //
      // Debug input
      //
      {
        DebugInput.Reset();
        DebugMovement = Vector2.Zero;

        // Mouse
        Vector2 mouseMovement = Vector2.Zero;
        //mouseMovement = timelessMouseDelta;

        // Keyboard
        Vector2 keyboardMovement = new Vector2();
        if(newKeyboard.IsKeyDown(Keys.A)) keyboardMovement.X -= 1.0f;
        if(newKeyboard.IsKeyDown(Keys.D)) keyboardMovement.X += 1.0f;
        if(newKeyboard.IsKeyDown(Keys.W)) keyboardMovement.Y -= 1.0f;
        if(newKeyboard.IsKeyDown(Keys.S)) keyboardMovement.Y += 1.0f;

        if(newKeyboard.WasKeyPressed(Keys.F1, ref _prevKeyboard)) DebugInput.ToggleMainDrawing = true;
        if(newKeyboard.WasKeyPressed(Keys.F2, ref _prevKeyboard)) DebugInput.ToggleDebugDrawing = true;
        if(newKeyboard.WasKeyPressed(Keys.F3, ref _prevKeyboard)) DebugInput.TogglePhysicsDebugView = true;
        if(newKeyboard.WasKeyPressed(Keys.F4, ref _prevKeyboard)) DebugInput.ToggleCameraVisibilityBounds = true;

        if(newKeyboard.WasKeyPressed(Keys.D1, ref _prevKeyboard)) DebugInput.SpeedMultiplier -= 0.5f;
        if(newKeyboard.WasKeyPressed(Keys.D2, ref _prevKeyboard)) DebugInput.SpeedMultiplier += 0.5f;
        if(newKeyboard.WasKeyPressed(Keys.D3, ref _prevKeyboard))
        {
          DebugInput.SpeedMultiplier = 1.0f;
        }
        else
        {
          DebugInput.SpeedMultiplier = MathHelper.Clamp(DebugInput.SpeedMultiplier, min: 0.1f, max: 10.0f);
        }
        if(newKeyboard.WasKeyPressed(Keys.D4, ref _prevKeyboard)) DebugInput.ResetCameraPosition = true;


        // Gamepad
        const int padIndex = 0;
        Vector2 gamepadMovement = newGamepad[padIndex].ThumbSticks.Right * RightThumbstickSensitivity[padIndex];

        // Finalize
        DebugMovement = (keyboardMovement + gamepadMovement).GetClampedTo(1.0f) + mouseMovement;
      }

      //
      // Platform input
      //
      PlatformInput.Reset();

      bool isAltDown = newKeyboard.IsKeyDown(Keys.LeftAlt) || newKeyboard.IsKeyDown(Keys.RightAlt);

      if(newKeyboard.WasKeyPressed(Keys.Escape, ref _prevKeyboard)) PlatformInput.WantsExit = true;
      if(isAltDown && newKeyboard.WasKeyPressed(Keys.F4, ref _prevKeyboard))
      {
        PlatformInput.WantsExit = true;
      }

      if(isAltDown && newKeyboard.WasKeyPressed(Keys.Enter, ref _prevKeyboard))
      {
        PlatformInput.ToggleFullscreen = true;
      }


      _prevKeyboard = newKeyboard;
      _prevGamepad = newGamepad;
      _prevMouse = newMouse;
    }
  }

}
