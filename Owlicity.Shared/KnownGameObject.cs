using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Owlicity
{
  public enum KnownGameObject
  {
    Unknown,

    // Misc
    Camera,

    // Characters
    Owliver,

    Shop,

    // Mobs
    Slurp,

    // Bosses
    Tankton,

    // Particles
    DeathConfetti,

    Projectile,

    // Static stuff
    BackgroundScreen,
    Gate,
    Tree_Fir,
    Tree_FirAlt, // is "upside down"
    Tree_Conifer,
    Tree_ConiferAlt, // is "upside down"
    Tree_Oak,
    Tree_Orange,
    Bush,

    // Pickups
    Bonbon_Gold,
    Bonbon_Red,
    Key_Gold,

    ShopItem_FruitBowl,
    ShopItem_FishingRod,
    ShopItem_Stick,

    // Random groups
    Random_FirTree,
    Random_FirTreeAlt,
    Random_OakTree,
  }
}
