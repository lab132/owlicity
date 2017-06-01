using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelcroPhysics.Dynamics;

namespace Owlicity
{
  public class MovementComponent : ComponentBase
  {
    //
    // Init data
    //
    public float MaxMovementSpeed = 400.0f;
    public float MovementDamping = 0.85f;

    //
    // Runtime data
    //
    public Vector2 InputVector;

    public BodyComponent ControlledBodyComponent
    {
      get => Owner.Components.OfType<BodyComponent>().FirstOrDefault();
    }

    public Body ControlledBody
    {
      get => ControlledBodyComponent?.Body;
    }

    public MovementComponent(GameObject owner) : base(owner)
    {
    }

    public Vector2 ConsumeInputVector()
    {
      Vector2 result = InputVector;
      InputVector = Vector2.Zero;
      return result;
    }

    public override void PrePhysicsUpdate(float deltaSeconds)
    {
      base.PrePhysicsUpdate(deltaSeconds);

      Vector2 inputVector = ConsumeInputVector();
      if(inputVector != Vector2.Zero)
      {
        Vector2 movementVector = inputVector.GetClampedTo(1.0f) * MaxMovementSpeed;
        Body body = ControlledBody;
        if(body != null)
        {
          body.LinearVelocity = movementVector;
        }
        else
        {
          // TODO(manu): Manual simulation.
        }
      }
    }

    public override void Update(float deltaSeconds)
    {
      base.Update(deltaSeconds);

      Body body = ControlledBody;
      if(body != null)
      {
        body.LinearVelocity *= MovementDamping;
      }
    }
  }
}
