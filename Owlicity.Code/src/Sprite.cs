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

    public void Draw(Renderer renderer, SpatialData spatial, float depth)
    {
      Point textureDim = TextureDim;
      if(textureDim == Point.Zero)
        textureDim = Texture.Bounds.Size;

      Rectangle sourceRect = new Rectangle
      {
        Location = TextureOffset,
        Size = textureDim
      };

      renderer.DrawSprite(
        position: spatial.Position,
        rotation: spatial.Rotation,
        scale: Scale,
        depth: depth,
        texture: Texture,
        hotspot: Hotspot,
        sourceRectangle: sourceRect,
        tint: Tint,
        spriteEffects: SpriteEffects);
    }
  }
}
