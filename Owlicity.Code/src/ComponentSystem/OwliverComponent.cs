﻿using Microsoft.Xna.Framework;
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
    Idle = 0,
    Walking = 2,
    Attacking = 4,
  }

  public enum OwliverFacingDirection
  {
    Right = 1,
    Left = 0,
  }

  public enum OwliverWeaponType
  {
    Stick = 0,
    FishingRod = 6,
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

    public HashSet<ShopItemComponent> ConnectedShopItems = new HashSet<ShopItemComponent>();

    public MovementComponent Movement;
    public HealthComponent Health;
    public MoneyBagComponent MoneyBag;
    public KeyRingComponent KeyRing;

    public OwliverState CurrentState;
    public Action<OwliverState, OwliverState> OnStateChanged;

    public GameInput Input;

    private List<SpriteAnimationType> _attackAnimations = new List<SpriteAnimationType>
    {
      SpriteAnimationType.Owliver_Attack_Stick_Left,
      SpriteAnimationType.Owliver_Attack_Stick_Right,
      SpriteAnimationType.Owliver_Attack_FishingRod_Left,
      SpriteAnimationType.Owliver_Attack_FishingRod_Right,
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
        {
          Vector2 offset = Global.ToMeters(20, -120);
          switch(CurrentState.WeaponType)
          {
            case OwliverWeaponType.Stick:
            {
              local = new AABB
              {
                LowerBound = Global.ToMeters(0, -50) + offset,
                UpperBound = Global.ToMeters(150, 70) + offset,
              };
            }
            break;

            case OwliverWeaponType.FishingRod:
            {
              local = new AABB
              {
                LowerBound = Global.ToMeters(0, -50) + offset,
                UpperBound = Global.ToMeters(150, 70) + offset,
              };
            }
            break;

            default: throw new ArgumentException();
          }
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

    public int Damage
    {
      get
      {
        int result;
        switch(CurrentState.WeaponType)
        {
          case OwliverWeaponType.Stick: result = 1; break;
          case OwliverWeaponType.FishingRod: result = 2; break;

          default: throw new ArgumentException(nameof(CurrentState.WeaponType));
        }

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

      foreach(ShopItemComponent shopItem in go.GetComponents<ShopItemComponent>())
      {
        ConnectedShopItems.Add(shopItem);
        shopItem.IsSelected = true;
        shopItem.IsAffordable = MoneyBag.CurrentAmount >= shopItem.PriceValue;
      }
    }

    private void OnSeparation(Fixture fixtureA, Fixture fixtureB, Contact contact)
    {
      GameObject go = ((ComponentBase)fixtureB.Body.UserData).Owner;

      foreach(ShopItemComponent shopItem in go.GetComponents<ShopItemComponent>())
      {
        shopItem.IsSelected = false;
        ConnectedShopItems.Remove(shopItem);
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
      MyBody.OnSeparation += OnSeparation;
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
        foreach(ShopItemComponent shopItem in ConnectedShopItems)
        {
          bool purchase = false;
          bool removeIfPurchased = true;

          int price = shopItem.PriceValue;
          if(MoneyBag.CurrentAmount >= price)
          {
            switch(shopItem.ItemType)
            {
              case ShopItemType.FruitBowl:
              {
                purchase = true;
                removeIfPurchased = false;
                Health.Heal(int.MaxValue);
              }
              break;

              case ShopItemType.FishingRod:
              {
                if(CurrentState.WeaponType != OwliverWeaponType.FishingRod)
                {
                  purchase = true;
                  newState.WeaponType = OwliverWeaponType.FishingRod;
                  ChangeState(ref newState);

                  var newGo = GameObjectFactory.CreateKnown(GameObjectType.ShopItem_Stick);
                  newGo.Spatial.CopyFrom(shopItem.Owner.Spatial);
                  Global.Game.AddGameObject(newGo);
                }
              }
              break;

              case ShopItemType.Stick:
              {
                if(CurrentState.WeaponType != OwliverWeaponType.Stick)
                {
                  purchase = true;
                  newState.WeaponType = OwliverWeaponType.Stick;
                  ChangeState(ref newState);

                  var newGo = GameObjectFactory.CreateKnown(GameObjectType.ShopItem_FishingRod);
                  newGo.Spatial.CopyFrom(shopItem.Owner.Spatial);
                  Global.Game.AddGameObject(newGo);
                }
              }
              break;

              default: throw new InvalidProgramException("Invalid item type.");
            }
          }

          if(purchase)
          {
            MoneyBag.CurrentAmount -= price;
            if(removeIfPurchased)
            {
              Global.Game.RemoveGameObject(shopItem.Owner);
            }
          }
        }
      }

      ChangeState(ref newState);

      AABB weaponAABB = WeaponAABB;
      if(input.WantsAttack && CurrentState.MovementMode == OwliverMovementMode.Attacking)
      {
        int damage = Damage;
        float force = 0.1f * damage;
        List<Fixture> fixtures = Global.Game.World.QueryAABB(ref weaponAABB);
        foreach(Body hitBody in fixtures.Where(f => f.CollidesWith.HasFlag(Global.OwliverWeaponCollisionCategory))
                                      .Select(f => f.Body)
                                      .Distinct())
        {
          Global.HandleDefaultHit(hitBody, MyBody.Position, damage, force);
        }

        {
          float sign = CurrentState.FacingDirection == OwliverFacingDirection.Left ? -1.0f : 1.0f;
          GameObject projectile = GameObjectFactory.CreateKnown(GameObjectType.Projectile);
          projectile.Spatial.CopyFrom(new SpatialData { Position = WeaponAABB.Center });
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

      SpriteAnimationType animType = GetAnimationType(ref newState);
      if(Animation.ChangeActiveAnimation(animType))
      {
        Console.WriteLine();
      }
    }

    public static SpriteAnimationType GetAnimationType(ref OwliverState state)
    {
      return GetAnimationType(state.MovementMode, state.FacingDirection, state.WeaponType);
    }

    public static SpriteAnimationType GetAnimationType(OwliverMovementMode movement, OwliverFacingDirection facing, OwliverWeaponType weapon)
    {
      int baseOffset = (int)SpriteAnimationType.Owliver_Idle_Stick_Left;
      var result = (SpriteAnimationType)(baseOffset + (int)weapon + (int)movement + (int)facing);
      return result;
    }
  }
}
