﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
      Texture2D atlas = Content.Load<Texture2D>("slurp_spritesheet");
      testAnimation = SpriteAnimationData.FromAtlas(atlas, 7, 210, 270);
      testAnimation.SecondsPerFrame = 0.05f;

      test = testAnimation.CreateInstance();
      testTransform = new Transform { Position = new Vector3(20, 20, 0) };
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
      if(GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
      {
        Exit();
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

      spriteBatch.Begin();
      test.Draw(spriteBatch, testTransform);
      spriteBatch.End();
      // TODO: Add your drawing code here

      base.Draw(gameTime);
    }
  }
}
