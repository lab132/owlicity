using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

    public void Initialize()
    {
      ComponentBase[] toInit = Components.Where(c => c.IsInitializationEnabled).ToArray();
      foreach(ComponentBase component in toInit)
      {
        component.Initialize();
      }

      foreach(ComponentBase component in toInit)
      {
        component.PostInitialize();
      }
    }

    public void Deinitialize()
    {
      // TODO(manu): Do we need this?
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

    public void Draw(float deltaSeconds, SpriteBatch batch)
    {
      foreach(ComponentBase component in Components.Where(c => c.IsDrawingEnabled))
      {
        component.Draw(deltaSeconds, batch);
      }
    }

    public void DebugDraw(float deltaSeconds, SpriteBatch batch)
    {
      foreach(ComponentBase component in Components.Where(c => c.IsDebugDrawingEnabled))
      {
        component.DebugDraw(deltaSeconds, batch);
      }
    }
  }

  public enum GameObjectType
  {
    Unknown,

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
        case GameObjectType.Owliver:
        {
          var bc = new BodyComponent(go)
          {
            InitMode = BodyComponentInitMode.Manual,
          };
          bc.OnInitialize += delegate()
          {
            SpatialData s = go.GetWorldSpatialData();
            bc.Body = new Body(
              world: Global.Game.World,
              position: s.Transform.p,
              rotation: s.Transform.q.GetAngle(),
              bodyType: BodyType.Dynamic,
              userdata: bc);
            bc.Body.FixedRotation = true;

            float radius = 50 * Global.OwliverScale.X;
            float density = 10; // ??
            FixtureFactory.AttachCircle(
              radius: radius,
              density: density,
              body: bc.Body,
              offset: new Vector2(0, 10) * Global.OwliverScale,
              userData: bc);
            FixtureFactory.AttachCircle(
              radius: radius,
              density: density,
              body: bc.Body,
              offset: new Vector2(0, 60) * Global.OwliverScale,
              userData: bc);

            go.RootComponent = bc;
          };

          var oc = new OwliverComponent(go)
          {
          };
          oc.ControlledBodyComponent = bc;

          var sa = new SpriteAnimationComponent(go)
          {
            AnimationTypes = new List<SpriteAnimationType>
            {
               SpriteAnimationType.Owliver_Idle_Left,
               SpriteAnimationType.Owliver_Idle_Right,
               SpriteAnimationType.Owliver_Walk_Left,
               SpriteAnimationType.Owliver_Walk_Right,
            },
          };
          sa.Spatial.Transform.p += new Vector2(0, -10);
          sa.AttachTo(bc);
        }
        break;

        case GameObjectType.Slurp:
        throw new NotImplementedException();

        case GameObjectType.BackgroundScreen:
        {
          go.Layer = GameLayer.Background;
          go.IsStationary = true;

          var bc = new BodyComponent(go)
          {
            InitMode = BodyComponentInitMode.FromContent,
            BodyType = BodyType.Static,
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
            case GameObjectType.Tree_Oak: animTypes.Add(SpriteAnimationType.Oak_Idle);  break;
            case GameObjectType.Tree_Orange: animTypes.Add(SpriteAnimationType.Orange_Idle); break;
            case GameObjectType.Bush: animTypes.Add(SpriteAnimationType.Bush_Idle); break;

            default: throw new InvalidProgramException();
          }

          var sa = new SpriteAnimationComponent(go)
          {
            AnimationTypes = animTypes,
          };
          sa.OnPostInitialize += () =>
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
