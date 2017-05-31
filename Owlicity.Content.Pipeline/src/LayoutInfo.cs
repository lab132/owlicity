using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using System;
using System.Collections.Generic;

namespace Owlicity.Content.Pipeline
{
  public class ColorMappingAttribute : Attribute
  {
    public Color Col { get; private set; }

    public ColorMappingAttribute(byte r, byte g, byte b)
    {
      Col = new Color(r, g, b);
    }
  }

  public enum LevelObjectType
  {
    [ColorMapping(0, 255, 0)] Tree_Orange,
    [ColorMapping(200, 200, 0)] Tree_Fir,
  }

  public class LevelLayout
  {
    public ContentIdentity identity;
    public BitmapContent map;

    public static IDictionary<Color, LevelObjectType> ColorToType
    {
      get
      {
        return new Dictionary<Color, LevelObjectType>
        {
          { new Color(  0, 255,   0), LevelObjectType.Tree_Fir },
          { new Color(200, 200,   0), LevelObjectType.Tree_Orange }
        };
      }
    }
  }
}
