using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Owlicity
{
  public class CameraObject : GameObject
  {
    public SpringArmComponent SpringArm;
    public CameraComponent CameraComponent;
    public MovementComponent Movement;

    public CameraObject()
    {
      SpringArm = new SpringArmComponent(this)
      {
        TargetInnerRange = 0.2f,
        TargetRange = float.MaxValue,

        SpeedFactor = 5,

        DebugDrawingEnabled = false,
      };
      RootComponent = SpringArm;

      CameraComponent = new CameraComponent(this);
      CameraComponent.AttachTo(SpringArm);

      Movement = new MovementComponent(this)
      {
        MaxMovementSpeed = 5.0f,
      };
    }
  }
}
