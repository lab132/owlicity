using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;
using VelcroPhysics.Dynamics;

namespace Owlicity
{
  public enum ChaserOutOfReachResponse
  {
    StopChasing,
    SnapToTargetAtMaximumRange,
  }

  public enum ChaserMovementType
  {
    Constant,
    Linear,
    SmoothProximity, // TODO(manu): Better name for this?
  }

  // Note(manu): To disable chasing, just set Target to null.
  public class ChaserComponent : SpatialComponent
  {
    public ISpatial Target;

    // The minimum range to the target. If the target is closer than this, no action is performed.
    public float TargetInnerRange;

    // The range within which the target is being chased.
    public float TargetRange = 1.5f;

    // What to do if the target gets out of range.
    public ChaserOutOfReachResponse OutOfReachResponse;

    public ChaserMovementType MovementType;
    public float Speed = 0.1f;

    public BodyComponent BodyComponent;
    public Body MyBody;

    bool _isChasing;
    public bool IsChasing
    {
      get => _isChasing && Target != null;
      set => _isChasing = value;
    }

    public ChaserComponent(GameObject owner)
      : base(owner)
    {
    }

    public override void Initialize()
    {
      base.Initialize();

      if(BodyComponent == null)
      {
        BodyComponent = Owner.GetComponent<BodyComponent>();
      }
    }

    public override void PostInitialize()
    {
      base.PostInitialize();

      MyBody = BodyComponent?.Body;

#if DEBUG
      if(MyBody != null)
      {
        if(MyBody.BodyType != BodyType.Static)
        {
          Debug.Assert(OutOfReachResponse != ChaserOutOfReachResponse.SnapToTargetAtMaximumRange,
            "Cannot snap to target if I have a non-static body!");
        }
      }
#endif
    }

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
      IsChasing = false;
      Body body = MyBody;

      if(Target != null)
      {
        SpatialData worldSpatial = this.GetWorldSpatialData();
        SpatialData targetSpatial = Target.GetWorldSpatialData();
        Vector2 targetDelta = targetSpatial.Position - worldSpatial.Position;
        targetDelta.GetDirectionAndLength(out Vector2 targetDir, out float targetDistance);

        if(targetDistance > TargetRange)
        {
          switch(OutOfReachResponse)
          {
            case ChaserOutOfReachResponse.StopChasing:
            {
              // Don't do anything.
            }
            break;

            case ChaserOutOfReachResponse.SnapToTargetAtMaximumRange:
            {
              IsChasing = true;

              // Note(manu): Without this value the target would always be out of range until it has _completely_ stopped.
              const float compensationForSlowMovement = 0.01f;
              Vector2 newPosition = targetSpatial.Position - (targetDir * (TargetRange - compensationForSlowMovement));
              if(body != null)
              {
                Debug.Assert(body.BodyType == BodyType.Static);
                body.Position = newPosition;
              }
              else
              {
                Spatial.SetWorldPosition(newPosition);
              }
            }
            break;

            default: throw new ArgumentException(nameof(OutOfReachResponse));
          }
        }
        else if(targetDistance > TargetInnerRange)
        {
          IsChasing = true;

          switch(MovementType)
          {
            case ChaserMovementType.Constant:
            {
              Vector2 velocity = targetDir * Speed;
              if(body != null)
              {
                if(body.BodyType == BodyType.Static)
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

            case ChaserMovementType.Linear:
            {
              Vector2 impulse = targetDir * Speed;
              if(body != null)
              {
                body.ApplyLinearImpulse(impulse);
              }
              else
              {
                throw new NotImplementedException();
              }
            }
            break;

            case ChaserMovementType.SmoothProximity:
            {
              float relevantTargetDistance = targetDistance - TargetInnerRange;
              float weightedDistance = relevantTargetDistance * Speed;
              Vector2 velocity = targetDir * weightedDistance;

              if(body != null)
              {
                if(body.BodyType == BodyType.Static)
                {
                  Vector2 deltaPosition = velocity * deltaSeconds;
                  body.Position += deltaPosition;
                }
                else
                {
                  // TODO(manu): Setting the linear velocity is kind of bad. Can we do this with an impulse instead?
                  // Note(manu): This code path is untested (2017-06-15).
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

            default: throw new ArgumentException(nameof(MovementType));
          }
        }
      }

      if(DebugDrawingEnabled)
      {
        Global.Game.DebugDrawCommands.Add(view =>
        {
          Vector2 p = this.GetWorldSpatialData().Position;
          view.DrawPoint(p, Global.ToMeters(3.0f), Color.Turquoise);
          view.DrawCircle(p, TargetInnerRange, Color.Yellow);
          view.DrawCircle(p, TargetRange, Color.Blue);
        });
      }
    }
  }
}
