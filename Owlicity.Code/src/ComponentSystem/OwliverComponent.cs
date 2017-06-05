using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Factories;

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

  public class OwliverComponent : MovementComponent
  {
    private float _oldX;

    public SpriteAnimationComponent AnimationComponent { get; set; }

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

    public GameInput ConsumeInput()
    {
      GameInput result = Input;
      Input.Reset();
      return result;
    }

    public override void Initialize()
    {
      base.Initialize();

      if(AnimationComponent == null)
      {
        AnimationComponent = Owner.GetComponent<SpriteAnimationComponent>();
        Debug.Assert(AnimationComponent != null, "Owliver has no animation component!");
      }

      AnimationComponent.OnAnimationPlaybackStateChanged += OnAnimationLoopFinished;
    }

    private void OnAnimationLoopFinished(SpriteAnimationType animType, SpriteAnimationPlaybackState oldPlaybackState, SpriteAnimationPlaybackState newPlaybackState)
    {
      bool isAttackAnimation = _attackAnimations.Contains(animType);
      if(isAttackAnimation)
      {
        if(oldPlaybackState == SpriteAnimationPlaybackState.Playing && newPlaybackState != SpriteAnimationPlaybackState.Playing)
        {
          OwliverState newState = CurrentState;
          newState.MovementMode = OwliverMovementMode.Walking;
          ChangeState(ref newState);
        }
      }
    }

    public override void PostInitialize()
    {
      base.PostInitialize();
    }

    public override void PrePhysicsUpdate(float deltaSeconds)
    {
      float x = ControlledBody.LinearVelocity.X;
      if(Math.Abs(x) > float.Epsilon)
      {
        _oldX = x;
      }

      base.PrePhysicsUpdate(deltaSeconds);
    }

    public override void Update(float deltaSeconds)
    {
      base.Update(deltaSeconds);

      Vector2 dp = ControlledBody.LinearVelocity;
      float movementSpeed = dp.Length();

      OwliverState newState = CurrentState;

      const float movementChangeThreshold = 10.0f;
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
        // TODO(manu)
        newState.MovementMode = OwliverMovementMode.Attacking;
      }

      if(input.WantsInteraction)
      {
        // TODO(manu)
      }

      ChangeState(ref newState);
    }

    public void ChangeState(ref OwliverState newState)
    {
      OwliverState oldState = CurrentState;
      CurrentState = newState;

      OnStateChanged?.Invoke(oldState, newState);

      //bool changedFacingDirFromLeftToRight = oldState.FacingDirection == OwliverFacingDirection.Left && CurrentState.FacingDirection == OwliverFacingDirection.Right;
      //bool changedFacingDirFromRightToLeft = oldState.FacingDirection == OwliverFacingDirection.Right && CurrentState.FacingDirection == OwliverFacingDirection.Left;

      bool transferAnimationState = oldState.MovementMode == newState.MovementMode &&
                                    oldState.FacingDirection != newState.FacingDirection;

      switch(newState.MovementMode)
      {
        case OwliverMovementMode.Idle:
        {
          if(newState.FacingDirection == OwliverFacingDirection.Right)
          {
            AnimationComponent.ChangeActiveAnimation(SpriteAnimationType.Owliver_Idle_Right, transferAnimationState);
          }
          else if(newState.FacingDirection == OwliverFacingDirection.Left)
          {
            AnimationComponent.ChangeActiveAnimation(SpriteAnimationType.Owliver_Idle_Left, transferAnimationState);
          }
        }
        break;

        case OwliverMovementMode.Walking:
        {
          if(newState.FacingDirection == OwliverFacingDirection.Right)
          {
            AnimationComponent.ChangeActiveAnimation(SpriteAnimationType.Owliver_Walk_Right, transferAnimationState);
          }
          else if(newState.FacingDirection == OwliverFacingDirection.Left)
          {
            AnimationComponent.ChangeActiveAnimation(SpriteAnimationType.Owliver_Walk_Left, transferAnimationState);
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
                AnimationComponent.ChangeActiveAnimation(SpriteAnimationType.Owliver_AttackStick_Right, transferAnimationState);
              }
              else if(newState.FacingDirection == OwliverFacingDirection.Left)
              {
                AnimationComponent.ChangeActiveAnimation(SpriteAnimationType.Owliver_AttackStick_Left, transferAnimationState);
              }
            }
            break;

            case OwliverWeaponType.FishingRod:
            {
              if(newState.FacingDirection == OwliverFacingDirection.Right)
              {
                AnimationComponent.ChangeActiveAnimation(SpriteAnimationType.Owliver_AttackFishingRod_Right, transferAnimationState);
              }
              else if(newState.FacingDirection == OwliverFacingDirection.Left)
              {
                AnimationComponent.ChangeActiveAnimation(SpriteAnimationType.Owliver_AttackFishingRod_Left, transferAnimationState);
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
