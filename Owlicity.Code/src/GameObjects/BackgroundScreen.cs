using Microsoft.Xna.Framework;
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
  public class BackgroundScreen : GameObject
  {
    public BodyComponent BodyComponent;
    public SpriteComponent Sprite;

    public string ShapeContentName;

    public BackgroundScreen()
    {
      Layer = GameLayer.Background;

      BodyComponent = new BodyComponent(this)
      {
        InitMode = BodyComponentInitMode.Manual,
      };
      RootComponent = BodyComponent;

      Sprite = new SpriteComponent(this)
      {
        DepthReference = null, // Don't determine depth automatically.
        RenderDepth = 1.0f, // Always render in the background.
      };
      Sprite.AttachTo(BodyComponent);
    }

    public override void Initialize()
    {
      var body = new Body(
        world: Global.Game.World,
        position: Spatial.Position,
        rotation: Spatial.Rotation.Radians,
        bodyType: BodyType.Static,
        userdata: BodyComponent);

      List<Vertices> listOfVertices = Global.Game.Content.Load<List<Vertices>>(ShapeContentName);
      foreach(Vertices vertices in listOfVertices)
      {
        FixtureFactory.AttachLoopShape(vertices, body, userData: BodyComponent);
      }
      body.CollisionCategories = Global.LevelCollisionCategory;
      BodyComponent.Body = body;

      base.Initialize();
    }
  }
}
