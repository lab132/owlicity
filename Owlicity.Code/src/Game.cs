using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

  public delegate void DebugDrawCommand(DebugView view);

  /// <summary>
  /// This is the main type for your game.
  /// </summary>
  public class OwlGame : Game
  {
    GraphicsDeviceManager graphics;
    public static readonly Point FullHD = new Point(1920, 1080);
    public static readonly Point DebugResolution = (0.8f * FullHD.ToVector2()).ToPoint();

    public Renderer WorldRenderer = new Renderer { BaseDepth = -1, BaseScale = Conversion.RenderScale, };
    public Renderer UIRenderer = new Renderer { BaseDepth = 0, BaseScale = 1.0f, };

    public CameraObject ActiveCamera;
    public Level CurrentLevel;

    public World World { get; set; }
    public InputHandler Input = new InputHandler();

    public Owliver Owliver { get; set; }

    public bool MainDrawingEnabled = true;

    public bool DebugDrawingEnabled;
    public List<DebugDrawCommand> DebugDrawCommands { get; } = new List<DebugDrawCommand>();

    public DebugView PhysicsDebugView { get; set; }

    public bool HudEnabled = true;
    public OwlHud Hud = new OwlHud();

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
      if(!GameObjectsPendingAdd.Contains(go))
      {
        GameObjectsPendingAdd.Add(go);
      }
    }

    public void RemoveGameObject(GameObject go)
    {
      if(!GameObjectsPendingAdd.Remove(go) && !GameObjectsPendingRemove.Contains(go))
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

      graphics = new GraphicsDeviceManager(this)
      {
        HardwareModeSwitch = false, // Use "borderless fullscreen window" (I guess?)
        SynchronizeWithVerticalRetrace = true,

#if DEBUG
        // Note(manu): Can't debug properly in fullscreen.
        IsFullScreen = false,
        PreferredBackBufferWidth = DebugResolution.X,
        PreferredBackBufferHeight = DebugResolution.Y,
#else
        IsFullScreen = true,
        PreferredBackBufferWidth = FullHD.X,
        PreferredBackBufferHeight = FullHD.Y,
#endif
      };

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

      SpriteAnimationFactory.Initialize(Content);
      GameObjectFactory.Initialize();

      World = new World(gravity: Vector2.Zero);

      PhysicsDebugView = new DebugView(World)
      {
        Flags = (DebugViewFlags)int.MaxValue,
        Enabled = false,
      };

      base.Initialize();

#if DEBUG
      Window.AllowUserResizing = true;
#endif
    }

    /// <summary>
    /// LoadContent will be called once per game and is the place to load
    /// all of your content.
    /// </summary>
    protected override void LoadContent()
    {
      WorldRenderer.Initialize(GraphicsDevice);
      UIRenderer.Initialize(GraphicsDevice);

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

      {
        Owliver = new Owliver();
        Owliver.Spatial.Position += Conversion.ToMeters(450, 600);
        AddGameObject(Owliver);

        CurrentLevel.CullingCenter = Owliver;
      }

      CurrentLevel.LoadContent();

      {
        ActiveCamera = new CameraObject();

        ActiveCamera.CameraComponent.VisibilityBounds = CurrentLevel.LevelBounds;
        ActiveCamera.CameraComponent.OnGraphicsDeviceReset(GraphicsDevice);
        Window.ClientSizeChanged += (o, e) =>
        {
          ActiveCamera.CameraComponent.OnGraphicsDeviceReset(GraphicsDevice);
        };

        ActiveCamera.SpringArm.BeforeInitialize += () =>
        {
          ActiveCamera.SpringArm.Target = Owliver;
        };

        AddGameObject(ActiveCamera);
      }

#if true
      {
        var testSlurp = GameObjectFactory.CreateKnown(KnownGameObject.Slurp);
        testSlurp.Spatial.Position += Conversion.ToMeters(300, 250);
        AddGameObject(testSlurp);
      }

      {
        var testTankton = new Tankton();
        testTankton.Spatial.Position += Conversion.ToMeters(900, 350);
        AddGameObject(testTankton);
      }

      {
        Random rand = new Random();

        Global.SpawnGameObjectsInRingFormation(
          center: Conversion.ToMeters(2400, 500),
          radius: 2.5f,
          numToSpawn: 15,
          rand: rand,
          types: new[] { KnownGameObject.Bonbon_Gold, KnownGameObject.Bonbon_Red });

        Global.SpawnGameObjectsInRingFormation(
          center: Conversion.ToMeters(2400, 500),
          radius: 1.0f,
          numToSpawn: 5,
          rand: rand,
          types: new[] { KnownGameObject.Bonbon_Gold, KnownGameObject.Bonbon_Red });
      }

      {
        var testKey = GameObjectFactory.CreateKnown(KnownGameObject.Key_Gold);
        testKey.Spatial.Position += Conversion.ToMeters(700, 720);
        AddGameObject(testKey);
      }

      {
        var testGate = GameObjectFactory.CreateKnown(KnownGameObject.Gate);
        testGate.Spatial.Position += Conversion.ToMeters(2300, 1100);
        AddGameObject(testGate);
      }

      {
        var testShop = new Shop();
        testShop.Spatial.Position += Conversion.ToMeters(1300, 500);
        AddGameObject(testShop);

        var testFruitBowl = GameObjectFactory.CreateKnown(KnownGameObject.ShopItem_FruitBowl);
        testFruitBowl.GetComponent<ShopItemComponent>().Price = ShopItemPriceType._20;
        testFruitBowl.AttachTo(testShop);
        testFruitBowl.Spatial.Position += Conversion.ToMeters(-110.0f, -90.0f);
        AddGameObject(testFruitBowl);

        var testRod = GameObjectFactory.CreateKnown(KnownGameObject.ShopItem_FishingRod);
        testRod.GetComponent<ShopItemComponent>().Price = ShopItemPriceType._100;
        testRod.AttachTo(testShop);
        testRod.Spatial.Position += Conversion.ToMeters(128.0f, -80.0f);
        AddGameObject(testRod);
      }
#endif

      {
        var BackgroundMusic = Content.Load<Song>("snd/FiluAndDina_-_Video_Game_Background_-_Edit");
        MediaPlayer.IsRepeating = true;
#if false
        MediaPlayer.Play(BackgroundMusic);
#endif
      }

      Hud.Initialize();
      Hud.ResetLayout(GraphicsDevice.Viewport.Bounds);
      Window.ClientSizeChanged += (o, e) =>
      {
        Hud.ResetLayout(GraphicsDevice.Viewport.Bounds);
      };
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

      if(Input.PlatformInput.ToggleFullscreen)
      {
        graphics.ToggleFullScreen();
      }

#if DEBUG
      deltaSeconds *= Input.DebugInput.SpeedMultiplier;

      {
        ActiveCamera.Movement.MovementVector = Input.DebugMovement;
      }

      if(Input.DebugInput.ToggleMainDrawing)
        MainDrawingEnabled = !MainDrawingEnabled;

      if(Input.DebugInput.ToggleDebugDrawing)
        DebugDrawingEnabled = !DebugDrawingEnabled;

      if(Input.DebugInput.TogglePhysicsDebugView)
        PhysicsDebugView.Enabled = !PhysicsDebugView.Enabled;

      if(Input.DebugInput.ToggleCameraVisibilityBounds)
      {
        if(ActiveCamera.CameraComponent.VisibilityBounds == null)
        {
          ActiveCamera.CameraComponent.VisibilityBounds = CurrentLevel.LevelBounds;
        }
        else
        {
          ActiveCamera.CameraComponent.VisibilityBounds = null;
        }

        if(ActiveCamera.SpringArm.Target == null)
        {
          ActiveCamera.SpringArm.Target = Owliver;
        }
        else
        {
          ActiveCamera.SpringArm.Target = null;
        }
      }

      if(Input.DebugInput.ResetCameraPosition)
      {
        ActiveCamera.Spatial.Position = Owliver.GetWorldSpatialData().Position;
      }
#endif

      {
        Owliver.Movement.MovementVector = Input.CharacterMovement;
        Owliver.Input = Input.CharacterInput;
      }

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

        AABB aabb = Global.CreateInvalidAABB();
        foreach(SpatialComponent sc in go.GetComponents<SpatialComponent>())
        {
          AABB other = sc.GetWorldSpatialData().AbsoluteAABB;
          aabb.Combine(ref other);
        }

        DebugDrawCommands.Add(view => view.DrawPoint(aabb.Center, Conversion.ToMeters(3), Color.MonoGameOrange));
        DebugDrawCommands.Add(view => view.DrawAABB(ref aabb, Color.Lime));
      }

      // Remove pending game objects.
      GameObjects.RemoveAll(go => GameObjectsPendingRemove.Contains(go));
      foreach(GameObject go in GameObjectsPendingRemove)
      {
        go.Destroy();
      }
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
      float deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
      base.Draw(gameTime);

      GraphicsDevice.Clear(Color.CornflowerBlue);
      CameraData cam = ActiveCamera.CameraComponent.CamData;
      Matrix viewMatrix = cam.Effect.View;
      Matrix projectionMatrix = cam.Effect.Projection;

      if(MainDrawingEnabled)
      {
        WorldRenderer.Begin(sortMode: SpriteSortMode.BackToFront, effect: cam.Effect);

        foreach(GameObject go in GameObjects)
        {
          go.Draw(WorldRenderer);
        }

        WorldRenderer.End();
      }

      if(HudEnabled)
      {
        UIRenderer.Begin(SpriteSortMode.Texture);
        Hud.Draw(UIRenderer, deltaSeconds);
        UIRenderer.End();
      }

      if(DebugDrawingEnabled)
      {
        PhysicsDebugView.BeginCustomDraw(ref projectionMatrix, ref viewMatrix);

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

      PhysicsDebugView.RenderDebugData(ref projectionMatrix, ref viewMatrix);
    }
  }
}
