using Microsoft.Xna.Framework;
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
        component.PrePhysicsUpdate(deltaSeconds);
      }
    }

    public void Update(float deltaSeconds)
    {
      foreach(ComponentBase component in Components.Where(c => c.IsUpdateEnabled))
      {
        component.Update(deltaSeconds);
      }
    }

    public void Draw(Renderer renderer)
    {
      foreach(ComponentBase component in Components.Where(c => c.IsDrawEnabled))
      {
        component.Draw(renderer);
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

    // Static stuff
    BackgroundScreen,
    Tree_Fir,
    Tree_FirAlt, // is "upside down"
    Tree_Conifer,
    Tree_ConiferAlt, // is "upside down"
    Tree_Oak,
    Tree_Orange,
    Bush,

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
          var sac = new SpringArmComponent(go)
          {
          };
          go.RootComponent = sac;

          var cc = new CameraComponent(go)
          {
          };
          cc.AttachTo(sac);

          var mc = new MovementComponent(go)
          {
          };
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
            float density = 1; // ??
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
            InitialAmount = 210,
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
            float density = 0.01f; // ??
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

          var sqc = new SquashComponent(go)
          {
          };

          var mc = new MovementComponent(go)
          {
          };

          var pec = new ParticleEmitterComponent(go)
          {
            NumParticles = 512,

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
            pec.Emitter.MaxParticleSpread = 0.1f;
            pec.Emitter.MaxParticleSpeed = 5f;
          };
          pec.AttachTo(bc);

          var ec = new EnemyComponent(go)
          {
            EnemyType = type,
          };
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
