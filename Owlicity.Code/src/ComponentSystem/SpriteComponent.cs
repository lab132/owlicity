using Microsoft.Xna.Framework.Graphics;

namespace Owlicity
{
  public class SpriteComponent : DrawComponent
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

    public override void Draw(SpriteBatch batch)
    {
      base.Draw(batch);

      SpatialData worldSpatial = Hotspot.GetWorldSpatialData();
      Sprite.Draw(batch, worldSpatial, RenderDepth);
    }
  }
}
