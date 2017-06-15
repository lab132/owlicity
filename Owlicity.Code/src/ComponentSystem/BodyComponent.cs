using Microsoft.Xna.Framework;
using System.Collections.Generic;
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

    public bool AllowDebugDrawing = true;

    //
    // Runtime data
    //
    public Body Body;

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
            position: Spatial.Position,
            rotation: Spatial.Rotation.Radians,
            bodyType: BodyType,
            userdata: this);
          List<Vertices> listOfVertices = Global.Game.Content.Load<List<Vertices>>(ShapeContentName);
          foreach(Vertices vertices in listOfVertices)
          {
            FixtureFactory.AttachLoopShape(vertices, Body, this);
          }
        }
        break;
      }
    }

    public override void Destroy()
    {
      base.Destroy();

      Global.Game.World.RemoveBody(Body);
    }

    public override void Update(float deltaSeconds)
    {
      base.Update(deltaSeconds);
      if(Body != null)
      {
        Spatial.Position = Body.Position;
        Spatial.Rotation.Radians = Body.Rotation;
      }

      if(AllowDebugDrawing)
      {
        Vector2 start = this.GetWorldSpatialData().Position;
        Vector2 end = start + Body.LinearVelocity;
        Global.Game.DebugDrawCommands.Add(view =>
        {
          view.DrawArrow(start, end,
            length: 0.1f, // arrow "head"
            width: 0.05f, // arrow "head"
            drawStartIndicator: false, // Note(manu): This one doesn't seem to work.
            color: Color.LimeGreen);
        });
      }
    }
  }
}
