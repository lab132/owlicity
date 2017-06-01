using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Owlicity
{
  public class Screen
  {
    private Texture2D _texture;
    public string AssetName { get; set; }
    public Vector2 AbsoulutePosition { get; set; }

    public void Initialize()
    {
    }

    public void LoadContent(ContentManager contentManager)
    {
      _texture = contentManager.Load<Texture2D>(AssetName);
    }

    public void UnloadContent()
    {
      _texture = null;
    }

    public void Update(float deltaSeconds)
    {
    }

    public void Draw(SpriteBatch batch)
    {
      batch.Draw(_texture,
        position: AbsoulutePosition,
        sourceRectangle: null,
        color: Color.White,
        rotation: 0,
        origin: Vector2.Zero,
        scale: 1,
        effects: SpriteEffects.None,
        layerDepth: 1.0f);
    }
  }
}
