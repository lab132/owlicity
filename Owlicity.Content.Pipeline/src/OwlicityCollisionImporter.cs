using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework;
using System.Drawing;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace Owlicity.Content.Pipeline
{
  /// <summary>
  /// This class will be instantiated by the XNA Framework Content Pipeline
  /// to import a file from disk into the specified type, TImport.
  ///
  /// This should be part of a Content Pipeline Extension Library project.
  /// </summary>
  [ContentImporter(".png", DisplayName = "Collision Importer - Owlicity", DefaultProcessor = "OwlicityCollisionProcessor")]
  public class OwlicityCollisionImporter : ContentImporter<CollisionTexture>
  {
    public override CollisionTexture Import(string filename, ContentImporterContext context)
    {
      var otherImporter = new TextureImporter();
      TextureContent content = otherImporter.Import(filename, context);
      BitmapContent bitmap = content.Faces[0][0];
      byte[] bytes = bitmap.GetPixelData();

      return new CollisionTexture
      {
        TextureWidth = bitmap.Width,
        Bytes = bytes,
      };
    }
  }
}
