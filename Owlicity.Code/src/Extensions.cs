using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Primitives2D;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using VelcroPhysics.Collision.Shapes;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Shared;

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

    public static void GetDirectionAndLength(this Vector2 self, out Vector2 direction, out float length)
    {
      length = self.Length();
      if(length == 0.0f)
      {
        direction = Vector2.Zero;
      }
      else
      {
        direction = self / length;
      }
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
      return (rand.NextFloat() * 2.0f) - 1;
    }

    public static float NextFloatBetween(this Random rand, float lower, float upper)
    {
      float t = rand.NextFloat();
      float result = MathHelper.LerpPrecise(lower, upper, t);
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

    public static T Choose<T>(this Random rand, params T[] choices)
    {
      Debug.Assert(choices.Length > 0);
      var idx = rand.Next(choices.Length);
      return choices[idx];
    }
  }

  public static class InputStateExtensions
  {
    //
    // Keyboard
    //

    // Whether the given key is now down but was previously up.
    public static bool WasKeyPressed(this KeyboardState self, Keys key, ref KeyboardState prevState)
    {
      bool result = self.IsKeyDown(key) && prevState.IsKeyUp(key);
      return result;
    }

    // Whether the given key is now up but was previously down.
    public static bool WasKeyReleased(this KeyboardState self, Keys key, ref KeyboardState prevState)
    {
      bool result = self.IsKeyUp(key) && prevState.IsKeyDown(key);
      return result;
    }


    //
    // GamePad
    //

    // Whether the given button is now down but was previously up.
    public static bool WasButtonPressed(this GamePadState self, Buttons button, ref GamePadState prevState)
    {
      bool result = self.IsButtonDown(button) && prevState.IsButtonUp(button);
      return result;
    }

    // Whether the given button is now up but was previously down.
    public static bool WasButtonReleased(this GamePadState self, Buttons button, ref GamePadState prevState)
    {
      bool result = self.IsButtonUp(button) && prevState.IsButtonDown(button);
      return result;
    }
  }

  public static class BodyExtensions
  {
    // Computes the combined AABB from this body by iterating all fixture shapes and calculating their AABB.
    public static AABB ComputeAABB(this Body self)
    {
      AABB result = new AABB
      {
        LowerBound = new Vector2(float.MaxValue),
        UpperBound = new Vector2(float.MinValue),
      };

      foreach(Fixture fixture in self.FixtureList)
      {
        Shape shape = fixture.Shape;
        for(int childIndex = 0; childIndex < shape.ChildCount; childIndex++)
        {
          shape.ComputeAABB(out AABB shapeAABB, ref self._xf, childIndex);
          result.Combine(ref shapeAABB);
        }
      }

      return result;
    }
  }
}
