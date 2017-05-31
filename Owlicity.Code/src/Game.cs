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
using System.Linq;

/*
  TODO:
    - Input "abstraction", but only a little bit.
    - Game object manager
    - Audio
*/

namespace Owlicity
{
  public class Dummy : ISpatial
  {
    public SpatialData Spatial { get; } = new SpatialData();

    public SpriteAnimationInstance anim;
    public SpatialData animOffset;
    public Body body;
    public ParticleEmitter particleEmitter;

    public void Initialize()
    {
      Vector2 offset = -0.5f * anim.Data.Config.TileDim.ToVector2();
      animOffset = new SpatialData
      {
        Parent = this,
        Transform = new Transform { p = offset },
        Depth = 0.5f,
      };

      body = new Body(OwlicityGame.Instance.World, bodyType: BodyType.Kinematic);

      var colors = new List<Color>
      {
        Color.White,
        Color.Red,
        Color.Green,
      };

      Texture2D particleTexture = OwlicityGame.Instance.Content.Load<Texture2D>("particle");

      var particles = new List<Texture2D>
      {
        particleTexture
      };

      particleEmitter = new ParticleEmitter(150, particles, colors);
    }

    public void LoadContent()
    {
      Vertices collisionVertices = OwlicityGame.Instance.Content.Load<Vertices>("slurp_collision");
      collisionVertices.Translate(-collisionVertices.GetAABB().Center);
      FixtureFactory.AttachLoopShape(collisionVertices, body);
    }

    public void ProcessInput(GameTime dt, Vector2 inputVector)
    {
      if(inputVector != Vector2.Zero)
      {
        float deltaSeconds = (float)dt.ElapsedGameTime.TotalSeconds;
        const float speed = 400.0f;
        Vector2 movementVector = inputVector.GetClampedTo(1.0f) * speed;

        Spatial.Transform.p += movementVector * deltaSeconds;
        body.LinearVelocity = movementVector;
      }
    }

    public void Update(float deltaSeconds)
    {
      anim.Update(deltaSeconds);
      body.LinearVelocity *= 0.85f;
      particleEmitter.Update(deltaSeconds);
      particleEmitter.emitParticles(Spatial.Transform.p);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
      anim.Draw(spriteBatch, animOffset.GetWorldSpatialData());
      particleEmitter.Draw(spriteBatch);
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
      SpriteAnimationFactory.Initialize(Content);

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

      dummy = new Dummy
      {
        anim = SpriteAnimationFactory.CreateAnimationInstance(SpriteAnimationType.Owliver_Walk_Left),
      };
      dummy.Initialize();
      dummy.LoadContent();

      testLevel = new Level(Content);

      for (uint i = 0; i < 4; i++)
      {
        for (uint j = 0; j < 7; j++)
        {
          var screen = new Screen();
          screen.AssetName = $"level01/level1_ground_{j}{i}";
          testLevel.AddScreen(i, j, screen);
          screen.LoadContent(Content);
        }
      }

      {
        Vertices vertices = Content.Load<Vertices>("level01/level1_ground_00_collision");
        Body body = new Body(World);
        FixtureFactory.AttachLoopShape(vertices, body);
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
      if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
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

      dummy.ProcessInput(gameTime, inputVector);

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

      const float speed = 400.0f;
      cam.Spatial.Transform.p += inputVector.GetClampedTo(1.0f) * (speed * deltaSeconds);

      World.Step(deltaSeconds);

      dummy.Update(deltaSeconds);

      cam.Update(deltaSeconds);

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