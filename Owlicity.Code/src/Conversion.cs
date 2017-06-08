using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Owlicity
{
  public static partial class Global
  {
    // 100 pixels map to 1 meter.
    public static readonly float PixelsPerMeter = 100.0f / 1.0f;
    public static readonly float MetersPerPixel = 1.0f / PixelsPerMeter;
    public static readonly float RenderScale = MetersPerPixel;

    //
    // Pixels to meters
    //

    public static float ToMeters(float a) => a * MetersPerPixel;

    public static Vector2 ToMeters(float a, float b) => new Vector2(a, b) * MetersPerPixel;
    public static Vector2 ToMeters(Vector2 a) => a * MetersPerPixel;

    public static Vector3 ToMeters(float a, float b, float c) => new Vector3(a, b, c) * MetersPerPixel;
    public static Vector3 ToMeters(Vector3 a) => a * MetersPerPixel;


    //
    // Meters to pixels
    //

    public static float ToPixels(float a) => a * PixelsPerMeter;

    public static Vector2 ToPixels(float a, float b) => new Vector2(a, b) * PixelsPerMeter;
    public static Vector2 ToPixels(Vector2 a) => a * PixelsPerMeter;

    public static Vector3 ToPixels(float a, float b, float c) => new Vector3(a, b, c) * PixelsPerMeter;
    public static Vector3 ToPixels(Vector3 a) => a * PixelsPerMeter;
  }
}
