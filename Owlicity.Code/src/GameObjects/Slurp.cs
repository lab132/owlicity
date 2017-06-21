using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelcroPhysics.Collision.ContactSystem;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Factories;

namespace Owlicity
{
  public class Slurp : GameObject
  {
    public float HitDuration = 0.25f;
    public int Damage = 1;
    public float ForceOnImpact = 0.05f;

    public BodyComponent BodyComponent;
    public SpriteAnimationComponent Animation;
    public HomingComponent Homing;
    public HealthComponent Health;
    public HealthDisplayComponent HealthDisplay;

    public Body MyBody => BodyComponent.Body;


    public Slurp()
    {
      BodyComponent = new BodyComponent(this)
      {
        InitMode = BodyComponentInitMode.Manual,
      };
      RootComponent = BodyComponent;

      Animation = new SpriteAnimationComponent(this)
      {
        AnimationTypes = new List<SpriteAnimationType>
        {
          SpriteAnimationType.Slurp_Idle_Left,
          SpriteAnimationType.Slurp_Idle_Right,
        },
      };
      Animation.AttachTo(BodyComponent);

      Health = new HealthComponent(this)
      {
        MaxHealth = 3,
      };
      Health.OnHit += OnHit;
      Health.OnDeath += OnDeath;
      HealthDisplay = new HealthDisplayComponent(this)
      {
        InitialDisplayOrigin = HealthDisplayComponent.DisplayOrigin.Bottom,
        HealthIcon = SpriteAnimationFactory.CreateAnimationInstance(SpriteAnimationType.Cross),
      };
      HealthDisplay.AttachTo(Animation);

      GameObjectFactory.CreateOnHitSquasher(this, Health).SetDefaultCurves(HitDuration);

      GameObjectFactory.CreateOnHitBlinkingSequence(this, Health).SetDefaultCurves(HitDuration);

      Homing = new HomingComponent(this)
      {
        TargetRange = 3.0f,
        Speed = 0.5f,

        DebugDrawingEnabled = true,
      };
      Homing.AttachTo(BodyComponent);
    }

    public void OnHit(int damage)
    {
      Health.MakeInvincible(HitDuration);
    }

    public void OnDeath(int damage)
    {
      GameObject confetti = GameObjectFactory.CreateKnown(KnownGameObject.DeathConfetti);
      confetti.Spatial.CopyFrom(this.Spatial);
      confetti.GetComponent<AutoDestructComponent>().DestructionDelay = TimeSpan.FromSeconds(1.0f);
      Global.Game.AddGameObject(confetti);

      Global.Game.RemoveGameObject(this);
    }

    public override void Initialize()
    {
      Homing.Target = Global.Game.Owliver;
      
      {
        SpatialData s = this.GetWorldSpatialData();
        Body body = new Body(
          world: Global.Game.World,
          position: s.Position,
          rotation: s.Rotation.Radians,
          bodyType: BodyType.Dynamic,
          userdata: BodyComponent);

        float radius = Conversion.ToMeters(80 * Global.SlurpScale.X);
        float density = Global.OwliverDensity;
        FixtureFactory.AttachCircle(
          radius: radius,
          density: density,
          body: body,
          offset: Conversion.ToMeters(0, -25) * Global.SlurpScale,
          userData: BodyComponent);
        FixtureFactory.AttachCircle(
          radius: radius,
          density: density,
          body: body,
          offset: Conversion.ToMeters(0, 25) * Global.SlurpScale,
          userData: BodyComponent);

        body.FixedRotation = true;
        body.LinearDamping = 5.0f;

        body.OnCollision += OnCollision;

        BodyComponent.Body = body;
      }

      base.Initialize();

      // TODO(manu): This could be its own component (it's a recurring thing).
      // Disable chasing when we are invincible.
      ISpatial previousTarget = null;
      Health.OnInvincibilityGained += () =>
      {
        Debug.Assert(previousTarget == null);
        previousTarget = Homing.Target;
        Homing.Target = null;
      };

      Health.OnInvincibilityLost += () =>
      {
        Homing.Target = previousTarget;
        previousTarget = null;
      };
    }

    private void OnCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
    {
      Debug.Assert(((BodyComponent)fixtureA.UserData).Owner == this);

      Body hitBody = fixtureB.Body;
      Global.HandleDefaultHit(hitBody, MyBody.Position, Damage, ForceOnImpact);
    }

    public override void Update(float deltaSeconds)
    {
      base.Update(deltaSeconds);

      // TODO(manu): This could be its own component (it's a recurring thing).
      if(Homing.IsHoming)
      {
        // Change the facing direction to the target.
        Vector2 myPosition = this.GetWorldSpatialData().Position;
        Vector2 targetPosition = Homing.Target.GetWorldSpatialData().Position;
        if(targetPosition.X < myPosition.X)
        {
          Animation.ChangeActiveAnimation(SpriteAnimationType.Slurp_Idle_Left);
        }
        else if(targetPosition.X > myPosition.X)
        {
          Animation.ChangeActiveAnimation(SpriteAnimationType.Slurp_Idle_Right);
        }
      }
    }
  }
}
