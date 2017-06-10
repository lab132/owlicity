using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Owlicity
{
  public class Camera
  {
    public Matrix ViewMatrix = Matrix.Identity;
    public Matrix ProjectionMatrix = Matrix.Identity;
    public Matrix WorldMatrix = Matrix.Identity;
    public Effect Effect;
  }
}
