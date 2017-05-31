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
  public class OwlicityLayoutProcessor : ContentProcessor<LevelLayout, Vertices>
  {
    public override Vertices Process(LevelLayout input, ContentProcessorContext context)
    {
      byte[] bytes = input.map.GetPixelData();
      Debug.Assert(bytes.Length % 4 == 0);

      List<Vector2> result = new List<Vector2>();
      uint[] data = new uint[bytes.Length / 4];
      for (int pixelIndex = 0; pixelIndex < data.Length; pixelIndex++)
      {
        int byteIndex = pixelIndex * 4;
        uint pixelValue = BitConverter.ToUInt32(bytes, byteIndex);
        Color pixel = new Color(pixelValue);

        if(pixel.A > 0)
        {
          LevelObjectType objectType;
          if(LevelLayout.ColorToType.TryGetValue(pixel, out objectType))
          {
            context.Logger.LogMessage($"Found {objectType}");
            // TODO: Process data, like x,y coordinates.
          }
          else
          {
            context.Logger.LogWarning(null, input.identity, $"Unknown layout color: {pixel}");
          }
        }
      }
      return null;
    }
  }
}