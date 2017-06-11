using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using VelcroPhysics.DebugViews.MonoGame;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Extensions.DebugView;
using VelcroPhysics.Shared;

/*
  TODO:
    - Input "abstraction", but only a little bit.
    - Game object manager
    - Audio
*/

namespace Owlicity
{
  public enum GameLayer
  {
    SomewhereInTheMiddle,
    CloseToTheScreen,
    Background,

    Default = SomewhereInTheMiddle,
  }

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

      if(newKeyboard.WasKeyPressed(Keys.Escape, ref _prevKeyboard)) PlatformInput.WantsExit = true;
      if((newKeyboard.IsKeyDown(Keys.LeftAlt) || newKeyboard.IsKeyDown(Keys.RightAlt)) &&
        newKeyboard.WasKeyPressed(Keys.F4, ref _prevKeyboard))
      {
        PlatformInput.WantsExit = true;
      }


      _prevKeyboard = newKeyboard;
      _prevGamepad = newGamepad;
      _prevMouse = newMouse;
    }
  }

  public delegate void DebugDrawCommand(DebugView view);

  public enum PerformanceSlots
  {
    InputUpdate,
    WorldStep,

    Particles,

    COUNT
  }

  public class Performance
  {
    public int NumSlots { get; private set; }
    public int NumSamplesPerFrame { get; private set; }
    public Stopwatch[,] Samples;

    public int CurrentSampleIndex;

    public void BeginSample(PerformanceSlots slot)
    {
      Stopwatch sample = Samples[(int)slot, CurrentSampleIndex];
      Debug.Assert(!sample.IsRunning);
      sample.Restart();
    }

    public void EndSample(PerformanceSlots slot)
    {
      Stopwatch sample = Samples[(int)slot, CurrentSampleIndex];
      Debug.Assert(sample.IsRunning);
      sample.Stop();
    }

    public void Initialize(int numSlots, int numFramesToCapture)
    {
      NumSlots = numSlots;
      NumSamplesPerFrame = numFramesToCapture;
      Samples = new Stopwatch[NumSlots, NumSamplesPerFrame];
      for(int slotIndex = 0; slotIndex < NumSlots; slotIndex++)
      {
        for(int sampleIndex = 0; sampleIndex < NumSamplesPerFrame; sampleIndex++)
        {
          Samples[slotIndex, sampleIndex] = new Stopwatch();
        }
      }

      CurrentSampleIndex = 0;
    }

    public void AdvanceFrame()
    {
      CurrentSampleIndex++;
      if(CurrentSampleIndex >= NumSamplesPerFrame)
      {
        CurrentSampleIndex = 0;
      }
    }
  }

  /// <summary>
  /// This is the main type for your game.
  /// </summary>
  public class OwlGame : Game
  {
    GraphicsDeviceManager graphics;
    SpriteBatch batch;

    public GameObject ActiveCamera;
    public Level CurrentLevel;

    public World World { get; set; }
    public InputHandler Input = new InputHandler();

    public GameObject Owliver { get; set; }

    public bool MainDrawingEnabled = true;

    public bool DebugDrawingEnabled;
    public List<DebugDrawCommand> DebugDrawCommands { get; } = new List<DebugDrawCommand>();

    public DebugView PhysicsDebugView { get; set; }

    public int CurrentFrameIndex { get; private set; }

    public Performance Perf { get; } = new Performance();

    //
    // Game object stuff
    //
    public List<GameObject> GameObjects = new List<GameObject>();
    public List<GameObject> GameObjectsPendingAdd = new List<GameObject>();
    public List<GameObject> GameObjectsPendingRemove = new List<GameObject>();

    public void AddGameObject(GameObject go)
    {
      Debug.Assert(!GameObjects.Contains(go));
      Debug.Assert(!GameObjectsPendingAdd.Contains(go));

      if(!GameObjectsPendingRemove.Contains(go))
      {
        GameObjectsPendingAdd.Add(go);
      }
    }

    public void RemoveGameObject(GameObject go)
    {
      Debug.Assert(GameObjects.Contains(go));
      Debug.Assert(GameObjectsPendingRemove.Contains(go));

      if(!GameObjectsPendingAdd.Remove(go))
      {
        GameObjectsPendingRemove.Add(go);
      }
    }

    public float CalcDepth(SpatialData spatial, GameLayer layer)
    {
      // Note(manu): 0 means front, 1 means back.
      float maxY = Level.SCREEN_DIMENSION * CurrentLevel.ScreenTileHeight;
      float y = MathHelper.Clamp(spatial.Position.Y, 0, maxY);
      float alpha = y / maxY;

      float depth;
      switch(layer)
      {
        case GameLayer.CloseToTheScreen:
        depth = MathHelper.Lerp(0.0999f, 0.0f, alpha);
        break;

        case GameLayer.SomewhereInTheMiddle:
        depth = MathHelper.Lerp(0.9f, 0.1f, alpha);
        break;

        case GameLayer.Background:
        depth = 1.0f;
        break;

        default: throw new ArgumentException("layer");
      }

      return depth;
    }

    public OwlGame()
    {
      Debug.Assert(Global.Game == null);
      Global.Game = this;

      graphics = new GraphicsDeviceManager(this);
      graphics.PreferredBackBufferHeight = 1080;
      graphics.PreferredBackBufferWidth = 1920;

#if DEBUG
      if(Environment.UserName == "manu")
      {
        // Note(manu): Because I have a really tiny screen...
        graphics.PreferredBackBufferHeight = (int)(0.5f * 1080);
        graphics.PreferredBackBufferWidth = (int)(0.5f * 1920);
      }
#endif

      Content.RootDirectory = "content";
    }

    /// <summary>
    /// Allows the game to perform any initialization it needs to before starting to run.
    /// This is where it can query for any required services and load any non-graphic
    /// related content.  Calling base.Initialize will enumerate through any components
    /// and initialize them as well.
    /// </summary>
    protected override void Initialize()
    {
      Perf.Initialize((int)PerformanceSlots.COUNT, 120);

#if DEBUG
      DebugDrawingEnabled = true;
#endif

      SpriteAnimationFactory.Initialize(Content);
      GameObjectFactory.Initialize();

      World = new World(gravity: Vector2.Zero);

      PhysicsDebugView = new DebugView(World)
      {
        Flags = DebugViewFlags.Shape |
                               DebugViewFlags.PolygonPoints |
                               DebugViewFlags.AABB
      };

      base.Initialize();
    }

    /// <summary>
    /// LoadContent will be called once per game and is the place to load
    /// all of your content.
    /// </summary>
    protected override void LoadContent()
    {
      // Create a new SpriteBatch, which can be used to draw textures.
      batch = new SpriteBatch(GraphicsDevice);

      PhysicsDebugView.LoadContent(GraphicsDevice, Content);

      CurrentLevel = new Level(Content)
      {
        ContentNameFormat_Ground = "level01/level1_ground_{0}{1}",
        ContentNameFormat_Collision = "level01/static_collision/static_collision_{0}{1}",
        ContentNameFormat_Layout = "level01/layout/layout_{0}{1}",
      };

      for(int x = 0; x < 4; x++)
      {
        for(int y = 0; y < 7; y++)
        {
          CurrentLevel.CreateScreen(x, y);
        }
      }

      CurrentLevel.LoadContent();

      {
        Owliver = GameObjectFactory.CreateKnown(GameObjectType.Owliver);
        Owliver.Spatial.Position += Global.ToMeters(450, 600);
        AddGameObject(Owliver);

        CurrentLevel.CullingCenter = Owliver;
      }

      {
        ActiveCamera = GameObjectFactory.CreateKnown(GameObjectType.Camera);

        var cc = ActiveCamera.GetComponent<CameraComponent>();
        Vector2 camExtents = 0.5f * Global.ToMeters(GraphicsDevice.Viewport.Bounds.Size.ToVector2());
        cc.Spatial.LocalAABB = new AABB
        {
          LowerBound = -camExtents,
          UpperBound = camExtents,
        };
        cc.VisibilityBounds = CurrentLevel.LevelBounds;

        var mc = ActiveCamera.GetComponent<MovementComponent>();
        mc.MaxMovementSpeed = 5.0f;

        var sac = ActiveCamera.GetComponent<SpringArmComponent>();
        sac.Target = Owliver;

        AddGameObject(ActiveCamera);
      }

      {
        var testSlurp = GameObjectFactory.CreateKnown(GameObjectType.Slurp);
        testSlurp.Spatial.Position += Global.ToMeters(500, 450);
        AddGameObject(testSlurp);
      }

      {
        var BackgroundMusic = Content.Load<Song>("snd/FiluAndDina_-_Video_Game_Background_-_Edit");
        MediaPlayer.IsRepeating = true;
        MediaPlayer.Play(BackgroundMusic);
      }
    }

    /// <summary>
    /// UnloadContent will be called once per game and is the place to unload
    /// game-specific content.
    /// </summary>
    protected override void UnloadContent()
    {
      // TODO: Unload any non ContentManager content here
    }

    float _secondsPerSimStep = 1.0f / 60.0f;
    float _excessSimTime;

    /// <summary>
    /// Allows the game to run logic such as updating the world,
    /// checking for collisions, gathering input, and playing audio.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    protected override void Update(GameTime gameTime)
    {
      CurrentFrameIndex++;
      DebugDrawCommands.Clear();

      float deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

      Perf.BeginSample(PerformanceSlots.InputUpdate);
      Input.Update(deltaSeconds);
      Perf.EndSample(PerformanceSlots.InputUpdate);

      if(Input.PlatformInput.WantsExit)
      {
        Exit();
      }

#if DEBUG
      deltaSeconds *= Input.DebugInput.SpeedMultiplier;

      {
        var mv = ActiveCamera.GetComponent<MovementComponent>();
        //mv.Input = Input.DebugInput;
        mv.MovementVector = Input.DebugMovement;
      }

      if(Input.DebugInput.ToggleMainDrawing)
        MainDrawingEnabled = !MainDrawingEnabled;

      if(Input.DebugInput.ToggleDebugDrawing)
        DebugDrawingEnabled = !DebugDrawingEnabled;

      if(Input.DebugInput.TogglePhysicsDebugView)
        PhysicsDebugView.Enabled = !PhysicsDebugView.Enabled;

      if(Input.DebugInput.ToggleCameraVisibilityBounds)
      {
        var cc = ActiveCamera.GetComponent<CameraComponent>();
        if(cc.VisibilityBounds == null)
        {
          cc.VisibilityBounds = CurrentLevel.LevelBounds;
        }
        else
        {
          cc.VisibilityBounds = null;
        }
      }

      if(Input.DebugInput.ResetCameraPosition)
      {
        ActiveCamera.Spatial.Position = Vector2.Zero;
      }
#endif

      OwliverComponent oc = Owliver.GetComponent<OwliverComponent>();
      oc.MovementVector = Input.CharacterMovement;
      oc.Input = Input.CharacterInput;

      // TODO(manu): Make use of `Input.CompationInput`!

      // Add pending game objects.
      GameObjects.AddRange(GameObjectsPendingAdd);
      foreach(GameObject go in GameObjectsPendingAdd)
      {
        go.Initialize();
      }
      GameObjectsPendingAdd.Clear();

      // Execute pre-physics update.
      foreach(GameObject go in GameObjects)
      {
        go.PrePhysicsUpdate(deltaSeconds);
      }

      // Physics simulation
      float simTime = _excessSimTime + deltaSeconds;
      Perf.BeginSample(PerformanceSlots.WorldStep);
      while(simTime > _secondsPerSimStep)
      {
        World.Step(_secondsPerSimStep);
        simTime -= _secondsPerSimStep;
      }
      Perf.EndSample(PerformanceSlots.WorldStep);
      _excessSimTime = simTime;

      CurrentLevel.Update(deltaSeconds);

      // Post-physics update.
      foreach(GameObject go in GameObjects)
      {
        go.Update(deltaSeconds);
      }

      // Remove pending game objects.
      GameObjects.RemoveAll(go => GameObjectsPendingRemove.Contains(go));
      // TODO(manu): Deinitialize game object?
      //foreach(GameObject go in GameObjectsPendingRemove)
      //{
      //  go.Deinitialize();
      //}
      GameObjectsPendingRemove.Clear();

      base.Update(gameTime);

      Perf.AdvanceFrame();
    }

    /// <summary>
    /// This is called when the game should draw itself.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    protected override void Draw(GameTime gameTime)
    {
      base.Draw(gameTime);

      GraphicsDevice.Clear(Color.CornflowerBlue);
      Camera cam = ActiveCamera.GetComponent<CameraComponent>().Camera;

      if(MainDrawingEnabled)
      {
        batch.Begin(sortMode: SpriteSortMode.BackToFront, effect: cam.Effect);

        foreach(GameObject go in GameObjects)
        {
          go.Draw(batch);
        }

        batch.End();
      }

      if(DebugDrawingEnabled)
      {
        PhysicsDebugView.BeginCustomDraw(ref cam.ProjectionMatrix, ref cam.ViewMatrix);

        foreach(DebugDrawCommand drawCommand in DebugDrawCommands)
        {
          drawCommand(PhysicsDebugView);
        }

        for(int slot = 0; slot < Perf.NumSlots; slot++)
        {
          TimeSpan slotMin = TimeSpan.MaxValue;
          TimeSpan slotMax = TimeSpan.MinValue;
          TimeSpan slotAvg = TimeSpan.Zero;
          for(int sampleIndex = 0; sampleIndex < Perf.NumSamplesPerFrame; sampleIndex++)
          {
            TimeSpan sampleValue = Perf.Samples[slot, sampleIndex].Elapsed;
            if(sampleValue < slotMin) slotMin = sampleValue;
            if(sampleValue > slotMax) slotMax = sampleValue;
            slotAvg += sampleValue;
          }
          slotAvg = new TimeSpan(ticks: slotAvg.Ticks / Perf.NumSamplesPerFrame);

          Vector2 pos = new Vector2(20, 30 * slot + 20);
          Func<TimeSpan, string> f = ts => $"{ts.TotalMilliseconds.ToString("N04", System.Globalization.CultureInfo.InvariantCulture)}ms";
          PhysicsDebugView.DrawString(pos, $"{(PerformanceSlots)slot}: min {f(slotMin)} | max {f(slotMax)} | avg {f(slotAvg)}");
        }

        PhysicsDebugView.EndCustomDraw();
      }

      PhysicsDebugView.RenderDebugData(ref cam.ProjectionMatrix, ref cam.ViewMatrix);
    }
  }
}