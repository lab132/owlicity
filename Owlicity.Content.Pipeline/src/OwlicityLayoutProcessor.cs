#define DETECT_CLOSE_PIXELS

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using VelcroPhysics.Shared;

namespace Owlicity.Content.Pipeline
{
  /// <summary>
  /// This class will be instantiated by the XNA Framework Content Pipeline
  /// to apply custom processing to content data, converting an object of
  /// type TInput to TOutput. The input and output types may be the same if
  /// the processor wishes to alter data without changing its type.
  ///
  /// This should be part of a Content Pipeline Extension Library project.
  /// </summary>
  [ContentProcessor(DisplayName = "Layout Processor - Owlicity")]
  public class OwlicityLayoutProcessor : ContentProcessor<LoadedLevelLayout, List<ScreenLayoutInfo>>
  {
    public override List<ScreenLayoutInfo> Process(LoadedLevelLayout input, ContentProcessorContext context)
    {
      byte[] bytes = input.map.GetPixelData();
      Debug.Assert(bytes.Length % 4 == 0);

      List<ScreenLayoutInfo> result = new List<ScreenLayoutInfo>();
      uint[] data = new uint[bytes.Length / 4];
      int x = 0;
      int y = 0;
      for (int pixelIndex = 0; pixelIndex < data.Length; pixelIndex++)
      {
        x++;
        if(pixelIndex % input.map.Width == 0)
        {
          y++;
          x = 0;
        }

        int byteIndex = pixelIndex * 4;
        uint pixelValue = BitConverter.ToUInt32(bytes, byteIndex);
        Color pixel = new Color(pixelValue);

        if(pixel.A > 0)
        {
          GameObjectType objectType;
          if(LoadedLevelLayout.ColorToType.TryGetValue(pixel, out objectType))
          {
            result.Add(new ScreenLayoutInfo
            {
              ObjectType = objectType,
              Offset = new Vector2(x, y),
            });
          }
          else
          {
            context.Logger.LogWarning(null, input.identity, $"Unknown layout color: {pixel}");
          }
        }
      }

#if DETECT_CLOSE_PIXELS
      for(int infoIndex = 0; infoIndex < result.Count; infoIndex++)
      {
        for(int otherIndex = infoIndex + 1; otherIndex < result.Count; otherIndex++)
        {
          Vector2 a = result[infoIndex].Offset;
          Vector2 b = result[otherIndex].Offset;

          float distance = Vector2.Distance(a, b);
          if(distance <= 2)
          {
            context.Logger.LogWarning(null, input.identity, $"Pixels are too close together: {a} <-> {b} | distance: {distance}");
          }
        }
      }
#endif

      return result;
    }
  }
}