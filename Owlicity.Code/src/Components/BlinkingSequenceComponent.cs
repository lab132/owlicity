using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Owlicity
{
  public class BlinkingSequenceComponent : ComponentBase
  {
    public Curve ColorTrackR;
    public Curve ColorTrackG;
    public Curve ColorTrackB;
    public Curve ColorTrackA;

    //
    // Runtime data.
    //
    public float MaxCursor;
    public float CurrentCursor;
    public bool IsBlinking => MaxCursor > 0.0f;

    public SpriteAnimationComponent Animation;


    public BlinkingSequenceComponent(GameObject owner)
      : base(owner)
    {
    }

    public void SetDefaultCurves(TimeSpan duration, Color? on = null, Color? off = null, int numSamples = 3)
    {
      Debug.Assert(numSamples > 1);

      ColorTrackR = new Curve();
      ColorTrackG = new Curve();
      ColorTrackB = new Curve();
      ColorTrackA = new Curve();

      float secondsBetweenSamples = (float)duration.TotalSeconds / numSamples;
      Color color0 = on ?? Color.White;
      Color color1 = off ?? Color.Red;
      for(int sampleIndex = 0; sampleIndex < numSamples; sampleIndex++)
      {
        float position = sampleIndex * secondsBetweenSamples;
        float lerp = sampleIndex / numSamples;
        Vector4 value = Color.Lerp(color0, color1, lerp).ToVector4();
        ColorTrackR.Keys.Add(new CurveKey(position, value.X));
        ColorTrackG.Keys.Add(new CurveKey(position, value.Y));
        ColorTrackB.Keys.Add(new CurveKey(position, value.Z));
        ColorTrackA.Keys.Add(new CurveKey(position, value.W));

        Color swap = color0;
        color0 = color1;
        color1 = swap;
      }
    }

    public void StartSequence()
    {
      CurrentCursor = 0.0f;
      MaxCursor = ColorTrackR.Keys.Last().Position;
    }

    public override void Update(float deltaSeconds)
    {
      base.Update(deltaSeconds);

      if(Animation != null && IsBlinking)
      {
        CurrentCursor += deltaSeconds;

        if(CurrentCursor >= MaxCursor)
        {
          CurrentCursor = 0.0f;
          MaxCursor = 0.0f;
          Animation.Tint = null;
        }
        else
        {
          Color tint = new Color(
            ColorTrackR.Evaluate(CurrentCursor),
            ColorTrackG.Evaluate(CurrentCursor),
            ColorTrackB.Evaluate(CurrentCursor),
            ColorTrackA.Evaluate(CurrentCursor));
          Animation.Tint = tint;
        }
      }
    }
  }
}
