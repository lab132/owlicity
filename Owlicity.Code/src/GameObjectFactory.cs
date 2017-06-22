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
          var adc = new AutoDestructComponent(go)
          {
            DestructionDelay = TimeSpan.FromSeconds(1.0f),
          };

          var pec = new ParticleEmitterComponent(go)
          {
            NumParticles = 64,

            TextureContentNames = new[]
            {
              "confetti/confetti_01",
              "confetti/confetti_02",
              "confetti/confetti_03",
              "confetti/confetti_04",
              "confetti/confetti_05",
              "confetti/confetti_06",
              "confetti/confetti_07",
            },

            AvailableColors = Global.AllConfettiColors,
          };

          pec.BeforePostInitialize += delegate ()
          {
            pec.Emitter.MaxTTL = 0.8f * (float)adc.DestructionDelay.TotalSeconds;
            pec.Emitter.MaxParticleSpread = 0.05f;
            pec.Emitter.MaxParticleSpeed = 5f;
            pec.Emit(go.GetWorldSpatialData().Position);
          };
        }
        break;

        case KnownGameObject.Projectile:
        {
          var bc = new BodyComponent(go)
          {
            InitMode = BodyComponentInitMode.Manual,
          };
          bc.BeforePostInitialize += () =>
          {
            SpatialData s = go.GetWorldSpatialData();
            bc.Body = BodyFactory.CreateCapsule(
              world: Global.Game.World,
              endRadius: Conversion.ToMeters(9),
              height: Conversion.ToMeters(50),
              userData: bc,
              position: s.Position,
              rotation: s.Rotation.Radians + new Angle { Degrees = 90.0f + new Random().NextBilateralFloat() * 2 }.Radians,
              density: 10 * Global.OwliverDensity,
              bodyType: BodyType.Dynamic);
            bc.Body.CollisionCategories = Global.OwliverWeaponCollisionCategory;
            bc.Body.CollidesWith = ~(Global.OwliverCollisionCategory | Global.OwliverWeaponCollisionCategory);
            bc.Body.IsBullet = true;
            bc.Body.OnCollision += (fixtureA, fixtureB, contact) =>
            {
              Global.HandleDefaultHit(fixtureB.Body, bc.Body.Position, 1, 0.1f);

              Global.Game.RemoveGameObject(go);

              var confetti = CreateKnown(KnownGameObject.DeathConfetti);
              confetti.Spatial.CopyFrom(go.Spatial);

              confetti.GetComponent<AutoDestructComponent>().DestructionDelay = TimeSpan.FromSeconds(0.25f);

              var confettiPec = confetti.GetComponent<ParticleEmitterComponent>();
              confettiPec.NumParticles = 16;
              Vector2 g = -2.5f * bc.Body.LinearVelocity;
              confettiPec.BeforePostInitialize += () =>
              {
                confettiPec.Emitter.Gravity = g;
              };

              Global.Game.AddGameObject(confetti);
            };
          };
          go.RootComponent = bc;

          var sac = new SpriteAnimationComponent(go)
          {
            AnimationTypes = new List<SpriteAnimationType>
            {
              SpriteAnimationType.Bonbon_Red,
            },
          };
          sac.Spatial.Rotation.Degrees -= 45.0f;
          sac.AttachTo(bc);

          var adc = new AutoDestructComponent(go);
        }
        break;

        case KnownGameObject.BackgroundScreen:
        {
          go.Layer = GameLayer.Background;

          var bc = new BodyComponent(go)
          {
            InitMode = BodyComponentInitMode.FromContent,
            BodyType = BodyType.Static,
          };
          bc.BeforePostInitialize += () =>
          {
            bc.Body.CollisionCategories = Global.LevelCollisionCategory;
          };
          go.RootComponent = bc;

          var sc = new SpriteComponent(go)
          {
            DepthReference = null, // Don't determine depth automatically
            RenderDepth = 1.0f,
          };
          sc.AttachTo(bc);
        }
        break;

        case KnownGameObject.Gate:
        {
          float outerLeft = Conversion.ToMeters(310);
          float innerLeft = Conversion.ToMeters(79);
          float inner = Conversion.ToMeters(80);
          float innerRight = Conversion.ToMeters(79);
          float outerRight = Conversion.ToMeters(222);
          float width = Conversion.ToMeters(768);
          float height = Conversion.ToMeters(128);
          float barrierHeight = Conversion.ToMeters(20);
          float density = Global.OwliverDensity;

          var leftEdge = new SpatialComponent(go);
          {
            SpriteAnimationData anim = SpriteAnimationFactory.GetAnimation(SpriteAnimationType.Gate_Closed);
            Vector2 hotspot = anim.Config.Hotspot * anim.Config.Scale;
            float offset = Conversion.ToMeters(hotspot.X - 128);
            leftEdge.Spatial.Position.X -= offset;
          }
          leftEdge.AttachTo(go);

          var bcInner = new BodyComponent(go)
          {
            InitMode = BodyComponentInitMode.Manual,
          };
          bcInner.BeforePostInitialize += () =>
          {
            SpatialData s = bcInner.GetWorldSpatialData();
            bcInner.Body = BodyFactory.CreateRectangle(
              world: Global.Game.World,
              width: innerLeft + inner + innerRight,
              height: barrierHeight,
              density: density,
              position: s.Position + new Vector2(outerLeft + innerLeft + 0.5f * inner, 0.0f),
              rotation: s.Rotation.Radians,
              bodyType: BodyType.Static,
              userData: bcInner);
          };
          bcInner.AttachTo(leftEdge);

          var bcOuter = new BodyComponent(go)
          {
            InitMode = BodyComponentInitMode.Manual,
          };
          bcOuter.BeforePostInitialize += () =>
          {
            SpatialData s = bcOuter.GetWorldSpatialData();
            bcOuter.Body = BodyFactory.CreateBody(
              world: Global.Game.World,
              position: s.Position,
              rotation: s.Rotation.Radians,
              bodyType: BodyType.Static,
              userData: bcOuter);

            Vector2 offsetRight = Conversion.ToMeters(300, 0);
            FixtureFactory.AttachRectangle(
              body: bcOuter.Body,
              width: outerLeft,
              height: barrierHeight,
              offset: new Vector2(0.5f * outerLeft, 0.0f),
              density: density,
              userData: bcOuter);
            FixtureFactory.AttachRectangle(
              body: bcOuter.Body,
              width: outerRight,
              height: barrierHeight,
              offset: new Vector2(width - 0.5f * outerRight, 0.0f),
              density: density,
              userData: bcOuter);
          };
          bcOuter.AttachTo(leftEdge);

          var sac = new SpriteAnimationComponent(go)
          {
            AnimationTypes = new List<SpriteAnimationType>
            {
               SpriteAnimationType.Gate_Closed,
               SpriteAnimationType.Gate_Open,
            },
          };
          sac.AttachTo(go);

          var gac = new GateComponent(go)
          {
            Dimensions = Conversion.ToMeters(60, 130),
            UnlockableBlockade = bcInner,
          };
          gac.Spatial.Position.Y -= Conversion.ToMeters((0.5f * 128) - 20);
          gac.AttachTo(go);
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
