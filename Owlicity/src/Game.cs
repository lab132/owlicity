using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Primitives2D;
using Owlicity.src;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using VelcroPhysics.Shared;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Factories;
using VelcroPhysics.DebugViews.MonoGame;
using VelcroPhysics.Extensions.DebugView;

/*
  TODO:
    - Input "abstraction", but only a little bit.
    - Game object manager
    - Audio
*/

namespace Owlicity
{
  public class Dummy : ITransformable
  {
    public Transform LocalTransform { get; } = new Transform();
    public SpriteAnimationInstance anim;
    public Transform animOffset;
    public Body body;

    public void Initialize()
    {
      animOffset = new Transform
      {
        Parent = this,
        Position = -0.5f * anim.Data.TileDim.ToVector2(),
        Depth = 0.5f,
      };

      body = new Body(OwlicityGame.Instance.World);
    }

    public void LoadContent()
    {
      Vertices collisionVertices = OwlicityGame.Instance.Content.Load<Vertices>("slurp_collision");
      FixtureFactory.AttachLoopShape(collisionVertices, body);
    }
    
    public void Update(GameTime dt)
    {
      anim.Update(dt);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
      anim.Draw(spriteBatch, animOffset.GetWorldTransform());
    }
  }

  /// <summary>
  /// This is the main type for your game.
  /// </summary>
  public class OwlicityGame : Game
  {
    public static OwlicityGame Instance { get; set; }

    GraphicsDeviceManager graphics;
    SpriteBatch spriteBatch;

    SpriteAnimationData testAnimation;
    Dummy dummy;

    Camera cam;
    Level testLevel;

    public World World { get; set; }
    public DebugView PhysicsDebugView { get; set; }


    public OwlicityGame()
    {
      Debug.Assert(Instance == null);
      Instance = this;

      graphics = new GraphicsDeviceManager(this);
      Content.RootDirectory = "content";
      graphics.PreferredBackBufferHeight = 1080;
      graphics.PreferredBackBufferWidth = 1920;
    }

    /// <summary>
    /// Allows the game to perform any initialization it needs to before starting to run.
    /// This is where it can query for any required services and load any non-graphic
    /// related content.  Calling base.Initialize will enumerate through any components
    /// and initialize them as well.
    /// </summary>
    protected override void Initialize()
    {
      cam = new Camera
      {
        //LookAt = dummy,
        Bounds = GraphicsDevice.Viewport.Bounds.Size.ToVector2()
      };
      cam.Initialize();

      World = new World(gravity: Vector2.Zero);

      PhysicsDebugView = new DebugView(World);
      PhysicsDebugView.Flags = DebugViewFlags.Shape | DebugViewFlags.PolygonPoints | DebugViewFlags.AABB;

      base.Initialize();
    }

    /// <summary>
    /// LoadContent will be called once per game and is the place to load
    /// all of your content.
    /// </summary>
    protected override void LoadContent()
    {
      // Create a new SpriteBatch, which can be used to draw textures.
      spriteBatch = new SpriteBatch(GraphicsDevice);

      PhysicsDebugView.LoadContent(GraphicsDevice, Content);

      // TODO: use this.Content to load your game content here
      Texture2D atlas = Content.Load<Texture2D>("owliver_walk_front_left_spritesheet");
      testAnimation = SpriteAnimationData.FromAtlas(atlas, 3, 256, 256);
      testAnimation.SecondsPerFrame = 0.05f;

      dummy = new Dummy
      {
        anim = testAnimation.CreateInstance(),
      };
      dummy.anim.PingPong = true;
      dummy.Initialize();
      dummy.LoadContent();

      testLevel = new Level(Content);

      for (uint i=0; i < 4; i++)
      {
        for (uint j = 0; j < 7; j++)
        {
          var screen = new Screen();
          screen.AssetName = $"level01/level1_ground_{j}{i}";
          testLevel.addScreen(i, j, screen);
          screen.LoadContent(Content);
        }
      }

      testLevel.CullingCenter = dummy;
    }

    /// <summary>
    /// UnloadContent will be called once per game and is the place to unload
    /// game-specific content.
    /// </summary>
    protected override void UnloadContent()
    {
      // TODO: Unload any non ContentManager content here
    }

    /// <summary>
    /// Allows the game to run logic such as updating the world,
    /// checking for collisions, gathering input, and playing audio.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    protected override void Update(GameTime gameTime)
    {
      float deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

      testLevel.Update(gameTime);
      if(GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
      {
        Exit();
      }

      Vector2 inputVector = Vector2.Zero;
      if (Keyboard.GetState().IsKeyDown(Keys.Right))
      {
        inputVector.X += 1.0f;
      }

      if (Keyboard.GetState().IsKeyDown(Keys.Left))
      {
        inputVector.X -= 1.0f;
      }

      if (Keyboard.GetState().IsKeyDown(Keys.Up))
      {
        inputVector.Y -= 1.0f;
      }

      if (Keyboard.GetState().IsKeyDown(Keys.Down))
      {
        inputVector.Y += 1.0f;
      }

      const float speed = 400.0f;
      dummy.LocalTransform.Position += inputVector.GetClampedTo(1.0f) * (speed * deltaSeconds);

      if(World.BodyList.Count > 0)
      {
        var body = World.BodyList[0];
        body.Position += inputVector.GetClampedTo(1.0f) * (speed * deltaSeconds);
      }

       inputVector = Vector2.Zero;
      if (Keyboard.GetState().IsKeyDown(Keys.D))
      {
        inputVector.X += 1.0f;
      }

      if (Keyboard.GetState().IsKeyDown(Keys.A))
      {
        inputVector.X -= 1.0f;
      }

      if (Keyboard.GetState().IsKeyDown(Keys.W))
      {
        inputVector.Y -= 1.0f;
      }

      if (Keyboard.GetState().IsKeyDown(Keys.S))
      {
        inputVector.Y += 1.0f;
      }

      cam.LocalTransform.Position += inputVector.GetClampedTo(1.0f) * (speed * deltaSeconds);

      World.Step(deltaSeconds);

      dummy.Update(gameTime);

      cam.Update(gameTime);

      base.Update(gameTime);
    }

    /// <summary>
    /// This is called when the game should draw itself.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    protected override void Draw(GameTime gameTime)
    {
      GraphicsDevice.Clear(Color.CornflowerBlue);
      Matrix viewMatrix = cam.ViewMatrix;
      Matrix projectionMatrix = cam.ProjectionMatrix;

#if true
      spriteBatch.Begin(SpriteSortMode.BackToFront, null, null, null, null, null, viewMatrix);

      testLevel.Draw(gameTime, spriteBatch);

      dummy.Draw(spriteBatch);

      // Draw the origin of the world.
      int radius = 2;
      spriteBatch.FillRectangle(new Rectangle { X = -radius, Y = -radius, Width = 2 * radius, Height = 2 * radius }, Color.Lime);

      spriteBatch.End();
#endif

      PhysicsDebugView.RenderDebugData(ref projectionMatrix, ref viewMatrix);

      base.Draw(gameTime);
    }
  }
}