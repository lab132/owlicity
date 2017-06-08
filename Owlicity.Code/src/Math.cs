using Microsoft.Xna.Framework;
using System.Diagnostics;
using VelcroPhysics.Shared;

namespace Owlicity
{
  public struct Angle
  {
    public float Radians;

    public float Degrees
    {
      get => MathHelper.ToDegrees(Radians);
      set { Radians = MathHelper.ToRadians(value); }
    }

    public static Angle operator +(Angle a, Angle b)
    {
      return new Angle { Radians = a.Radians + b.Radians };
    }

    public static Angle operator -(Angle a, Angle b)
    {
      return new Angle { Radians = a.Radians - b.Radians };
    }
  }

  public interface ISpatial
  {
    SpatialData Spatial { get; }
  }

  public class SpatialData : ISpatial
  {
    public SpatialData Spatial { get => this; }

    public ISpatial Parent;
    public Vector2 Position;
    public Angle Rotation;
    public Transform Transform_ => new Transform { p = Position, q = new Rot(Rotation.Radians), };

    public override string ToString()
    {
      return $"{Position}|{Rotation.Degrees}°";
    }
  }

  public static class ISpatialExtensions
  {
    /// <summary>
    /// Gets the world position and rotation for this spatial object.
    /// RenderDepth is not inherited.
    /// The result will have no parent.
    /// </summary>
    public static SpatialData GetWorldSpatialData(this ISpatial self)
    {
      Vector2 position = Vector2.Zero;
      float radians = 0.0f;

      ISpatial parent = self;
      while(parent != null)
      {
        SpatialData spatial = parent.Spatial;
        position += spatial.Position;
        radians += spatial.Rotation.Radians;

        Debug.Assert(spatial.Parent != parent);
        parent = spatial.Parent;
      }

      SpatialData result = new SpatialData
      {
        Position = position,
        Rotation = new Angle { Radians = radians },
      };

      return result;
    }

    public static void AttachTo(this ISpatial self, ISpatial newParent)
    {
      if(newParent != self)
      {
        self.Spatial.Parent = newParent;
      }
    }

    public static void AttachWithOffsetTo(this ISpatial self, ISpatial newParent,
      Vector2 positionOffset, Angle rotationOffset)
    {
      if(newParent != self)
      {
        SpatialData offset = new SpatialData();
        offset.Position = positionOffset;
        offset.Rotation = rotationOffset;

        offset.AttachTo(newParent);
        self.AttachTo(offset);
      }
    }
  }
}
