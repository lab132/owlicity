using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Owlicity
{
  public class KeyRingComponent : ComponentBase
  {
    public int[] CurrentKeyAmounts = new int[(int)KeyType.COUNT];

    public int this[KeyType keyType]
    {
      get => CurrentKeyAmounts[(int)keyType];
      set => CurrentKeyAmounts[(int)keyType] = value;
    }

    public KeyRingComponent(GameObject owner)
      : base(owner)
    {
    }
  }
}
