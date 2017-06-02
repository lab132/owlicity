using System;
using Microsoft.Xna.Framework.Graphics;
using VelcroPhysics;
using VelcroPhysics.Shared;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using Microsoft.Xna.Framework.Primitives2D;

namespace Owlicity
{
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
    public SpriteAnimationInstance ActiveAnimation { get { return _animInstances[ActiveAnimationType]; } }

    private Dictionary<SpriteAnimationType, SpriteAnimationInstance> _animInstances =
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
        _animInstances.Add(type, animation);
      }

      Hotspot.AttachTo(this);
      ChangeActiveAnimation(AnimationTypes[0]);
    }

    public void ChangeActiveAnimation(SpriteAnimationType newAnimationType)
    {
      Debug.Assert(newAnimationType != SpriteAnimationType.Unknown);

      if(newAnimationType != ActiveAnimationType)
      {
        if(ActiveAnimationType != SpriteAnimationType.Unknown)
        {
          ActiveAnimation.Stop();
        }

        ActiveAnimationType = newAnimationType;
        Hotspot.Transform.p = -(ActiveAnimation.State.Hotspot * ActiveAnimation.State.Scale);

        ActiveAnimation.Play();
      }
    }

    public override void Update(float deltaSeconds)
    {
      base.Update(deltaSeconds);

      ActiveAnimation.Update(deltaSeconds);
    }

    public override void Draw(float deltaSeconds, SpriteBatch batch)
    {
      base.Draw(deltaSeconds, batch);

      SpatialData worldSpatial = Hotspot.GetWorldSpatialData();
      ActiveAnimation.Draw(batch, worldSpatial, RenderDepth);
    }

    public override void DebugDraw(float deltaSeconds, SpriteBatch batch)
    {
      base.DebugDraw(deltaSeconds, batch);

      batch.DrawCircle(this.GetWorldSpatialData().Transform.p, 10, 9, new Color(0xdd, 0x99, 0x44));
    }
  }
}
