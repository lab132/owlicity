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

      float width = Global.ToPixels(Spatial.LocalAABB.Width);
      float height = Global.ToPixels(Spatial.LocalAABB.Height);
      Matrix projection = Matrix.CreateOrthographicOffCenter(
        left: 0,
        right: width,
        bottom: height,
        top: 0,
        zNearPlane: -1,
        zFarPlane: 1);

      // See: https://gamedev.stackexchange.com/a/18511
      Matrix halfPixelOffset = Matrix.CreateTranslation(-0.5f, -0.5f, 0);
      Camera.ProjectionMatrix = halfPixelOffset * projection;

      // We need to change the basis ("world") to compensate
      // for inconsistent coordinate systems with XNA and DirectX.
      // See: https://gamedev.stackexchange.com/a/69757
      Camera.WorldMatrix = Matrix.CreateWorld(
        position: Vector3.Zero,
        forward: new Vector3(0, 0, -1),
        up: new Vector3(0, 1, 0));

      Camera.Effect = new BasicEffect(Global.Game.GraphicsDevice)
      {
        View = Camera.ViewMatrix,
        Projection = Camera.ProjectionMatrix,
        World = Camera.WorldMatrix,

        VertexColorEnabled = true,
        TextureEnabled = true,
      };

      CameraBodyComponent = Owner.GetComponent<BodyComponent>();
    }

    public override void Update(float deltaSeconds)
    {
      base.Update(deltaSeconds);

      Spatial.Position = Vector2.Zero;

      if(VisibilityBounds != null)
      {
        SpatialData worldSpatial = this.GetWorldSpatialData();
        AABB aabb = worldSpatial.WorldAABB;
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
        AABB worldAABB = worldSpatial.WorldAABB;
        Vector2 upperLeftCorner = worldAABB.LowerBound;
        mat.Translation = new Vector3(upperLeftCorner, 0.0f);

        // Note(manu): No rotation.

        // Scale
        Vector2 scale2D = new Vector2(_invZoom, _invZoom) * Global.RenderScale;
        mat.Scale = new Vector3(scale2D, 1.0f);

        Matrix.Invert(ref mat, out Camera.ViewMatrix);

        // Update the effect as well.
        ((IEffectMatrices)Camera.Effect).View = Camera.ViewMatrix;
      }
    }
  }
}
