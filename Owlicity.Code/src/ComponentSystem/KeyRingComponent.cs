using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Owlicity
{
  public enum KeyType
  {
    Gold,

    COUNT
  }

  public class KeyRingComponent : ComponentBase
  {
    //
    // Initialization data.
    //
    public int[] InitialKeyAmounts = new int[(int)KeyType.COUNT];

    //
    // Runtime data.
    //
    public int[] CurrentKeyAmounts = new int[(int)KeyType.COUNT];

    public KeyRingComponent(GameObject owner)
      : base(owner)
    {
    }

    public override void Initialize()
    {
      base.Initialize();

      for(int keyIndex = 0; keyIndex < (int)KeyType.COUNT; keyIndex++)
      {
        CurrentKeyAmounts[keyIndex] = InitialKeyAmounts[keyIndex];
      }
    }
  }
}
