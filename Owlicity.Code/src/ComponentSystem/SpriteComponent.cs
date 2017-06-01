using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace Owlicity
{
  public class SpriteComponent : SpatialComponent
  {
    //
    // Init data
    //
    public string SpriteContentName;

    //
    // Runtime data
    //
    public Sprite Sprite = new Sprite();

    public SpriteComponent(GameObject owner) : base(owner)
    {
    }

    public override void Initialize()
    {
      base.Initialize();

      if(SpriteContentName != null)
      {
        Sprite.Texture = Global.Game.Content.Load<Texture2D>(SpriteContentName);
      }
    }

    public override void Draw(float deltaSeconds, SpriteBatch batch)
    {
      base.Draw(deltaSeconds, batch);

      if(Sprite != null)
      {
        SpatialData worldSpatial = this.GetWorldSpatialData();
        Sprite.Draw(batch, worldSpatial);
      }
    }
  }
}
