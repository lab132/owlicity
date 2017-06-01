using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Factories;
using VelcroPhysics.Shared;

namespace Owlicity
{
  public enum BodyComponentInitMode
  {
    Manual,
    FromContent,
  }

  public class BodyComponent : SpatialComponent
  {
    //
    // Init data
    //
    public BodyComponentInitMode InitMode;
    public BodyType BodyType;
    public string ShapeContentName;

    //
    // Runtime data
    //
    public Body Body;
    public Fixture Fixture;

    public BodyComponent(GameObject owner) : base(owner)
    {
    }

    public override void Initialize()
    {
      base.Initialize();
      switch(InitMode)
      {
        case BodyComponentInitMode.Manual:
        {
          // TODO(manu): ??
        }
        break;

        case BodyComponentInitMode.FromContent:
        {
          Body = new Body(
            world: Global.Game.World,
            position: Spatial.Transform.p,
            rotation: Spatial.Transform.q.GetAngle(),
            bodyType: BodyType,
            userdata: this);
          Vertices vertices = Global.Game.Content.Load<Vertices>(ShapeContentName);
          Fixture = FixtureFactory.AttachLoopShape(vertices, Body);
        }
        break;
      }
    }

    public override void Update(float deltaSeconds)
    {
      base.Update(deltaSeconds);
      if(Body != null)
      {
        Body.GetTransform(out Spatial.Transform);
      }
    }
  }
}
