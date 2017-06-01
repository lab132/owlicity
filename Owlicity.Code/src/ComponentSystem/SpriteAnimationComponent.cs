using System;
using Microsoft.Xna.Framework.Graphics;
using VelcroPhysics;
using VelcroPhysics.Shared;
using System.Collections.Generic;

namespace Owlicity
{
  public class SpriteAnimationComponent : SpatialComponent
  {
    //
    // Input data
    //
    public List<SpriteAnimationType> AnimationTypes { get; set; }

    //
    // Runtime data
    //
    public SpriteAnimationInstance ActiveAnimation { get; private set; }

    private Dictionary<SpriteAnimationType, SpriteAnimationInstance> _sprites{get; set;}

    public SpriteAnimationComponent(GameObject owner) : base(owner)
    {
      _sprites = new Dictionary<SpriteAnimationType, SpriteAnimationInstance>();
    }

    public override void Initialize()
    {
      base.Initialize();
      foreach (SpriteAnimationType type in AnimationTypes) {
        var animation = SpriteAnimationFactory.CreateAnimationInstance(type);
        _sprites.Add(type, animation);
        ActiveAnimation = animation;
      }
      
      Spatial.Transform.p -= 0.5f * ActiveAnimation.Data.Config.TileDim.ToVector2();
    }

    public void setActiveAnimation(SpriteAnimationType type)
    {
      ActiveAnimation = _sprites[type];
    }

    public override void Update(float deltaSeconds)
    {
      base.Update(deltaSeconds);

      ActiveAnimation.Update(deltaSeconds);
    }

    public override void Draw(float deltaSeconds, SpriteBatch batch)
    {
      base.Draw(deltaSeconds, batch);

      SpatialData worldSpatial = this.GetWorldSpatialData();
      ActiveAnimation.Draw(batch, worldSpatial);
    }
  }
}
