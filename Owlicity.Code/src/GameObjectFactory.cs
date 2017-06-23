using Microsoft.Xna.Framework;
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

        case KnownGameObject.Tree_Fir:
        case KnownGameObject.Tree_FirAlt:
        case KnownGameObject.Tree_Conifer:
        case KnownGameObject.Tree_ConiferAlt:
        case KnownGameObject.Tree_Oak:
        case KnownGameObject.Tree_Orange:
        case KnownGameObject.Bush:
        {
          List<SpriteAnimationType> animTypes = new List<SpriteAnimationType>();
          switch(type)
          {
            case KnownGameObject.Tree_Fir: animTypes.Add(SpriteAnimationType.Fir_Idle); break;
            case KnownGameObject.Tree_FirAlt: animTypes.Add(SpriteAnimationType.FirAlt_Idle); go.Layer = GameLayer.CloseToTheScreen; break;
            case KnownGameObject.Tree_Conifer: animTypes.Add(SpriteAnimationType.Conifer_Idle); break;
            case KnownGameObject.Tree_ConiferAlt: animTypes.Add(SpriteAnimationType.ConiferAlt_Idle); go.Layer = GameLayer.CloseToTheScreen; break;
            case KnownGameObject.Tree_Oak: animTypes.Add(SpriteAnimationType.Oak_Idle); break;
            case KnownGameObject.Tree_Orange: animTypes.Add(SpriteAnimationType.Orange_Idle); break;
            case KnownGameObject.Bush: animTypes.Add(SpriteAnimationType.Bush_Idle); break;

            default: throw new InvalidProgramException();
          }

          var sa = new SpriteAnimationComponent(go)
          {
            AnimationTypes = animTypes,
            DepthReference = null, // Don't determine depth automatically
          };
          sa.BeforePostInitialize += () =>
          {
            sa.RenderDepth = Global.Game.CalcDepth(sa.GetWorldSpatialData(), go.Layer);
          };
          sa.AttachTo(go);
        }
        break;

        case KnownGameObject.Bonbon_Gold:
        case KnownGameObject.Bonbon_Red:
        {
          SpriteAnimationType animType;
          switch(type)
          {
            case KnownGameObject.Bonbon_Gold: animType = SpriteAnimationType.Bonbon_Gold; break;
            case KnownGameObject.Bonbon_Red: animType = SpriteAnimationType.Bonbon_Red; break;
            default: throw new InvalidProgramException();
          }

          var bc = new BodyComponent(go)
          {
            InitMode = BodyComponentInitMode.Manual,
          };

          bc.BeforeInitialize += () =>
          {
            SpatialData s = bc.GetWorldSpatialData();
            bc.Body = BodyFactory.CreateCircle(
              world: Global.Game.World,
              radius: 0.2f,
              density: 0.5f * Global.OwliverDensity,
              position: s.Position,
              userData: bc);
            bc.Body.IsSensor = true;
            bc.Body.CollidesWith = Global.OwliverCollisionCategory;
          };

          go.RootComponent = bc;

          var sac = new SpriteAnimationComponent(go)
          {
            AnimationTypes = new List<SpriteAnimationType>
            {
              animType,
            }
          };
          sac.AttachTo(bc);

          var mbc = new MoneyBagComponent(go)
          {
            InitialAmount = 10,
          };

          var puc = new PickupComponent(go);

          var hoc = new HomingComponent(go)
          {
            TargetRange = 1.0f,
            Speed = 3.0f,

            DebugDrawingEnabled = true,
          };
          hoc.BeforeInitialize += () =>
          {
            hoc.Target = Global.Game.Owliver;
          };
          hoc.AttachTo(bc);
        }
        break;

        case KnownGameObject.Key_Gold:
        {
          var bc = new BodyComponent(go)
          {
            InitMode = BodyComponentInitMode.Manual,
          };

          bc.BeforeInitialize += () =>
          {
            SpatialData s = bc.GetWorldSpatialData();
            bc.Body = BodyFactory.CreateCircle(
              world: Global.Game.World,
              radius: 0.2f,
              density: 2 * Global.OwliverDensity,
              position: s.Position,
              userData: bc);
            bc.Body.IsSensor = true;
            bc.Body.CollidesWith = Global.OwliverCollisionCategory;
          };

          go.RootComponent = bc;

          var sac = new SpriteAnimationComponent(go)
          {
            AnimationTypes = new List<SpriteAnimationType>
            {
              SpriteAnimationType.Key_Gold,
            }
          };
          sac.AttachTo(bc);

          var krc = new KeyRingComponent(go);
          krc.InitialKeyAmounts[(int)KeyType.Gold] = 1;

          var puc = new PickupComponent(go);

          var hoc = new HomingComponent(go)
          {
            TargetRange = 1.0f,
            Speed = 3.0f,

            DebugDrawingEnabled = true,
          };
          hoc.BeforeInitialize += () =>
          {
            hoc.Target = Global.Game.Owliver;
          };
          hoc.AttachTo(bc);
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
          KnownGameObject choice = _random.Choose(KnownGameObject.Tree_Fir, KnownGameObject.Tree_Conifer);
          go = CreateKnown(choice);
        }
        break;

        case KnownGameObject.Random_FirTreeAlt:
        {
          KnownGameObject choice = _random.Choose(KnownGameObject.Tree_FirAlt, KnownGameObject.Tree_ConiferAlt);
          go = CreateKnown(choice);
        }
        break;

        case KnownGameObject.Random_OakTree:
        {
          KnownGameObject choice = _random.Choose(KnownGameObject.Tree_Oak, KnownGameObject.Tree_Orange);
          go = CreateKnown(choice);
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
