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
    public Point Offset;
  }

  public class SpriteAnimationData
  {
    public Texture2D Atlas { get; set; }
    public Point TileDim { get; set; }
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
        anim.TileDim = new Point(tileWidth, tileHeight);
        int spriteIndex = 0;
        for (int row = 0; row < numRows; row++)
        {
          for (int col = 0; col < numCols; col++, spriteIndex++)
          {
            if (spriteIndex >= totalSpriteCount)
              goto DONE_LABEL;

            int x = col * tileWidth;
            int y = row * tileHeight;
            SpriteAnimationFrame frame = new SpriteAnimationFrame
            {
              Offset = new Point(x, y)
            };
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

  public enum SpriteAnimationPlaybackMode
  {
    Forward,
    Backward,
  }

  public enum SpriteAnimationPlaybackState
  {
    Stopped,
    Started,
    Paused,
  }

  public class SpriteAnimationInstance
  {
    public SpriteAnimationData Data { get; set; }
    public float CurrentFrameTime { get; set; }
    public int CurrentFrameIndex { get; set; }
    public SpriteAnimationPlaybackMode PlaybackMode { get; set; }
    public SpriteAnimationPlaybackState PlaybackState { get; set; }
    public bool PingPong { get; set; }

    private Sprite _currentSprite;

    public void Init(SpriteAnimationData data)
    {
      Data = data;
      _currentSprite = new Sprite
      {
        Texture = data.Atlas,
        TextureDim = data.TileDim,
      };
    }

    public void Update(GameTime dt)
    {
      if (Data.Frames.Count > 0)
      {
        float deltaSeconds = (float)dt.ElapsedGameTime.TotalSeconds;
        CurrentFrameTime += deltaSeconds;
        int oldFrameIndex = CurrentFrameIndex;
        while (CurrentFrameTime >= Data.SecondsPerFrame)
        {
          CurrentFrameTime -= Data.SecondsPerFrame;
          AdvanceFrameIndex();
        }

        if (CurrentFrameIndex != oldFrameIndex)
        {
          SpriteAnimationFrame frame = Data.Frames[CurrentFrameIndex];
          _currentSprite.TextureOffset = frame.Offset;
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

    public void AdvanceFrameIndex()
    {
      int newFrameIndex;
      switch(PlaybackMode)
      {
        case SpriteAnimationPlaybackMode.Forward:
        {
          newFrameIndex = CurrentFrameIndex + 1;
          if(newFrameIndex >= Data.Frames.Count)
          {
            if(PingPong)
            {
              // Note(manu): Just reverse the playback mode.
              PlaybackMode = SpriteAnimationPlaybackMode.Backward;
              newFrameIndex = CurrentFrameIndex;
            }
            else
            {
              newFrameIndex = 0;
            }
          }
        } break;

        case SpriteAnimationPlaybackMode.Backward:
        {
          newFrameIndex = CurrentFrameIndex - 1;
          if(newFrameIndex < 0)
          {
            if(PingPong)
            {
              // Note(manu): Just reverse the playback mode.
              PlaybackMode = SpriteAnimationPlaybackMode.Forward;
              newFrameIndex = CurrentFrameIndex;
            }
            else
            {
              newFrameIndex = Data.Frames.Count - 1;
            }
          }
        } break;

        default:
          throw new ArgumentException("Unknown playback mode.");
      }

      CurrentFrameIndex = newFrameIndex;
    }
  }
}
