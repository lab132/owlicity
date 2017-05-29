using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Owlicity
{
  class Camera
  {
    private Vector2 _position;
    public Vector2 Bounds { get; set; }
    public ITransformable LookAt { get; set; }
    public float Zoom { get; set; } = 1.0f;

    public Matrix ViewMatrix
    {
      get
      {
        var mat = Matrix.CreateTranslation(new Vector3(_position - 0.5f * Bounds, 0.0f));
        mat.Scale = new Vector3(1/Zoom, 1/Zoom, 1.0f);
        return Matrix.Invert(mat);
      }
    }

    public void Update(GameTime dt)
    {
      float deltaSeconds = (float)dt.ElapsedGameTime.TotalSeconds;
      Vector2 focus = LookAt.GetWorldTransform().Position;
      Vector2 delta = focus - _position;

      Vector2 velocity = delta * 5;

      _position += velocity * deltaSeconds;
    }
  }
}
