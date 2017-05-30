using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Owlicity
{
  class OwlWorld
  {
    public static OwlWorld theWorld;
    public OwlWorld() { }
    public OwlWorld Instance { get {
        if (theWorld == null) {
          theWorld = new  OwlWorld();
        }
        return theWorld;
      } }
  }
}
