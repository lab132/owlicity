using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Owlicity
{
  public class GameObject
  {
    public List<ComponentBase> Components { get; set; }

    public void Initialize()
    {
      foreach(ComponentBase component in Components.Where(c => c.WantsInitialize))
      {
        component.Initialize();
      }
    }

    public void Update(float deltaSeconds)
    {
      foreach(ComponentBase component in Components.Where(c => c.WantsUpdate))
      {
        component.Update(deltaSeconds);
      }
    }

    public void Draw(float deltaSeconds, SpriteBatch batch)
    {
      foreach(ComponentBase component in Components.Where(c => c.WantsDraw))
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
      GameObject result;
      switch(type)
      {
        case GameObjectType.Owliver:
        {
          result = new GameObject();
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
