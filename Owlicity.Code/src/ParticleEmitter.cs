using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using VelcroPhysics.Utilities;

namespace Owlicity
{
  // Note(manu): @Jules
  // I've exposed (i.e. made public) most members and here's my reasioning.
  // In order to properly configure a particle emitter from a particle emitter component,
  // there are a number of options:
  //   1) The ParticleEmitter constructor takes all configuration parameters and initializes itself from that.
  //      This requires all knowledge of the particle emitter data _before_ it is contructed,
  //      which implies that the particle emitter component effectively duplicates the emitter interface.
  //      This also means that users of the emitter _component_ need to wait until the component itself is fully initialized to configure the underlying emitter (e.g. MaxTTL).
  //   2) The ParticleEmitter exposes its parameters, which are set by the outside world, and the Initialize procedure then intializes the particle emitter form the parameters.
  //      This allows the emitter to be instantiated and configured immediately without duplicating the interface.
  //   3) Introduce a struct (e.g. ParticleEmitterInfo) that describes all parameters (e.g. MaxTTL) which accumulates the data in the emitter component and passes that data to the emitter at construction / intialization time.
  //
  // I chose 2) because it is the simplest approach and doesn't require me to duplicate code and data.
  public class ParticleEmitter
  {
    public int MaxNumParticles = 150;
    public float MinTTL = 0.5f;
    public float MaxTTL = 2.0f;
    public float MaxParticleSpeed = 20.0f;
    public float MaxParticleSpread = 20.0f;
    public float MaxAngularVelocity = 10.0f;
    public Vector2 Gravity = new Vector2(0.0f, 1.5f);
    public Texture2D[] Textures;
    public Color[] Colors;

    public Random Random = new Random();
    public Particle[] Particles;

    private Stack<int> _freeParticleSlots;


    private void Initialize()
    {
      Debug.Assert(Textures != null && Textures.Length > 0);
      Particles = new Particle[MaxNumParticles];
      _freeParticleSlots = new Stack<int>(Enumerable.Range(0, MaxNumParticles));
    }

    public void EmitParticles(Vector2 position, int numParticles = -1)
    {
      if (numParticles == -1)
      {
        numParticles = MaxNumParticles;
      }

      while (_freeParticleSlots.Count > 0 && numParticles > 0)
      {
        int idx = _freeParticleSlots.Pop();
        Particle particle = new Particle
        {
          Velocity = MaxParticleSpeed * Random.NextBilateralVector2().GetClampedTo(1.0f),
          Position = position + MaxParticleSpread * Random.NextBilateralVector2().GetClampedTo(1.0f),
          Color = Random.Choose(Colors),
          Texture = Random.Choose(Textures),
          TTL = Random.NextFloatBetween(MinTTL, MaxTTL),
          AngularVelocity = Random.NextFloatBetween(-MaxAngularVelocity, MaxAngularVelocity)
        };
        Particles[idx] = particle;
        numParticles--;
      }
    }

    public void Update(float deltaSeconds)
    {
      for (int i = 0; i < MaxNumParticles; i++)
      {
        ref Particle p = ref Particles[i];
        if (p.TTL > 0)
        {
          p.TTL -= deltaSeconds;

          if (p.TTL <= 0)
          {
            _freeParticleSlots.Push(i);
          }
          else
          {
            p.Velocity += Gravity * deltaSeconds;
            p.Position += p.Velocity * deltaSeconds;
            p.Rotation += p.AngularVelocity * deltaSeconds;
          }
        }
      }
    }

    public void Draw(Renderer renderer)
    {
      Global.Game.Perf.BeginSample(PerformanceSlots.Particles);

      for(int i = 0; i < MaxNumParticles; i++)
      {
        ref Particle p = ref Particles[i];
        if (p.TTL > 0)
        {
          Vector2 hotspot = Vector2.Zero;
#if false
          // Note(manu): Enable the following to rotate particles around their center.
          hotspot = 0.5f * p.Texture.Bounds.Size.ToVector2();
#endif
          // TODO(manu): Proper depth for particles.
          float depth = 0.0f;

          renderer.DrawSprite(
            position: p.Position,
            rotation: new Angle { Radians = p.Rotation },
            scale: Vector2.One,
            depth: depth,
            sourceRectangle: null,
            texture: p.Texture,
            hotspot: hotspot,
            tint: p.Color,
            spriteEffects: SpriteEffects.None);
        }
      }

      Global.Game.Perf.EndSample(PerformanceSlots.Particles);
    }
  }
}
