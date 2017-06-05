using Microsoft.Xna.Framework;
using System;
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
  }

  public class OwliverComponent : MovementComponent
  {
    private float _oldX;

    public SpriteAnimationComponent AnimationComponent { get; set; }

    public OwliverState CurrentState;
    public Action<OwliverState, OwliverState> OnStateChanged;

    public GameInput Input;

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
    }

    public override void PostInitialize()
    {
      base.PostInitialize();

      //FixtureFactory.AttachRectangle(10, 100, 10000, Vector2.Zero, ControlledBody, ControlledBodyComponent);
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

#if false
      bool wasMoving = _isMoving;
      _isMoving = movementSpeed > 10.0f;
      bool wasFacingRight = _oldX > 0.01f;
      bool isFacingLeft = dp.X < -0.01f;
      bool isFacingRight = dp.X > 0.01f;
#endif

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
      }

      if(input.WantsInteraction)
      {
        // TODO(manu)
      }

      if(newState != CurrentState)
      {
        OwliverState oldState = CurrentState;
        CurrentState = newState;
        StateChanged(ref oldState);
      }

#if false
      if(!_isAttacking)
      {
        SpriteAnimationType newAnimationType = SpriteAnimationType.Unknown;
        if(_isMoving)
        {
          if(isFacingLeft) newAnimationType = SpriteAnimationType.Owliver_Walk_Left;
          else if(isFacingRight) newAnimationType = SpriteAnimationType.Owliver_Walk_Right;
        }
        else
        {
          if(wasMoving)
          {
            if(wasFacingRight) newAnimationType = SpriteAnimationType.Owliver_Idle_Right;
            else newAnimationType = SpriteAnimationType.Owliver_Idle_Left;
          }
          else
          {
            if(isFacingLeft) newAnimationType = SpriteAnimationType.Owliver_Idle_Left;
            else if(isFacingRight) newAnimationType = SpriteAnimationType.Owliver_Idle_Right;
          }
        }

        if(newAnimationType != SpriteAnimationType.Unknown)
        {
          AnimationComponent.ChangeActiveAnimation(newAnimationType);
        }
      }
#endif
    }

    public void StateChanged(ref OwliverState oldState)
    {
      OnStateChanged?.Invoke(oldState, CurrentState);

      //bool changedFacingDirFromLeftToRight = oldState.FacingDirection == OwliverFacingDirection.Left && CurrentState.FacingDirection == OwliverFacingDirection.Right;
      //bool changedFacingDirFromRightToLeft = oldState.FacingDirection == OwliverFacingDirection.Right && CurrentState.FacingDirection == OwliverFacingDirection.Left;

      switch(CurrentState.MovementMode)
      {
        case OwliverMovementMode.Idle:
        {
          if(CurrentState.FacingDirection == OwliverFacingDirection.Right)
          {
            AnimationComponent.ChangeActiveAnimation(SpriteAnimationType.Owliver_Idle_Right);
          }
          else if(CurrentState.FacingDirection == OwliverFacingDirection.Left)
          {
            AnimationComponent.ChangeActiveAnimation(SpriteAnimationType.Owliver_Idle_Left);
          }
        }
        break;

        case OwliverMovementMode.Walking:
        {
          if(CurrentState.FacingDirection == OwliverFacingDirection.Right)
          {
            AnimationComponent.ChangeActiveAnimation(SpriteAnimationType.Owliver_Walk_Right);
          }
          else if(CurrentState.FacingDirection == OwliverFacingDirection.Left)
          {
            AnimationComponent.ChangeActiveAnimation(SpriteAnimationType.Owliver_Walk_Left);
          }
        }
        break;

        case OwliverMovementMode.Attacking:
        {
          switch(CurrentState.WeaponType)
          {
            case OwliverWeaponType.Stick:
            {
              if(CurrentState.FacingDirection == OwliverFacingDirection.Right)
              {
                AnimationComponent.ChangeActiveAnimation(SpriteAnimationType.Owliver_AttackStick_Right);
              }
              else if(CurrentState.FacingDirection == OwliverFacingDirection.Left)
              {
                AnimationComponent.ChangeActiveAnimation(SpriteAnimationType.Owliver_AttackStick_Left);
              }
            }
            break;

            case OwliverWeaponType.FishingRod:
            {
              if(CurrentState.FacingDirection == OwliverFacingDirection.Right)
              {
                AnimationComponent.ChangeActiveAnimation(SpriteAnimationType.Owliver_AttackFishingRod_Right);
              }
              else if(CurrentState.FacingDirection == OwliverFacingDirection.Left)
              {
                AnimationComponent.ChangeActiveAnimation(SpriteAnimationType.Owliver_AttackFishingRod_Left);
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
