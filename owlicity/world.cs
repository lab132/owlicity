using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Owlicity
{
  class World
  {
    public static World theWorld;
    public World() { }
    public World Instance { get {
        if (theWorld == null) {
          theWorld = new  World();
        }
        return theWorld;
      } }
  }
}
