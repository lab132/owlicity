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
    public float InitialInvincibilityDuration;
    public float DefaultInvincibilityDuration;

    //
    // Runtime data.
    //
    public int CurrentHealth { get; private set; }

    public float CurrentHealthPercent => (float)CurrentHealth / MaxHealth;

    // Is zero when not invincible.
    public float CurrentInvincibilityDuration { get; private set; }

    // between 0 and CurrentInvincibilityDuration.
    public float CurrentInvincibilityTime { get; private set; }

    public bool IsInvincible => CurrentInvincibilityDuration > 0.0f;

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

    public Action OnInvincibilityGained;
    public Action OnInvincibilityLost;

    public void MakeInvincible(float durationInSeconds)
    {
      Debug.Assert(durationInSeconds > 0, "Invalid invincibility duration.");

      CurrentInvincibilityDuration = durationInSeconds;
      CurrentInvincibilityTime = 0.0f;
      OnInvincibilityGained?.Invoke();
    }

    public void StopInvincibility()
    {
      CurrentInvincibilityDuration = 0.0f;
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

      if(InitialInvincibilityDuration > 0)
      {
        MakeInvincible(InitialInvincibilityDuration);
      }
    }

    public override void PrePhysicsUpdate(float deltaSeconds)
    {
      base.PrePhysicsUpdate(deltaSeconds);

      if(IsInvincible)
      {
        CurrentInvincibilityTime += deltaSeconds;
        if(CurrentInvincibilityTime >= CurrentInvincibilityDuration)
        {
          StopInvincibility();
        }
      }
    }
  }
}
