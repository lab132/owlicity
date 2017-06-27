using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Factories;

namespace Owlicity
{
  public class Shop : GameObject
  {
    public BodyComponent BodyComponent;
    public SpriteAnimationComponent ShopAnimation;
    public SpriteAnimationComponent ShopkeeperAnimation;

    public Shop()
    {
      BodyComponent = new BodyComponent(this)
      {
        InitMode = BodyComponentInitMode.Manual,
      };
      RootComponent = BodyComponent;

      ShopAnimation = new SpriteAnimationComponent(this)
      {
        AnimationTypes = new List<SpriteAnimationType>
        {
          SpriteAnimationType.Shop,
        },
      };
      ShopAnimation.AttachTo(BodyComponent);

      ShopkeeperAnimation = new SpriteAnimationComponent(this)
      {
        AnimationTypes = new List<SpriteAnimationType>
        {
          SpriteAnimationType.Shopkeeper_Idle_Front,
        },
      };
      ShopkeeperAnimation.Spatial.Position.Y -= Conversion.ToMeters(100);
      ShopkeeperAnimation.AttachTo(ShopAnimation);
    }

    public override void Initialize()
    {
      SpatialData spatial = this.GetWorldSpatialData();

      Body body = BodyFactory.CreateBody(
        world: Global.Game.World,
        position: spatial.Position,
        rotation: spatial.Rotation.Radians);

      FixtureFactory.AttachRectangle(
        body: body,
        offset: Conversion.ToMeters(0, -50),
        width: Conversion.ToMeters(330),
        height: Conversion.ToMeters(100),
        density: Global.OwliverDensity);
      FixtureFactory.AttachRectangle(
        body: body,
        offset: Conversion.ToMeters(0, -160),
        width: Conversion.ToMeters(280),
        height: Conversion.ToMeters(150),
        density: Global.OwliverDensity);

      BodyComponent.Body = body;

      base.Initialize();
    }
  }
}
