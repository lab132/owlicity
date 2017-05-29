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
    public float CamAcceleration { get; set; } = 1.0f;
    public ITransformable LookAt { get; set; }
    public float Zoom { get; set; } = 1.0f;

    public Matrix ViewMatrix
    {
      get
      {
        Vector2 direction = (LookAt.GetWorldTransform().Position - _position);

        if (direction.Length() > CamAcceleration)
        {
          direction.Normalize();
          direction *= CamAcceleration;
        }
        _position += direction;
        var mat = Matrix.CreateTranslation(new Vector3(_position, 0.0f));
        mat.Scale = new Vector3(1/Zoom, 1/Zoom, 1.0f);
        return Matrix.Invert(mat);
      }
    }
  }
}
