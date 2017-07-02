using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Factories;
using VelcroPhysics.Shared;

namespace Owlicity
{
  public enum SpikeTrapOrientation
  {
    None,

    Horizontal,
    Vertical,
  }

  public enum SpikeTrapState
  {
    Searching,
    Attacking,
    WaitingAfterAttacking,
    Returning,
  }

  public class SpikeTrap : GameObject
  {
    public SpatialComponent FixedOrigin;
    public TargetSensorComponent TargetSensor;
    public AABB TargetSensorAABB;

    public BodyComponent MovingBodyComponent;
    public Body MovingBody => MovingBodyComponent.Body;
    public SpriteAnimationComponent Animation;

    public SpikeTrapOrientation Orientation;
    public int Damage = 1;
    public float ForceOnImpact = 0.1f;
    public float SensorThickness = 0.5f;
    public float SensorReach = 6.0f;
    public float AttackSpeed = 8.0f;
    public TimeSpan DelayBeforeReturning = TimeSpan.FromSeconds(1);
    public float ReturnSpeed = 2.0f;
    public SpikeTrapState TrapState;

    public TimeSpan HitCooldown = TimeSpan.FromSeconds(0.5f);
    public TimeSpan CurrentHitCooldown;

    private TimeSpan CurrentWaitTime;


    public SpikeTrap()
    {
      FixedOrigin = new SpatialComponent(this);

      TargetSensor = new TargetSensorComponent(this)
      {
        SensorType = TargetSensorType.Rectangle,
        TargetCollisionCategories = CollisionCategory.Friendly,
      };
      TargetSensor.AttachTo(FixedOrigin);

      MovingBodyComponent = new BodyComponent(this)
      {
        InitMode = BodyComponentInitMode.Manual,
      };
      RootComponent = MovingBodyComponent;

      Animation = new SpriteAnimationComponent(this)
      {
        AnimationTypes = new List<SpriteAnimationType>
        {
          SpriteAnimationType.Shopkeeper_Idle_Front
        },
      };
      Animation.AttachTo(this);
    }

    public override void Initialize()
    {
      Debug.Assert(Orientation != SpikeTrapOrientation.None);

      SpatialData s = this.GetWorldSpatialData();
      FixedOrigin.Spatial.CopyFrom(s);

      float halfThickness = 0.5f * SensorThickness;
      float halfReach = 0.5f * SensorReach;
      switch(Orientation)
      {
        case SpikeTrapOrientation.Horizontal:
        {
          TargetSensor.RectangleSensorLocalAABB = new AABB
          {
            LowerBound = new Vector2(-halfReach, -halfThickness),
            UpperBound = new Vector2(halfReach, halfThickness),
          };
        }
        break;

        case SpikeTrapOrientation.Vertical:
        {
          TargetSensor.RectangleSensorLocalAABB = new AABB
          {
            LowerBound = new Vector2(-halfThickness, -halfReach),
            UpperBound = new Vector2(halfThickness, halfReach),
          };
        }
        break;

        default: throw new ArgumentException(nameof(Orientation));
      }

      {
        Body body = BodyFactory.CreateRectangle(
          world: Global.Game.World,
          bodyType: BodyType.Kinematic,
          position: s.Position,
          rotation: s.Rotation.Radians,
          width: 1,
          height: 1,
          density: Global.OwliverDensity);

        body.FixedRotation = true;
        body.CollisionCategories = CollisionCategory.Enemy | CollisionCategory.EnemyWeapon;
        body.OnCollision += MovingBody_OnCollision;

        MovingBodyComponent.Body = body;
      }

      base.Initialize();

      TargetSensorAABB = TargetSensor.Body.ComputeAABB();
    }

    private void MovingBody_OnCollision(Fixture ourFixture, Fixture theirFixture, VelcroPhysics.Collision.ContactSystem.Contact contact)
    {
      if(CurrentHitCooldown == TimeSpan.Zero)
      {
        Global.HandleDefaultHit(theirFixture.Body, ourFixture.Body.Position, Damage, ForceOnImpact);
        CurrentHitCooldown = HitCooldown;
      }

      switch(TrapState)
      {
        case SpikeTrapState.Attacking:
        case SpikeTrapState.Returning:
        {
          EnterState(SpikeTrapState.WaitingAfterAttacking);
        }
        break;
      }
    }

    private void EnterState(SpikeTrapState newState)
    {
      switch(newState)
      {
        case SpikeTrapState.Searching:
        {
          MovingBody.LinearVelocity = Vector2.Zero;
        }
        break;

        case SpikeTrapState.Attacking:
        {
        }
        break;

        case SpikeTrapState.WaitingAfterAttacking:
        {
          CurrentWaitTime = TimeSpan.Zero;
          MovingBody.LinearVelocity = Vector2.Zero;
        }
        break;

        case SpikeTrapState.Returning:
        {
        }
        break;

        default: throw new ArgumentException(nameof(newState));
      }

      TrapState = newState;
    }

    public override void Update(float deltaSeconds)
    {
      base.Update(deltaSeconds);

      switch(TrapState)
      {
        case SpikeTrapState.Searching:
        {
          Body target = TargetSensor.CurrentMainTarget;
          if(target != null)
          {
            SpatialData originSpatial = FixedOrigin.GetWorldSpatialData();
            Vector2 attackDir;
            switch(Orientation)
            {
              case SpikeTrapOrientation.Horizontal:
              {
                float deltaX = target.Position.X - originSpatial.Position.X;
                attackDir = new Vector2(Math.Sign(deltaX), 0);
              }
              break;

              case SpikeTrapOrientation.Vertical:
              {
                float deltaY = target.Position.Y - originSpatial.Position.Y;
                attackDir = new Vector2(0, Math.Sign(deltaY));
              }
              break;

              default: throw new ArgumentException(nameof(Orientation));
            }

            MovingBody.LinearVelocity = attackDir * AttackSpeed;

            EnterState(SpikeTrapState.Attacking);
          }
        }
        break;

        case SpikeTrapState.Attacking:
        {
          Vector2 movementDir = MovingBody.LinearVelocity.GetNormalizedSafe();
          SpatialData originSpatial = FixedOrigin.GetWorldSpatialData();
          Vector2 attackPosition = originSpatial.Position + movementDir * SensorReach;

          bool isAtEnd;
          switch(Orientation)
          {
            case SpikeTrapOrientation.Horizontal:
            {
              if(movementDir.X > 0)
                isAtEnd = MovingBody.Position.X >= attackPosition.X;
              else
                isAtEnd = MovingBody.Position.X <= attackPosition.X;
            }
            break;

            case SpikeTrapOrientation.Vertical:
            {
              if(movementDir.Y > 0)
                isAtEnd = MovingBody.Position.Y >= attackPosition.Y;
              else
                isAtEnd = MovingBody.Position.Y <= attackPosition.Y;
            }
            break;

            default: throw new ArgumentException(nameof(Orientation));
          }

          if(isAtEnd)
          {
            EnterState(SpikeTrapState.WaitingAfterAttacking);
            MovingBody.Position = attackPosition;
          }
        }
        break;

        case SpikeTrapState.WaitingAfterAttacking:
        {
          CurrentWaitTime += TimeSpan.FromSeconds(deltaSeconds);
          if(CurrentWaitTime >= DelayBeforeReturning)
          {
            EnterState(SpikeTrapState.Returning);

            Vector2 originDelta = FixedOrigin.GetWorldSpatialData().Position - MovingBody.Position;
            Vector2 originDir = originDelta.GetNormalizedSafe();
            MovingBody.LinearVelocity = originDir * ReturnSpeed;
          }
        }
        break;

        case SpikeTrapState.Returning:
        {
          Vector2 movementDir = MovingBody.LinearVelocity.GetNormalizedSafe();
          SpatialData originSpatial = FixedOrigin.GetWorldSpatialData();

          bool isAtEnd;
          switch(Orientation)
          {
            case SpikeTrapOrientation.Horizontal:
            {
              if(movementDir.X > 0)
                isAtEnd = MovingBody.Position.X >= originSpatial.Position.X;
              else
                isAtEnd = MovingBody.Position.X <= originSpatial.Position.X;
            }
            break;

            case SpikeTrapOrientation.Vertical:
            {
              if(movementDir.Y > 0)
                isAtEnd = MovingBody.Position.Y >= originSpatial.Position.Y;
              else
                isAtEnd = MovingBody.Position.Y <= originSpatial.Position.Y;
            }
            break;

            default: throw new ArgumentException(nameof(Orientation));
          }

          if(isAtEnd)
          {
            EnterState(SpikeTrapState.Searching);
            MovingBody.Position = originSpatial.Position;
            MovingBody.Awake = false;
          }
        }
        break;

        default: throw new ArgumentException(nameof(TrapState));
      }

      if(CurrentHitCooldown > TimeSpan.Zero)
      {
        CurrentHitCooldown -= TimeSpan.FromSeconds(deltaSeconds);
        if(CurrentHitCooldown <= TimeSpan.Zero)
        {
          CurrentHitCooldown = TimeSpan.Zero;
        }
      }

      {
        Global.Game.DebugDrawCommands.Add(view =>
        {
          view.DrawString(20, 400, $"{TrapState}");
        });
      }
    }
  }
}
