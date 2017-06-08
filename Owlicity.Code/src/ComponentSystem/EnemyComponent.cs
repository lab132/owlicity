using Microsoft.Xna.Framework;
using System;
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
    public SpriteAnimationComponent SpriteAnimationComponent { get; set; }

    public float MeanHitTime = 0.25f;
    public float HalfMeanHitTime => 0.5f * MeanHitTime;
    public float CurrentHitTime;

    public bool IsHit => CurrentHitTime > 0.0f;

    private Vector2 NormalScale = Vector2.One;
    private Vector2 HitScale = new Vector2(1.5f, 0.5f);

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

      if(SpriteAnimationComponent == null)
      {
        SpriteAnimationComponent = Owner.GetComponent<SpriteAnimationComponent>();
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
      bc.Body.OnCollision += delegate (Fixture fixtureA, Fixture fixtureB, Contact contact)
      {
        if(fixtureA.UserData == owliverBC || fixtureB.UserData == owliverBC)
        {
          pec.Emit(fixtureB.Body.Position + contact.Manifold.LocalPoint);
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

      if(CurrentHitTime > 0.0f)
      {
        CurrentHitTime -= deltaSeconds;
        if(CurrentHitTime < 0.0f)
        {
          CurrentHitTime = 0.0f;
        }
      }

      if(SpriteAnimationComponent != null)
      {
        if(IsHit)
        {
          // TODO(manu): Replace this with a curve!
          Vector2 scale;
          if(CurrentHitTime > HalfMeanHitTime)
          {
            float time = CurrentHitTime - HalfMeanHitTime;
            float alpha = time / HalfMeanHitTime;
            scale = Vector2.LerpPrecise(HitScale, NormalScale, alpha);
          }
          else
          {
            float alpha = CurrentHitTime / HalfMeanHitTime;
            scale = Vector2.LerpPrecise(NormalScale, HitScale, alpha);
          }

          SpriteAnimationComponent.AdditionalScale = scale;
        }
        else
        {
          SpriteAnimationComponent.AdditionalScale = null;
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
      CurrentHitTime = MeanHitTime * strength;
    }
  }
}
