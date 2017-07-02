using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Factories;
using VelcroPhysics.Collision.ContactSystem;
using VelcroPhysics.Collision.Filtering;

namespace Owlicity
{
  public enum HomingType
  {
    ConstantSpeed,
    ConstantAcceleration,
  }

  // Note(manu): To disable homing, just set Target to null.
  public class HomingComponent : SpatialComponent
  {
    public BodyComponent BodyComponentToMove;
    public Body BodyToMove => BodyComponentToMove?.Body;

    public TargetSensorComponent TargetSensor;

    public HomingType HomingType;
    public float Speed = 0.1f;

    public bool IsHoming => TargetSensor.CurrentTargetList.Count > 0;

    public HomingComponent(GameObject owner)
      : base(owner)
    {
    }

    public override void Initialize()
    {
      Debug.Assert(TargetSensor != null);
      base.Initialize();
    }

    public override void Update(float deltaSeconds)
    {
      base.Update(deltaSeconds);

      Body target = TargetSensor.CurrentMainTarget;
      if(target != null)
      {
        Vector2 deltaPosition = target.Position - this.GetWorldSpatialData().Position;
        deltaPosition.GetDirectionAndLength(out Vector2 targetDir, out float targetDistance);

        Body bodyToMove = BodyToMove;
        switch(HomingType)
        {
          case HomingType.ConstantSpeed:
          {
            Vector2 velocity = targetDir * Speed;
            if(bodyToMove.IsStatic)
            {
              bodyToMove.Position += velocity * deltaSeconds;
            }
            else
            {
              bodyToMove.LinearVelocity = velocity;
            }
          }
          break;

          case HomingType.ConstantAcceleration:
          {
            if(bodyToMove.IsStatic)
            {
              // Note(manu): This is untested and might not work well.
              Vector2 acceleration = targetDir * Speed * Speed;
              Vector2 velocity = acceleration * deltaSeconds;
              bodyToMove.Position += velocity * deltaSeconds;
            }
            else
            {
              Vector2 velocity = targetDir * Speed;
              Vector2 impulse = bodyToMove.Mass * velocity;
              bodyToMove.ApplyLinearImpulse(impulse);
            }
          }
          break;

          default: throw new ArgumentException(nameof(HomingType));
        }
      }
    }

#if true
#else
    public override void PrePhysicsUpdate(float deltaSeconds)
    {
      base.PrePhysicsUpdate(deltaSeconds);

      if(MyBody != null)
      {
        PerformChase(deltaSeconds);
      }
    }

    public override void Update(float deltaSeconds)
    {
      base.Update(deltaSeconds);

      if(MyBody == null)
      {
        PerformChase(deltaSeconds);
      }
    }

    public void PerformChase(float deltaSeconds)
    {
      IsHoming = false;
      Body body = MyBody;

      if(Target != null)
      {
        SpatialData worldSpatial = this.GetWorldSpatialData();
        SpatialData targetSpatial = Target.GetWorldSpatialData();
        Vector2 targetDelta = targetSpatial.Position - worldSpatial.Position;
        targetDelta.GetDirectionAndLength(out Vector2 targetDir, out float targetDistance);

        if(targetDistance > TargetRange)
        {
          // Don't do anything.
        }
        else if(targetDistance > TargetInnerRange)
        {
          IsHoming = true;
          Vector2 velocity = targetDir * Speed;

          switch(HomingType)
          {
            case HomingType.ConstantSpeed:
            {
              if(body != null)
              {
                if(body.IsStatic)
                {
                  Vector2 deltaPosition = velocity * deltaSeconds;
                  body.Position += deltaPosition;
                }
                else
                {
                  body.LinearVelocity = velocity;
                }
              }
              else
              {
                Vector2 deltaPosition = velocity * deltaSeconds;
                Vector2 newPosition = worldSpatial.Position + deltaPosition;
                Spatial.SetWorldPosition(newPosition);
              }
            }
            break;

            case HomingType.ConstantAcceleration:
            {
              if(body != null)
              {
                Vector2 impulse = body.Mass * velocity;
                body.ApplyLinearImpulse(ref impulse);
              }
              else
              {
                throw new NotImplementedException();
              }
            }
            break;

            default: throw new ArgumentException(nameof(HomingType));
          }
        }
      }

      if(DebugDrawingEnabled)
      {
        Global.Game.DebugDrawCommands.Add(view =>
        {
          Vector2 p = this.GetWorldSpatialData().Position;
          view.DrawPoint(p, Conversion.ToMeters(3.0f), Color.Turquoise);
          view.DrawCircle(p, TargetInnerRange, Color.Yellow);
          view.DrawCircle(p, TargetRange, Color.Blue);
        });
      }
    }
#endif
  }
}
