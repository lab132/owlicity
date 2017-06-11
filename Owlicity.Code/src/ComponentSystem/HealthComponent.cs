using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Owlicity
{
  public class HealthComponent : ComponentBase
  {
    //
    // Initialization data.
    //
    public float MaxHealth = 1.0f;
    public float InitialHealth = -1.0f; // Will be MaxHealth if < 0.

    //
    // Runtime data.
    //
    private float _health;
    public float CurrentHealth
    {
      get => _health;
      set
      {
        float oldValue = _health;
        _health = value;
        if(_health > 0.0f)
        {
          OnDamageTaken?.Invoke(oldValue, _health);
        }
        else
        {
          OnDeath?.Invoke(oldValue, _health);
        }
      }
    }

    // Only invoked when not dead.
    public Action<float, float> OnDamageTaken;
    public Action<float, float> OnDeath;

    public HealthComponent(GameObject owner)
      : base(owner)
    {
    }

    public override void Initialize()
    {
      base.Initialize();

      if(InitialHealth < 0)
      {
        InitialHealth = MaxHealth;
      }

      _health = InitialHealth;
    }
  }
}
