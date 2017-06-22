using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Owlicity
{
  public class DeathConfetti : GameObject
  {
    public AutoDestructComponent AutoDestruct;
    public ParticleEmitterComponent ParticleEmitter;

    public DeathConfetti()
    {
      AutoDestruct = new AutoDestructComponent(this)
      {
        DestructionDelay = TimeSpan.FromSeconds(1.0f),
      };

      ParticleEmitter = new ParticleEmitterComponent(this)
      {
        Emitter = new ParticleEmitter
        {
          MaxNumParticles = 64,
          Colors = Global.AllConfettiColors,
        },

        AdditionalTextures = new[]
        {
          "confetti/confetti_01",
          "confetti/confetti_02",
          "confetti/confetti_03",
          "confetti/confetti_04",
          "confetti/confetti_05",
          "confetti/confetti_06",
          "confetti/confetti_07",
        },
      };
    }

    public override void Initialize()
    {
      base.Initialize();

      ParticleEmitter.Emitter.MaxTTL = 0.8f * (float)AutoDestruct.DestructionDelay.TotalSeconds;
      ParticleEmitter.Emitter.MaxParticleSpread = 0.05f;
      ParticleEmitter.Emitter.MaxParticleSpeed = 5f;
      ParticleEmitter.Emit(this.GetWorldSpatialData().Position);
    }
  }
}
