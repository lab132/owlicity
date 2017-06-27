using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelcroPhysics.Collision.ContactSystem;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Factories;

namespace Owlicity
{
  public class Singer : GameObject
  {
    public BodyComponent BodyComponent;
    public BodyComponent Trigger;
    public SpriteAnimationComponent Animation;

    public float Reach = 3.0f;

    public GameObject CurrentTarget;
    public Projectile CurrentProjectile;

    public Singer()
    {
      BodyComponent = new BodyComponent(this)
      {
        InitMode = BodyComponentInitMode.Manual,
      };
      RootComponent = BodyComponent;

      Trigger = new BodyComponent(this)
      {
        InitMode = BodyComponentInitMode.Manual,
      };
      Trigger.AttachTo(RootComponent);

      Animation = new SpriteAnimationComponent(this)
      {
        AnimationTypes = new List<SpriteAnimationType>
        {
          SpriteAnimationType.Singer_Idle_Left,
          SpriteAnimationType.Singer_Idle_Right,
        },
      };
      Animation.AttachTo(RootComponent);
    }

    public override void Initialize()
    {
      {
        float halfWidth = Conversion.ToMeters(80) * Global.SingerScale.X;
        SpatialData s = this.GetWorldSpatialData();
        Body body = BodyFactory.CreateCapsule(
          world: Global.Game.World,
          bodyType: BodyType.Dynamic,
          position: s.Position,
          rotation: s.Rotation.Radians,
          height: Conversion.ToMeters(240) * Global.SingerScale.Y - 2.0f * halfWidth,
          endRadius: halfWidth,
          density: Global.OwliverDensity);
        body.LinearDamping = 10.0f;
        body.FixedRotation = true;
        body.CollisionCategories = CollisionCategory.Enemy;

        BodyComponent.Body = body;
      }

      {
        SpatialData s = Trigger.GetWorldSpatialData();
        Body body = BodyFactory.CreateCircle(
          world: Global.Game.World,
          bodyType: BodyType.Static,
          position: s.Position,
          radius: Reach,
          density: 0);
        body.IsSensor = true;
        body.CollidesWith = CollisionCategory.Friendly;
        body.OnCollision += OnCollisionWithTrigger;
        body.OnSeparation += OnSeparationWithTrigger;

        Trigger.Body = body;
      }

      base.Initialize();
    }

    private void OnCollisionWithTrigger(Fixture myFixture, Fixture theirFixture, Contact contact)
    {
      GameObject target = ((BodyComponent)theirFixture.UserData).Owner;
      if(CurrentTarget == null)
      {
        CurrentTarget = target;
      }
    }

    private void OnSeparationWithTrigger(Fixture myFixture, Fixture theirFixture, Contact contact)
    {
      GameObject target = ((BodyComponent)theirFixture.UserData).Owner;
      if(CurrentTarget == target)
      {
        CurrentTarget = null;
      }
    }

    public override void Update(float deltaSeconds)
    {
      Vector2 myPosition = this.GetWorldSpatialData().Position;
      Trigger.Body.Position = myPosition;

      if(CurrentTarget != null)
      {
        Vector2 targetPosition = CurrentTarget.GetWorldSpatialData().Position;
        (targetPosition - myPosition).GetDirectionAndLength(out Vector2 targetDir, out float targetDistance);

        if(CurrentProjectile == null)
        {
          float speed = 5.0f;
          CurrentProjectile = new Projectile
          {
            MaxSpeed = speed,
            CollisionCategories = CollisionCategory.EnemyWeapon,
            CollidesWith = CollisionCategory.World | CollisionCategory.AnyFriendly,
          };
          CurrentProjectile.Animation.AnimationTypes = new List<SpriteAnimationType> { SpriteAnimationType.Bonbon_Gold };
          CurrentProjectile.Spatial.Position = myPosition + targetDir * 0.5f;

          CurrentProjectile.BodyComponent.BeforePostInitialize += () =>
          {
            CurrentProjectile.BodyComponent.Body.LinearVelocity = targetDir * speed;
          };

          var hoc = new HomingComponent(CurrentProjectile)
          {
            BodyComponent = CurrentProjectile.BodyComponent,
            Target = CurrentTarget,
            TargetRange = 1.0f,
            Speed = 0.25f * speed,

            DebugDrawingEnabled = true,
          };
          hoc.AttachTo(CurrentProjectile);

          CurrentProjectile.AutoDestruct.DestructionDelay = TimeSpan.FromSeconds(1);
          CurrentProjectile.AutoDestruct.BeforeDestroy += () =>
          {
            CurrentProjectile = null;
          };

          Global.Game.AddGameObject(CurrentProjectile);
        }

        Global.Game.DebugDrawCommands.Add(view =>
        {
          view.DrawCircle(targetPosition, 1.0f, Color.Red);
        });
      }
      base.Update(deltaSeconds);
    }
  }
}
