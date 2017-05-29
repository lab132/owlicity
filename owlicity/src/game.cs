using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Primitives2D;
using Owlicity.src;
using System.Collections.Generic;
using System.Diagnostics;
using System;

namespace Owlicity
{
  public class Dummy : ITransformable
  {
    public Transform LocalTransform { get; } = new Transform();
    public SpriteAnimationInstance anim;
    public Transform animOffset;

    public void Initialize()
    {
      animOffset = new Transform
      {
        Parent = this,
        Position = -0.5f * anim.Data.TileDim.ToVector2()
      };
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
    GraphicsDeviceManager graphics;
    SpriteBatch spriteBatch;

    SpriteAnimationData testAnimation;
    Dummy dummy;

    Camera cam;
    Level testLevel;


    public OwlicityGame()
    {
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
      // TODO: Add your initialization logic here

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

      cam = new Camera
      {
        LookAt = dummy,
        Bounds = GraphicsDevice.Viewport.Bounds.Size.ToVector2()
      };

      testLevel = new Level(Content);

      for (uint i=0; i < 4; i++)
      {
        for (uint j = 0; j < 7; j++)
        {
          var screen = new Screen();
          screen.AssetName = $"level01/level01_{i}{j}";
          testLevel.addScreen(j, i, screen);
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

      float deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
      const float speed = 400.0f;
      dummy.LocalTransform.Position += inputVector.GetClampedTo(1.0f) * (speed * deltaSeconds);

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

      spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, cam.ViewMatrix);
      dummy.Draw(spriteBatch);
      testLevel.Draw(gameTime, spriteBatch);
      int radius = 2;
      spriteBatch.FillRectangle(new Rectangle { X = -radius, Y = -radius, Width = 2 * radius, Height = 2 * radius }, Color.Lime);
      spriteBatch.End();

      base.Draw(gameTime);
    }
  }
}