using Microsoft.Xna.Framework;
using VelcroPhysics.Dynamics;

namespace Owlicity
{
  public class MovementComponent : ComponentBase
  {
    //
    // Init data
    //
    public bool ManualInputProcessing;
    public float MaxMovementSpeed = 1.5f;

    //
    // Runtime data
    //
    public BodyComponent BodyComponent { get; set; }
    public Body MyBody => BodyComponent?.Body;

    public bool IsMovementEnabled = true;

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

    public override void PrePhysicsUpdate(float deltaSeconds)
    {
      base.PrePhysicsUpdate(deltaSeconds);

      if(!ManualInputProcessing)
      {
        Vector2 movementVector = ConsumeMovementVector();
        PerformMovement(movementVector, deltaSeconds);
      }
    }

    public void PerformMovement(Vector2 movementVector, float deltaSeconds)
    {
      if(movementVector != Vector2.Zero)
      {
        Body body = MyBody;
        if(body != null)
        {
          Vector2 impulse = body.Mass * movementVector;
          body.ApplyLinearImpulse(ref impulse);
          body.LinearVelocity = body.LinearVelocity.GetClampedTo(MaxMovementSpeed);
        }
        else
        {
          Owner.Spatial.Position += movementVector * MaxMovementSpeed * deltaSeconds;
        }
      }
    }
  }
}
