using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;

namespace Owlicity
{
  public enum ChaserOutOfReachResponse
  {
    StopChasing,
    SnapToTargetAtMaxDistance,
  }

  public class ChaserComponent : SpatialComponent
  {
    public ISpatial Target;

    public float MinDistanceToTarget = 0.5f;
    public float MaxDistanceToTarget = 1.5f;
    public ChaserOutOfReachResponse OutOfReachResponse;
    public float DistanceWeight = 1.0f;

    public bool DebugDrawingEnabled = true;

    public ChaserComponent(GameObject owner)
      : base(owner)
    {
    }

    public override void Initialize()
    {
      base.Initialize();

      Debug.Assert(Owner.GetComponent<BodyComponent>() == null, "SpringArmComponent is not compatible with BodyComponent (yet?).");
    }

    public override void Update(float deltaSeconds)
    {
      base.Update(deltaSeconds);

      if(Target != null)
      {
        SpatialData worldSpatial = this.GetWorldSpatialData();
        SpatialData targetSpatial = Target.GetWorldSpatialData();
        Vector2 targetDelta = targetSpatial.Position - worldSpatial.Position;
        targetDelta.GetDirectionAndLength(out Vector2 targetDir, out float targetDistance);

        if(targetDistance > MaxDistanceToTarget)
        {
          switch(OutOfReachResponse)
          {
            case ChaserOutOfReachResponse.StopChasing:
            {
              // Don't do anything.
            }
            break;

            case ChaserOutOfReachResponse.SnapToTargetAtMaxDistance:
            {
              const float compensationForSlowMovement = 0.01f;
              Vector2 newPosition = targetSpatial.Position - (targetDir * (MaxDistanceToTarget - compensationForSlowMovement));
              Spatial.SetWorldPosition(newPosition);
            }
            break;

            default: throw new ArgumentException(nameof(OutOfReachResponse));
          }
        }
        else if(targetDistance > MinDistanceToTarget)
        {
          float relevantTargetDistance = targetDistance - MinDistanceToTarget;
          Vector2 velocity = targetDir * (relevantTargetDistance * DistanceWeight);
          Vector2 deltaPosition = velocity * deltaSeconds;
          Vector2 newPosition = worldSpatial.Position + deltaPosition;
          Spatial.SetWorldPosition(newPosition);
        }
      }

      if(DebugDrawingEnabled)
      {
        Global.Game.DebugDrawCommands.Add(view =>
        {
          Vector2 p = this.GetWorldSpatialData().Position;
          view.DrawPoint(p, Global.ToMeters(3.0f), Color.Turquoise);
          view.DrawCircle(p, MinDistanceToTarget, Color.Yellow);
          view.DrawCircle(p, MaxDistanceToTarget, Color.Blue);
        });
      }
    }
  }
}
