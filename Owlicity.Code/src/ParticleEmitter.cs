using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Owlicity
{
  public class ParticleEmitter
  {
    private  int _maxNumParticles = 150;
    public float MinTTL { get; set; } = 0.5f;
    public float MaxTTL { get; set; } = 2.0f;
    public float MaxParticleSpeed { get; set; } = 20.0f;
    public float MaxParticleSpread { get; set; } = 20.0f;
    public Vector2 Gravity { get; set; } = new Vector2(0.0f, -1.5f);
    private Random _random = new Random();
    private Particle[] _particles;
    private Texture2D[] _textures;
    private Color[] _colors;
    private Stack<int> _freeParticleSlots;

    public ParticleEmitter(int maxNumParticles, Texture2D texture, params Color[] colors)
      : this(maxNumParticles, new List<Texture2D> { texture }, new List<Color>(colors))
    {
      _maxNumParticles = maxNumParticles;
      _textures = new[] { texture };
      _colors = colors;
    }

    public ParticleEmitter(int maxNumParticles, List<Texture2D> textures, List<Color> colors)
    {
      _maxNumParticles = maxNumParticles;
      _textures = textures.ToArray();
      _colors = colors.ToArray();
      Initialize();
    }

    private void Initialize()
    {
      _particles = new Particle[_maxNumParticles];
      _freeParticleSlots = new Stack<int>(Enumerable.Range(0, _maxNumParticles));
    }

    public void EmitParticles(Vector2 position)
    {
      while (_freeParticleSlots.Count > 0)
      {
        int idx = _freeParticleSlots.Pop();
        var particle = new Particle
        {
          Velocity = MaxParticleSpeed * _random.NextBilateralVector2(),
          Position = position + MaxParticleSpread * _random.NextBilateralVector2(),
          Color = _random.Choose(_colors),
          Texture = _random.Choose(_textures),
          TTL = _random.NextFloatBetween(MinTTL, MaxTTL),
        };
        _particles[idx] = particle;
      }
    }

    public void Update(float deltaSeconds)
    {
      for (int i = 0; i < _maxNumParticles; i++)
      {
        if (_particles[i].TTL > 0)
        {
          _particles[i].TTL -= deltaSeconds;

          if (_particles[i].TTL <= 0)
          {
            _freeParticleSlots.Push(i);
          }
          else
          {
            _particles[i].Velocity += Gravity * deltaSeconds;
            _particles[i].Position += _particles[i].Velocity * deltaSeconds;
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
  }
}
