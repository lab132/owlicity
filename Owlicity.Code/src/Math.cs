using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelcroPhysics.Shared;

namespace Owlicity
{
  public struct Angle
  {
    private float _value;

    public float Radians
    {
      get { return _value; }
      set { _value = value; }
    }

    public float Degrees
    {
      get { return _value; }
      set { _value = value; }
    }

    public static Angle operator +(Angle A, Angle B)
    {
      return new Angle { _value = A._value + B._value };
    }

    public static Angle operator -(Angle A, Angle B)
    {
      return new Angle { _value = A._value - B._value };
    }
  }

  public struct RectF
  {
    public Vector2 Center;
    public Vector2 HalfExtents;

    public Vector2 Extents
    {
      get { return 2.0f * HalfExtents; }
      set { HalfExtents = 0.5f * value; }
    }

    public Vector2 BottomLeft
    {
      get { return Center + new Vector2(-HalfExtents.X, HalfExtents.Y); }
      set { Center = value + new Vector2(HalfExtents.X, -HalfExtents.Y); }
    }

    public Vector2 Left
    {
      get { return Center + new Vector2(-HalfExtents.X, 0.0f); }
      set { Center = value + new Vector2(HalfExtents.X, 0.0f); }
    }

    public Vector2 TopLeft
    {
      get { return Center + new Vector2(-HalfExtents.X, -HalfExtents.Y); }
      set { Center = value + new Vector2(HalfExtents.X, HalfExtents.Y); }
    }

    public Vector2 Top
    {
      get { return Center + new Vector2(0.0f, -HalfExtents.Y); }
      set { Center = value + new Vector2(0.0f, HalfExtents.Y); }
    }

    public Vector2 TopRight
    {
      get { return Center + new Vector2(HalfExtents.X, -HalfExtents.Y); }
      set { Center = value + new Vector2(-HalfExtents.X, HalfExtents.Y); }
    }

    public Vector2 Right
    {
      get { return Center + new Vector2(HalfExtents.X, 0.0f); }
      set { Center = value + new Vector2(-HalfExtents.X, 0.0f); }
    }

    public Vector2 BottomRight
    {
      get { return Center + new Vector2(HalfExtents.X, HalfExtents.Y); }
      set { Center = value + new Vector2(-HalfExtents.X, -HalfExtents.Y); }
    }

    public Vector2 Bottom
    {
      get { return Center + new Vector2(0.0f, HalfExtents.Y); }
      set { Center = value + new Vector2(0.0f, -HalfExtents.Y); }
    }

    public Rectangle ToRectangle()
    {
      return new Rectangle
      {
        Location = Left.ToPoint(),
        Size = Extents.ToPoint(),
      };
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
    public Transform Transform;
    public float Depth;
  }

  public static class ISpatialExtensions
  {
    public static SpatialData GetWorldSpatialData(this ISpatial self)
    {
      SpatialData result = new SpatialData();
      float angle = 0.0f;
      ISpatial parent = self;
      while(parent != null)
      {
        SpatialData spatial = parent.Spatial;
        result.Transform.p += spatial.Transform.p;
        angle += spatial.Transform.q.GetAngle();
        result.Depth += spatial.Depth;

        parent = spatial.Parent;
      }
      result.Transform.q = new Rot(angle);

      return result;
    }
  }
}
