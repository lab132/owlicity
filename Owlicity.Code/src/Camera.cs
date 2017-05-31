using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Owlicity
{
  class Camera : ISpatial
  {
    public SpatialData Spatial { get; } = new SpatialData();
    public Vector2 Bounds { get; set; }
    public ISpatial LookAt { get; set; }
    public float Zoom { get; set; } = 1.0f;

    public Matrix ViewMatrix
    {
      get
      {
        SpatialData worldSpatial = Spatial.GetWorldSpatialData();
        var mat = Matrix.CreateTranslation(new Vector3(worldSpatial.Transform.p - 0.5f * Bounds, 0.0f));
        mat.Scale = new Vector3(1/Zoom, 1/Zoom, 1.0f);
        return Matrix.Invert(mat);
      }
    }

    public Matrix ProjectionMatrix { get; set; }

    public Camera() { LookAt = this; }

    public void Initialize()
    {
      ProjectionMatrix = Matrix.CreateOrthographicOffCenter(0, Bounds.X, Bounds.Y, 0, -1, 1);
    }

    public void Update(float deltaSeconds)
    {
      SpatialData worldSpatial = Spatial.GetWorldSpatialData();
      Vector2 focus = worldSpatial.Transform.p;
      Vector2 delta = focus - worldSpatial.Transform.p;

      Vector2 velocity = delta * 5;
      Spatial.Transform.p += velocity * deltaSeconds;
    }
  }
}
