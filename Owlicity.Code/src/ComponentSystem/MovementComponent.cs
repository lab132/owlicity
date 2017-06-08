using Microsoft.Xna.Framework;
using VelcroPhysics.Dynamics;

namespace Owlicity
{
  public class MovementComponent : ComponentBase
  {
    //
    // Init data
    //
    public float MaxMovementSpeed = 1.5f;
    public float MovementDamping = 0.15f; // Loss of linear velocity per frame.

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
          Vector2 impulse = movementVector;
          body.ApplyLinearImpulse(ref impulse);
          body.LinearVelocity = body.LinearVelocity.GetClampedTo(MaxMovementSpeed);
        }
        else
        {
          Owner.Spatial.Position += movementVector * MaxMovementSpeed * deltaSeconds;
        }
      }
    }

    public override void Update(float deltaSeconds)
    {
      base.Update(deltaSeconds);

      Body body = ControlledBody;
      if(body != null && MovementDamping > 0.0f)
      {
        float preserved = 1 - MovementDamping;
        body.LinearVelocity *= preserved;
      }
    }
  }
}
