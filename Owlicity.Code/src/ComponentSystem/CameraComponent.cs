using Microsoft.Xna.Framework;
using System.Diagnostics;
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
      get => 1 / _invZoom;
      set => _invZoom = 1 / value;
    }

    //
    // Runtime data
    //
    public BodyComponent CameraBodyComponent;
    public Body CameraBody => CameraBodyComponent?.Body;
    public Camera Camera = new Camera();

    public CameraComponent(GameObject owner)
      : base(owner)
    {
    }

    public override void Initialize()
    {
      base.Initialize();

      Vector2 bounds = Bounds; // Global.ToMeters(Bounds);
      Camera.ProjectionMatrix = Matrix.CreateOrthographicOffCenter(0, bounds.X, bounds.Y, 0, -1, 1);

      CameraBodyComponent = Owner.GetComponent<BodyComponent>();
    }

    public override void PrePhysicsUpdate(float deltaSeconds)
    {
      base.PrePhysicsUpdate(deltaSeconds);

      // TODO(manu): Follow "target" here.
    }

    public override void Update(float deltaSeconds)
    {
      base.Update(deltaSeconds);

      // Update view matrix
      Matrix mat = Matrix.Identity;

      // Translation
      Vector2 position = this.GetWorldSpatialData().Position;
      Vector2 center = position - 0.5f * Global.ToMeters(Bounds);
      mat.Translation = new Vector3(center, 0.0f);

      // Scale
      Vector2 scale2D = new Vector2(_invZoom, _invZoom) * Global.RenderScale;
      mat.Scale = new Vector3(scale2D, 1.0f);

      Matrix.Invert(ref mat, out Camera.ViewMatrix);
    }
  }
}
