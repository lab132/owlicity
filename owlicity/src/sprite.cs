using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Primitives2D;
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
    public Point TextureOffset { get; set; }
    public Point TextureDim { get; set; }
    public Color Tint { get; set; } = Color.White;
    public SpriteEffects SpriteEffects { get; set; }

    public void Draw(SpriteBatch spriteBatch, Transform transform)
    {
      Point textureDim = TextureDim;
      if(textureDim == Point.Zero)
        textureDim = Texture.Bounds.Size;

      spriteBatch.Draw(
        texture: Texture,
        position: transform.Position,
        sourceRectangle: new Rectangle { Location = TextureOffset, Size = textureDim },
        color: Tint,
        rotation: transform.Rotation.Radians,
        origin: Vector2.Zero,
        scale: transform.Scale,
        effects: SpriteEffects,
        layerDepth: transform.Depth);

      spriteBatch.DrawRectangle(transform.Position, textureDim.ToVector2(), Color.Red);
    }
  }
}
