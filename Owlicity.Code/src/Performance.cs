using System.Diagnostics;

namespace Owlicity
{
  public enum PerformanceSlots
  {
    InputUpdate,
    WorldStep,

    Particles,

    COUNT
  }

  public class Performance
  {
    public int NumSlots { get; private set; }
    public int NumSamplesPerFrame { get; private set; }
    public Stopwatch[,] Samples;

    public int CurrentSampleIndex;

    public void BeginSample(PerformanceSlots slot)
    {
      Stopwatch sample = Samples[(int)slot, CurrentSampleIndex];
      Debug.Assert(!sample.IsRunning);
      sample.Restart();
    }

    public void EndSample(PerformanceSlots slot)
    {
      Stopwatch sample = Samples[(int)slot, CurrentSampleIndex];
      Debug.Assert(sample.IsRunning);
      sample.Stop();
    }

    public void Initialize(int numSlots, int numFramesToCapture)
    {
      NumSlots = numSlots;
      NumSamplesPerFrame = numFramesToCapture;
      Samples = new Stopwatch[NumSlots, NumSamplesPerFrame];
      for(int slotIndex = 0; slotIndex < NumSlots; slotIndex++)
      {
        for(int sampleIndex = 0; sampleIndex < NumSamplesPerFrame; sampleIndex++)
        {
          Samples[slotIndex, sampleIndex] = new Stopwatch();
        }
      }

      CurrentSampleIndex = 0;
    }

    public void AdvanceFrame()
    {
      CurrentSampleIndex++;
      if(CurrentSampleIndex >= NumSamplesPerFrame)
      {
        CurrentSampleIndex = 0;
      }
    }
  }
}
