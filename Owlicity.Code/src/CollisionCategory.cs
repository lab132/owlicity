using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelcroPhysics.Collision.Filtering;

namespace Owlicity
{
  public static class CollisionCategory
  {
    public const Category World = Category.Cat1;

    public const Category Friendly = Category.Cat10;
    public const Category FriendlyWeapon = Category.Cat11;
    public const Category AnyFriendly = Friendly | FriendlyWeapon;

    public const Category Enemy = Category.Cat20;
    public const Category EnemyWeapon = Category.Cat21;
    public const Category AnyEnemy = Enemy | EnemyWeapon;
  }
}
