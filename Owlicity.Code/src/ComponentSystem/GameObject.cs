﻿// #define CAMERA_BODY

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
          var cc = new CameraComponent(go)
          {
          };

#if CAMERA_BODY
          var bc = new BodyComponent(go)
          {
            InitMode = BodyComponentInitMode.Manual,
          };
          bc.OnInitialize += () =>
          {
            SpatialData initSpatial = go.GetWorldSpatialData();
            bc.Body = BodyFactory.CreateRectangle(
              world: Global.Game.World,
              width: cc.Bounds.X,
              height: cc.Bounds.Y,
              density: 10.0f, // ??
              position: initSpatial.Transform.p,
              rotation: initSpatial.Transform.q.GetAngle(),
              bodyType: BodyType.Kinematic, // TODO(manu): Make this dynamic.
              userData: bc);
            bc.Body.FixedRotation = true;
            bc.Body.CollisionCategories = Category.None;
            bc.Body.CollidesWith = Global.LevelCollisionCategory;
          };
          go.RootComponent = bc;

          cc.CameraBodyComponent = bc;
          cc.AttachTo(bc);
#else
          cc.AttachTo(go);
#endif

          var mc = new MovementComponent(go)
          {
          };
#if CAMERA_BODY
          mc.ControlledBodyComponent = bc;
#endif
        }
        break;

        case GameObjectType.Owliver:
        {
          var bc = new BodyComponent(go)
          {
            InitMode = BodyComponentInitMode.Manual,
          };
          bc.OnInitialize += () =>
          {
            SpatialData s = go.GetWorldSpatialData();
            bc.Body = new Body(
              world: Global.Game.World,
              position: s.Transform.p,
              rotation: s.Transform.q.GetAngle(),
              bodyType: BodyType.Dynamic,
              userdata: bc);
            bc.Body.FixedRotation = true;
            bc.Body.CollisionCategories = Global.OwliverCollisionCategory;
            bc.Body.CollidesWith = Global.LevelCollisionCategory | Global.EnemyCollisionCategory;

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
        {
            var bc = new BodyComponent(go)
            {
              InitMode = BodyComponentInitMode.Manual,
              ShapeContentName = "slurp_collision",
              BodyType = BodyType.Static
            };

            bc.OnInitialize += delegate ()
            {
              SpatialData s = go.GetWorldSpatialData();
              bc.Body = new Body(
                world: Global.Game.World,
                position: s.Transform.p,
                rotation: s.Transform.q.GetAngle(),
                bodyType: BodyType.Dynamic,
                userdata: bc);
              bc.Body.FixedRotation = true;

              float radius = 80 * Global.SlurpScale.X;
              float density = 10; // ??
              FixtureFactory.AttachCircle(
                radius: radius,
                density: density,
                body: bc.Body,
                offset: new Vector2(0, -25) * Global.SlurpScale,
                userData: bc);
              FixtureFactory.AttachCircle(
                radius: radius,
                density: density,
                body: bc.Body,
                offset: new Vector2(0, 100) * Global.SlurpScale,
                userData: bc);
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

            var mc = new MovementComponent(go)
            {
              ControlledBodyComponent = bc,
            };

            var pc = new ParticleEmitterComponent(go)
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

              AvailableColors = new[]
              {
                  new Color(0x73, 0x4c, 0x87), // purple
                  new Color(0xa3, 0x3b, 0x41), // red
                  new Color(0xda, 0x67, 0x77), // red2
                  new Color(0x41, 0x6d, 0x9c), // blue
                  new Color(0x7a, 0xaa, 0xdd), // blue2
                  new Color(0x5f, 0x72, 0x2d), // green
                  new Color(0x9f, 0xb5, 0x63), // green2
                  new Color(0xda, 0xa7, 0x44), // yellow
                  new Color(0xf4, 0xd3, 0x92), // yellow2
              },

              IsEmittingEnabled =false,

            };

            pc.OnPostInitialize += delegate ()
            {
              pc.Emitter.MaxParticleSpread = 0;
              pc.Emitter.MaxParticleSpeed = 100;
            };
            pc.AttachTo(bc);

            var ec = new EnemyComponent(type, go);
           

        }
        break; 
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
          bc.OnPostInitialize += () =>
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
