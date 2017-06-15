using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Shared;
using VelcroPhysics.Collision.ContactSystem;

namespace Owlicity
{
  public enum OwliverMovementMode
  {
    Idle,
    Walking,
    Attacking,
  }

  public enum OwliverFacingDirection
  {
    Right,
    Left,
  }

  public enum OwliverWeaponType
  {
    Stick,
    FishingRod,
  }

  public struct OwliverState
  {
    public OwliverMovementMode MovementMode;
    public OwliverFacingDirection FacingDirection;
    public OwliverWeaponType WeaponType;

    public static bool operator ==(OwliverState a, OwliverState b)
    {
      return a.MovementMode == b.MovementMode &&
             a.FacingDirection == b.FacingDirection &&
             a.WeaponType == b.WeaponType;
    }

    public static bool operator !=(OwliverState a, OwliverState b)
    {
      return !(a == b);
    }

    public override bool Equals(object obj)
    {
      return obj is OwliverState && this == (OwliverState)obj;
    }

    public override int GetHashCode()
    {
      // TODO(manu): BETTER HASH FUNCTION!!!
      return (int)MovementMode << 16 |
             (int)FacingDirection << 8 |
             (int)WeaponType << 0;
    }
  }

  public class OwliverComponent : ComponentBase
  {
    public SpriteAnimationComponent Animation;
    public float HitDuration = 0.25f;

    public BodyComponent BodyComponent;
    public Body MyBody => BodyComponent?.Body;

    public MovementComponent Movement;
    public HealthComponent Health;
    public MoneyBagComponent MoneyBag;
    public KeyRingComponent KeyRing;

    public OwliverState CurrentState;
    public Action<OwliverState, OwliverState> OnStateChanged;

    public GameInput Input;

    private List<SpriteAnimationType> _attackAnimations = new List<SpriteAnimationType>
    {
      SpriteAnimationType.Owliver_AttackStick_Left,
      SpriteAnimationType.Owliver_AttackStick_Right,
      SpriteAnimationType.Owliver_AttackFishingRod_Left,
      SpriteAnimationType.Owliver_AttackFishingRod_Right,
    };

    public OwliverComponent(GameObject owner) : base(owner)
    {
    }

    public AABB WeaponAABB
    {
      get
      {
        Vector2 worldPosition = Owner.GetWorldSpatialData().Position;

        AABB local;
        switch(CurrentState.WeaponType)
        {
          case OwliverWeaponType.Stick:
          local = new AABB
          {
            LowerBound = Global.ToMeters(0, -50),
            UpperBound = Global.ToMeters(150, 70),
          };
          break;

          case OwliverWeaponType.FishingRod:
          throw new NotImplementedException();
          //break;

          default: throw new ArgumentException();
        }

        if(CurrentState.FacingDirection == OwliverFacingDirection.Left)
        {
          // Mirror along the y-axis.
          float lowerX = local.LowerBound.X;
          float upperX = local.UpperBound.X;
          local.UpperBound.X = -lowerX;
          local.LowerBound.X = -upperX;
        }

        AABB result = new AABB
        {
          LowerBound = Global.OwliverScale * local.LowerBound + worldPosition,
          UpperBound = Global.OwliverScale * local.UpperBound + worldPosition,
        };

        return result;
      }
    }

    public GameInput ConsumeInput()
    {
      GameInput result = Input;
      Input.Reset();
      return result;
    }

    private void OnCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
    {
      Debug.Assert(fixtureA.Body == MyBody);

      GameObject go = ((ComponentBase)fixtureB.Body.UserData).Owner;

      if (MoneyBag != null)
      {
        foreach (MoneyBagComponent moneyBag in go.GetComponents<MoneyBagComponent>())
        {
          int amountStolen = moneyBag.CurrentAmount;
          moneyBag.CurrentAmount = 0;

          MoneyBag.CurrentAmount += amountStolen;
        }
      }

      if (KeyRing != null)
      {
        foreach (KeyRingComponent keyRing in go.GetComponents<KeyRingComponent>())
        {
          for (int keyTypeIndex = 0; keyTypeIndex < (int)KeyType.COUNT; keyTypeIndex++)
          {
            int amountStolen = keyRing.CurrentKeyAmounts[keyTypeIndex];
            keyRing.CurrentKeyAmounts[keyTypeIndex] = 0;
            KeyRing.CurrentKeyAmounts[keyTypeIndex] += amountStolen;
          }
        }
      }
    }

    private void OnAnimationLoopFinished(SpriteAnimationType animType, SpriteAnimationPlaybackState oldPlaybackState, SpriteAnimationPlaybackState newPlaybackState)
    {
      bool isAttackAnimation = _attackAnimations.Contains(animType);
      if (isAttackAnimation)
      {
        if (oldPlaybackState == SpriteAnimationPlaybackState.Playing && newPlaybackState != SpriteAnimationPlaybackState.Playing)
        {
          OwliverState newState = CurrentState;
          newState.MovementMode = OwliverMovementMode.Walking;
          ChangeState(ref newState);
        }
      }
    }

    public override void Initialize()
    {
      base.Initialize();

      if(Animation == null)
      {
        Animation = Owner.GetComponent<SpriteAnimationComponent>();
        Debug.Assert(Animation != null, "Owliver has no animation component!");
      }

      if(BodyComponent == null)
      {
        BodyComponent = Owner.GetComponent<BodyComponent>();
        Debug.Assert(BodyComponent != null, "Owliver has no body component!");
      }

      if(Movement == null)
      {
        Movement = Owner.GetComponent<MovementComponent>();
      }

      if(Health == null)
      {
        Health = Owner.GetComponent<HealthComponent>();
        Debug.Assert(Health != null, "Owliver has no health component!");
      }

      if(MoneyBag == null)
      {
        MoneyBag = Owner.GetComponent<MoneyBagComponent>();
      }

      if(KeyRing == null)
      {
        KeyRing = Owner.GetComponent<KeyRingComponent>();
      }
    }

    public override void PostInitialize()
    {
      base.PostInitialize();

      Animation.OnAnimationPlaybackStateChanged += OnAnimationLoopFinished;

      MyBody.OnCollision += OnCollision;
    }

    public override void Update(float deltaSeconds)
    {
      base.Update(deltaSeconds);

      Vector2 dp = MyBody.LinearVelocity;
      float movementSpeed = dp.Length();

      OwliverState newState = CurrentState;

      const float movementChangeThreshold = 0.01f;
      switch(CurrentState.MovementMode)
      {
        case OwliverMovementMode.Idle:
        {
          if(movementSpeed >= movementChangeThreshold)
          {
            newState.MovementMode = OwliverMovementMode.Walking;
          }
        }
        break;

        case OwliverMovementMode.Walking:
        {
          if(movementSpeed < movementChangeThreshold)
          {
            newState.MovementMode = OwliverMovementMode.Idle;
          }
        }
        break;
      }

      const float facingChangeThreshold = 0.01f;
      if(CurrentState.FacingDirection == OwliverFacingDirection.Left && dp.X > facingChangeThreshold)
      {
        newState.FacingDirection = OwliverFacingDirection.Right;
      }
      else if(CurrentState.FacingDirection == OwliverFacingDirection.Right && dp.X < -facingChangeThreshold)
      {
        newState.FacingDirection = OwliverFacingDirection.Left;
      }

      GameInput input = ConsumeInput();

      if(input.WantsAttack)
      {
        newState.MovementMode = OwliverMovementMode.Attacking;
      }

      if(input.WantsInteraction)
      {
        // TODO(manu)
      }

      ChangeState(ref newState);

      AABB weaponAABB = WeaponAABB;
      if(input.WantsAttack && CurrentState.MovementMode == OwliverMovementMode.Attacking)
      {
        List<Fixture> fixtures = Global.Game.World.QueryAABB(ref weaponAABB);
        foreach(Body hitBody in fixtures.Where(f => f.CollidesWith.HasFlag(Global.OwliverWeaponCollisionCategory))
                                      .Select(f => f.Body)
                                      .Distinct())
        {
          Global.HandleDefaultHit(hitBody, MyBody.Position, damage: 1, force: 0.1f);
        }

        {
          float sign = CurrentState.FacingDirection == OwliverFacingDirection.Left ? -1.0f : 1.0f;
          GameObject projectile = GameObjectFactory.CreateKnown(GameObjectType.Projectile);
          projectile.Spatial.CopyFrom(Owner.GetWorldSpatialData());
          projectile.Spatial.Position.X += sign * 0.1f;
          projectile.GetComponent<AutoDestructComponent>().SecondsUntilDestruction = 2.0f;
          var bc = projectile.GetComponent<BodyComponent>();
          bc.BeforePostInitialize += () =>
          {
            bc.Body.CollidesWith = ~Global.OwliverCollisionCategory;
            bc.Body.LinearVelocity = sign * new Vector2(10.0f, 0.0f);
          };
          Global.Game.AddGameObject(projectile);
        }
      }
      else
      {
        if(!Health.IsInvincible)
        {
          Vector2 movementVector = Movement.ConsumeMovementVector();
          Movement.PerformMovement(movementVector, deltaSeconds);
        }
      }

      // Debug drawing.
      {
        Color color = CurrentState.MovementMode == OwliverMovementMode.Attacking ? Color.Navy : Color.Gray;
        Global.Game.DebugDrawCommands.Add(view =>
        {
          view.DrawAABB(ref weaponAABB, color);
        });
      }
    }

    public void ChangeState(ref OwliverState newState)
    {
      OwliverState oldState = CurrentState;
      CurrentState = newState;

      OnStateChanged?.Invoke(oldState, newState);

      bool transferAnimationState = oldState.MovementMode == newState.MovementMode &&
                                    oldState.FacingDirection != newState.FacingDirection;

      switch(newState.MovementMode)
      {
        case OwliverMovementMode.Idle:
        {
          if(newState.FacingDirection == OwliverFacingDirection.Right)
          {
            Animation.ChangeActiveAnimation(SpriteAnimationType.Owliver_Idle_Right, transferAnimationState);
          }
          else if(newState.FacingDirection == OwliverFacingDirection.Left)
          {
            Animation.ChangeActiveAnimation(SpriteAnimationType.Owliver_Idle_Left, transferAnimationState);
          }
        }
        break;

        case OwliverMovementMode.Walking:
        {
          if(newState.FacingDirection == OwliverFacingDirection.Right)
          {
            Animation.ChangeActiveAnimation(SpriteAnimationType.Owliver_Walk_Right, transferAnimationState);
          }
          else if(newState.FacingDirection == OwliverFacingDirection.Left)
          {
            Animation.ChangeActiveAnimation(SpriteAnimationType.Owliver_Walk_Left, transferAnimationState);
          }
        }
        break;

        case OwliverMovementMode.Attacking:
        {
          switch(newState.WeaponType)
          {
            case OwliverWeaponType.Stick:
            {
              if(newState.FacingDirection == OwliverFacingDirection.Right)
              {
                Animation.ChangeActiveAnimation(SpriteAnimationType.Owliver_AttackStick_Right, transferAnimationState);
              }
              else if(newState.FacingDirection == OwliverFacingDirection.Left)
              {
                Animation.ChangeActiveAnimation(SpriteAnimationType.Owliver_AttackStick_Left, transferAnimationState);
              }
            }
            break;

            case OwliverWeaponType.FishingRod:
            {
              if(newState.FacingDirection == OwliverFacingDirection.Right)
              {
                Animation.ChangeActiveAnimation(SpriteAnimationType.Owliver_AttackFishingRod_Right, transferAnimationState);
              }
              else if(newState.FacingDirection == OwliverFacingDirection.Left)
              {
                Animation.ChangeActiveAnimation(SpriteAnimationType.Owliver_AttackFishingRod_Left, transferAnimationState);
              }
            }
            break;
          }
        }
        break;
      }
    }
  }
}
