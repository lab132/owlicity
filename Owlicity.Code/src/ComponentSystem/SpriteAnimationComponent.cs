using System;
using Microsoft.Xna.Framework.Graphics;
using VelcroPhysics;
using VelcroPhysics.Shared;

namespace Owlicity
{
  public class SpriteAnimationComponent : ComponentBase, ISpatial
  {
    public SpriteAnimationType AnimationType { get; set; }

    public SpriteAnimationInstance Animation { get; set; }

    public SpatialData Spatial { get; } = new SpatialData();

    public Transform LocalTransform { get; set; } = new Transform();


    public SpriteAnimationComponent(GameObject owner) : base(owner)
    {
    }

    public override void Initialize()
    {
      base.Initialize();

      Animation = SpriteAnimationFactory.CreateAnimationInstance(AnimationType);
    }

    public override void Update(float deltaSeconds)
    {
      base.Update(deltaSeconds);

      Animation.Update(deltaSeconds);
    }

    public override void Draw(float deltaSeconds, SpriteBatch batch)
    {
      base.Draw(deltaSeconds, batch);

      SpatialData worldSpatial = Spatial.GetWorldSpatialData();
      Animation.Draw(batch, worldSpatial);
    }
  }
}
