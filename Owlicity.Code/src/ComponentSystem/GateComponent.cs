using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelcroPhysics.Collision.ContactSystem;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Factories;
using VelcroPhysics.Shared;

namespace Owlicity
{
  public class GateComponent : SpatialComponent
  {
    //
    // Initialization data.
    //
    public Vector2 Dimensions;
    public BodyComponent UnlockableBlockade;

    //
    // Runtime data.
    //
    public BodyComponent Trigger;
    public bool IsOpen => Trigger == null;

    public SpriteAnimationComponent Animation;


    public GateComponent(GameObject owner)
      : base(owner)
    {
      Trigger = new BodyComponent(owner)
      {
        InitMode = BodyComponentInitMode.Manual,
      };
    }

    public override void Initialize()
    {
      base.Initialize();

      if(Animation == null)
      {
        Animation = Owner.GetComponent<SpriteAnimationComponent>();
      }

      Spatial.LocalAABB = new AABB
      {
        LowerBound = -0.5f * Dimensions,
        UpperBound = 0.5f * Dimensions,
      };

      SpatialData s = this.GetWorldSpatialData();
      Trigger.Body = BodyFactory.CreateRectangle(
        world: Global.Game.World,
        width: s.LocalAABB.Width,
        height: s.LocalAABB.Height,
        density: 0,
        position: s.Position,
        rotation: s.Rotation.Radians,
        bodyType: BodyType.Static,
        userData: Trigger);
      Trigger.Body.IsSensor = true;
      Trigger.Body.CollidesWith = Global.OwliverCollisionCategory;
      Trigger.Body.OnCollision += OnCollision;
    }

    private void OnCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
    {
      Debug.Assert(fixtureA.Body == Trigger.Body);

      GameObject go = ((BodyComponent)fixtureB.UserData).Owner;

      foreach(KeyRingComponent keyRing in go.GetComponents<KeyRingComponent>())
      {
        const int keyIndex = (int)KeyType.Gold;
        if(keyRing.CurrentKeyAmounts[keyIndex] > 0)
        {
          keyRing.CurrentKeyAmounts[keyIndex]--;

          Trigger.Body.OnCollision -= OnCollision;
          Global.Game.World.RemoveBody(Trigger.Body);
          Trigger.Body = null;

          if(Animation != null)
          {
            Animation.ChangeActiveAnimation(SpriteAnimationType.Gate_Open);
          }

          Body blockingBody = UnlockableBlockade?.Body;
          if(blockingBody != null)
          {
            Global.Game.World.RemoveBody(blockingBody);
            UnlockableBlockade.Body = null;
          }

          break;
        }
      }
    }
  }
}
