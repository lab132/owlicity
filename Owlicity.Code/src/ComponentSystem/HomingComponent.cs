using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;
using VelcroPhysics.Dynamics;

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
    #region Dependencies

    // (optional)
    public BodyComponent BodyComponent;

    #endregion

    public ISpatial Target;

    // The minimum range to the target. If the target is closer than this, no action is performed.
    public float TargetInnerRange;

    // The range within which the target is being chased.
    public float TargetRange = 1.5f;

    public HomingType HomingType;
    public float Speed = 0.1f;

    public Body MyBody => BodyComponent?.Body;

    private bool _isChasing;
    public bool IsChasing
    {
      get => _isChasing && Target != null;
      set => _isChasing = value;
    }

    public HomingComponent(GameObject owner)
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
          // Don't do anything.
        }
        else if(targetDistance > TargetInnerRange)
        {
          IsChasing = true;
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
          view.DrawPoint(p, Global.ToMeters(3.0f), Color.Turquoise);
          view.DrawCircle(p, TargetInnerRange, Color.Yellow);
          view.DrawCircle(p, TargetRange, Color.Blue);
        });
      }
    }
  }
}
