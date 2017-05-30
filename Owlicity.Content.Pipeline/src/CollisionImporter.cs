using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework;

namespace Owlicity.Content.Pipeline
{
  /// <summary>
  /// This class will be instantiated by the XNA Framework Content Pipeline
  /// to import a file from disk into the specified type, TImport.
  ///
  /// This should be part of a Content Pipeline Extension Library project.
  /// </summary>
  [ContentImporter(".png", DisplayName = "Collision Importer - Owlicity", DefaultProcessor = "Owlicity.Content.Pipeline.CollisionProcessor")]
  public class CollisionImporter : ContentImporter<Vector2>
  {
    public override Vector2 Import(string filename, ContentImporterContext context)
    {
      return new Vector2(1, 2);
    }
  }
}
