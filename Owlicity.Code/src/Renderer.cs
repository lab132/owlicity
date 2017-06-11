using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Owlicity
{
  public class Renderer
  {
    public SpriteBatch Batch;
    public float BaseDepth;
    public float BaseScale;

    public void Initialize(GraphicsDevice device)
    {
      Batch = new SpriteBatch(device);
    }

    public void Begin(SpriteSortMode sortMode = SpriteSortMode.Deferred,
                      BlendState blendState = null,
                      SamplerState samplerState = null,
                      DepthStencilState depthStencilState = null,
                      RasterizerState rasterizerState = null,
                      Effect effect = null,
                      Matrix? transformMatrix = null)
    {
      Batch.Begin(sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, transformMatrix);
    }

    public void End()
    {
      Batch.End();
    }

    public void DrawSprite(
      Vector2 position, Angle rotation, Vector2 scale, float depth,
      Texture2D texture, Vector2 hotspot, Rectangle? sourceRectangle,
      Color tint, SpriteEffects spriteEffects)
    {
      Batch.Draw(
        position: position,
        scale: BaseScale * scale,
        rotation: rotation.Radians,
        layerDepth: BaseDepth + depth,

        texture: texture,
        origin: hotspot,
        sourceRectangle: sourceRectangle,

        color: tint,
        effects: spriteEffects);
    }
  }
}
