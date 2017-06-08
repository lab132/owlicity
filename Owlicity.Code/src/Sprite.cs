using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Owlicity
{
  public class Sprite
  {
    public Texture2D Texture;
    public Point TextureOffset;
    public Point TextureDim;
    public Vector2 Hotspot;
    public Vector2 Scale = Vector2.One;
    public Color Tint = Color.White;
    public SpriteEffects SpriteEffects;

    public void Draw(SpriteBatch spriteBatch, SpatialData spatial, float depth)
    {
      Point textureDim = TextureDim;
      if(textureDim == Point.Zero)
        textureDim = Texture.Bounds.Size;

      Rectangle sourceRect = new Rectangle { Location = TextureOffset, Size = textureDim };
      Vector2 scale = Scale * Global.RenderScale;

      spriteBatch.Draw(
        texture: Texture,
        position: spatial.Position,
        sourceRectangle: sourceRect,
        color: Tint,
        rotation: spatial.Rotation.Radians,
        origin: Hotspot,
        scale: scale,
        effects: SpriteEffects,
        layerDepth: depth);

#if false
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
