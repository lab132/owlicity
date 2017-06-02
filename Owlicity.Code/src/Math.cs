﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

    public override string ToString()
    {
      float degrees = new Angle { Radians = Transform.q.GetAngle() }.Degrees;
      return $"{Transform.p}|{degrees}°";
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
      Vector2 position = new Vector2();
      float angle = 0.0f;

      ISpatial parent = self;
      while(parent != null)
      {
        SpatialData spatial = parent.Spatial;
        position += spatial.Transform.p;
        angle += spatial.Transform.q.GetAngle();

        Debug.Assert(spatial.Parent != parent);
        parent = spatial.Parent;
      }

      SpatialData result = new SpatialData
      {
        Transform = new Transform
        {
          p = position,
          q = new Rot(angle)
        },
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
        offset.Transform.p = positionOffset;
        offset.Transform.q = new Rot(rotationOffset.Radians);

        offset.AttachTo(newParent);
        self.AttachTo(offset);
      }
    }
  }
}
