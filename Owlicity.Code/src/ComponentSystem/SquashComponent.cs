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
    public HealthComponent Health;
    public Curve SquashCurveX;
    public Curve SquashCurveY;

    //
    // Runtime data.
    //
    public SpriteAnimationComponent SpriteAnimationComponent { get; set; }

    public float MaxCursor;
    public float CurrentCursor;
    public bool IsSquashing => MaxCursor > 0.0f;


    public SquashComponent(GameObject owner)
      : base(owner)
    {
    }

    public void SetDefaultCurves(float duration, Vector2? initialScale = null, Vector2? extremeScale = null)
    {
      Vector2 init = initialScale ?? Vector2.One;
      Vector2 extreme = extremeScale ?? new Vector2(1.5f, 0.75f);

      SquashCurveX = new Curve();
      SquashCurveX.Keys.Add(new CurveKey(0.0f * duration, init.X));
      SquashCurveX.Keys.Add(new CurveKey(0.5f * duration, extreme.X));
      SquashCurveX.Keys.Add(new CurveKey(1.0f * duration, init.X));

      SquashCurveY = new Curve();
      SquashCurveY.Keys.Add(new CurveKey(0.0f * duration, init.Y));
      SquashCurveY.Keys.Add(new CurveKey(0.5f * duration, extreme.Y));
      SquashCurveY.Keys.Add(new CurveKey(1.0f * duration, init.Y));
    }

    public void StartSequence()
    {
      if(IsSquashing)
      {
        // TODO(manu): What to do? Restart?
      }

      CurrentCursor = 0.0f;
      MaxCursor = SquashCurveX.Keys.Last().Position;
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
        CurrentCursor += deltaSeconds;

        if(CurrentCursor >= MaxCursor)
        {
          // Squashing has ended. :(
          CurrentCursor = 0.0f;
          MaxCursor = 0.0f;
          SpriteAnimationComponent.AdditionalScale = null;
        }
        else
        {
          float scaleX = SquashCurveX.Evaluate(CurrentCursor);
          float scaleY = SquashCurveY.Evaluate(CurrentCursor);
          SpriteAnimationComponent.AdditionalScale = new Vector2(scaleX, scaleY);
        }
      }
    }
  }
}
