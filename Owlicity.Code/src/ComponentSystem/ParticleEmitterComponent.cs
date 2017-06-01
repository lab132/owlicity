using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Owlicity
{
  public class ParticleEmitterComponent : ComponentBase, ISpatial
  {
    //
    // Init data
    //
    public int NumParticlesPerTexture;
    public string[] TextureContentNames;
    public Color[] AvailableColors;

    //
    // Runtime data
    //
    public SpatialData Spatial { get; } = new SpatialData();
    public List<ParticleEmitter> Emitters = new List<ParticleEmitter>();

    public ParticleEmitterComponent(GameObject owner) : base(owner)
    {
    }

    public override void Initialize()
    {
      base.Initialize();

      foreach(string textureName in TextureContentNames)
      {
        Texture2D texture = Global.Game.Content.Load<Texture2D>(textureName);
        ParticleEmitter emitter = new ParticleEmitter(NumParticlesPerTexture, texture, AvailableColors);
        Emitters.Add(emitter);
      }

      this.AttachTo(Owner);
    }

    public override void Update(float deltaSeconds)
    {
      base.Update(deltaSeconds);

      Vector2 spawnPosition = this.GetWorldSpatialData().Transform.p;
      foreach(ParticleEmitter emitter in Emitters)
      {
        emitter.EmitParticles(spawnPosition);
        emitter.Update(deltaSeconds);
      }
    }

    public override void Draw(float deltaSeconds, SpriteBatch batch)
    {
      base.Draw(deltaSeconds, batch);

      foreach(ParticleEmitter emitter in Emitters)
      {
        emitter.Draw(batch);
      }
    }
  }
}
