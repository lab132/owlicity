using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelcroPhysics.Collision.Filtering;

namespace Owlicity
{
  public static class Global
  {
    public static OwlGame Game { get; set; }
    public static GameObject Owliver { get; set; }

    public static readonly Vector2 OwliverScale = new Vector2(0.5f);
    public static readonly Vector2 BonbonScale = new Vector2(0.6f);
    public static readonly Vector2 SlurpScale = new Vector2(0.5f);

    public const Category LevelCollisionCategory = Category.Cat1;
    public const Category OwliverCollisionCategory = Category.Cat2;
    public const Category EnemyCollisionCategory = Category.Cat3;
  }
}
