using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Owlicity
{
  public class ParticleEmitterComponent : SpatialComponent
  {
    public string[] AdditionalTextures;
    public ParticleEmitter Emitter;

    public ParticleEmitterComponent(GameObject owner)
      : base(owner)
    {
    }

    public void Emit(int numParticles = -1)
    {
      Vector2 emitAt = this.GetWorldSpatialData().Position;
      Emit(emitAt, numParticles);
    }

    public void Emit(Vector2 emitAt, int numParticles = -1)
    {
      Emitter.EmitParticles(emitAt, numParticles);
    }

    public override void Initialize()
    {
      base.Initialize();

      Debug.Assert(Emitter != null);
      if(AdditionalTextures != null)
      {
        IEnumerable<Texture2D> textures = AdditionalTextures.Select(textureName => {
          return Global.Game.Content.Load<Texture2D>(textureName);
        });

        if(Emitter.Textures == null)
        {
          Emitter.Textures = new List<Texture2D>();
        }

        Emitter.Textures.AddRange(textures);
      }

      Emitter.Initialize();
    }

    public override void Update(float deltaSeconds)
    {
      base.Update(deltaSeconds);
      Emitter.Update(deltaSeconds);
    }

    public override void Draw(Renderer renderer)
    {
      base.Draw(renderer);
      Emitter.Draw(renderer);
    }
  }
}
