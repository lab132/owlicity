using Microsoft.Xna.Framework;

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
}
