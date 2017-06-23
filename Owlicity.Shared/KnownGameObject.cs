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

    // Flora
    // Note(manu): Keep the flora stuff together please.
    Flora_Fir,
    Flora_FirAlt, // is "upside down"
    Flora_Conifer,
    Flora_ConiferAlt, // is "upside down"
    Flora_Oak,
    Flora_Orange,
    Flora_Bush,

    // Pickups
    Bonbon_Gold,
    Bonbon_Red,
    Key_Gold,

    // Shop items
    ShopItem_FruitBowl,
    ShopItem_FishingRod,
    ShopItem_Stick,

    // Random groups
    Random_FirTree,
    Random_FirTreeAlt,
    Random_OakTree,
  }
}
