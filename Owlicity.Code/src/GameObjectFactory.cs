﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Factories;

namespace Owlicity
{
  public static class GameObjectFactory
  {
    private static Random _random;
    private static int[] _knownCreationCount;

    public static void Initialize()
    {
      _random = new Random();
      _knownCreationCount = new int[Enum.GetNames(typeof(KnownGameObject)).Length];
    }

    public static SquashComponent CreateOnHitSquasher(GameObject go, HealthComponent health)
    {
      var result = new SquashComponent(go);
      health.OnHit += (damage) =>
      {
        result.StartSequence();
      };

      return result;
    }

    public static BlinkingSequenceComponent CreateOnHitBlinkingSequence(GameObject go, HealthComponent health)
    {
      var result = new BlinkingSequenceComponent(go);
      health.OnHit += (damage) =>
      {
        result.StartSequence();
      };

      return result;
    }

    public static GameObject CreateKnown(KnownGameObject type)
    {
      GameObject go = new GameObject();
      switch(type)
      {
        case KnownGameObject.Owliver:
        {
          go = new Owliver();
        }
        break;

        case KnownGameObject.Shop:
        {
          go = new Shop();
        }
        break;

        case KnownGameObject.Slurp:
        {
          go = new Slurp();
        }
        break;

        case KnownGameObject.Tankton:
        {
          go = new Tankton();
        }
        break;

        case KnownGameObject.DeathConfetti:
        {
          go = new DeathConfetti();
        }
        break;

        case KnownGameObject.Projectile:
        {
          go = new Projectile();
        }
        break;

        case KnownGameObject.BackgroundScreen:
        {
          go = new BackgroundScreen();
        }
        break;

        case KnownGameObject.Gate:
        {
          go = new Gate();
        }
        break;

        case KnownGameObject.Flora_Fir:
        case KnownGameObject.Flora_FirAlt:
        case KnownGameObject.Flora_Conifer:
        case KnownGameObject.Flora_ConiferAlt:
        case KnownGameObject.Flora_Oak:
        case KnownGameObject.Flora_Orange:
        case KnownGameObject.Flora_Bush:
        {
          FloraType floraType = (FloraType)(type - KnownGameObject.Flora_Fir);
          go = new Flora()
          {
            TreeType = floraType,
          };
        }
        break;

        case KnownGameObject.Bonbon_Gold:
        case KnownGameObject.Bonbon_Red:
        {
          BonbonType bonbonType = (BonbonType)(type - KnownGameObject.Bonbon_Gold);
          go = new BonbonPickup()
          {
            BonbonType = bonbonType,
          };
        }
        break;

        case KnownGameObject.Key_Gold:
        {
          KeyType keyType = (KeyType)(type - KnownGameObject.Key_Gold);
          go = new KeyPickup()
          {
            KeyType = keyType,
          };
        }
        break;

        case KnownGameObject.ShopItem_FruitBowl:
        case KnownGameObject.ShopItem_FishingRod:
        case KnownGameObject.ShopItem_Stick:
        {
          var bc = new BodyComponent(go)
          {
            InitMode = BodyComponentInitMode.Manual,
          };
          bc.BeforeInitialize += () =>
          {
            SpatialData s = bc.GetWorldSpatialData();
            bc.Body = BodyFactory.CreateBody(
              world: Global.Game.World,
              position: s.Position,
              rotation: s.Rotation.Radians,
              bodyType: BodyType.Static,
              userData: bc);
            FixtureFactory.AttachRectangle(
              body: bc.Body,
              width: Conversion.ToMeters(100),
              height: Conversion.ToMeters(100),
              offset: Conversion.ToMeters(0, 100),
              density: Global.OwliverDensity,
              userData: bc);
            bc.Body.IsSensor = true;
            bc.Body.CollidesWith = Global.OwliverCollisionCategory;
          };
          go.RootComponent = bc;

          var sacItem = new SpriteAnimationComponent(go)
          {
            DepthReference = null,
            RenderDepth = 0.1f,
            AnimationTypes = new List<SpriteAnimationType>(),
          };
          sacItem.AttachTo(bc);

          var sacPriceTag = new SpriteAnimationComponent(go)
          {
            DepthReference = null,
            RenderDepth = 0.11f,
            AnimationTypes = new List<SpriteAnimationType>
            {
              SpriteAnimationType.PriceTag_20,
              SpriteAnimationType.PriceTag_100,
            },
          };
          sacPriceTag.Spatial.Position.Y -= Conversion.ToMeters(10);
          sacPriceTag.AttachTo(sacItem);

          var sic = new ShopItemComponent(go)
          {
            PriceTag = sacPriceTag,
          };

          switch(type)
          {
            case KnownGameObject.ShopItem_FruitBowl:
            {
              sacItem.AnimationTypes.Add(SpriteAnimationType.FruitBowl);
              sic.ItemType = ShopItemType.FruitBowl;
            }
            break;

            case KnownGameObject.ShopItem_FishingRod:
            {
              sacItem.AnimationTypes.Add(SpriteAnimationType.FishingRod_Left);
              sic.ItemType = ShopItemType.FishingRod;
            }
            break;

            case KnownGameObject.ShopItem_Stick:
            {
              sacItem.AnimationTypes.Add(SpriteAnimationType.Stick_Left);
              sic.ItemType = ShopItemType.Stick;
            }
            break;

            default: throw new ArgumentException(nameof(type));
          }
        }
        break;

        case KnownGameObject.Random_FirTree:
        {
          FloraType floraType = _random.Choose(FloraType.Fir, FloraType.Conifer);
          go = new Flora() { TreeType = floraType, };
        }
        break;

        case KnownGameObject.Random_FirTreeAlt:
        {
          FloraType floraType = _random.Choose(FloraType.FirAlt, FloraType.ConiferAlt);
          go = new Flora() { TreeType = floraType, };
        }
        break;

        case KnownGameObject.Random_OakTree:
        {
          FloraType floraType = _random.Choose(FloraType.Oak, FloraType.Orange);
          go = new Flora() { TreeType = floraType, };
        }
        break;

        default:
        throw new ArgumentException("Unknown game object type.");
      }

      int instanceID = _knownCreationCount[(int)type]++;
      go.Name = $"{type}_{instanceID}";

      return go;
    }
  }
}
