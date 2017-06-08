using Microsoft.Xna.Framework;
using VelcroPhysics.Collision.Filtering;

namespace Owlicity
{
  public static partial class Global
  {
    public static OwlGame Game { get; set; }

    public static readonly Vector2 OwliverScale = new Vector2(0.5f);
    public static readonly Vector2 BonbonScale = new Vector2(0.6f);
    public static readonly Vector2 SlurpScale = new Vector2(0.5f);

    public const Category LevelCollisionCategory = Category.Cat1;
    public const Category OwliverCollisionCategory = Category.Cat2;
    public const Category OwliverWeaponCollisionCategory = Category.Cat3;
    public const Category EnemyCollisionCategory = Category.Cat4;
  }
}
