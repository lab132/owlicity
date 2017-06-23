using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelcroPhysics.Collision.ContactSystem;
using VelcroPhysics.Dynamics;

namespace Owlicity
{
  public class PickupComponent : ComponentBase
  {
    public BodyComponent BodyComponent { get; set; }

    public PickupComponent(GameObject owner) : base(owner)
    {
    }

    public override void Initialize()
    {
      base.Initialize();

      if(BodyComponent == null)
      {
        BodyComponent = Owner.GetComponent<BodyComponent>();
      }

      BodyComponent.Body.OnCollision += OnCollision;
    }

    private void OnCollision(Fixture myFixture, Fixture theirFixture, Contact contact)
    {
      Global.Game.RemoveGameObject(Owner);
    }
  }
}
