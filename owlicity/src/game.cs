using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Primitives2D;
using Owlicity.src;
using System.Collections.Generic;
using System.Diagnostics;

namespace Owlicity
{

  /// <summary>
  /// This is the main type for your game.
  /// </summary>
  public class OwlicityGame : Game
  {
    GraphicsDeviceManager graphics;
    SpriteBatch spriteBatch;

    SpriteAnimationData testAnimation;
    SpriteAnimationInstance test;
    Transform testTransform;
    Camera cam;
    Level testLevel;

    public OwlicityGame()
    {
      graphics = new GraphicsDeviceManager(this);
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

      test = testAnimation.CreateInstance();
      test.PingPong = true;
      testTransform = new Transform { Position = new Vector3(20, 20, 0) };

      cam = new Camera();
      cam.LookAt = testTransform;

      testLevel = new Level(Content);

      Screen testScreen1 = new Screen();
      testScreen1.AssetName = "dummy_level_1";
      Screen testScreen2 = new Screen();
      testScreen2.AssetName = "dummy_level_2";
      Screen testScreen3 = new Screen();
      testScreen3.AssetName = "dummy_level_3";
      Screen testScreen4 = new Screen();
      testScreen4.AssetName = "dummy_level_4";

      testLevel.addScreen(0, 0, testScreen1);
      testLevel.addScreen(0, 1, testScreen2);
      testLevel.addScreen(1, 0, testScreen3);
      testLevel.addScreen(1, 1, testScreen4);

      testLevel.CullingCenter = testTransform;

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
      if (Keyboard.GetState().IsKeyDown(Keys.Right))
      {
        testTransform.Position += new Vector3(1.0f, 0.0f, 0.0f);
      }

      if (Keyboard.GetState().IsKeyDown(Keys.Left))
      {
        testTransform.Position += new Vector3(-1.0f, 0.0f, 0.0f);
      }

      if (Keyboard.GetState().IsKeyDown(Keys.Up))
      {
        testTransform.Position += new Vector3(0.0f, -1.0f, 0.0f);
      }

      if (Keyboard.GetState().IsKeyDown(Keys.Down))
      {
        testTransform.Position += new Vector3(0.0f, 1.0f, 0.0f);
      }

      // TODO: Add your update logic here
      test.Update(gameTime);

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
      testLevel.Draw(gameTime, spriteBatch);
      test.Draw(spriteBatch, testTransform);
      spriteBatch.End();
      // TODO: Add your drawing code here

      base.Draw(gameTime);
    }
  }
}