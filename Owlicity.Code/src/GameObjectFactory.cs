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

    public static SquashComponent CreateOnHitSquasher(GameObject go, HealthComponent health, SpriteAnimationComponent animation)
    {
      var result = new SquashComponent(go)
      {
        Animation = animation,
      };
      health.OnHit += (damage) =>
      {
        result.StartSequence();
      };

      return result;
    }

    public static BlinkingSequenceComponent CreateOnHitBlinkingSequence(GameObject go, HealthComponent health, SpriteAnimationComponent animation)
    {
      var result = new BlinkingSequenceComponent(go)
      {
        Animation = animation,
      };
      health.OnHit += (damage) =>
      {
        result.StartSequence();
      };

      return result;
    }

    public static HomingComponent CreateDefaultHomingCircle(
      GameObject owner,
      BodyComponent bodyComponentToMove,
      float sensorRadius,
      HomingType homingType,
      float homingSpeed)
    {
      var tsc = new TargetSensorComponent(owner)
      {
        TargetCollisionCategories = CollisionCategory.Owliver,
        SensorType = TargetSensorType.Circle,
        CircleSensorRadius = sensorRadius,
      };
      tsc.AttachTo(bodyComponentToMove);

      var hoc = new HomingComponent(owner)
      {
        BodyComponentToMove = bodyComponentToMove,
        TargetSensor = tsc,
        Speed = homingSpeed,
        HomingType = homingType,

        DebugDrawingEnabled = true,
      };
      hoc.AttachTo(bodyComponentToMove);

      return hoc;
    }

    public static HealthComponent CreateDefaultHealth(GameObject owner,
      int maxHealth,
      TimeSpan hitDuration,
      TimeSpan deathParticleTimeToLive)
    {
      var hc = new HealthComponent(owner) { MaxHealth = maxHealth };
      hc.OnHit += (damage) =>
      {
        hc.MakeInvincible(hitDuration);
      };

      hc.OnDeath += (damage) =>
      {
        DeathConfetti confetti = new DeathConfetti();
        confetti.Spatial.CopyFrom(owner.Spatial);
        confetti.AutoDestruct.DestructionDelay = deathParticleTimeToLive;
        Global.Game.AddGameObject(confetti);

        Global.Game.RemoveGameObject(owner);
      };

      return hc;
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
          ShopItemType itemType = (ShopItemType)(type - KnownGameObject.ShopItem_FruitBowl);
          go = new ShopItem() { ItemType = itemType };
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
