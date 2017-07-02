using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelcroPhysics.Collision.ContactSystem;
using VelcroPhysics.Collision.Filtering;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Factories;

namespace Owlicity
{
  public enum TargetSensorType
  {
    Unknown,

    Circle,
  }

  public class TargetSensorComponent : BodyComponent
  {
    public Category TargetCollisionCategories = 0;
    public TargetSensorType SensorType;

    public float CircleSensorRadius;

    public List<ISpatial> CurrentTargetList = new List<ISpatial>();
    public ISpatial CurrentMainTarget => CurrentTargetList.FirstOrDefault();


    public TargetSensorComponent(GameObject owner)
      : base(owner)
    {
    }

    public override void Initialize()
    {
      Debug.Assert(TargetCollisionCategories != 0, "You should have set target collision categories by now.");
      SpatialData s = this.GetWorldSpatialData();

      switch(SensorType)
      {
        case TargetSensorType.Unknown: throw new NotImplementedException();

        case TargetSensorType.Circle:
        {
          Debug.Assert(CircleSensorRadius > 0.0f);
          Body = BodyFactory.CreateCircle(
            world: Global.Game.World,
            radius: CircleSensorRadius,
            density: 0,
            position: s.Position,
            bodyType: BodyType.Static);
        }
        break;

        default: throw new ArgumentException(nameof(SensorType));
      }

      base.Initialize();

      Body.IsSensor = true;
      Body.CollidesWith = TargetCollisionCategories;
      Body.OnCollision += OnCollision;
      Body.OnSeparation += OnSeparation;
    }

    public override void Update(float deltaSeconds)
    {
      SpatialData s = this.GetWorldSpatialData();
      Body.SetTransform(ref s.Position, s.Rotation.Radians);

      base.Update(deltaSeconds);
    }

    private void OnCollision(Fixture ourFixture, Fixture theirFixture, Contact contact)
    {
      ISpatial target = ((BodyComponent)theirFixture.UserData).Owner;
      if(!CurrentTargetList.Contains(target))
        CurrentTargetList.Add(target);
    }

    private void OnSeparation(Fixture ourFixture, Fixture theirFixture, Contact contact)
    {
      ISpatial target = ((BodyComponent)theirFixture.UserData).Owner;
      bool wasRemoved = CurrentTargetList.Remove(target);
      Debug.Assert(wasRemoved);
    }
  }
}
