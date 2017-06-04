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
    public BodyComponent ControlledBodyComponent { get; set; }

    public Body ControlledBody
    {
      get => ControlledBodyComponent?.Body;
    }

    // Set this from the outside to make this component perform some movement.
    public Vector2 MovementVector;

    public MovementComponent(GameObject owner) : base(owner)
    {
    }

    public Vector2 ConsumeMovementVector()
    {
      Vector2 result = MovementVector;
      MovementVector = Vector2.Zero;
      return result;
    }

    public override void Initialize()
    {
      base.Initialize();

      if(ControlledBodyComponent == null)
      {
        ControlledBodyComponent = Owner.GetComponent<BodyComponent>();
      }
    }

    public override void PrePhysicsUpdate(float deltaSeconds)
    {
      base.PrePhysicsUpdate(deltaSeconds);

      Vector2 movementVector = ConsumeMovementVector();
      PerformMovement(movementVector, deltaSeconds);
    }

    public void PerformMovement(Vector2 movementVector, float deltaSeconds)
    {
      if(movementVector != Vector2.Zero)
      {
        Body body = ControlledBody;
        if(body != null)
        {
          body.LinearVelocity = movementVector * MaxMovementSpeed;
        }
        else
        {
          Owner.Spatial.Transform.p += movementVector * MaxMovementSpeed * deltaSeconds;
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
