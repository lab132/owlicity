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

    public SpriteAnimationType AnimationType_Idle_Left;
    public SpriteAnimationType AnimationType_Idle_Right;

    //
    // Runtime data.
    //
    public bool IsChasing;

    public BodyComponent BodyComponent;
    public MovementComponent Movement;
    public HealthComponent Health;
    public SpriteAnimationComponent Animation;

    //
    // Optional components
    //
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

      if(Movement == null)
      {
        Movement = Owner.GetComponent<MovementComponent>();
        Debug.Assert(Movement != null);
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

      if(Animation == null)
      {
        Animation = Owner.GetComponent<SpriteAnimationComponent>();
        Debug.Assert(Animation != null);
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

    public override void Update(float deltaSeconds)
    {
      base.Update(deltaSeconds);

      if(Chaser.IsChasing)
      {
        // Change the facing direction to the target.
        Vector2 myPosition = Owner.GetWorldSpatialData().Position;
        Vector2 targetPosition = Chaser.Target.GetWorldSpatialData().Position;
        if(targetPosition.X < myPosition.X)
        {
          Animation.ChangeActiveAnimation(AnimationType_Idle_Left);
        }
        else if(targetPosition.X > myPosition.X)
        {
          Animation.ChangeActiveAnimation(AnimationType_Idle_Right);
        }
      }

      Global.Game.DebugDrawCommands.Add(view =>
      {
        Color color = Health.IsInvincible ? Color.Red : IsChasing ? Color.Yellow : Color.Green;
        view.DrawCircle(Owner.GetWorldSpatialData().Position, DetectionDistance, color);
      });
    }
  }
}
