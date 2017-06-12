﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Owlicity
{
  public enum CurrencyType
  {
    Bonbon,
    Key,
  }

  public class CurrencyComponent
    : ComponentBase
  {
    //
    // Initialization data.
    //
    public int MaxAmount = int.MaxValue;
    public int InitialAmount;

    //
    // Runtime data.
    //
    public int CurrentAmount;

    /// <summary>
    /// Between 0 and 1 (MaxAmount).
    /// </summary>
    public float CurrentAmountPercent => (float)CurrentAmount / MaxAmount;


    public CurrencyComponent(GameObject owner)
      : base(owner)
    {
    }

    public override void Initialize()
    {
      base.Initialize();

      CurrentAmount = InitialAmount;
    }
  }
}
