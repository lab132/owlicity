using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using VelcroPhysics.Shared;

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
    Playing,
    Paused,
    Stopped,
  }

  public struct SpriteAnimationState
  {
    public SpriteEffects SpriteEffects;
    public Vector2 Scale;
    public bool PingPong;
    public int NumLoopsToPlay;
    public int CurrentLoopIndex;
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
    public Vector2 Hotspot;
  }

  public class SpriteAnimationInstance
  {
    public SpriteAnimationData Data;
    public Vector2 ScaledDim => State.Scale * Data.Config.TileDim.ToVector2();

    public SpriteAnimationState State;
    public bool IsPlaying { get => State.PlaybackState == SpriteAnimationPlaybackState.Playing; }
    public bool IsPaused { get => State.PlaybackState == SpriteAnimationPlaybackState.Paused; }
    public bool IsStopped { get => State.PlaybackState == SpriteAnimationPlaybackState.Stopped; }

    public void Init(SpriteAnimationData data)
    {
      Data = data;
      State = new SpriteAnimationState
      {
        SecondsPerFrame = data.Config.SecondsPerFrame,
        PingPong = data.Config.PingPong,
        NumLoopsToPlay = data.Config.NumLoopsToPlay ?? int.MaxValue,
        Scale = data.Config.Scale,
        SpriteEffects = data.Config.SpriteEffects,
        Hotspot = data.Config.Hotspot,
      };
    }

    public void Update(float deltaSeconds)
    {
      if (Data.Frames.Count > 0 && State.PlaybackState == SpriteAnimationPlaybackState.Playing)
      {
        State.CurrentFrameTime += deltaSeconds;
        int oldFrameIndex = State.CurrentFrameIndex;
        while (State.CurrentFrameTime >= State.SecondsPerFrame && State.PlaybackState == SpriteAnimationPlaybackState.Playing)
        {
          State.CurrentFrameTime -= State.SecondsPerFrame;
          AdvanceFrameIndex();
        }
      }
    }

    public void Draw(Renderer renderer, SpatialData spatial, float depth = 0.0f, Vector2? additionalScale = null, Color? tint = null)
    {
      if (Data.Frames.Count > 0)
      {
        Vector2 scale = State.Scale;
        if(additionalScale != null)
          scale *= additionalScale.Value;

        int frameIndex = MathHelper.Clamp(State.CurrentFrameIndex, 0, Data.Frames.Count - 1);
        SpriteAnimationFrame frame = Data.Frames[frameIndex];

        renderer.DrawSprite(
          spatial.Position,
          spatial.Rotation,
          scale,
          depth,
          Data.TileSheet,
          State.Hotspot,
          new Rectangle { Location = frame.Offset, Size = Data.Config.TileDim },
          tint ?? Color.White,
          State.SpriteEffects);
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
            State.CurrentLoopIndex++;
            if(State.CurrentLoopIndex >= State.NumLoopsToPlay)
            {
              Stop();
            }
            else
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
          }
        } break;

        case SpriteAnimationPlaybackMode.Backward:
        {
          newFrameIndex = State.CurrentFrameIndex - 1;
          if(newFrameIndex < 0)
          {
            State.CurrentLoopIndex++;
            if(State.CurrentLoopIndex >= State.NumLoopsToPlay)
            {
              Stop();
            }
            else
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
          }
        } break;

        default:
          throw new ArgumentException("Unknown playback mode.");
      }

      State.CurrentFrameIndex = newFrameIndex;
    }

    public void Play()
    {
      State.PlaybackState = SpriteAnimationPlaybackState.Playing;
    }

    public void Pause()
    {
      State.PlaybackState = SpriteAnimationPlaybackState.Paused;
    }

    public void Stop()
    {
      State.CurrentFrameTime = 0.0f;
      State.CurrentFrameIndex = 0;
      State.PlaybackState = SpriteAnimationPlaybackState.Stopped;
    }

    public AABB CalcAABB()
    {
      Vector2 scale = State.Scale;
      Vector2 hotspot = State.Hotspot;
      Vector2 dim = Data.Config.TileDim.ToVector2();
      AABB result = new AABB
      {
        LowerBound = scale * (-hotspot),
        UpperBound = scale * (-hotspot + dim),
      };

      return result;
    }
  }

  public enum SpriteAnimationType
  {
    Unknown,

    Owliver_Idle_Left,
    Owliver_Idle_Right,
    Owliver_Walk_Left,
    Owliver_Walk_Right,
    Owliver_AttackStick_Left,
    Owliver_AttackStick_Right,
    Owliver_AttackFishingRod_Left,
    Owliver_AttackFishingRod_Right,

    Slurp_Idle,

    Tankton_Idle_Left,
    Tankton_Idle_Right,

    Fir_Idle,
    FirAlt_Idle,

    Conifer_Idle,
    ConiferAlt_Idle,

    Oak_Idle,

    Orange_Idle,

    Bush_Idle,

    Key_Gold,

    Bonbon_Gold,
    Bonbon_Red,

    Cross,

    Digit0,
    Digit1,
    Digit2,
    Digit3,
    Digit4,
    Digit5,
    Digit6,
    Digit7,
    Digit8,
    Digit9,

    OwlHealthIcon,
  }

  public struct SpriteAnimationConfig
  {
    public string TileSheetName;
    public int TileCount;
    public Point TileDim;
    public Vector2 Scale;
    public SpriteEffects SpriteEffects;
    public float SecondsPerFrame;
    public bool PingPong;
    public int? NumLoopsToPlay;
    public Vector2 Hotspot;

    public Vector2 ScaledTileDim => Scale * TileDim.ToVector2();

    public static readonly SpriteAnimationConfig Default = new SpriteAnimationConfig
    {
      TileCount = 3,
      TileDim = new Point(256, 256),
      Scale = Vector2.One,
      SecondsPerFrame = 0.05f,
      PingPong = true,
      Hotspot = new Vector2(128.0f, 128.0f),
    };
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
        result = new SpriteAnimationData()
        {
          Config = config
        };
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
        SpriteAnimationConfig config = SpriteAnimationConfig.Default;

        switch(animType)
        {
          case SpriteAnimationType.Owliver_Idle_Left:
          {
            config.TileSheetName = "owliver_idle_front_left_spritesheet";
            config.Scale = Global.OwliverScale;
            config.Hotspot = new Vector2(121, 90);
          }
          break;

          case SpriteAnimationType.Owliver_Idle_Right:
          {
            //config.TileSheetName = "owliver_idle_front_right_spritesheet";
            config.TileSheetName = "owliver_idle_front_left_spritesheet";
            config.SpriteEffects = SpriteEffects.FlipHorizontally;
            config.Scale = Global.OwliverScale;
            config.Hotspot = new Vector2(133, 90);
          }
          break;

          case SpriteAnimationType.Owliver_Walk_Left:
          {
            config.TileSheetName = "owliver_walk_front_left_spritesheet";
            config.Scale = Global.OwliverScale;
            config.Hotspot = new Vector2(121, 90);
          }
          break;

          case SpriteAnimationType.Owliver_Walk_Right:
          {
            //TileSheetName = "owliver_walk_front_right_spritesheet",
            config.TileSheetName = "owliver_walk_front_left_spritesheet";
            config.SpriteEffects = SpriteEffects.FlipHorizontally;
            config.Scale = Global.OwliverScale;
            config.Hotspot = new Vector2(133, 90);
          }
          break;

          case SpriteAnimationType.Owliver_AttackStick_Left:
          {
            config.TileSheetName = "owliver_attack_spritesheet";
            config.Scale = Global.OwliverScale;
            config.Hotspot = new Vector2(164, 90);
            config.TileCount = 5;
            config.PingPong = false;
            config.NumLoopsToPlay = 1;
          }
          break;

          case SpriteAnimationType.Owliver_AttackStick_Right:
          {
            config.TileSheetName = "owliver_attack_spritesheet";
            config.SpriteEffects = SpriteEffects.FlipHorizontally;
            config.Scale = Global.OwliverScale;
            config.Hotspot = new Vector2(90, 90);
            config.TileCount = 5;
            config.PingPong = false;
            config.NumLoopsToPlay = 1;
          }
          break;

          case SpriteAnimationType.Owliver_AttackFishingRod_Left:
          {
            config.TileSheetName = "owliver_attack_fishingrod_spritesheet";
            config.Scale = Global.OwliverScale;
            config.Hotspot = new Vector2(164, 90);
            config.TileCount = 5;
            config.PingPong = false;
            config.NumLoopsToPlay = 1;
          }
          break;

          case SpriteAnimationType.Owliver_AttackFishingRod_Right:
          {
            config.TileSheetName = "owliver_attack_fishingrod_spritesheet";
            config.SpriteEffects = SpriteEffects.FlipHorizontally;
            config.Scale = Global.OwliverScale;
            config.Hotspot = new Vector2(90, 90);
            config.TileCount = 5;
            config.PingPong = false;
            config.NumLoopsToPlay = 1;
          }
          break;

          case SpriteAnimationType.Slurp_Idle:
          {
            config.TileSheetName = "slurp_spritesheet";
            config.TileCount = 7;
            config.TileDim = new Point(210, 270);
            config.Hotspot = 0.5f * config.TileDim.ToVector2();
            config.Scale = Global.SlurpScale;
          }
          break;

          case SpriteAnimationType.Tankton_Idle_Left:
          {
            config.SpriteEffects = SpriteEffects.FlipHorizontally;
          }
          goto case SpriteAnimationType.Tankton_Idle_Right;

          case SpriteAnimationType.Tankton_Idle_Right:
          {
            config.TileSheetName = "boss_spritesheet";
            config.TileDim = new Point(512);
            config.Hotspot = new Vector2(256, 450);
            config.Scale = Global.TanktonScale;
            config.PingPong = false;
            config.SecondsPerFrame *= 2;
          }
          break;

          case SpriteAnimationType.Fir_Idle:
          {
            config.TileSheetName = "fir_spritesheet";
            config.Hotspot = new Vector2(136, 235);
          }
          break;

          case SpriteAnimationType.FirAlt_Idle:
          {
            config.TileSheetName = "fir_spritesheet";
            config.Hotspot = new Vector2(128, 17);
          }
          break;

          case SpriteAnimationType.Conifer_Idle:
          {
            config.TileSheetName = "conifer_spritesheet";
            config.Hotspot = new Vector2(134, 233);
          }
          break;

          case SpriteAnimationType.ConiferAlt_Idle:
          {
            config.TileSheetName = "conifer_spritesheet";
            config.Hotspot = new Vector2(135, 18);
          }
          break;

          case SpriteAnimationType.Oak_Idle:
          {
            config.TileSheetName = "oak_spritesheet";
            config.Hotspot = new Vector2(126, 224);
          }
          break;

          case SpriteAnimationType.Orange_Idle:
          {
            config.TileSheetName = "orange_spritesheet";
            config.Hotspot = new Vector2(127, 227);
          }
          break;

          case SpriteAnimationType.Bush_Idle:
          {
            config.TileSheetName = "bush_spritesheet";
            config.Hotspot = new Vector2(130, 212);
          }
          break;

          case SpriteAnimationType.Key_Gold:
          {
            config.TileSheetName = "key_spritesheet";
            config.TileDim = new Point(128);
            config.Hotspot = 0.5f * config.TileDim.ToVector2();
            config.Scale = new Vector2(0.5f);
          }
          break;

          case SpriteAnimationType.Bonbon_Gold:
          {
            config.TileSheetName = "bonbon_gold";
            config.TileDim = new Point(64);
            config.Hotspot = new Vector2(32);
          }
          break;

          case SpriteAnimationType.Bonbon_Red:
          {
            config.TileSheetName = "bonbon_red";
            config.TileDim = new Point(64);
            config.Hotspot = new Vector2(32);
          }
          break;

          case SpriteAnimationType.Cross:
          {
            config.TileSheetName = "cross";
            config.TileDim = new Point(64);
            config.Hotspot = new Vector2(32);
            config.Scale = new Vector2(0.5f);
          }
          break;

          case SpriteAnimationType.Digit0:
          case SpriteAnimationType.Digit1:
          case SpriteAnimationType.Digit2:
          case SpriteAnimationType.Digit3:
          case SpriteAnimationType.Digit4:
          case SpriteAnimationType.Digit5:
          case SpriteAnimationType.Digit6:
          case SpriteAnimationType.Digit7:
          case SpriteAnimationType.Digit8:
          case SpriteAnimationType.Digit9:
          {
            int digit = animType - SpriteAnimationType.Digit0;
            config.TileSheetName = $"digits/{digit}";
            config.TileDim = new Point(64);
            config.Hotspot = new Vector2(32);

            config.Scale = new Vector2(0.5f);
            // TODO(manu): Remove this once we have all digits.
            if(digit > 2)
              config.TileSheetName = $"cross";
          }
          break;

          case SpriteAnimationType.OwlHealthIcon:
          {
            config.TileSheetName = "health_icon_spritesheet";
            config.TileDim = new Point(128);
            config.Hotspot = 0.5f * config.TileDim.ToVector2();
            config.Scale = new Vector2(0.5f);
          }
          break;

          default: throw new ArgumentException("Unknown sprite animation type.");
        }

        if(config.TileSheetName != null)
        {
          result = CreateAnimation(ref config);
          _known[(int)animType] = result;
        }
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
