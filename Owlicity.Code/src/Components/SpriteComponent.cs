using Microsoft.Xna.Framework.Graphics;

namespace Owlicity
{
  public class SpriteComponent : DrawComponent
  {
    public string SpriteContentName;
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
        Spatial.LocalAABB = Conversion.ToMeters(Sprite.CalcAABB());
      }
    }

    public override void Draw(Renderer renderer)
    {
      base.Draw(renderer);

      SpatialData worldSpatial = this.GetWorldSpatialData();
      Sprite.Draw(renderer, worldSpatial, RenderDepth);
    }
  }
}
