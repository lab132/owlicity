using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelcroPhysics.Collision.ContactSystem;
using VelcroPhysics.Dynamics;

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

  public class ShopItemComponent : ComponentBase
  {
    //
    // Initialization data.
    //
    public ShopItemType ItemType;
    public int ItemAmount = 1;
    public ShopItemPriceType Price;
    public SpriteAnimationComponent PriceTag; // Required.

    //
    // Runtime data.
    //
    public int PriceValue
    {
      get => (int)Price;
      set => Price = (ShopItemPriceType)value;
    }

    public bool IsSelected;
    public bool IsAffordable;


    public ShopItemComponent(GameObject owner)
      : base(owner)
    {
    }

    public override void Initialize()
    {
      base.Initialize();

      switch(Price)
      {
        case ShopItemPriceType.Free:
        {
          PriceTag.IsUpdateEnabled = false;
          PriceTag.IsDrawEnabled = false;
        }
        break;

        case ShopItemPriceType._20:
        {
          PriceTag.ChangeActiveAnimation(SpriteAnimationType.PriceTag_20);
        }
        break;

        case ShopItemPriceType._100:
        {
          PriceTag.ChangeActiveAnimation(SpriteAnimationType.PriceTag_100);
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
        PriceTag.AdditionalScale = new Vector2(1.5f);

        if(IsAffordable)
        {
          PriceTag.Tint = new Color(0.5f, 1.0f, 0.5f, 1.0f);
        }
        else
        {
          PriceTag.Tint = new Color(1.0f, 0.5f, 0.5f, 1.0f);
        }
      }
      else
      {
        PriceTag.AdditionalScale = null;
        PriceTag.Tint = null;
      }
    }
  }
}
