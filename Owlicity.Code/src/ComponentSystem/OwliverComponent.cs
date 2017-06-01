using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Owlicity
{
  public class OwliverComponent : MovementComponent
  {
    private float _oldX;
    private bool _isMoving;

    private SpriteAnimationComponent _animationComponent;

    public OwliverComponent(GameObject owner) : base(owner)
    {
    }

    public override void Initialize()
    {
      base.Initialize();
      _animationComponent = Owner.Components.OfType<SpriteAnimationComponent>().First();
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

      bool wasMoving = _isMoving;
      _isMoving = movementSpeed > 10.0f;
      bool wasFacingRight = _oldX > 0.01f;
      bool isFacingLeft = dp.X < -0.01f;
      bool isFacingRight = dp.X > 0.01f;

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
        _animationComponent.ChangeActiveAnimation(newAnimationType);
      }
    }
  }
}
