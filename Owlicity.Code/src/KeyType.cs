using System;
using System.Collections.Generic;

namespace Owlicity
{
  public enum KeyType
  {
    Gold,

    COUNT
  }

  public static partial class Global
  {
    public static IEnumerable<KeyType> IterKeyTypes()
    {
      for(int index = 0; index < (int)KeyType.COUNT; index++)
      {
        KeyType keyType = (KeyType)index;
        yield return keyType;
      }
    }
  }
}
