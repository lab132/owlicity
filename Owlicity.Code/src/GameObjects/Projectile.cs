using Microsoft.Xna.Framework;
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
  public class Projectile : GameObject
  {
    public BodyComponent BodyComponent;
    public SpriteAnimationComponent Animation;
    public AutoDestructComponent AutoDestruct;

    public int Damage = 1;
    public float ForceOnImpact = 0.1f;
    public float TimeToLive = 0.25f;
    public int NumParticlesToSpawn = 16;

    public Projectile()
    {
      BodyComponent = new BodyComponent(this)
      {
        InitMode = BodyComponentInitMode.Manual,
      };
      this.RootComponent = BodyComponent;

      Animation = new SpriteAnimationComponent(this)
      {
        AnimationTypes = new List<SpriteAnimationType>
        {
          SpriteAnimationType.Bonbon_Red,
        },
      };
      Animation.Spatial.Rotation.Degrees -= 40.0f;
      Animation.AttachTo(BodyComponent);

      AutoDestruct = new AutoDestructComponent(this);
    }

    public override void Initialize()
    {
      SpatialData s = this.GetWorldSpatialData();
      Body body = BodyFactory.CreateCapsule(
        world: Global.Game.World,
        endRadius: Conversion.ToMeters(9),
        height: Conversion.ToMeters(50),
        userData: BodyComponent,
        position: s.Position,
        rotation: s.Rotation.Radians + new Angle { Degrees = 90.0f + new Random().NextBilateralFloat() * 2 }.Radians,
        density: 10 * Global.OwliverDensity,
        bodyType: BodyType.Dynamic);
      body.CollisionCategories = Global.OwliverWeaponCollisionCategory;
      body.CollidesWith = ~(Global.OwliverCollisionCategory | Global.OwliverWeaponCollisionCategory);
      body.IsBullet = true;
      body.OnCollision += OnCollision;
      BodyComponent.Body = body;

      base.Initialize();
    }

    private void OnCollision(Fixture ourFixture, Fixture theirFixture, VelcroPhysics.Collision.ContactSystem.Contact contact)
    {
      Body ourBody = ourFixture.Body;
      Body theirBody = theirFixture.Body;
      Debug.Assert(ourBody == BodyComponent.Body);

      Global.HandleDefaultHit(theirBody, ourBody.Position, Damage, ForceOnImpact);

      DeathConfetti confetti = new DeathConfetti();
      confetti.Spatial.CopyFrom(this.Spatial);
      confetti.AutoDestruct.DestructionDelay = TimeSpan.FromSeconds(TimeToLive);
      confetti.ParticleEmitter.Emitter.MaxNumParticles = 16;
      Vector2 g = -2.5f * BodyComponent.Body.LinearVelocity;
      confetti.ParticleEmitter.Emitter.Gravity = g;
      Global.Game.AddGameObject(confetti);

      Global.Game.RemoveGameObject(this);
    }
  }
}
