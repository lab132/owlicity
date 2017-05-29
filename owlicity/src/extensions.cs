using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
