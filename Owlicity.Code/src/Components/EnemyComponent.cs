using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using VelcroPhysics.Collision.ContactSystem;
using VelcroPhysics.Dynamics;

namespace Owlicity
{
  // Note(manu): This component is not used at the moment...
  public class EnemyComponent : ComponentBase
  {
    //
    // Initialization data.
    //
    public float HitDuration = 0.25f;
    public int Damage = 1;
    public float ForceOnImpact = 0.05f;
    public SpriteAnimationType AnimationType_Idle_Left;
    public SpriteAnimationType AnimationType_Idle_Right;

    //
    // Runtime data.
    //
    public BodyComponent BodyComponent;
    public HealthComponent Health;
    public SpriteAnimationComponent Animation;

    //
    // Optional components
    //
    public HomingComponent Chaser;

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

      if(Chaser == null)
      {
        Chaser = Owner.GetComponent<HomingComponent>();
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

      BodyComponent.Body.OnCollision += OnCollisionWithOwliver;

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
      Global.HandleDefaultHit(hitBody, MyBody.Position, Damage, ForceOnImpact);
    }

    public override void Update(float deltaSeconds)
    {
      base.Update(deltaSeconds);

      if(Chaser != null && Chaser.IsHoming)
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
    }
  }
}
