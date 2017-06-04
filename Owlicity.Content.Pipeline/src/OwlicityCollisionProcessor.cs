
#define DIAGNOSTICS

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using VelcroPhysics.Shared;
using VelcroPhysics.Tools.TextureTools;

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
  [ContentProcessor(DisplayName = "Collision Processor - Owlicity")]
  public class OwlicityCollisionProcessor : ContentProcessor<TextureContent, List<Vertices>>
  {
    public VerticesDetectionType PolygonDetectionType { get; set; }
    public bool HoleDetection { get; set; }
    public bool MultipartDetection { get; set; }
    public bool PixelOffsetOptimization { get; set; }
    public float UniformScale { get; set; } = 1.0f;
    public int AlphaTolerance { get; set; } = 20;
    public float HullTolerance { get; set; } = 1.5f;

    public override List<Vertices> Process(TextureContent input, ContentProcessorContext context)
    {
      BitmapContent bitmap = input.Faces[0][0];
      byte[] bytes = bitmap.GetPixelData();
      Debug.Assert(bytes.Length % 4 == 0);

      // Note(manu): If this were C/C++, we could simply reinterpret the byte-array as a uint-array...
      uint[] data = new uint[bytes.Length / 4];
      for(int dataIndex = 0; dataIndex < data.Length; dataIndex++)
      {
        int byteIndex = dataIndex * 4;
        data[dataIndex] = BitConverter.ToUInt32(bytes, byteIndex);
      }

      DateTime vertStart = DateTime.Now;
      TextureConverter textureConverter = new TextureConverter(data, bitmap.Width)
      {
        PolygonDetectionType = PolygonDetectionType,
        HoleDetection = HoleDetection,
        MultipartDetection = MultipartDetection,
        PixelOffsetOptimization = PixelOffsetOptimization,
        Transform = Matrix.CreateScale(UniformScale), // TODO(manu): Use z=1 instead?
        AlphaTolerance = (byte)AlphaTolerance,
        HullTolerance = HullTolerance,
      };
      List<Vertices> vertices = textureConverter.DetectVertices();
      TimeSpan vertDuration = DateTime.Now - vertStart;
      Diagnostic(context.Logger, $"Parsing vertices took {vertDuration.TotalSeconds.ToString("0.000")} seconds (VelcroPhysics).");

      return vertices;
    }

    void Diagnostic(ContentBuildLogger logger, string message)
    {
#if DIAGNOSTICS
      logger.LogMessage(message);
#endif
    }
  }
}