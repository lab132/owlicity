using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Owlicity
{
  public class GameObject : ISpatial
  {
    public List<ComponentBase> Components { get; } = new List<ComponentBase>();

    SpatialData _cachedSpatial;
    public SpatialData Spatial
    {
      get
      {
        if(_cachedSpatial == null)
        {
          BodyComponent bc = Components.OfType<BodyComponent>().FirstOrDefault();
          if(bc != null)
          {
            _cachedSpatial = bc.Spatial;
          }
          else
          {
            ISpatial spatialComponent = Components.OfType<ISpatial>().FirstOrDefault();
            if(spatialComponent != null)
            {
              _cachedSpatial = spatialComponent.Spatial;
            }
          }
        }

        return _cachedSpatial;
      }
    }

    public GameObject()
    {
      Global.Game.AddGameObject(this);
    }

    public void AddComponent(ComponentBase newComponent)
    {
      Debug.Assert(!Components.Contains(newComponent));
      Components.Add(newComponent);
    }

    public void Initialize()
    {
      foreach(ComponentBase component in Components.Where(c => !c.IsInitialized))
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
      foreach(ComponentBase component in Components)
      {
        component.PrePhysicsUpdate(deltaSeconds);
      }
    }

    public void Update(float deltaSeconds)
    {
      foreach(ComponentBase component in Components)
      {
        component.Update(deltaSeconds);
      }
    }

    public void Draw(float deltaSeconds, SpriteBatch batch)
    {
      foreach(ComponentBase component in Components)
      {
        component.Draw(deltaSeconds, batch);
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
      GameObject result = new GameObject();
      switch(type)
      {
        case GameObjectType.Owliver:
        {
          // TODO(manu)
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

      return result;
    }
  }
}
