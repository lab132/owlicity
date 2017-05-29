using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Owlicity
{
  class Sprite
  {
    public Texture2D Texture { get; set; }
    public Vector2 TextureOffset { get; set; }
    public Vector2 TextureUV { get; set; } = Vector2.One;
    public Color Tint { get; set; } = Color.White;
    public SpriteEffects SpriteEffects { get; set; }

    public void Draw(SpriteBatch spriteBatch, Transform transform)
    {
      Vector2 textureDim = new Vector2(Texture.Width, Texture.Height);
      Rectangle sourceRect = new Rectangle
      {
        Location = (TextureOffset * textureDim).ToPoint(),
        Size = (TextureUV * textureDim).ToPoint()
      };

      spriteBatch.Draw(
        texture: Texture,
        position: transform.Position.GetXY(),
        sourceRectangle: sourceRect,
        color: Tint,
        rotation: transform.Rotation.Radians,
        origin: TextureOffset,
        scale: transform.Scale,
        effects: SpriteEffects,
        layerDepth: transform.Position.Z);
    }
  }
}
