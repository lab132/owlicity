using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelcroPhysics.Dynamics;

namespace Owlicity
{
  public class AutoBrakeComponent : ComponentBase
  {
    public Body MyBody;
    public float BrakeAmountPerFrame = 0.15f; // Loss of linear velocity per frame in percent.

    public AutoBrakeComponent(GameObject owner)
      : base(owner)
    {
    }

    public override void Initialize()
    {
      base.Initialize();

      if(MyBody == null)
      {
        BodyComponent bc = Owner.GetComponent<BodyComponent>();
        if(bc != null)
        {
          MyBody = bc.Body;
        }
      }

      Debug.Assert(MyBody != null, nameof(AutoBrakeComponent) + " requires a body to work (right now).");
    }

    public override void Update(float deltaSeconds)
    {
      base.Update(deltaSeconds);

      if(MyBody != null && BrakeAmountPerFrame > 0.0f)
      {
        float preserved = 1 - BrakeAmountPerFrame;
        MyBody.LinearVelocity *= preserved;
      }
    }
  }
}
