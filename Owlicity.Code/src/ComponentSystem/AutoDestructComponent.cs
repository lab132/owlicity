using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Owlicity
{
  public class AutoDestructComponent : ComponentBase
  {
    public float SecondsUntilDestruction;
    public float CurrentTime;

    public AutoDestructComponent(GameObject owner)
      : base(owner)
    {
    }

    public override void Update(float deltaSeconds)
    {
      base.Update(deltaSeconds);

      CurrentTime += deltaSeconds;
      if(CurrentTime >= SecondsUntilDestruction)
      {
        Global.Game.RemoveGameObject(Owner);
      }
    }
  }
}
