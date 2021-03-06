﻿using Microsoft.Xna.Framework;
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
    public TimeSpan HitDuration = TimeSpan.FromSeconds(0.25f);
    public int Damage = 1;
    public float ForceOnImpact = 0.1f;

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

      Health = GameObjectFactory.CreateDefaultHealth(this,
        maxHealth: 3,
        hitDuration: HitDuration,
        deathParticleTimeToLive: TimeSpan.FromSeconds(1));

      HealthDisplay = new HealthDisplayComponent(this)
      {
        Health = Health,
        InitialDisplayOrigin = HealthDisplayComponent.DisplayOrigin.Bottom,
        HealthIcon = SpriteAnimationFactory.CreateAnimationInstance(SpriteAnimationType.Cross),
      };
      HealthDisplay.AttachTo(Animation);

      GameObjectFactory.CreateOnHitSquasher(this, Health, Animation).SetDefaultCurves(HitDuration);

      GameObjectFactory.CreateOnHitBlinkingSequence(this, Health, Animation).SetDefaultCurves(HitDuration);

      Homing = GameObjectFactory.CreateDefaultHomingCircle(this, BodyComponent,
        sensorRadius: 3.0f,
        homingType: HomingType.ConstantSpeed,
        homingSpeed: 0.5f);
    }

    public override void Initialize()
    {
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
          offset: Conversion.ToMeters(0, -25) * Global.SlurpScale);
        FixtureFactory.AttachCircle(
          radius: radius,
          density: density,
          body: body,
          offset: Conversion.ToMeters(0, 25) * Global.SlurpScale);

        body.FixedRotation = true;
        body.LinearDamping = 5.0f;

        body.OnCollision += OnCollision;
        body.CollisionCategories = CollisionCategory.Enemy;

        BodyComponent.Body = body;
      }

      base.Initialize();

      // Disable chasing when we are invincible.
      Health.OnInvincibilityGained += () =>
      {
        Homing.TargetSensor.Body.Enabled = false;
      };

      Health.OnInvincibilityLost += () =>
      {
        Homing.TargetSensor.Body.Enabled = true;
      };
    }

    private void OnCollision(Fixture ourFixture, Fixture theirFixture, Contact contact)
    {
      Debug.Assert(ourFixture.GetGameObject() == this);

      Body hitBody = theirFixture.Body;
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
        Vector2 targetPosition = Homing.TargetSensor.CurrentMainTarget.Position;
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
