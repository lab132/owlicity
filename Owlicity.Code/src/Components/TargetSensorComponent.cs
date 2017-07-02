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
using VelcroPhysics.Shared;

namespace Owlicity
{
  public enum TargetSensorType
  {
    Unknown,

    Circle,
    Rectangle,
  }

  public class TargetSensorComponent : BodyComponent
  {
    public Category TargetCollisionCategories = 0;
    public TargetSensorType SensorType;

    public float CircleSensorRadius;

    public AABB RectangleSensorLocalAABB;

    public List<Body> CurrentTargetList = new List<Body>();
    public Body CurrentMainTarget => CurrentTargetList.FirstOrDefault();


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
            bodyType: BodyType.Static,
            position: s.Position,
            radius: CircleSensorRadius,
            density: 0);
        }
        break;

        case TargetSensorType.Rectangle:
        {
          Body = BodyFactory.CreateRectangle(
            world: Global.Game.World,
            bodyType: BodyType.Static,
            position: RectangleSensorLocalAABB.Center,
            width: RectangleSensorLocalAABB.Width,
            height: RectangleSensorLocalAABB.Height,
            density: 0);
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
      Body target = theirFixture.Body;
      if(!CurrentTargetList.Contains(target))
        CurrentTargetList.Add(target);
    }

    private void OnSeparation(Fixture ourFixture, Fixture theirFixture, Contact contact)
    {
      Body target = theirFixture.Body;
      bool wasRemoved = CurrentTargetList.Remove(target);
      Debug.Assert(wasRemoved);
    }
  }
}
