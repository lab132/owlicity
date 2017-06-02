using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Primitives2D;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using VelcroPhysics.Shared;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Factories;
using VelcroPhysics.DebugViews.MonoGame;
using VelcroPhysics.Extensions.DebugView;
using System.Linq;
using Microsoft.Xna.Framework.Media;

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
    // Has a length of <= 1.0f
    public Vector2 MovementVector;

    public bool WantsInteraction;
    public bool WantsAttack;
    public bool WantsPause;
  }

  public class InputHandler
  {
    private KeyboardState _prevKeyboard;
    private GamePadState[] _prevGamepad = new GamePadState[3];
    private MouseState _prevMouse;

    public void Update(float deltaSeconds,
      out GameInput characterInput, out GameInput companionInput, out GameInput debugInput)
    {
      KeyboardState newKeyboard = Keyboard.GetState();
      GamePadState[] newGamepad = new[]
      {
        GamePad.GetState(0),
        GamePad.GetState(1),
        GamePad.GetState(2),
      };
      MouseState newMouse = Mouse.GetState();

      Vector2 mouseDelta = Vector2.Zero;
      Vector2 timelessMouseDelta = Vector2.Zero;
      if(deltaSeconds > 0)
      {
        mouseDelta = (newMouse.Position - _prevMouse.Position).ToVector2();
        timelessMouseDelta = mouseDelta / deltaSeconds;
      }

      //
      // Character input
      //
      {
        characterInput = new GameInput();

        // Mouse
        Vector2 mouseMovement = Vector2.Zero;

        // Keyboard
        Vector2 keyboardMovement = new Vector2();
        if(newKeyboard.IsKeyDown(Keys.Left)) keyboardMovement.X -= 1.0f;
        if(newKeyboard.IsKeyDown(Keys.Right)) keyboardMovement.X += 1.0f;
        if(newKeyboard.IsKeyDown(Keys.Up)) keyboardMovement.Y -= 1.0f;
        if(newKeyboard.IsKeyDown(Keys.Down)) keyboardMovement.Y += 1.0f;
        if(newKeyboard.IsKeyDown(Keys.Space) && _prevKeyboard.IsKeyUp(Keys.Space)) characterInput.WantsAttack = true;
        if(newKeyboard.IsKeyDown(Keys.Enter) && _prevKeyboard.IsKeyUp(Keys.Enter)) characterInput.WantsInteraction = true;
        if(newKeyboard.IsKeyDown(Keys.Escape) && _prevKeyboard.IsKeyUp(Keys.Escape)) characterInput.WantsPause = true;

        // Gamepad
        if(newGamepad[0].IsButtonDown(Buttons.Y) && _prevGamepad[0].IsButtonUp(Buttons.Y)) characterInput.WantsAttack = true;
        if(newGamepad[0].IsButtonDown(Buttons.A) && _prevGamepad[0].IsButtonUp(Buttons.A)) characterInput.WantsInteraction = true;
        if(newGamepad[0].IsButtonDown(Buttons.Start) && _prevGamepad[0].IsButtonUp(Buttons.Start)) characterInput.WantsInteraction = true;

        Vector2 gamepadMovement = new Vector2(
          newGamepad[0].ThumbSticks.Left.X,
          -newGamepad[0].ThumbSticks.Left.Y);

        // Finalize
        characterInput.MovementVector = (keyboardMovement + gamepadMovement).GetClampedTo(1.0f) + mouseMovement;
      }

      //
      // Companion input
      //
      {
        companionInput = new GameInput();

        // Mouse
        Vector2 mouseMovement = Vector2.Zero;

        // Keyboard
        Vector2 keyboardMovement = new Vector2();

        // Gamepad
        if(newGamepad[1].IsButtonDown(Buttons.Y) && _prevGamepad[1].IsButtonUp(Buttons.Y)) companionInput.WantsAttack = true;
        if(newGamepad[1].IsButtonDown(Buttons.A) && _prevGamepad[1].IsButtonUp(Buttons.A)) companionInput.WantsInteraction = true;
        if(newGamepad[1].IsButtonDown(Buttons.Start) && _prevGamepad[1].IsButtonUp(Buttons.Start)) companionInput.WantsInteraction = true;

        Vector2 gamepadMovement = new Vector2(
          newGamepad[1].ThumbSticks.Left.X,
          -newGamepad[1].ThumbSticks.Left.Y);

        // Finalize
        companionInput.MovementVector = (keyboardMovement + gamepadMovement).GetClampedTo(1.0f) + mouseMovement;
      }

      //
      // Debug input
      //
      {
        debugInput = new GameInput();

        // Mouse
        Vector2 mouseMovement = timelessMouseDelta;

        // Keyboard
        Vector2 keyboardMovement = new Vector2();
        if(newKeyboard.IsKeyDown(Keys.A)) keyboardMovement.X -= 1.0f;
        if(newKeyboard.IsKeyDown(Keys.D)) keyboardMovement.X += 1.0f;
        if(newKeyboard.IsKeyDown(Keys.W)) keyboardMovement.Y -= 1.0f;
        if(newKeyboard.IsKeyDown(Keys.S)) keyboardMovement.Y += 1.0f;

        // Gamepad
        if(newGamepad[2].IsButtonDown(Buttons.Y) && _prevGamepad[2].IsButtonUp(Buttons.Y)) debugInput.WantsAttack = true;
        if(newGamepad[2].IsButtonDown(Buttons.A) && _prevGamepad[2].IsButtonUp(Buttons.A)) debugInput.WantsInteraction = true;
        if(newGamepad[2].IsButtonDown(Buttons.Start) && _prevGamepad[2].IsButtonUp(Buttons.Start)) debugInput.WantsInteraction = true;

        Vector2 gamepadMovement = new Vector2(
          newGamepad[0].ThumbSticks.Right.X,
          -newGamepad[0].ThumbSticks.Right.Y);

        // Finalize
        debugInput.MovementVector = (keyboardMovement + gamepadMovement).GetClampedTo(1.0f) + mouseMovement;
      }

      _prevKeyboard = newKeyboard;
      _prevGamepad = newGamepad;
      _prevMouse = newMouse;
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
    public DebugView PhysicsDebugView { get; set; }
    public InputHandler Input = new InputHandler();

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
      float y = MathHelper.Clamp(spatial.Transform.p.Y, 0, maxY);
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
      SpriteAnimationFactory.Initialize(Content);
      GameObjectFactory.Initialize();

      World = new World(gravity: Vector2.Zero);

      PhysicsDebugView = new DebugView(World);
      PhysicsDebugView.Flags = DebugViewFlags.Shape |
                               DebugViewFlags.PolygonPoints |
                               DebugViewFlags.AABB;

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

#if false
      {
        var go = new GameObject();
        var bc = new BodyComponent(go)
        {
          InitMode = BodyComponentInitMode.FromContent,
          BodyType = BodyType.Kinematic,
          ShapeContentName = "slurp_collision",
        };
        go.RootComponent = bc;

        var mv = new MovementComponent(go)
        {
          MaxMovementSpeed = 800.0f,
        };
        mv.ControlledBodyComponent = bc;

        var sa = new SpriteAnimationComponent(go)
        {
          AnimationTypes = new List<SpriteAnimationType>
          {
            SpriteAnimationType.Slurp_Idle
          }
        };
        sa.AttachTo(bc);

        var pe = new ParticleEmitterComponent(go)
        {
          NumParticlesPerTexture = 100,
          TextureContentNames = new[] { "particle" },
          AvailableColors = new[] { Color.White, Color.Red, Color.Green, },
        };
        //pe.Spatial.Transform.p += sa.
        pe.AttachTo(bc);

        AddGameObject(go);
      }
#endif

      {
        var go = GameObjectFactory.CreateKnown(GameObjectType.Owliver);
        go.Spatial.Transform.p += new Vector2(450, 600);
        AddGameObject(go);
        Global.Owliver = go;
      }
	  
	  {
        var go = GameObjectFactory.CreateKnown(GameObjectType.Camera);
        go.GetComponent<CameraComponent>().Bounds = GraphicsDevice.Viewport.Bounds.Size.ToVector2();

        go.Spatial.Transform.p = Global.Owliver.GetWorldSpatialData().Transform.p;

        ActiveCamera = go;
        AddGameObject(ActiveCamera);
	  }
      var testSlurp = GameObjectFactory.CreateKnown(GameObjectType.Slurp);
      testSlurp.Spatial.Transform.p += new Vector2(500, 450);
      AddGameObject(testSlurp);

      CurrentLevel.LoadContent();
      CurrentLevel.CullingCenter = Global.Owliver;

      var BackgroundMusic = Content.Load<Song>("snd/FiluAndDina_-_Video_Game_Background_-_Edit");
      MediaPlayer.IsRepeating = true;
      MediaPlayer.Play(BackgroundMusic);
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
      float deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

      Input.Update(deltaSeconds,
        out GameInput owliverInput,
        out GameInput companionInput,
        out GameInput debugInput);

      if(owliverInput.WantsPause)
      {
        Exit();
      }

      Global.Owliver.GetComponent<OwliverComponent>().Input = owliverInput;

      // TODO(manu): Make use of `compationInput`!

#if DEBUG
      ActiveCamera.GetComponent<MovementComponent>().Input = debugInput;
#endif

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
      while(simTime > _secondsPerSimStep)
      {
        World.Step(_secondsPerSimStep);
        simTime -= _secondsPerSimStep;
      }
      _excessSimTime = simTime;

      CurrentLevel.Update(deltaSeconds);

      // Post-physics update.
      foreach(GameObject go in GameObjects)
      {
        go.Update(deltaSeconds);
      }

      // Remove pending game objects.
      GameObjects.RemoveAll(go => GameObjectsPendingRemove.Contains(go));
      foreach(GameObject go in GameObjectsPendingRemove)
      {
        go.Deinitialize();
      }
      GameObjectsPendingRemove.Clear();

      base.Update(gameTime);
    }

    /// <summary>
    /// This is called when the game should draw itself.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    protected override void Draw(GameTime gameTime)
    {
      float deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

      GraphicsDevice.Clear(Color.CornflowerBlue);
      Camera cam = ActiveCamera.GetComponent<CameraComponent>().Camera;
      Matrix viewMatrix = cam.ViewMatrix;
      Matrix projectionMatrix = cam.ProjectionMatrix;

      batch.Begin(SpriteSortMode.BackToFront, null, null, null, null, null, viewMatrix);
      CurrentLevel.Draw(deltaSeconds, batch);

      foreach(GameObject go in GameObjects)
      {
        go.Draw(deltaSeconds, batch);
      }

      //dummy.Draw(batch);

      // Draw the origin of the world.
      int radius = 2;
      batch.FillRectangle(new Rectangle { X = -radius, Y = -radius, Width = 2 * radius, Height = 2 * radius }, Color.Lime);

      batch.End();

      batch.Begin(SpriteSortMode.BackToFront, null, null, null, null, null, viewMatrix);
      foreach(GameObject go in GameObjects)
      {
        go.DebugDraw(deltaSeconds, batch);
      }
      batch.End();

      PhysicsDebugView.RenderDebugData(ref projectionMatrix, ref viewMatrix);

      base.Draw(gameTime);
    }
  }
}