using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using VelcroPhysics.Collision.ContactSystem;
using VelcroPhysics.Dynamics;

namespace Owlicity
{
  public class EnemyComponent : ComponentBase
  {
    public GameObjectType EnemyType { get; set; }

    // Only start chasing of Owliver is closer than this.
    public float DetectionDistance { get; set; } = 2.5f;

    // Don't get closer than this.
    public float MinimumDistance { get; set; } = 0.01f;

    // Chase owliver at this speed.
    public float ChasingSpeed { get; set; } = 0.01f;

    public bool IsChasing;

    public MovementComponent MovementComponent { get; set; }
    public SquashComponent SquashComponent { get; set; }

    public float DefaultHitDuration = 0.25f;
    public float CurrentHitDuration;
    public float CurrentHitTime;

    public bool IsHit => CurrentHitDuration > 0.0f;

    public EnemyComponent(GameObject owner) : base(owner)
    {
    }

    public override void Initialize()
    {
      base.Initialize();

      if(MovementComponent == null)
      {
        MovementComponent = Owner.GetComponent<MovementComponent>();
      }

      if(SquashComponent == null)
      {
        SquashComponent = Owner.GetComponent<SquashComponent>();
      }

      switch(EnemyType)
      {
        case GameObjectType.Slurp:
        {
          MovementComponent.MaxMovementSpeed = 0.5f;
        }
        break;

        default: throw new NotImplementedException();
      }
    }

    public override void PostInitialize()
    {
      base.PostInitialize();

      var bc = Owner.GetComponent<BodyComponent>();
      var pec = Owner.GetComponent<ParticleEmitterComponent>();

      var owliverBC = Global.Game.Owliver.GetComponent<BodyComponent>();
      var owliverSqc = Global.Game.Owliver.GetComponent<SquashComponent>();
      bc.Body.OnCollision += delegate (Fixture fixtureA, Fixture fixtureB, Contact contact)
      {
        Debug.Assert(fixtureA.UserData != owliverBC);
        Debug.Assert(fixtureA.UserData == bc);

        if(fixtureB.UserData == owliverBC)
        {
          Vector2 contactWorldPosition = fixtureB.Body.Position + contact.Manifold.LocalPoint;
          pec.Emit(contactWorldPosition, 40);

          owliverSqc.SetupDefaultSquashData(0.25f);
          owliverSqc.StartSquashing();
        }
      };
    }

    public override void PrePhysicsUpdate(float deltaSeconds)
    {
      Vector2 movementVector = MovementComponent.ConsumeMovementVector();
      base.PrePhysicsUpdate(deltaSeconds);

      // Only move when not being hit.
      if(!IsHit)
      {
        Vector2 owliverPosition = Global.Game.Owliver.GetWorldSpatialData().Position;
        Vector2 myPosition = Owner.GetWorldSpatialData().Position;

        Vector2 deltaVector = owliverPosition - myPosition;
        deltaVector.GetDirectionAndLength(out Vector2 owliverDir, out float owliverDistance);

        switch(EnemyType)
        {
          case GameObjectType.Slurp:
          {
            if(owliverDistance > MinimumDistance && owliverDistance < DetectionDistance)
            {
              movementVector += owliverDir * ChasingSpeed;
              IsChasing = true;
            }
            else
            {
              IsChasing = false;
            }
          }
          break;

          default: throw new NotImplementedException();
        }

#if true
        MovementComponent.PerformMovement(movementVector, deltaSeconds);
#endif
      }
    }

    public override void Update(float deltaSeconds)
    {
      base.Update(deltaSeconds);

      if(IsHit)
      {
        CurrentHitTime += deltaSeconds;
        if(CurrentHitTime >= CurrentHitDuration)
        {
          CurrentHitDuration = 0.0f;
        }
        else
        {
          // TODO(manu): Do something while being hit?
        }
      }

      Global.Game.DebugDrawCommands.Add(view =>
      {
        Color color = IsHit ? Color.Red : IsChasing ? Color.Yellow : Color.Green;
        view.DrawCircle(Owner.GetWorldSpatialData().Position, DetectionDistance, color);
      });
    }

    public void Hit(float strength)
    {
      CurrentHitDuration = strength * DefaultHitDuration;
      CurrentHitTime = 0.0f;

      SquashComponent.SetupDefaultSquashData(CurrentHitDuration);
      SquashComponent.StartSquashing();

      // TODO(manu): Particle effects!
    }
  }
}
