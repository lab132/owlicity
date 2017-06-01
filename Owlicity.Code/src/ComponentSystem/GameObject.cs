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

    public void AddComponent(ComponentBase newComponent)
    {
      Debug.Assert(!Components.Contains(newComponent));
      Components.Add(newComponent);
    }

    public void Initialize()
    {
      foreach(ComponentBase component in Components.Where(c => c.IsInitializationEnabled))
      {
        component.Initialize();
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

    // Static elements
    Tree_Fir,
    Tree_Orange,
  }

  public static class GameObjectFactory
  {
    private static ContentManager _content;

    public static void Initialize(ContentManager content)
    {
      _content = content;
    }

    public static GameObject CreateKnown(GameObjectType type)
    {
      GameObject go = new GameObject();
      switch(type)
      {
        case GameObjectType.Owliver:
        {
          Vector2 hotspot = new Vector2(127, 238) * Global.OwliverScale;
          Vector2 tileDim = new Vector2(256, 256) * Global.OwliverScale;
          Vector2 colDim = new Vector2(120, 180) * Global.OwliverScale;

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
              bodyType: BodyType.Kinematic,
              userdata: bc);
            FixtureFactory.AttachEllipse(
              xRadius: 0.5f * colDim.X,
              yRadius: 0.5f * colDim.Y,
              edges: 8, // Note(manu): 8 is already the maximum...
              density: 0,
              body: bc.Body,
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

        case GameObjectType.Tree_Fir:
        throw new NotImplementedException();

        case GameObjectType.Tree_Orange:
        throw new NotImplementedException();

        default:
        throw new ArgumentException("Unknown game object type.");
      }

      return go;
    }
  }
}
