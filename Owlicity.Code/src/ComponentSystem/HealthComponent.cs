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
    public int MaxHealth = 1;
    public int InitialHealth = -1; // Will be MaxHealth if < 0.

    //
    // Runtime data.
    //
    private int _health;
    public int CurrentHealth
    {
      get => _health;
      set
      {
        int oldValue = CurrentHealth;
        _health = value;
        if(_health > 0)
        {
          OnDamageTaken?.Invoke(oldValue, value);
        }
        else
        {
          OnDeath?.Invoke(oldValue, value);
        }
      }
    }

    public float CurrentHealthPercent => (float)_health / MaxHealth;

    // Only invoked when not dead.
    public Action<int, int> OnDamageTaken;
    public Action<int, int> OnDeath;

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
