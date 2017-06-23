using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using VelcroPhysics.Factories;
using VelcroPhysics.Dynamics;
using System.Diagnostics;
using VelcroPhysics.Collision.ContactSystem;
using VelcroPhysics.Shared;

namespace Owlicity
{
  public class Gate : GameObject
  {
    public SpatialComponent LeftEdgeOffset;
    public BodyComponent InnerBodyComponent;
    public BodyComponent OuterBodyComponent;
    public BodyComponent Trigger;
    public SpriteAnimationComponent Animation;

    public KeyType KeyTypeToUnlock;

    public bool IsOpen => Trigger.Body == null;

    public Gate()
    {
      LeftEdgeOffset = new SpatialComponent(this);
      {
        SpriteAnimationData anim = SpriteAnimationFactory.GetAnimation(SpriteAnimationType.Gate_Closed);
        Vector2 hotspot = anim.Config.Hotspot * anim.Config.Scale;
        float offset = Conversion.ToMeters(hotspot.X - 128);
        LeftEdgeOffset.Spatial.Position.X -= offset;
      }
      LeftEdgeOffset.AttachTo(this);

      InnerBodyComponent = new BodyComponent(this)
      {
        InitMode = BodyComponentInitMode.Manual,
      };
      InnerBodyComponent.AttachTo(LeftEdgeOffset);

      OuterBodyComponent = new BodyComponent(this)
      {
        InitMode = BodyComponentInitMode.Manual,
      };
      OuterBodyComponent.AttachTo(LeftEdgeOffset);

      Trigger = new BodyComponent(this)
      {
        InitMode = BodyComponentInitMode.Manual,
      };
      Trigger.AttachTo(this);

      Animation = new SpriteAnimationComponent(this)
      {
        AnimationTypes = new List<SpriteAnimationType>
        {
           SpriteAnimationType.Gate_Closed,
           SpriteAnimationType.Gate_Open,
        },
      };
      Animation.AttachTo(this);
    }

    public override void Initialize()
    {
      float outerLeft = Conversion.ToMeters(310);
      float innerLeft = Conversion.ToMeters(79);
      float inner = Conversion.ToMeters(80);
      float innerRight = Conversion.ToMeters(79);
      float outerRight = Conversion.ToMeters(222);
      float width = Conversion.ToMeters(768);
      float height = Conversion.ToMeters(128);
      float barrierHeight = Conversion.ToMeters(20);
      float density = Global.OwliverDensity;

      //
      // Inner body
      //
      {
        // TODO(manu): Set InnerBodyComponent.Spatial.LocalAABB.
        SpatialData s = InnerBodyComponent.GetWorldSpatialData();
        InnerBodyComponent.Body = BodyFactory.CreateRectangle(
          world: Global.Game.World,
          width: innerLeft + inner + innerRight,
          height: barrierHeight,
          density: density,
          position: s.Position + new Vector2(outerLeft + innerLeft + 0.5f * inner, 0.0f),
          rotation: s.Rotation.Radians,
          bodyType: BodyType.Static,
          userData: InnerBodyComponent);
      }

      //
      // Outer body
      //
      {
        // TODO(manu): Set OuterBodyComponent.Spatial.LocalAABB.
        SpatialData s = OuterBodyComponent.GetWorldSpatialData();
        OuterBodyComponent.Body = BodyFactory.CreateBody(
          world: Global.Game.World,
          position: s.Position,
          rotation: s.Rotation.Radians,
          bodyType: BodyType.Static,
          userData: OuterBodyComponent);

        Vector2 offsetRight = Conversion.ToMeters(300, 0);
        FixtureFactory.AttachRectangle(
          body: OuterBodyComponent.Body,
          width: outerLeft,
          height: barrierHeight,
          offset: new Vector2(0.5f * outerLeft, 0.0f),
          density: density,
          userData: OuterBodyComponent);
        FixtureFactory.AttachRectangle(
          body: OuterBodyComponent.Body,
          width: outerRight,
          height: barrierHeight,
          offset: new Vector2(width - 0.5f * outerRight, 0.0f),
          density: density,
          userData: OuterBodyComponent);
      }

      //
      // Trigger body
      //
      {
        Vector2 dim = Conversion.ToMeters(60, 130);
        Trigger.Spatial.LocalAABB = new AABB
        {
          LowerBound = -0.5f * dim,
          UpperBound = 0.5f * dim,
        };

        SpatialData s = Trigger.GetWorldSpatialData();
        s.Position.Y -= Conversion.ToMeters((0.5f * 128) - 20);
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
        Trigger.Body.OnCollision += OnCollisionWithTrigger;
      }

      base.Initialize();
    }

    private void OnCollisionWithTrigger(Fixture myFixture, Fixture theirFixture, Contact contact)
    {
      Debug.Assert(myFixture.Body == Trigger.Body);

      GameObject go = ((BodyComponent)theirFixture.UserData).Owner;

      foreach(KeyRingComponent keyRing in go.GetComponents<KeyRingComponent>())
      {
        if(keyRing[KeyTypeToUnlock] > 0)
        {
          keyRing[KeyTypeToUnlock]--;

          Trigger.Body.OnCollision -= OnCollisionWithTrigger;
          Global.Game.World.RemoveBody(Trigger.Body);
          Trigger.Body = null;

          if(Animation != null)
          {
            Animation.ChangeActiveAnimation(SpriteAnimationType.Gate_Open);
          }

          Body blockingBody = InnerBodyComponent.Body;
          if(blockingBody != null)
          {
            Global.Game.World.RemoveBody(blockingBody);
            InnerBodyComponent.Body = null;
          }

          break;
        }
      }
    }
  }
}
