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

  public class LoadedLevelLayout
  {
    public ContentIdentity identity;
    public BitmapContent map;

    public static IDictionary<Color, GameObjectType> ColorToType
    {
      get
      {
        return new Dictionary<Color, GameObjectType>
        {
          { new Color(255,   0,   0), GameObjectType.Random_FirTree },
          { new Color(  0,   0, 255), GameObjectType.Random_OakTree },
          { new Color(255,   0, 255), GameObjectType.Random_FirTreeAlt },
          { new Color(255, 255,   0), GameObjectType.Bush },
        };
      }
    }
  }
}
