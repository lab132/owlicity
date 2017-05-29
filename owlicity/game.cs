using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Diagnostics;

namespace Owlicity
{
  public static class VectorExtensions
  {
    public static Vector2 GetXY(this Vector3 v)
    {
      return new Vector2(v.X, v.Y);
    }
  }

  struct Angle
  {
    private float _value;

    public float Radians
    {
      get { return _value; }
      set { _value = value; }
    }

    public float Degrees
    {
      get { return _value; }
      set { _value = value; }
    }

    public static Angle operator +(Angle A, Angle B)
    {
      return new Angle { _value = A._value + B._value };
    }

    public static Angle operator -(Angle A, Angle B)
    {
      return new Angle { _value = A._value - B._value };
    }
  }

  class Transform
  {
    public Vector3 Position { get; set; }
    public Angle Rotation { get; set; }
    public Vector2 Scale { get; set; } = Vector2.One;
  }

  class Sprite
  {
    public Texture2D Texture { get; set; }
    public Vector2 TextureOffset { get; set; }
    public Vector2 TextureUV { get; set; } = Vector2.One;
    public Color Tint { get; set; } = Color.White;
    public SpriteEffects SpriteEffects { get; set; }

    public void Draw(SpriteBatch spriteBatch, Transform transform)
    {
      Vector2 textureDim = new Vector2(Texture.Width, Texture.Height);
      Rectangle sourceRect = new Rectangle
      {
        Location = (TextureOffset * textureDim).ToPoint(),
        Size = (TextureUV * textureDim).ToPoint()
      };

      spriteBatch.Draw(
        texture: Texture,
        position: transform.Position.GetXY(),
        sourceRectangle: sourceRect,
        color: Tint,
        rotation: transform.Rotation.Radians,
        origin: TextureOffset,
        scale: transform.Scale,
        effects: SpriteEffects,
        layerDepth: transform.Position.Z);
    }
  }

  public struct SpriteAnimationFrame
  {
    public Vector2 Offset;
  }

  class SpriteAnimationData
  {
    public Texture2D Atlas { get; set; }
    public Vector2 TileUV { get; set; }
    public List<SpriteAnimationFrame> Frames { get; set; } = new List<SpriteAnimationFrame>();
    public float FramesPerSecond
    {
      get { return 1 / SecondsPerFrame; }
      set { SecondsPerFrame = 1 / value; }
    }
    public float SecondsPerFrame { get; set; } = 1.0f / 24.0f;

    public static SpriteAnimationData FromAtlas(Texture2D atlas, int totalSpriteCount, int tileWidth, int tileHeight)
    {
      SpriteAnimationData anim = new SpriteAnimationData();
      if(totalSpriteCount > 0)
      {
        anim.Frames.Capacity = totalSpriteCount;
        int numCols = atlas.Width / tileWidth;
        int numRows = atlas.Height / tileHeight;
        anim.Atlas = atlas;
        Vector2 invAtlasDim = new Vector2(1.0f / atlas.Width, 1.0f / atlas.Height);
        anim.TileUV = new Vector2(tileWidth, tileHeight) * invAtlasDim;
        int spriteIndex = 0;
        for(int row = 0; row < numRows; row++)
        {
          for(int col = 0; col < numCols; col++, spriteIndex++)
          {
            if(spriteIndex >= totalSpriteCount)
              goto DONE_LABEL;

            int x = col * tileWidth;
            int y = row * tileHeight;
            SpriteAnimationFrame frame = new SpriteAnimationFrame { Offset = new Vector2(x, y) * invAtlasDim };
            anim.Frames.Add(frame);
          }
        }

        DONE_LABEL:;
      }

      return anim;
    }

    public SpriteAnimationInstance CreateInstance()
    {
      SpriteAnimationInstance instance = new SpriteAnimationInstance();
      instance.Init(this);
      return instance;
    }
  }

  class SpriteAnimationInstance
  {
    public SpriteAnimationData Data { get; set; }
    public float CurrentFrameTime { get; set; }
    public int CurrentFrameIndex { get; set; }

    private Sprite _currentSprite;

    public void Init(SpriteAnimationData data)
    {
      Data = data;
      _currentSprite = new Sprite
      {
        Texture = data.Atlas,
        TextureUV = data.TileUV,
      };
    }

    public void Update(GameTime dt)
    {
      if(Data.Frames.Count > 0)
      {
        float deltaSeconds = (float)dt.ElapsedGameTime.TotalSeconds;
        CurrentFrameTime += deltaSeconds;
        int newFrameIndex = CurrentFrameIndex;
        while(CurrentFrameTime > Data.SecondsPerFrame)
        {
          CurrentFrameTime -= Data.SecondsPerFrame;
          ++newFrameIndex;
          if(newFrameIndex >= Data.Frames.Count)
          {
            newFrameIndex = 0;
          }
        }

        if(newFrameIndex != CurrentFrameIndex)
        {
          SpriteAnimationFrame frame = Data.Frames[newFrameIndex];
          _currentSprite.TextureOffset = frame.Offset;
          CurrentFrameIndex = newFrameIndex;
        }
      }
    }

    public void Draw(SpriteBatch spriteBatch, Transform transform)
    {
      if(Data.Frames.Count > 0)
      {
        _currentSprite.Draw(spriteBatch, transform);
      }
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
