using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelcroPhysics.Collision.ContactSystem;
using VelcroPhysics.Dynamics;

namespace Owlicity
{
  public class EnemyComponent : ComponentBase
  {
    public GameObjectType EnemyType { get; set; }
    public EnemyComponent(GameObjectType enemyType, GameObject owner) : base(owner)
    {
      EnemyType = enemyType;
    }

    public override void Initialize()
    {
      base.Initialize();

      var bc = Owner.Components.OfType<BodyComponent>().Single();
      var pec = Owner.Components.OfType<ParticleEmitterComponent>().Single();

      var owliverBC = Global.Owliver.Components.OfType<BodyComponent>().Single();
      bc.OnPostInitialize += delegate ()
      {
        bc.Body.OnCollision += delegate (Fixture fixtureA, Fixture fixtureB, Contact contact)
        {
          if (fixtureA.UserData == owliverBC || fixtureB.UserData == owliverBC)
          {
            pec.Emit(fixtureB.Body.Position + contact.Manifold.LocalPoint);
          }
        };
      };
      
    }
    public override void Update(float deltaSeconds)
    {
      switch(EnemyType)
      {
        case GameObjectType.Slurp:
        {
            float movementSpeed = 0.1f;
            base.Update(deltaSeconds);
            var mov = Owner.Components.OfType<MovementComponent>().Single();

            var owliverPosition = Global.Owliver.GetWorldSpatialData().Transform.p;
            var enemyPos = Owner.GetWorldSpatialData().Transform.p;

            var movementVec = (owliverPosition - enemyPos).GetNormalized() * movementSpeed;
            mov.InputVector = movementVec;
          }
          break;
        default:
          throw new NotImplementedException();
      }

    }
  }
}
