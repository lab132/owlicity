using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Owlicity
{
  abstract class GameObject
  {
    abstract public void Update(GameTime gameTime);
    abstract public void Draw(GameTime gameTime);
    abstract public void LoadContent();
    abstract public void Initialize();
  }
}
