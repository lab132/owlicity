using System;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework;

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
  public class CollisionProcessor : ContentProcessor<Vector2, Vector3>
  {
    public override Vector3 Process(Vector2 input, ContentProcessorContext context)
    {
      return new Vector3(input, 3);
    }
  }
}