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
    //
    // Initialization data.
    //
    public GameObjectType EnemyType { get; set; }
    public float HitDuration = 0.25f;

    // Only start chasing if Owliver is closer than this.
    public float DetectionDistance { get; set; } = 2.5f;

    // Don't get closer than this.
    public float MinimumDistance { get; set; } = 0.01f;

    // Chase owliver at this speed.
    public float ChasingSpeed { get; set; } = 0.01f;

    // The amount of damage caused by this enemy.
    public int Damage = 1;

    // Weight of the impulse when hitting owliver.
    public float ForceOnImpact = 0.05f;

    //
    // Runtime data.
    //
    public bool IsChasing;

    public BodyComponent BodyComponent;
    public MovementComponent Movement;
    public HealthComponent Health;

    //
    // Optional components
    //
    public SquashComponent Squasher;
    public ChaserComponent Chaser;

    public Body MyBody => BodyComponent.Body;

    public EnemyComponent(GameObject owner) : base(owner)
    {
    }

    public override void Initialize()
    {
      base.Initialize();

      if(BodyComponent == null)
      {
        BodyComponent = Owner.GetComponent<BodyComponent>();
        Debug.Assert(BodyComponent != null);
      }

      if (Movement == null)
      {
        Movement = Owner.GetComponent<MovementComponent>();
        Debug.Assert(Movement != null);
      }

      if(Squasher == null)
      {
        Squasher = Owner.GetComponent<SquashComponent>();
      }

      if(Chaser == null)
      {
        Chaser = Owner.GetComponent<ChaserComponent>();
      }

      if(Health == null)
      {
        Health = Owner.GetComponent<HealthComponent>();
        Debug.Assert(Health != null);
      }
    }

    public override void PostInitialize()
    {
      base.PostInitialize();

      switch (EnemyType)
      {
        case GameObjectType.Slurp:
        {
          Movement.MaxMovementSpeed = 0.5f;
          Body body = BodyComponent.Body;
          body.OnCollision += OnCollisionWithOwliver;
        }
        break;

        default: throw new NotImplementedException();
      }

      Health.OnHit += (damage) =>
      {
        Health.MakeInvincible(HitDuration);
      };

      Health.OnDeath += (damage) =>
      {
        Global.Game.RemoveGameObject(Owner);

        var go = new GameObject();
        Owner.Spatial.CopyTo(go.Spatial);

        var adc = new AutoDestructComponent(go)
        {
          SecondsUntilDestruction = 1.0f,
        };

        var pec = new ParticleEmitterComponent(go)
        {
          NumParticles = 512,

          TextureContentNames = new[]
          {
            "confetti/confetti_01",
            "confetti/confetti_02",
            "confetti/confetti_03",
            "confetti/confetti_04",
            "confetti/confetti_05",
            "confetti/confetti_06",
            "confetti/confetti_07",
          },

          AvailableColors = Global.AllConfettiColors,
        };

        pec.BeforePostInitialize += delegate ()
        {
          pec.Emitter.MaxTTL = 0.8f * adc.SecondsUntilDestruction;
          pec.Emitter.MaxParticleSpread = 0.05f;
          pec.Emitter.MaxParticleSpeed = 5f;
          pec.Emit(go.GetWorldSpatialData().Position);
        };

        Global.Game.AddGameObject(go);
      };

      if (Squasher != null)
      {
        Squasher.SetupDefaultSquashData(HitDuration);
        Health.OnHit += (damage) =>
        {
          Squasher.StartSquashing();
        };
      }

      if(Chaser != null)
      {
        // Disable chasing when we are invincible.
        ISpatial previousTarget = null;
        Health.OnInvincibilityGained += () =>
        {
          Debug.Assert(previousTarget == null);
          previousTarget = Chaser.Target;
          Chaser.Target = null;
        };

        Health.OnInvincibilityLost += () =>
        {
          Chaser.Target = previousTarget;
          previousTarget = null;
        };
      }
    }

    private void OnCollisionWithOwliver(Fixture fixtureA, Fixture fixtureB, Contact contact)
    {
      Debug.Assert(((BodyComponent)fixtureA.UserData).Owner == Owner);

      Body hitBody = fixtureB.Body;

      GameObject go = ((BodyComponent)hitBody.UserData).Owner;
      bool sendItToHell = true;

      // Handle health component
      HealthComponent hc = go.GetComponent<HealthComponent>();
      if (hc != null)
      {
        if (!hc.IsInvincible)
        {
          hc.Hit(Damage);
        }
        else
        {
          sendItToHell = false;
        }
      }

      if (sendItToHell)
      {
        // Apply impulse
        Vector2 deltaPosition = hitBody.Position - BodyComponent.Body.Position;
        deltaPosition.GetDirectionAndLength(out Vector2 dir, out float distance);
        Vector2 impulse = ForceOnImpact * dir;
        hitBody.ApplyLinearImpulse(impulse);
      }
    }

    public override void PrePhysicsUpdate(float deltaSeconds)
    {
      base.PrePhysicsUpdate(deltaSeconds);

      // Only move when not being hit.
      if(!Health.IsInvincible)
      {
        Vector2 owliverPosition = Global.Game.Owliver.GetWorldSpatialData().Position;
        Vector2 myPosition = Owner.GetWorldSpatialData().Position;

        Vector2 deltaVector = owliverPosition - myPosition;
        deltaVector.GetDirectionAndLength(out Vector2 owliverDir, out float owliverDistance);

        Vector2 movementVector = Movement.ConsumeMovementVector();
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

#if false
        Movement.PerformMovement(movementVector, deltaSeconds);
#endif
      }
    }

    public override void Update(float deltaSeconds)
    {
      base.Update(deltaSeconds);

      Global.Game.DebugDrawCommands.Add(view =>
      {
        Color color = Health.IsInvincible ? Color.Red : IsChasing ? Color.Yellow : Color.Green;
        view.DrawCircle(Owner.GetWorldSpatialData().Position, DetectionDistance, color);
      });
    }
  }
}
