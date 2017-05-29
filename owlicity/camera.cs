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
    private Vector3 _position;
    public float CamAcceleration { get; set; } = 1.0f;
    public Transform LookAt { get; set; }
    public Matrix ViewMatrix
    {
      get
      {
        Vector3 direction = (LookAt.Position - _position);

        if (direction.Length() > CamAcceleration)
        {
          direction.Normalize();
          direction *= CamAcceleration;
        }
        _position += direction;
        var mat = Matrix.CreateTranslation(_position);
        return Matrix.Invert(mat);
      }
    }
    public Camera() { _position = Vector3.Zero; }
  }


}
