using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Owlicity
{
  public enum FloraType
  {
    Fir,
    Conifer,
    Oak,
    Orange,

    Bush,

    FirAlt,
    ConiferAlt,
  }

  public class Flora : GameObject
  {
    public SpriteAnimationComponent Animation;

    public FloraType TreeType;

    public Flora()
    {
      Animation = new SpriteAnimationComponent(this)
      {
        AnimationTypes = new List<SpriteAnimationType>(),
        DepthReference = null, // Don't determine depth automatically
      };
      Animation.AttachTo(this);
    }

    public override void Initialize()
    {
      switch(TreeType)
      {
        case FloraType.Fir: Animation.AnimationTypes.Add(SpriteAnimationType.Fir_Idle); break;
        case FloraType.FirAlt: Animation.AnimationTypes.Add(SpriteAnimationType.FirAlt_Idle); this.Layer = GameLayer.CloseToTheScreen; break;
        case FloraType.Conifer: Animation.AnimationTypes.Add(SpriteAnimationType.Conifer_Idle); break;
        case FloraType.ConiferAlt: Animation.AnimationTypes.Add(SpriteAnimationType.ConiferAlt_Idle); this.Layer = GameLayer.CloseToTheScreen; break;
        case FloraType.Oak: Animation.AnimationTypes.Add(SpriteAnimationType.Oak_Idle); break;
        case FloraType.Orange: Animation.AnimationTypes.Add(SpriteAnimationType.Orange_Idle); break;
        case FloraType.Bush: Animation.AnimationTypes.Add(SpriteAnimationType.Bush_Idle); break;

        default: throw new ArgumentException(nameof(Animation));
      }

      Animation.RenderDepth = Global.Game.CalcDepth(Animation.GetWorldSpatialData(), this.Layer);

      base.Initialize();
    }
  }
}
