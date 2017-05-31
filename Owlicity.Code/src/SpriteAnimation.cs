using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public SpriteAnimationConfig Config;
    public Texture2D TileSheet;
    public List<SpriteAnimationFrame> Frames = new List<SpriteAnimationFrame>();

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

  public struct SpriteAnimationState
  {
    public bool PingPong;
    public float SecondsPerFrame;

    public float FramesPerSecond
    {
      get { return 1 / SecondsPerFrame; }
      set { SecondsPerFrame = 1 / value; }
    }

    public float CurrentFrameTime;
    public int CurrentFrameIndex;
    public SpriteAnimationPlaybackMode PlaybackMode;
    public SpriteAnimationPlaybackState PlaybackState;
  }

  public class SpriteAnimationInstance
  {
    public SpriteAnimationData Data;
    public SpriteAnimationState State;

    private Sprite _currentSprite;

    public void Init(SpriteAnimationData data)
    {
      Data = data;
      State = new SpriteAnimationState
      {
        SecondsPerFrame = data.Config.SecondsPerFrame,
        PingPong = data.Config.PingPong,
      };
      _currentSprite = new Sprite
      {
        Texture = data.TileSheet,
        TextureDim = data.Config.TileDim,
      };
    }

    public void Update(GameTime dt)
    {
      if (Data.Frames.Count > 0)
      {
        float deltaSeconds = (float)dt.ElapsedGameTime.TotalSeconds;
        State.CurrentFrameTime += deltaSeconds;
        int oldFrameIndex = State.CurrentFrameIndex;
        while (State.CurrentFrameTime >= State.SecondsPerFrame)
        {
          State.CurrentFrameTime -= State.SecondsPerFrame;
          AdvanceFrameIndex();
        }

        if (State.CurrentFrameIndex != oldFrameIndex)
        {
          SpriteAnimationFrame frame = Data.Frames[State.CurrentFrameIndex];
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
      switch(State.PlaybackMode)
      {
        case SpriteAnimationPlaybackMode.Forward:
        {
          newFrameIndex = State.CurrentFrameIndex + 1;
          if(newFrameIndex >= Data.Frames.Count)
          {
            if(State.PingPong)
            {
              // Note(manu): Just reverse the playback mode.
              State.PlaybackMode = SpriteAnimationPlaybackMode.Backward;
              newFrameIndex = State.CurrentFrameIndex;
            }
            else
            {
              newFrameIndex = 0;
            }
          }
        } break;

        case SpriteAnimationPlaybackMode.Backward:
        {
          newFrameIndex = State.CurrentFrameIndex - 1;
          if(newFrameIndex < 0)
          {
            if(State.PingPong)
            {
              // Note(manu): Just reverse the playback mode.
              State.PlaybackMode = SpriteAnimationPlaybackMode.Forward;
              newFrameIndex = State.CurrentFrameIndex;
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

      State.CurrentFrameIndex = newFrameIndex;
    }
  }

  public enum SpriteAnimationType
  {
    Owliver_Idle_Left,
    Owliver_Idle_Right,
    Owliver_Walk_Left,
    Owliver_Walk_Right,

    Slurp_Idle,
  }

  public struct SpriteAnimationConfig
  {
    public string TileSheetName;
    public int TileCount;
    public Point TileDim;
    public float SecondsPerFrame;
    public bool PingPong;
  }

  public static class SpriteAnimationFactory
  {
    private static ContentManager _content;
    private static SpriteAnimationData[] _known;

    public static void Initialize(ContentManager content)
    {
      if(content != null)
      {
        _content = content;
        int numAnimations = Enum.GetNames(typeof(SpriteAnimationType)).Length;
        _known = new SpriteAnimationData[numAnimations];
      }
    }

    public static SpriteAnimationData CreateAnimation(SpriteAnimationConfig config)
    {
      return CreateAnimation(ref config);
    }

    public static SpriteAnimationData CreateAnimation(ref SpriteAnimationConfig config)
    {
      Texture2D atlas = _content.Load<Texture2D>(config.TileSheetName);
      return CreateAnimation(atlas, ref config);
    }

    public static SpriteAnimationData CreateAnimation(Texture2D atlas, ref SpriteAnimationConfig config)
    {
      SpriteAnimationData result = null;
      if(config.TileCount > 0 || config.TileDim.X > 0 || config.TileDim.Y > 0)
      {
        result = new SpriteAnimationData();
        result.Config = config;
        result.Frames.Capacity = config.TileCount;
        int numCols = atlas.Width / config.TileDim.X;
        int numRows = atlas.Height / config.TileDim.Y;
        result.TileSheet = atlas;
        int spriteIndex = 0;
        for(int row = 0; row < numRows; row++)
        {
          for(int col = 0; col < numCols; col++, spriteIndex++)
          {
            if(spriteIndex >= config.TileCount)
              goto DONE_LABEL;

            SpriteAnimationFrame frame = new SpriteAnimationFrame
            {
              Offset = new Point(col, row) * config.TileDim
            };
            result.Frames.Add(frame);
          }
        }

        DONE_LABEL:;
      }

      return result;
    }

    public static SpriteAnimationData GetAnimation(SpriteAnimationType animType)
    {
      Debug.Assert(_content != null, "Not initialized.");
      SpriteAnimationData result = _known[(int)animType];

      if(result == null)
      {
        SpriteAnimationConfig config;

        switch(animType)
        {
          case SpriteAnimationType.Owliver_Idle_Left:
          {
            throw new NotImplementedException();
          }

          case SpriteAnimationType.Owliver_Idle_Right:
          {
            throw new NotImplementedException();
          }

          case SpriteAnimationType.Owliver_Walk_Left:
          {
            config = new SpriteAnimationConfig
            {
              TileSheetName = "owliver_walk_front_left_spritesheet",
              TileCount = 3,
              TileDim = new Point(256, 256),
              SecondsPerFrame = 0.05f,
              PingPong = true,
            };
          }
          break;

          case SpriteAnimationType.Owliver_Walk_Right:
          {
            throw new NotImplementedException();
          }

          case SpriteAnimationType.Slurp_Idle:
          {
            throw new NotImplementedException();
          }

          default:
          {
            throw new ArgumentException("Unknown sprite animation type.");
          }
        }

        result = CreateAnimation(ref config);
        _known[(int)animType] = result;
      }

      return result;
    }

    // This is just short for GetAnimation(...).CreateInstance();
    public static SpriteAnimationInstance CreateAnimationInstance(SpriteAnimationType animType)
    {
      SpriteAnimationData anim = GetAnimation(animType);
      SpriteAnimationInstance result = anim.CreateInstance();
      return result;
    }
  }
}
