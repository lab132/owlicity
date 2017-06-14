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
    public SpatialData Spatial => this;

    public ISpatial Parent;
    public Vector2 Position;
    public Angle Rotation;

    // Note(manu): Local to this SpatialData object, i.e. Position is not being accounted for.
    public AABB LocalAABB;

    public Transform Transform_ => new Transform { p = Position, q = new Rot(Rotation.Radians), };

    public AABB AbsoluteAABB => new AABB
    {
      LowerBound = LocalAABB.LowerBound + Position,
      UpperBound = LocalAABB.UpperBound + Position
    };

    public void CopyTo(SpatialData other)
    {
      other.Parent = Parent;
      other.Position = Position;
      other.Rotation = Rotation;
      other.LocalAABB = LocalAABB;
    }

    public SpatialData GetCopy()
    {
      SpatialData result = new SpatialData();
      CopyTo(result);
      return result;
    }

    public override string ToString()
    {
      return $"{Position}|{Rotation.Degrees}°";
    }
  }

  public static class ISpatialExtensions
  {
    /// <summary>
    /// Gets the world position and rotation for this spatial object.
    /// The result has the same LocalAABB, i.e. it is not inherited.
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
        LocalAABB = self.Spatial.LocalAABB,
      };

      return result;
    }

    public static void SetWorldPositionAndRotation(this ISpatial self, Vector2 worldPosition, Angle worldRotation)
    {
      if(self.Spatial.Parent != null)
      {
        SpatialData parent = self.Spatial.Parent.GetWorldSpatialData();
        self.Spatial.Position = worldPosition - parent.Position;
        self.Spatial.Rotation = worldRotation - parent.Rotation;
      }
      else
      {
        self.Spatial.Position = worldPosition;
        self.Spatial.Rotation = worldRotation;
      }
    }


    public static void SetWorldPosition(this ISpatial self, Vector2 worldPosition)
    {
      if(self.Spatial.Parent != null)
      {
        SpatialData parent = self.Spatial.Parent.GetWorldSpatialData();
        self.Spatial.Position = worldPosition - parent.Position;
      }
      else
      {
        self.Spatial.Position = worldPosition;
      }
    }

    public static void SetWorldRotation(this ISpatial self, Angle worldRotation)
    {
      if(self.Spatial.Parent != null)
      {
        SpatialData parent = self.Spatial.Parent.GetWorldSpatialData();
        self.Spatial.Rotation = worldRotation - parent.Rotation;
      }
      else
      {
        self.Spatial.Rotation = worldRotation;
      }
    }

    public static void AttachTo(this ISpatial self, ISpatial newParent)
    {
      if(newParent != self)
      {
        self.Spatial.Parent = newParent;
      }
    }

    /// <summary>
    /// Returns the SpatialData instance representing the offset.
    /// </summary>
    public static SpatialData AttachWithOffsetTo(this ISpatial self, ISpatial newParent,
      Vector2 positionOffset, Angle rotationOffset)
    {
      SpatialData result = null;

      if(newParent != self)
      {
        result = new SpatialData()
        {
          Position = positionOffset,
          Rotation = rotationOffset
        };

        result.AttachTo(newParent);
        self.AttachTo(result);
      }

      return result;
    }
  }
}
