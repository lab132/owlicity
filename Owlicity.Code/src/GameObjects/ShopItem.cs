using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Factories;

namespace Owlicity
{
  public enum ShopItemPriceType
  {
    Free = 0,
    _20 = 20,
    _100 = 100,
  }

  public enum ShopItemType
  {
    Unknown,

    FruitBowl,
    FishingRod,
    Stick,
  }

  public class ShopItem : GameObject
  {
    public BodyComponent BodyComponent;
    public SpriteAnimationComponent ItemAnimation;
    public SpriteAnimationComponent PriceTagAnimation;

    public ShopItemType ItemType;
    public int ItemAmount = 1;
    public ShopItemPriceType Price;

    public int PriceValue
    {
      get => (int)Price;
      set => Price = (ShopItemPriceType)value;
    }

    public bool IsSelected;
    public bool IsAffordable;


    public ShopItem()
    {
      BodyComponent = new BodyComponent(this)
      {
        InitMode = BodyComponentInitMode.Manual,
      };
      this.RootComponent = BodyComponent;

      ItemAnimation = new SpriteAnimationComponent(this)
      {
        DepthReference = null,
        RenderDepth = 0.1f,
        AnimationTypes = new List<SpriteAnimationType>(),
      };
      ItemAnimation.AttachTo(BodyComponent);

      PriceTagAnimation = new SpriteAnimationComponent(this)
      {
        DepthReference = null,
        RenderDepth = 0.11f,
        AnimationTypes = new List<SpriteAnimationType>
            {
              SpriteAnimationType.PriceTag_20,
              SpriteAnimationType.PriceTag_100,
            },
      };
      PriceTagAnimation.Spatial.Position.Y -= Conversion.ToMeters(10);
      PriceTagAnimation.AttachTo(ItemAnimation);
    }

    public override void Initialize()
    {
      SpatialData s = BodyComponent.GetWorldSpatialData();
      BodyComponent.Body = BodyFactory.CreateBody(
        world: Global.Game.World,
        position: s.Position,
        rotation: s.Rotation.Radians,
        bodyType: BodyType.Static);
      FixtureFactory.AttachRectangle(
        body: BodyComponent.Body,
        width: Conversion.ToMeters(100),
        height: Conversion.ToMeters(100),
        offset: Conversion.ToMeters(0, 100),
        density: Global.OwliverDensity);
      BodyComponent.Body.IsSensor = true;
      BodyComponent.Body.CollidesWith = CollisionCategory.Friendly;

      switch(ItemType)
      {
        case ShopItemType.FruitBowl: ItemAnimation.AnimationTypes.Add(SpriteAnimationType.FruitBowl); break;
        case ShopItemType.FishingRod: ItemAnimation.AnimationTypes.Add(SpriteAnimationType.FishingRod_Left); break;
        case ShopItemType.Stick: ItemAnimation.AnimationTypes.Add(SpriteAnimationType.Stick_Left); break;

        default: throw new ArgumentException(nameof(ItemType));
      }

      base.Initialize();

      switch(Price)
      {
        case ShopItemPriceType.Free:
        {
          PriceTagAnimation.IsUpdateEnabled = false;
          PriceTagAnimation.IsDrawEnabled = false;
        }
        break;

        case ShopItemPriceType._20:
        {
          PriceTagAnimation.ChangeActiveAnimation(SpriteAnimationType.PriceTag_20);
        }
        break;

        case ShopItemPriceType._100:
        {
          PriceTagAnimation.ChangeActiveAnimation(SpriteAnimationType.PriceTag_100);
        }
        break;

        default: throw new ArgumentException(nameof(Price));
      }
    }

    public override void Update(float deltaSeconds)
    {
      base.Update(deltaSeconds);

      if(IsSelected)
      {
        PriceTagAnimation.AdditionalScale = new Vector2(1.5f);

        if(IsAffordable)
        {
          PriceTagAnimation.Tint = new Color(0.5f, 1.0f, 0.5f, 1.0f);
        }
        else
        {
          PriceTagAnimation.Tint = new Color(1.0f, 0.5f, 0.5f, 1.0f);
        }
      }
      else
      {
        PriceTagAnimation.AdditionalScale = null;
        PriceTagAnimation.Tint = null;
      }
    }
  }
}
