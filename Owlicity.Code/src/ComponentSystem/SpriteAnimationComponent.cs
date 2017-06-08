using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;

namespace Owlicity
{
  // TODO(manu): Consider not using SpriteAnimationType in this class but rather a handle-based approach.
  //             The current implementation does not enable you to provide any kind of animiation, only the predefined ones.
  //             It also prevents the ability to pre-configure the default state of an animation instance.
  //             The SpriteAnimationType enum is just convenience to create known animations.
  public class SpriteAnimationComponent : DrawComponent
  {
    //
    // Input data
    //
    public List<SpriteAnimationType> AnimationTypes { get; set; }

    //
    // Runtime data
    //
    public SpriteAnimationType ActiveAnimationType { get; private set; }
    public SpriteAnimationInstance ActiveAnimation { get { return AnimationInstances[ActiveAnimationType]; } }
    public Action<SpriteAnimationType, SpriteAnimationPlaybackState, SpriteAnimationPlaybackState> OnAnimationPlaybackStateChanged;

    public Dictionary<SpriteAnimationType, SpriteAnimationInstance> AnimationInstances =
      new Dictionary<SpriteAnimationType, SpriteAnimationInstance>();

    public SpriteAnimationComponent(GameObject owner) : base(owner)
    {
    }

    public override void Initialize()
    {
      base.Initialize();

      foreach(SpriteAnimationType type in AnimationTypes)
      {
        SpriteAnimationInstance animation = SpriteAnimationFactory.CreateAnimationInstance(type);
        AnimationInstances.Add(type, animation);
      }

      Hotspot.AttachTo(this);
      ChangeActiveAnimation(AnimationTypes[0]);
    }

    public void ChangeActiveAnimation(SpriteAnimationType newAnimationType, bool transferState = false)
    {
      Debug.Assert(newAnimationType != SpriteAnimationType.Unknown);

      if(newAnimationType != ActiveAnimationType)
      {
        bool hasOldState = false;
        SpriteAnimationState oldState = default(SpriteAnimationState);
        if(ActiveAnimationType != SpriteAnimationType.Unknown)
        {
          hasOldState = true;
          oldState = ActiveAnimation.State;

          ActiveAnimation.Stop();
        }

        ActiveAnimationType = newAnimationType;
        Hotspot.Position = -(ActiveAnimation.State.Hotspot * ActiveAnimation.State.Scale * Global.MetersPerPixel);

        ActiveAnimation.Play();

        if(hasOldState && transferState)
        {
          ActiveAnimation.State.CurrentFrameTime = oldState.CurrentFrameTime;
          ActiveAnimation.State.CurrentFrameIndex = oldState.CurrentFrameIndex;
        }
      }
    }

    public override void Update(float deltaSeconds)
    {
      base.Update(deltaSeconds);

      SpriteAnimationPlaybackState oldPlaybackState = ActiveAnimation.State.PlaybackState;
      ActiveAnimation.Update(deltaSeconds);
      SpriteAnimationPlaybackState newPlaybackState = ActiveAnimation.State.PlaybackState;

      if(newPlaybackState != oldPlaybackState)
      {
        OnAnimationPlaybackStateChanged?.Invoke(ActiveAnimationType, oldPlaybackState, newPlaybackState);
      }

      Global.Game.DebugDrawCommands.Add(view =>
      {
        view.DrawCircle(this.GetWorldSpatialData().Position, Global.ToMeters(10), new Color(0xdd, 0x99, 0x44));
      });
    }

    public override void Draw(SpriteBatch batch)
    {
      base.Draw(batch);

      SpatialData worldSpatial = Hotspot.GetWorldSpatialData();
      ActiveAnimation.Draw(batch, worldSpatial, RenderDepth);
    }
  }
}
