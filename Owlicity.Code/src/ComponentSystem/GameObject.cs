﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Factories;

namespace Owlicity
{
  public class GameObject : ISpatial
  {
    public List<ComponentBase> Components { get; } = new List<ComponentBase>();

    public SpatialComponent RootComponent;

    private SpatialData _spatial = new SpatialData();
    public SpatialData Spatial
    {
      get
      {
        SpatialData result;
        if(RootComponent != null)
        {
          result = RootComponent.Spatial;
        }
        else
        {
          result = _spatial;
        }

        return result;
      }
    }

    public GameLayer Layer = GameLayer.Default;
    public bool IsStationary;

    public void AddComponent(ComponentBase newComponent)
    {
      Debug.Assert(!Components.Contains(newComponent));
      Components.Add(newComponent);
    }

    public T GetComponent<T>()
      where T : ComponentBase
    {
      return Components.OfType<T>().FirstOrDefault();
    }

    public IEnumerable<T> GetComponents<T>()
      where T : ComponentBase
    {
      return Components.OfType<T>();
    }

    public void Initialize()
    {
      ComponentBase[] toInit = Components.Where(c => c.IsInitializationEnabled).ToArray();
      foreach(ComponentBase component in toInit)
      {
        component.BeforeInitialize?.Invoke();
        component.Initialize();
      }

      foreach(ComponentBase component in toInit)
      {
        component.BeforePostInitialize?.Invoke();
        component.PostInitialize();
      }
    }

    public void PrePhysicsUpdate(float deltaSeconds)
    {
      foreach(ComponentBase component in Components.Where(c => c.IsPrePhysicsUpdateEnabled))
      {
        component.BeforePrePhysicsUpdate?.Invoke();
        component.PrePhysicsUpdate(deltaSeconds);
      }
    }

    public void Update(float deltaSeconds)
    {
      foreach(ComponentBase component in Components.Where(c => c.IsUpdateEnabled))
      {
        component.BeforeUpdate?.Invoke();
        component.Update(deltaSeconds);
      }
    }

    public void Draw(Renderer renderer)
    {
      foreach(ComponentBase component in Components.Where(c => c.IsDrawEnabled))
      {
        component.BeforeDraw?.Invoke();
        component.Draw(renderer);
      }
    }

    public void Destroy()
    {
      foreach(ComponentBase component in Components)
      {
        component.BeforeDestroy?.Invoke();
        component.Destroy();
      }
    }
  }

  public enum GameObjectType
  {
    Unknown,

    // Misc
    Camera,

    // Characters
    Owliver,

    // Mobs
    Slurp,

    // Bosses
    Tankton,

    // Static stuff
    BackgroundScreen,
    Tree_Fir,
    Tree_FirAlt, // is "upside down"
    Tree_Conifer,
    Tree_ConiferAlt, // is "upside down"
    Tree_Oak,
    Tree_Orange,
    Bush,

    // Pickups
    Bonbon_Gold,
    Bonbon_Red,
    Key_Gold,

    // Random groups
    Random_FirTree,
    Random_FirTreeAlt,
    Random_OakTree,
  }

  public static class GameObjectFactory
  {
    private static Random _random;

    public static void Initialize()
    {
      _random = new Random();
    }

    public static GameObject CreateKnown(GameObjectType type)
    {
      GameObject go = new GameObject();
      switch(type)
      {
        case GameObjectType.Camera:
        {
          var chc = new ChaserComponent(go)
          {
            Target = Global.Game.Owliver,
            TargetInnerRange = 0.2f,
            TargetRange = float.MaxValue,
            OutOfReachResponse = ChaserOutOfReachResponse.SnapToTargetAtMaximumRange,
            MovementType = ChaserMovementType.SmoothProximity,

            Speed = 5,

            DebugDrawingEnabled = false,
          };
          go.RootComponent = chc;

          var cc = new CameraComponent(go)
          {
          };
          cc.AttachTo(chc);

#if DEBUG
          var mc = new MovementComponent(go)
          {
            MaxMovementSpeed = 5.0f,
          };
#endif
        }
        break;

        case GameObjectType.Owliver:
        {
          var bc = new BodyComponent(go)
          {
            InitMode = BodyComponentInitMode.Manual,
          };
          bc.BeforeInitialize += () =>
          {
            SpatialData s = go.GetWorldSpatialData();
            bc.Body = new Body(
              world: Global.Game.World,
              position: s.Position,
              rotation: s.Rotation.Radians,
              bodyType: BodyType.Dynamic,
              userdata: bc);

            float radius = Global.ToMeters(50 * Global.OwliverScale.X);
            float density = Global.OwliverDensity;
            FixtureFactory.AttachCircle(
              radius: radius,
              density: density,
              body: bc.Body,
              offset: Global.ToMeters(0, 10) * Global.OwliverScale,
              userData: bc);
            FixtureFactory.AttachCircle(
              radius: radius,
              density: density,
              body: bc.Body,
              offset: Global.ToMeters(0, 60) * Global.OwliverScale,
              userData: bc);

            bc.Body.FixedRotation = true;
            bc.Body.CollisionCategories = Global.OwliverCollisionCategory;
            bc.Body.CollidesWith = Global.LevelCollisionCategory | Global.EnemyCollisionCategory;
          };

          go.RootComponent = bc;

          var oc = new OwliverComponent(go)
          {
          };

          var mc = new MovementComponent(go)
          {
            ManualInputProcessing = true,
            MaxMovementSpeed = 2.0f,
          };

          var sqc = new SquashComponent(go);

          var sa = new SpriteAnimationComponent(go)
          {
            AnimationTypes = new List<SpriteAnimationType>
            {
              SpriteAnimationType.Owliver_Idle_Left,
              SpriteAnimationType.Owliver_Idle_Right,
              SpriteAnimationType.Owliver_Walk_Left,
              SpriteAnimationType.Owliver_Walk_Right,
              SpriteAnimationType.Owliver_AttackStick_Left,
              SpriteAnimationType.Owliver_AttackStick_Right,
              SpriteAnimationType.Owliver_AttackFishingRod_Left,
              SpriteAnimationType.Owliver_AttackFishingRod_Right,
            },
          };
          sa.Spatial.Position += Global.ToMeters(0, -10);
          sa.AttachTo(bc);

          var hc = new HealthComponent(go)
          {
            MaxHealth = 3,
          };

          var moc = new MoneyBagComponent(go)
          {
            InitialAmount = 0,
          };

          var kc = new KeyRingComponent(go);
        }
        break;

        case GameObjectType.Slurp:
        {
          var bc = new BodyComponent(go)
          {
            InitMode = BodyComponentInitMode.Manual,
          };

          bc.BeforeInitialize += () =>
          {
            SpatialData s = go.GetWorldSpatialData();
            bc.Body = new Body(
              world: Global.Game.World,
              position: s.Position,
              rotation: s.Rotation.Radians,
              bodyType: BodyType.Dynamic,
              userdata: bc);

            float radius = Global.ToMeters(80 * Global.SlurpScale.X);
            float density = Global.OwliverDensity;
            FixtureFactory.AttachCircle(
              radius: radius,
              density: density,
              body: bc.Body,
              offset: Global.ToMeters(0, -25) * Global.SlurpScale,
              userData: bc);
            FixtureFactory.AttachCircle(
              radius: radius,
              density: density,
              body: bc.Body,
              offset: Global.ToMeters(0, 25) * Global.SlurpScale,
              userData: bc);

            bc.Body.FixedRotation = true;
          };

          go.RootComponent = bc;

          var sa = new SpriteAnimationComponent(go)
          {
            AnimationTypes = new List<SpriteAnimationType>
            {
              SpriteAnimationType.Slurp_Idle,
            }
          };
          sa.AttachTo(bc);

          var hdc = new HealthDisplayComponent(go)
          {
            InitialDisplayOrigin = HealthDisplayComponent.DisplayOrigin.Bottom,
            HealthIcon = SpriteAnimationFactory.CreateAnimationInstance(SpriteAnimationType.Cross),
          };
          hdc.AttachTo(sa);

          var mc = new MovementComponent(go)
          {
            ManualInputProcessing = true,
          };

          var sqc = new SquashComponent(go)
          {
          };

          var hc = new HealthComponent(go)
          {
            MaxHealth = 3,
          };

          var ec = new EnemyComponent(go)
          {
            EnemyType = type,
          };

          var chc = new ChaserComponent(go)
          {
            Target = Global.Game.Owliver,
            TargetRange = 2.0f,

            Speed = 0.01f,

            DebugDrawingEnabled = true,
          };
          chc.AttachTo(bc);
        }
        break;

        case GameObjectType.Tankton:
        {
          var tankton = new TanktonComponent(go)
          {
            HitDuration = 0.25f,
          };

          var bc = new BodyComponent(go)
          {
            InitMode = BodyComponentInitMode.Manual,
          };
          bc.BeforeInitialize += () =>
          {
            SpatialData s = go.GetWorldSpatialData();
            Vector2 dim = Global.ToMeters(350, 400) * Global.TanktonScale;
            bc.Body = BodyFactory.CreateRoundedRectangle(
              world: Global.Game.World,
              position: s.Position,
              rotation: s.Rotation.Radians,
              bodyType: BodyType.Dynamic,
              userData: bc,
              width: dim.X,
              height: dim.Y,
              xRadius: 0.5f,
              yRadius: 0.5f,
              density: 2 * Global.OwliverDensity,
              segments: 0);
            /*
            bc.Body = BodyFactory.CreateRectangle(
              world: Global.Game.World,
              position: s.Position,
              rotation: s.Rotation.Radians,
              width: Global.ToMeters(350 * Global.TanktonScale.X),
              height: Global.ToMeters(400 * Global.TanktonScale.Y),
              density: 2 * Global.OwliverDensity,
              bodyType: BodyType.Dynamic,
              userData: bc
            );
            */

            bc.Body.FixedRotation = true;
          };
          go.RootComponent = bc;

          var mc = new MovementComponent(go)
          {
            ManualInputProcessing = true,
          };

          var sa = new SpriteAnimationComponent(go)
          {
            AnimationTypes = new List<SpriteAnimationType>
            {
              SpriteAnimationType.Tankton_Idle_Left,
              SpriteAnimationType.Tankton_Idle_Right,
            },
          };
          sa.Spatial.Position.Y += Global.ToMeters(100);
          sa.AttachTo(bc);

          var hc = new HealthComponent(go)
          {
            MaxHealth = 20,
          };

          var hdc = new HealthDisplayComponent(go)
          {
            HealthIcon = SpriteAnimationFactory.CreateAnimationInstance(SpriteAnimationType.Cross),
            InitialDisplayOrigin = HealthDisplayComponent.DisplayOrigin.Bottom,
            NumIconsPerRow = 5,
          };
          hdc.AttachTo(sa);
        }
        break;

        case GameObjectType.BackgroundScreen:
        {
          go.Layer = GameLayer.Background;
          go.IsStationary = true;

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
            RenderDepth = 1.0f,
          };
          sc.AttachTo(bc);
        }
        break;

        case GameObjectType.Tree_Fir:
        case GameObjectType.Tree_FirAlt:
        case GameObjectType.Tree_Conifer:
        case GameObjectType.Tree_ConiferAlt:
        case GameObjectType.Tree_Oak:
        case GameObjectType.Tree_Orange:
        case GameObjectType.Bush:
        {
          go.IsStationary = true;

          List<SpriteAnimationType> animTypes = new List<SpriteAnimationType>();
          switch(type)
          {
            case GameObjectType.Tree_Fir: animTypes.Add(SpriteAnimationType.Fir_Idle); break;
            case GameObjectType.Tree_FirAlt: animTypes.Add(SpriteAnimationType.FirAlt_Idle); go.Layer = GameLayer.CloseToTheScreen; break;
            case GameObjectType.Tree_Conifer: animTypes.Add(SpriteAnimationType.Conifer_Idle); break;
            case GameObjectType.Tree_ConiferAlt: animTypes.Add(SpriteAnimationType.ConiferAlt_Idle); go.Layer = GameLayer.CloseToTheScreen; break;
            case GameObjectType.Tree_Oak: animTypes.Add(SpriteAnimationType.Oak_Idle); break;
            case GameObjectType.Tree_Orange: animTypes.Add(SpriteAnimationType.Orange_Idle); break;
            case GameObjectType.Bush: animTypes.Add(SpriteAnimationType.Bush_Idle); break;

            default: throw new InvalidProgramException();
          }

          var sa = new SpriteAnimationComponent(go)
          {
            AnimationTypes = animTypes,
          };
          sa.BeforePostInitialize += () =>
          {
            sa.RenderDepth = Global.Game.CalcDepth(sa.GetWorldSpatialData(), go.Layer);
          };
          sa.AttachTo(go);
        }
        break;

        case GameObjectType.Bonbon_Gold:
        case GameObjectType.Bonbon_Red:
        {
          SpriteAnimationType animType;
          switch(type)
          {
            case GameObjectType.Bonbon_Gold: animType = SpriteAnimationType.Bonbon_Gold; break;
            case GameObjectType.Bonbon_Red: animType = SpriteAnimationType.Bonbon_Red; break;
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

          var chc = new ChaserComponent(go)
          {
            Target = Global.Game.Owliver,
            TargetRange = 1.0f,
            Speed = 0.05f,

            DebugDrawingEnabled = true,
          };
          chc.AttachTo(bc);
        }
        break;

        case GameObjectType.Key_Gold:
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

          var chc = new ChaserComponent(go)
          {
            Target = Global.Game.Owliver,
            TargetRange = 1.0f,
            Speed = 0.05f,

            DebugDrawingEnabled = true,
          };
          chc.AttachTo(bc);
        }
        break;

        case GameObjectType.Random_FirTree:
        {
          GameObjectType choice = _random.Choose(GameObjectType.Tree_Fir, GameObjectType.Tree_Conifer);
          go = CreateKnown(choice);
        }
        break;

        case GameObjectType.Random_FirTreeAlt:
        {
          GameObjectType choice = _random.Choose(GameObjectType.Tree_FirAlt, GameObjectType.Tree_ConiferAlt);
          go = CreateKnown(choice);
        }
        break;

        case GameObjectType.Random_OakTree:
        {
          GameObjectType choice = _random.Choose(GameObjectType.Tree_Oak, GameObjectType.Tree_Orange);
          go = CreateKnown(choice);
        }
        break;

        default:
        throw new ArgumentException("Unknown game object type.");
      }

      return go;
    }
  }
}
