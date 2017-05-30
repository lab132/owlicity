using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

  public class Transform : ITransformable
  {
    public Transform LocalTransform { get { return this; } }

    public ITransformable Parent;

    public Vector2 Position;
    public float Depth;
    public Angle Rotation;
    public Vector2 Scale = Vector2.One;

    public RectF Bounds;

    public Vector3 Translation
    {
      get { return new Vector3(Position, Depth); }
      set { Position = value.GetXY(); Depth = value.Z; }
    }
  }

  public interface ITransformable
  {
    Transform LocalTransform { get; }
  }

  public static class Transformable
  {
    public static Transform GetWorldTransform(this ITransformable self)
    {
      Transform result = new Transform();
      Transform transform = self.LocalTransform;
      while(true)
      {
        result.Position += transform.Position;
        result.Depth += transform.Depth;
        result.Rotation += transform.Rotation;
        result.Scale *= transform.Scale;

        if(transform.Parent == null)
          break;
        transform = transform.Parent.LocalTransform;
      }

      return result;
    }
  }
}
