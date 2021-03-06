﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Factories;

namespace Owlicity
{
  public class Tankton : GameObject
  {
    public BodyComponent BodyComponent;
    public SpriteAnimationComponent Animation;
    public HealthComponent Health;
    public HealthDisplayComponent HealthDisplay;

    public TimeSpan HitDuration = TimeSpan.FromSeconds(0.25f);


    public Tankton()
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
          SpriteAnimationType.Tankton_Idle_Left,
          SpriteAnimationType.Tankton_Idle_Right,
        },
      };
      Animation.Spatial.Position.Y += Conversion.ToMeters(100);
      Animation.AttachTo(BodyComponent);

      Health = new HealthComponent(this)
      {
        MaxHealth = 20,
      };
      Health.OnHit += OnHit;
      Health.OnDeath += OnDeath;

      HealthDisplay = new HealthDisplayComponent(this)
      {
        Health = Health,
        HealthIcon = SpriteAnimationFactory.CreateAnimationInstance(SpriteAnimationType.Cross),
        InitialDisplayOrigin = HealthDisplayComponent.DisplayOrigin.Bottom,
        NumIconsPerRow = 5,
      };
      HealthDisplay.AttachTo(Animation);

      GameObjectFactory.CreateOnHitSquasher(this, Health, Animation).SetDefaultCurves(
        duration: HitDuration,
        extremeScale: new Vector2(0.9f, 1.1f));

      GameObjectFactory.CreateOnHitBlinkingSequence(this, Health, Animation).SetDefaultCurves(HitDuration);
    }

    private void OnHit(int damage)
    {
      Health.MakeInvincible(HitDuration);
    }

    private void OnDeath(int damage)
    {
      Global.Game.RemoveGameObject(this);

      DeathConfetti confetti = new DeathConfetti();
      confetti.Spatial.CopyFrom(this.Spatial);
      confetti.AutoDestruct.DestructionDelay = TimeSpan.FromSeconds(5.0f);
      confetti.ParticleEmitter.Emitter.MaxNumParticles = 4096;
      // TODO(manu): Somehow, this doesn't work...
      confetti.ParticleEmitter.Emitter.MaxParticleSpeed = 200.0f;

      Global.Game.AddGameObject(confetti);
    }

    public override void Initialize()
    {
      SpatialData s = this.GetWorldSpatialData();
      Vector2 dim = Conversion.ToMeters(350, 400) * Global.TanktonScale;
      Body body = BodyFactory.CreateRoundedRectangle(
        world: Global.Game.World,
        position: s.Position,
        rotation: s.Rotation.Radians,
        bodyType: BodyType.Dynamic,
        width: dim.X,
        height: dim.Y,
        xRadius: 0.5f,
        yRadius: 0.5f,
        density: 2 * Global.OwliverDensity,
        segments: 0);
      body.FixedRotation = true;
      body.LinearDamping = 20.0f;
      body.CollisionCategories = CollisionCategory.Enemy;

      BodyComponent.Body = body;

      base.Initialize();
    }

    public override void Update(float deltaSeconds)
    {
      base.Update(deltaSeconds);

      Vector2 deltaToOwliver = Global.Game.Owliver.GetWorldSpatialData().Position - this.GetWorldSpatialData().Position;
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
