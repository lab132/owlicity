using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Factories;

namespace Owlicity
{
  public class KeyPickup : GameObject
  {
    public BodyComponent BodyComponent;
    public SpriteAnimationComponent Animation;
    public KeyRingComponent KeyRing;
    public PickupComponent Pickup;
    public HomingComponent Homing;

    public KeyType KeyType;

    public KeyPickup()
    {
      BodyComponent = new BodyComponent(this)
      {
        InitMode = BodyComponentInitMode.Manual,
      };
      this.RootComponent = BodyComponent;

      Animation = new SpriteAnimationComponent(this)
      {
        AnimationTypes = new List<SpriteAnimationType>(),
      };
      Animation.AttachTo(RootComponent);

      KeyRing = new KeyRingComponent(this);

      Pickup = new PickupComponent(this)
      {
        BodyComponent = BodyComponent,
      };

      Homing = new HomingComponent(this)
      {
        BodyComponent = BodyComponent,
        TargetRange = 1.0f,
        Speed = 3.0f,

        DebugDrawingEnabled = true,
      };
      Homing.AttachTo(RootComponent);
    }

    public override void Initialize()
    {
      SpatialData s = BodyComponent.GetWorldSpatialData();
      BodyComponent.Body = BodyFactory.CreateCircle(
        world: Global.Game.World,
        bodyType: BodyType.Static,
        radius: 0.2f,
        density: 2 * Global.OwliverDensity,
        position: s.Position);
      BodyComponent.Body.IsSensor = true;
      BodyComponent.Body.CollidesWith = CollisionCategory.Friendly;

      SpriteAnimationType animType = SpriteAnimationType.Key_Gold + (int)KeyType;
      Animation.AnimationTypes.Add(animType);

      KeyRing[KeyType] = 1;

      Homing.Target = Global.Game.Owliver.Center;

      base.Initialize();
    }
  }
}
