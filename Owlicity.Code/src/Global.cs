using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Owlicity
{
  public static class Global
  {
    public static OwlGame Game { get; set; }
    public static GameObject Owliver { get; set; }

    public static readonly Vector2 OwliverScale = new Vector2(0.5f);
    public static readonly Vector2 BonbonScale = new Vector2(0.6f);
  }
}
