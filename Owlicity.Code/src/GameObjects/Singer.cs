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
    public TargetSensorComponent TargetSensor;
    public SpriteAnimationComponent Animation;

    public float Reach = 3.0f;

    public Projectile CurrentProjectile;
    public TimeSpan ProjectileLaunchCooldown = TimeSpan.FromSeconds(0.5f);
    public TimeSpan ProjectileTimeToLive = TimeSpan.FromSeconds(5);

    private float CurrentProjectileLaunchCooldown;

    public bool CanLaunchProjectile => CurrentProjectile == null && CurrentProjectileLaunchCooldown == 0.0f;


    public Singer()
    {
      BodyComponent = new BodyComponent(this)
      {
        InitMode = BodyComponentInitMode.Manual,
      };
      RootComponent = BodyComponent;

      TargetSensor = new TargetSensorComponent(this)
      {
        SensorType = TargetSensorType.Circle,
        CircleSensorRadius = Reach,
        TargetCollisionCategories = CollisionCategory.Friendly,
      };
      TargetSensor.AttachTo(RootComponent);

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

      base.Initialize();
    }

    public override void Update(float deltaSeconds)
    {
      if(CurrentProjectile == null && CurrentProjectileLaunchCooldown > 0)
      {
        CurrentProjectileLaunchCooldown -= deltaSeconds;
        if(CurrentProjectileLaunchCooldown < 0)
        {
          CurrentProjectileLaunchCooldown = 0.0f;
        }
      }

      if(CanLaunchProjectile && TargetSensor.CurrentMainTarget != null)
      {
        Vector2 targetPosition = TargetSensor.CurrentMainTarget.GetWorldSpatialData().Position;
        if(CurrentProjectile == null)
        {
          Vector2 myPosition = this.GetWorldSpatialData().Position;
          (targetPosition - myPosition).GetDirectionAndLength(out Vector2 targetDir, out float targetDistance);

          const float speed = 1.8f;
          Projectile projectile = new Projectile
          {
            MaxSpeed = speed,
            CollisionCategories = CollisionCategory.EnemyWeapon,
            CollidesWith = CollisionCategory.World | CollisionCategory.AnyFriendly,
          };
          projectile.Animation.AnimationTypes = new List<SpriteAnimationType> { SpriteAnimationType.Bonbon_Gold };
          float relativeSpawnOffset = 0.5f; // meters
          projectile.Spatial.Position = myPosition + targetDir * relativeSpawnOffset;

          projectile.BodyComponent.BeforePostInitialize += () =>
          {
            projectile.BodyComponent.Body.LinearVelocity = targetDir * speed;
          };

          Global.CreateDefaultHomingCircle(projectile, projectile.BodyComponent,
            sensorRadius: 2.0f,
            homingType: HomingType.ConstantAcceleration,
            homingSpeed: 0.05f * speed);

          projectile.AutoDestruct.DestructionDelay = ProjectileTimeToLive;


          CurrentProjectile = projectile;
          projectile.AutoDestruct.BeforeDestroy += () =>
          {
            CurrentProjectile = null;
          };

          CurrentProjectileLaunchCooldown = (float)ProjectileLaunchCooldown.TotalSeconds;

          Global.Game.AddGameObject(projectile);
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
