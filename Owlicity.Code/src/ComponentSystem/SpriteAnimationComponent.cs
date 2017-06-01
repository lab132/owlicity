using System;
using Microsoft.Xna.Framework.Graphics;
using VelcroPhysics;
using VelcroPhysics.Shared;

namespace Owlicity
{
  public class SpriteAnimationComponent : SpatialComponent
  {
    //
    // Input data
    //
    public SpriteAnimationType AnimationType { get; set; }

    //
    // Runtime data
    //
    public SpriteAnimationInstance Animation { get; set; }


    public SpriteAnimationComponent(GameObject owner) : base(owner)
    {
    }

    public override void Initialize()
    {
      base.Initialize();

      Animation = SpriteAnimationFactory.CreateAnimationInstance(AnimationType);
      Spatial.Transform.p -= 0.5f * Animation.Data.Config.TileDim.ToVector2();
    }

    public override void Update(float deltaSeconds)
    {
      base.Update(deltaSeconds);

      Animation.Update(deltaSeconds);
    }

    public override void Draw(float deltaSeconds, SpriteBatch batch)
    {
      base.Draw(deltaSeconds, batch);

      SpatialData worldSpatial = this.GetWorldSpatialData();
      Animation.Draw(batch, worldSpatial);
    }
  }
}
