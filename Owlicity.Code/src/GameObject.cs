using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Owlicity
{
  public enum GameObjectType
  {
    Owliver,

    // Mobs
    Slurp,

    // Static elements
    Tree_Fir,
    Tree_Orange,
  }

  public interface IGameObject
  {
    void Initialize();
    void BeginPlay();
    void Update(float deltaSeconds);
    void EndPlay();
  }

  abstract class GameObject
  {
    abstract public void Update(GameTime gameTime);
    abstract public void Draw(GameTime gameTime);
    abstract public void LoadContent();
    abstract public void Initialize();
  }
}
