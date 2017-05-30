using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
  [ContentProcessor(DisplayName = "CollisionProcessor - Owlicity")]
  public class OwlicityCollisionProcessor : ContentProcessor<CollisionTexture, Vertices>
  {
    public override Vertices Process(CollisionTexture input, ContentProcessorContext context)
    {
      List<Vector2> result = new List<Vector2>();
      Debug.Assert(input.Bytes.Length % 4 == 0);
      uint[] data = new uint[input.Bytes.Length / 4];
      for (int dataIndex = 0; dataIndex < data.Length; dataIndex++)
      {
        int byteIndex = dataIndex * 4;
        uint value = BitConverter.ToUInt32(input.Bytes, byteIndex);
        data[dataIndex] = value;
      }
      Vertices vertices = TextureConverter.DetectVertices(data, input.TextureWidth);
      return vertices;
    }
  }
}