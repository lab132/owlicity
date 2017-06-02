using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelcroPhysics.Dynamics;

namespace Owlicity
{
  public class CameraComponent : SpatialComponent
  {
    //
    // Input data
    //
    public Vector2 Bounds;
    private float _invZoom = 1.0f;
    public float Zoom
    {
      get => 1.0f / _invZoom;
      set => _invZoom = value != 0.0f ? 1.0f / value : 0.0f;
    }
    public float MovementSpeed = 400.0f;

    //
    // Runtime data
    //
    public BodyComponent CameraBodyComponent;
    public Body CameraBody { get => CameraBodyComponent?.Body; }
    public Camera Camera = new Camera();

    public CameraComponent(GameObject owner) : base(owner)
    {
    }

    public override void Initialize()
    {
      base.Initialize();

      Camera.ProjectionMatrix = Matrix.CreateOrthographicOffCenter(0, Bounds.X, Bounds.Y, 0, -1, 1);
    }

    public override void PrePhysicsUpdate(float deltaSeconds)
    {
      base.PrePhysicsUpdate(deltaSeconds);

#if false
      // Follow target
      ISpatial parent = Owner.Spatial.Parent;
      if(parent != null)
      {
        SpatialData current = this.GetWorldSpatialData();
        SpatialData target = parent.GetWorldSpatialData();
        Vector2 focus = target.Transform.p;
        Vector2 delta = focus - current.Transform.p;
        velocity += delta * 5;
      }

      CameraBody.LinearVelocity += velocity;
#endif
    }

    public override void Update(float deltaSeconds)
    {
      base.Update(deltaSeconds);

      // Update view matrix
      SpatialData worldSpatial = this.GetWorldSpatialData();
      var mat = Matrix.CreateTranslation(new Vector3(worldSpatial.Transform.p - 0.5f * Bounds, 0.0f));
      mat.Scale = new Vector3(_invZoom, _invZoom, 1.0f);
      Camera.ViewMatrix = Matrix.Invert(mat);
    }
  }
}
