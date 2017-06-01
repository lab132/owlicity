using System;
using Microsoft.Xna.Framework.Graphics;
using VelcroPhysics;
using VelcroPhysics.Shared;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

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

      ActiveAnimationType = AnimationTypes[0];

      foreach(SpriteAnimationType type in AnimationTypes)
      {
        SpriteAnimationInstance animation = SpriteAnimationFactory.CreateAnimationInstance(type);
        _animInstances.Add(type, animation);
      }

      Vector2 dim = ActiveAnimation.Data.Config.TileDim.ToVector2() * ActiveAnimation.Data.Config.Scale;
      Spatial.Transform.p -= 0.5f * dim;
    }

    public void ChangeActiveAnimation(SpriteAnimationType newAnimationType)
    {
      if(newAnimationType != ActiveAnimationType)
      {
        ActiveAnimation.Stop();
        ActiveAnimationType = newAnimationType;
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

      SpatialData worldSpatial = this.GetWorldSpatialData();
      ActiveAnimation.Draw(batch, worldSpatial);
    }
  }
}
