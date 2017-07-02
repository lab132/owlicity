using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelcroPhysics.Dynamics;

namespace Owlicity
{
  public static class Extensions
  {
    public static BodyComponent GetBodyComponent(this Body self)
    {
      return (BodyComponent)self.UserData;
    }

    public static BodyComponent GetBodyComponent(this Fixture self)
    {
      return (BodyComponent)self.UserData;
    }

    public static GameObject GetGameObject(this Body self)
    {
      return self.GetBodyComponent().Owner;
    }

    public static GameObject GetGameObject(this Fixture self)
    {
      return self.GetBodyComponent().Owner;
    }

  }
}
