using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Owlicity
{
  public class ParticleEmitterComponent : SpatialComponent
  {
    //
    // Init data
    //
    public int NumParticles;
    public string[] TextureContentNames;
    public Color[] AvailableColors;

    //
    // Runtime data
    //
    public ParticleEmitter Emitter;
    public bool IsEmittingEnabled = true;

    public ParticleEmitterComponent(GameObject owner) : base(owner)
    {
    }

    public override void Initialize()
    {
      base.Initialize();

      List<Texture2D> textures = new List<Texture2D>(TextureContentNames.Length);
      foreach(string textureName in TextureContentNames)
      {
        Texture2D texture = Global.Game.Content.Load<Texture2D>(textureName);
        textures.Add(texture);
      }

      Emitter = new ParticleEmitter(NumParticles, textures, AvailableColors.ToList());
    }

    public void Emit(Vector2? emitAt = null, int numParticles = -1)
    {
      Vector2 spawnPosition;
      if (emitAt != null)
      {
        spawnPosition = emitAt.Value;
      } else
      {
        spawnPosition = this.GetWorldSpatialData().Transform.p;
      }
      
      Emitter.EmitParticles(spawnPosition);
    }

    public override void Update(float deltaSeconds)
    {
      base.Update(deltaSeconds);

      if(IsEmittingEnabled)
      {
        Vector2 spawnPosition = this.GetWorldSpatialData().Transform.p;
        Emitter.EmitParticles(spawnPosition);
      }
      Emitter.Update(deltaSeconds);
    }

    public override void Draw(float deltaSeconds, SpriteBatch batch)
    {
      base.Draw(deltaSeconds, batch);

      Emitter.Draw(batch);
    }
  }
}
