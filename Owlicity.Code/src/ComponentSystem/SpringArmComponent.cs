using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;
using VelcroPhysics.Dynamics;

namespace Owlicity
{
  public class SpringArmComponent : SpatialComponent
  {
    #region Dependencies

    // The body to move towards the target (optional).
    public BodyComponent BodyComponent;

    #endregion Dependencies
    #region Input data

    public ISpatial Target;

    // The minimum range to the target. If the target is closer than this, no action is performed.
    public float TargetInnerRange;

    // The range within which the target is being chased.
    public float TargetRange = 1.5f;

    // The actual speed is determined by the distance to the target times this factor.
    public float SpeedFactor = 0.1f;

    #endregion Input data
    #region Runtime data

    public Body MyBody => BodyComponent?.Body;

    #endregion

    public SpringArmComponent(GameObject owner)
      : base(owner)
    {
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
      Body body = MyBody;

      if(Target != null)
      {
        Vector2 worldPosition = this.GetWorldSpatialData().Position;
        Vector2 targetPosition = Target.GetWorldSpatialData().Position;
        Vector2 targetDelta = targetPosition - worldPosition;
        targetDelta.GetDirectionAndLength(out Vector2 targetDir, out float targetDistance);

        if(targetDistance > TargetRange)
        {
          // Note(manu): Without this value the target would always be out of range until it has _completely_ stopped.
          const float compensationForSlowMovement = 0.01f;
          Vector2 newPosition = targetPosition - (targetDir * (TargetRange - compensationForSlowMovement));
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
        else if(targetDistance > TargetInnerRange)
        {
          float relevantTargetDistance = targetDistance - TargetInnerRange;
          float weightedDistance = relevantTargetDistance * SpeedFactor;
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
            Vector2 newPosition = worldPosition + deltaPosition;
            Spatial.SetWorldPosition(newPosition);
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
