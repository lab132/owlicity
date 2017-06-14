using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Owlicity
{
  public class TanktonComponent : ComponentBase
  {
    //
    // Initialization data.
    //
    public float HitDuration = 0.5f;

    //
    // Runtime data.
    //
    public HealthComponent Health;
    public SpriteAnimationComponent Animation;


    public TanktonComponent(GameObject owner) : base(owner)
    {
    }

    public override void Initialize()
    {
      base.Initialize();

      if(Health == null)
      {
        Health = Owner.GetComponent<HealthComponent>();
        Debug.Assert(Health != null);
      }

      if(Animation == null)
      {
        Animation = Owner.GetComponent<SpriteAnimationComponent>();
        Debug.Assert(Animation != null);
      }
    }

    public override void PostInitialize()
    {
      base.PostInitialize();

      Health.OnHit += (damage) =>
      {
        Health.MakeInvincible(HitDuration);
      };
    }

    public override void Update(float deltaSeconds)
    {
      base.Update(deltaSeconds);

      Vector2 deltaToOwliver = Global.Game.Owliver.GetWorldSpatialData().Position - Owner.GetWorldSpatialData().Position;
      if(deltaToOwliver.X < 0)
      {
        Animation.ChangeActiveAnimation(SpriteAnimationType.Tankton_Idle_Left);
      }
      else if(deltaToOwliver.X > 0)
      {
        Animation.ChangeActiveAnimation(SpriteAnimationType.Tankton_Idle_Right);
      }
    }
  }
}
