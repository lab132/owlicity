using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Owlicity
{
  public class AutoDestructComponent : ComponentBase
  {
    //
    // Input data.
    //
    public TimeSpan DestructionDelay;
    public bool AutoStart = true;

    //
    // Runtime data.
    //
    public TimeSpan CurrentTime;
    public bool IsRunning;


    public AutoDestructComponent(GameObject owner)
      : base(owner)
    {
    }

    public void Resume()
    {
      IsRunning = true;
    }

    public void Pause()
    {
      IsRunning = false;
    }

    public void Reset()
    {
      CurrentTime = TimeSpan.Zero;
    }

    public void Stop()
    {
      Reset();
      Pause();
    }

    public void Start()
    {
      Reset();
      Resume();
    }

    public override void Initialize()
    {
      base.Initialize();

      if(AutoStart)
      {
        Start();
      }
    }

    public override void Update(float deltaSeconds)
    {
      base.Update(deltaSeconds);

      if(IsRunning)
      {
        CurrentTime += TimeSpan.FromSeconds(deltaSeconds);
        if(CurrentTime >= DestructionDelay)
        {
          Pause();
          Global.Game.RemoveGameObject(Owner);
        }
      }
    }
  }
}
