using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Owlicity
{
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
      if (totalSpriteCount > 0)
      {
        anim.Frames.Capacity = totalSpriteCount;
        int numCols = atlas.Width / tileWidth;
        int numRows = atlas.Height / tileHeight;
        anim.Atlas = atlas;
        Vector2 invAtlasDim = new Vector2(1.0f / atlas.Width, 1.0f / atlas.Height);
        anim.TileUV = new Vector2(tileWidth, tileHeight) * invAtlasDim;
        int spriteIndex = 0;
        for (int row = 0; row < numRows; row++)
        {
          for (int col = 0; col < numCols; col++, spriteIndex++)
          {
            if (spriteIndex >= totalSpriteCount)
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
      if (Data.Frames.Count > 0)
      {
        float deltaSeconds = (float)dt.ElapsedGameTime.TotalSeconds;
        CurrentFrameTime += deltaSeconds;
        int newFrameIndex = CurrentFrameIndex;
        while (CurrentFrameTime > Data.SecondsPerFrame)
        {
          CurrentFrameTime -= Data.SecondsPerFrame;
          ++newFrameIndex;
          if (newFrameIndex >= Data.Frames.Count)
          {
            newFrameIndex = 0;
          }
        }

        if (newFrameIndex != CurrentFrameIndex)
        {
          SpriteAnimationFrame frame = Data.Frames[newFrameIndex];
          _currentSprite.TextureOffset = frame.Offset;
          CurrentFrameIndex = newFrameIndex;
        }
      }
    }

    public void Draw(SpriteBatch spriteBatch, Transform transform)
    {
      if (Data.Frames.Count > 0)
      {
        _currentSprite.Draw(spriteBatch, transform);
      }
    }
  }

}
