using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Primitives2D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelcroPhysics.Shared;

namespace Owlicity
{
  class Sprite
  {
    public Texture2D Texture { get; set; }
    public Point TextureOffset { get; set; }
    public Point TextureDim { get; set; }
    public Color Tint { get; set; } = Color.White;
    public SpriteEffects SpriteEffects { get; set; }

    public void Draw(SpriteBatch spriteBatch, SpatialData spatial)
    {
      Point textureDim = TextureDim;
      if(textureDim == Point.Zero)
        textureDim = Texture.Bounds.Size;

      spriteBatch.Draw(
        texture: Texture,
        position: spatial.Transform.p,
        sourceRectangle: new Rectangle { Location = TextureOffset, Size = textureDim },
        color: Tint,
        rotation: spatial.Transform.q.GetAngle(),
        origin: Vector2.Zero,
        scale: Vector2.One,
        effects: SpriteEffects,
        layerDepth: spatial.Depth);

      spriteBatch.DrawRectangle(spatial.Transform.p, textureDim.ToVector2(), Color.Red);
    }
  }
}
