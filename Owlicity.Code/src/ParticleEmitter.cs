using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Owlicity.src
{
  public class ParticleEmitter
  {
    private  int _maxNumParticles = 150;
    public float MinTTL { get; set; } = 0.5f;
    public float MaxTTL { get; set; } = 1.5f;
    public float MaxParticleSpeed { get; set; } = 20.0f;
    public float MaxParticleSpread { get; set; } = 20.0f;
    public Vector2 Gravity { get; set; } = new Vector2(0.0f, -1.5f);
    private Random _random;
    private Particle[] _particles;
    private List<Texture2D> _textures;
    private List<Color> _colors;
    private Stack<int> _freeParticleSlots;

    public ParticleEmitter(int maxNumParticles, List<Texture2D> textures, List<Color> colors)
    {
      _maxNumParticles = maxNumParticles;
      _random = new Random();
      _particles = new Particle[_maxNumParticles];
      _textures = textures;
      _colors = colors;
      _freeParticleSlots = new Stack<int>(Enumerable.Range(0, _maxNumParticles));

    }

    public void emitParticles(Vector2 position)
    {
      while (_freeParticleSlots.Count > 0)
      {
        int idx = _freeParticleSlots.Pop();
        var particle = new Particle
        {
          Velocity = new Vector2(getRandomSignedFloat(MaxParticleSpeed), getRandomSignedFloat(MaxParticleSpeed)),
          Position = position + new Vector2(getRandomSignedFloat(MaxParticleSpread), getRandomSignedFloat(MaxParticleSpread)),
          Color = getRandomChoice(_colors),
          Texture = getRandomChoice(_textures),
          TTL = MinTTL + getRandomUnSignedFloat(MaxTTL)
        };
        _particles[idx] = particle;
      }
    }

    public void Update(GameTime gameTime)
    {
      var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

      for (int i = 0; i < _maxNumParticles; i++)
      {
        if (_particles[i].TTL > 0)
        {
          _particles[i].TTL -= dt;

          if (_particles[i].TTL <= 0)
          {
            _freeParticleSlots.Push(i);
          }
          else
          {
            _particles[i].Velocity -= Gravity * dt;
            _particles[i].Position += _particles[i].Velocity * dt;
          }
        }
      }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
      for (int i = 0; i < _maxNumParticles; i++)
      {
        if (_particles[i].TTL > 0)
        {
          spriteBatch.Draw(_particles[i].Texture, _particles[i].Position, _particles[i].Color);
        }
      }
    }

    private T getRandomChoice<T>(List<T> list)
    {
      var idx = _random.Next(list.Count);
      return list[idx];
    }
    private float getRandomSignedFloat(float upperBound)
    {
      return (float) (_random.NextDouble()  * 2 - 1 ) * upperBound;
    }

    private float getRandomUnSignedFloat(float upperBound)
    {
      return (float)_random.NextDouble() * upperBound;
    }
  }
}
