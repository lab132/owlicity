using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelcroPhysics.Factories;

namespace Owlicity
{
  public class BonbonPickup : GameObject
  {
    public BodyComponent BodyComponent;
    public SpriteAnimationComponent Animation;
    public MoneyBagComponent MoneyBag;
    public PickupComponent Pickup;
    public HomingComponent Homing;

    public BonbonType BonbonType;

    public BonbonPickup()
    {
      BodyComponent = new BodyComponent(this)
      {
        InitMode = BodyComponentInitMode.Manual,
      };

      BodyComponent.BeforeInitialize += () =>
      {
      };
      this.RootComponent = BodyComponent;

      Animation = new SpriteAnimationComponent(this)
      {
        AnimationTypes = new List<SpriteAnimationType>(),
      };
      Animation.AttachTo(BodyComponent);

      MoneyBag = new MoneyBagComponent(this)
      {
        InitialAmount = 10,
      };

      Homing = new HomingComponent(this)
      {
        BodyComponent = BodyComponent,
        TargetRange = 1.0f,
        Speed = 3.0f,

        DebugDrawingEnabled = true,
      };
      Homing.AttachTo(BodyComponent);

      Pickup = new PickupComponent(this)
      {
        BodyComponent = BodyComponent,
      };
    }

    public override void Initialize()
    {
      SpatialData s = BodyComponent.GetWorldSpatialData();
      BodyComponent.Body = BodyFactory.CreateCircle(
        world: Global.Game.World,
        radius: 0.2f,
        density: 0.5f * Global.OwliverDensity,
        position: s.Position,
        userData: BodyComponent);
      BodyComponent.Body.IsSensor = true;
      BodyComponent.Body.CollidesWith = Global.OwliverCollisionCategory;

      SpriteAnimationType animType = SpriteAnimationType.Bonbon_Gold + (int)BonbonType;
      Animation.AnimationTypes.Add(animType);

      Homing.Target = Global.Game.Owliver.Center;

      base.Initialize();
    }
  }
}
