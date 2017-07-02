using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public TimeSpan InitialInvincibilityDuration;
    public TimeSpan DefaultInvincibilityDuration;

    //
    // Runtime data.
    //
    public int CurrentHealth { get; private set; }

    public float CurrentHealthPercent => (float)CurrentHealth / MaxHealth;
    public bool IsAlive => CurrentHealth > 0;
    public bool IsDead => !IsAlive;

    // Is zero when not invincible.
    public TimeSpan CurrentInvincibilityDuration { get; private set; }

    // between 0 and CurrentInvincibilityDuration.
    public TimeSpan CurrentInvincibilityTime { get; private set; }

    public bool IsInvincible => CurrentInvincibilityDuration.Ticks > 0;

    // Only invoked when not dead.
    public Action<int> OnHit;
    public Action<int> OnDeath;

    public void Hit(int damage)
    {
      int oldHP = CurrentHealth;
      int newHP = oldHP - damage;
      CurrentHealth = newHP;
      if(newHP > 0)
      {
        OnHit?.Invoke(damage);
      }
      else
      {
        OnDeath?.Invoke(damage);
      }
    }

    public void Heal(int amount)
    {
      // TODO(manu): Callbacks for this one?

      long hp = (long)CurrentHealth + amount;

      if(hp > MaxHealth)
      {
        hp = MaxHealth;
      }

      if(hp < 0)
      {
        hp = 0;
      }

      CurrentHealth = (int)hp;
    }

    public Action OnInvincibilityGained;
    public Action OnInvincibilityLost;

    public void MakeInvincible(TimeSpan duration)
    {
      Debug.Assert(duration.Ticks > 0, "Invalid invincibility duration.");

      CurrentInvincibilityDuration = duration;
      CurrentInvincibilityTime = TimeSpan.Zero;
      OnInvincibilityGained?.Invoke();
    }

    public void StopInvincibility()
    {
      CurrentInvincibilityDuration = TimeSpan.Zero;
      OnInvincibilityLost?.Invoke();
    }

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

      CurrentHealth = InitialHealth;

      if(InitialInvincibilityDuration.Ticks > 0)
      {
        MakeInvincible(InitialInvincibilityDuration);
      }
    }

    public override void PrePhysicsUpdate(float deltaSeconds)
    {
      base.PrePhysicsUpdate(deltaSeconds);

      if(IsInvincible)
      {
        CurrentInvincibilityTime += TimeSpan.FromSeconds(deltaSeconds);
        if(CurrentInvincibilityTime >= CurrentInvincibilityDuration)
        {
          StopInvincibility();
        }
      }
    }
  }
}
