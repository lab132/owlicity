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
  public class Sprite
  {
    public Texture2D Texture { get; set; }
    public Point TextureOffset { get; set; }
    public Point TextureDim { get; set; }
    public Vector2 Scale { get; set; } = Vector2.One;
    public Color Tint { get; set; } = Color.White;
    public SpriteEffects SpriteEffects { get; set; }

    public void Draw(SpriteBatch spriteBatch, SpatialData spatial, float depth)
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
        origin: Vector2.Zero, // Note(manu): I have no idea what this parameter does.
        scale: Scale,
        effects: SpriteEffects,
        layerDepth: depth);

#if true
      spriteBatch.DrawString(
        spriteFont: Global.Game.Content.Load<SpriteFont>("Font"),
        text: $"depth: {depth}",
        position: spatial.Transform.p,
        color: Color.White
      );

      spriteBatch.DrawRectangle(spatial.Transform.p, textureDim.ToVector2() * Scale, Color.Red);
#endif
    }
  }
}
