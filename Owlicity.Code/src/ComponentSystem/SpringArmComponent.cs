using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace Owlicity
{
  public class SpringArmComponent : SpatialComponent
  {
    public ISpatial Target;

    public bool MovementLagEnabled = true;
    public float MinDistanceToTarget = 0.5f;
    public float MaxDistanceToTarget = 1.5f;
    public float DistanceWeight = 5.0f;

    public bool DebugDrawingEnabled = false;

    public SpringArmComponent(GameObject owner)
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
        if(MovementLagEnabled)
        {
          SpatialData worldSpatial = this.GetWorldSpatialData();
          SpatialData targetSpatial = Target.GetWorldSpatialData();
          Vector2 targetDelta = targetSpatial.Position - worldSpatial.Position;
          targetDelta.GetDirectionAndLength(out Vector2 targetDir, out float targetDistance);
          if(targetDistance > MinDistanceToTarget)
          {
            float relevantTargetDistance = targetDistance - MinDistanceToTarget;
            Vector2 velocity = targetDir * (relevantTargetDistance * DistanceWeight);
            Vector2 deltaPosition = velocity * deltaSeconds;
            Vector2 newPosition = worldSpatial.Position + deltaPosition;

            Vector2 newTargetDelta = targetSpatial.Position - newPosition;
            newTargetDelta.GetDirectionAndLength(out Vector2 newTargetDir, out float newTargetDistance);

            if(newTargetDistance > MaxDistanceToTarget)
            {
              newPosition = targetSpatial.Position - (newTargetDir * MaxDistanceToTarget);
            }

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
          view.DrawCircle(p, MinDistanceToTarget, Color.Yellow);
          view.DrawCircle(p, MaxDistanceToTarget, Color.Blue);
        });
      }
    }
  }
}
