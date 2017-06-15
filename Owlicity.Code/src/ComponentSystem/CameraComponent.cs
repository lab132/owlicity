using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Shared;

namespace Owlicity
{
  public class CameraComponent : SpatialComponent
  {
    //
    // Input data
    //
    public AABB? VisibilityBounds;

    private float _invZoom = 1.0f;
    public float Zoom
    {
      get => 1 / _invZoom;
      set => _invZoom = 1 / value;
    }

    //
    // Runtime data
    //
    public Camera Camera = new Camera();

    public CameraComponent(GameObject owner)
      : base(owner)
    {
    }

    public void OnGraphicsDeviceReset(GraphicsDevice device)
    {
      int width = device.Viewport.Width;
      int height = device.Viewport.Height;
      Matrix baseProjection = Matrix.CreateOrthographicOffCenter(
        left: 0,
        right: width,
        bottom: height,
        top: 0,
        zNearPlane: -1,
        zFarPlane: 1);

      // See: https://gamedev.stackexchange.com/a/18511
      Matrix halfPixelOffset = Matrix.CreateTranslation(-0.5f, -0.5f, 0);
      Matrix projection = halfPixelOffset * baseProjection;

      Matrix view = Camera.Effect?.View ?? Matrix.Identity;

      Camera.Reset(device, ref projection, ref view);

      Vector2 screenSizeInMeters = Global.ToMeters(Camera.Viewport.Bounds.Size.ToVector2());
      Spatial.LocalAABB.LowerBound = -0.5f * screenSizeInMeters;
      Spatial.LocalAABB.UpperBound = 0.5f * screenSizeInMeters;
    }

    public override void Update(float deltaSeconds)
    {
      base.Update(deltaSeconds);

      Spatial.Position = Vector2.Zero;

      // Constrain the camera to the visibility bounds.
      if(VisibilityBounds != null)
      {
        SpatialData worldSpatial = this.GetWorldSpatialData();
        AABB aabb = worldSpatial.AbsoluteAABB;
        Vector2 delta = Vector2.Zero;

        // Left edge
        if(aabb.LowerBound.X < VisibilityBounds.Value.LowerBound.X)
        {
          delta.X = VisibilityBounds.Value.LowerBound.X - aabb.LowerBound.X;
        }

        // Top edge
        if(aabb.LowerBound.Y < VisibilityBounds.Value.LowerBound.Y)
        {
          delta.Y = VisibilityBounds.Value.LowerBound.Y - aabb.LowerBound.Y;
        }

        // Right edge
        if(aabb.UpperBound.X > VisibilityBounds.Value.UpperBound.X)
        {
          delta.X = VisibilityBounds.Value.UpperBound.X - aabb.UpperBound.X;
        }

        // Bottom edge
        if(aabb.UpperBound.Y > VisibilityBounds.Value.UpperBound.Y)
        {
          delta.Y = VisibilityBounds.Value.UpperBound.Y - aabb.UpperBound.Y;
        }

        if(delta != Vector2.Zero)
        {
          this.SetWorldPosition(worldSpatial.Position + delta);
        }
      }

      // Update view matrix
      {
        Matrix mat = Matrix.Identity;

        // Translation
        SpatialData worldSpatial = this.GetWorldSpatialData();
        AABB worldAABB = worldSpatial.AbsoluteAABB;
        Vector2 upperLeftCorner = worldAABB.LowerBound;
        mat.Translation = new Vector3(upperLeftCorner, 0.0f);

        // Note(manu): No rotation.

        // Scale
        Vector2 scale2D = new Vector2(_invZoom, _invZoom) * Global.RenderScale;
        mat.Scale = new Vector3(scale2D, 1.0f);

        Camera.Effect.View = Matrix.Invert(mat);
      }
    }
  }
}
