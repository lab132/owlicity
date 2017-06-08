using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Owlicity
{
  public class SquashComponent : ComponentBase
  {
    //
    // Initialization data. See also SetupDefaultSquashData().
    //
    public Curve SquashCurveX;
    public Curve SquashCurveY;

    //
    // Runtime data.
    //
    public SpriteAnimationComponent SpriteAnimationComponent { get; set; }

    public float MaxCurvePosition;
    public float CurrentCurvePosition;
    public bool IsSquashing => MaxCurvePosition > 0.0f;


    public SquashComponent(GameObject owner)
      : base(owner)
    {
    }

    public void SetupDefaultSquashData(float duration)
    {
      SquashCurveX = new Curve();
      SquashCurveX.Keys.Add(new CurveKey(0.0f * duration, 1.0f));
      SquashCurveX.Keys.Add(new CurveKey(0.5f * duration, 1.5f));
      SquashCurveX.Keys.Add(new CurveKey(1.0f * duration, 1.0f));

      SquashCurveY = new Curve();
      SquashCurveY.Keys.Add(new CurveKey(0.0f * duration, 1.0f));
      SquashCurveY.Keys.Add(new CurveKey(0.5f * duration, 0.75f));
      SquashCurveY.Keys.Add(new CurveKey(1.0f * duration, 1.0f));
    }

    public override void Initialize()
    {
      base.Initialize();

      if(SpriteAnimationComponent == null)
      {
        SpriteAnimationComponent = Owner.GetComponent<SpriteAnimationComponent>();
      }
    }

    public override void Update(float deltaSeconds)
    {
      base.Update(deltaSeconds);

      if(SpriteAnimationComponent != null && IsSquashing)
      {
        CurrentCurvePosition += deltaSeconds;

        if(CurrentCurvePosition >= MaxCurvePosition)
        {
          // Squashing has ended. :(
          CurrentCurvePosition = 0.0f;
          MaxCurvePosition = 0.0f;
          SpriteAnimationComponent.AdditionalScale = null;
        }
        else
        {
          float scaleX = SquashCurveX.Evaluate(CurrentCurvePosition);
          float scaleY = SquashCurveY.Evaluate(CurrentCurvePosition);
          SpriteAnimationComponent.AdditionalScale = new Vector2(scaleX, scaleY);
        }
      }
    }

    public void StartSquashing()
    {
      if(IsSquashing)
      {
        // TODO(manu): What to do? Restart?
      }

      CurrentCurvePosition = 0.0f;
      MaxCurvePosition = SquashCurveX.Keys.Last().Position;
    }
  }
}
