using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Primitives2D;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace Owlicity
{
  public static class VectorExtensions
  {
    public static Vector2 GetXY(this Vector3 v)
    {
      return new Vector2(v.X, v.Y);
    }

    public static Vector2 GetNormalized(this Vector2 v)
    {
      Vector2 result = v;
      result.Normalize();
      return result;
    }

    public static Vector2 GetNormalizedSafe(this Vector2 v)
    {
      Vector2 result = v;
      if(v.LengthSquared() >= float.Epsilon)
      {
        result.Normalize();
      }

      return result;
    }

    // In case maxLength = 1: "normalize if length is greater than one."
    public static Vector2 GetClampedTo(this Vector2 v, float maxLength)
    {
      Vector2 result = v;
      if(result.LengthSquared() > maxLength * maxLength)
      {
        result = result.GetNormalized() * maxLength;
      }

      return result;
    }
  }

  public static class SpriteBatchExtensions
  {
    public static void FillRectangle(this SpriteBatch self, RectF rect, Color color)
    {
      self.FillRectangle(rect.ToRectangle(), color);
    }

    public static void FillRectangle(this SpriteBatch self, RectF rect, Color color, Angle angle)
    {
      self.FillRectangle(rect.ToRectangle(), color, angle.Radians);
    }
  }

  public static class RandomExtensions
  {
    public static float NextFloat(this Random rand)
    {
      return (float)rand.NextDouble();
    }

    public static float NextBilateralFloat(this Random rand)
    {
      return rand.NextFloat() * 2 - 1;
    }

    public static float NextFloatBetween(this Random rand, float lower, float upper)
    {
      float t = rand.NextFloat();
      float result = ((1 - t) * lower) + (t * upper);
      return result;
    }

    public static Vector2 NextVector2(this Random rand)
    {
      return new Vector2(rand.NextFloat(), rand.NextFloat());
    }

    public static Vector2 NextBilateralVector2(this Random rand)
    {
      return new Vector2(rand.NextBilateralFloat(), rand.NextBilateralFloat());
    }

    public static Vector3 NextVector3(this Random rand)
    {
      return new Vector3(rand.NextFloat(), rand.NextFloat(), rand.NextFloat());
    }

    public static Vector3 NextBilateralVector3(this Random rand)
    {
      return new Vector3(rand.NextBilateralFloat(), rand.NextBilateralFloat(), rand.NextBilateralFloat());
    }

    public static T Choose<T>(this Random rand, List<T> list)
    {
      Debug.Assert(list.Count > 0);
      var idx = rand.Next(list.Count);
      return list[idx];
    }
  }
}
