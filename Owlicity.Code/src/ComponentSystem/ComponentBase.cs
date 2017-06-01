using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Owlicity
{
  public abstract class ComponentBase
  {
    public GameObject Owner;
    public bool IsInitializationEnabled = true;
    public bool IsPrePhysicsUpdateEnabled = true;
    public bool IsUpdateEnabled = true;
    public bool IsDrawingEnabled = true;

    public ComponentBase(GameObject owner)
    {
      Owner = owner;
      Owner.AddComponent(this);
    }

    public virtual void Initialize() { IsInitializationEnabled = false; }
    public virtual void PrePhysicsUpdate(float deltaSeconds) { }
    public virtual void Update(float deltaSeconds) { }
    public virtual void Draw(float deltaSeconds, SpriteBatch batch) { }
  }

  public abstract class SpatialComponent : ComponentBase, ISpatial
  {
    public SpatialData Spatial { get; set; } = new SpatialData();

    public SpatialComponent(GameObject owner) : base(owner)
    {
    }
  }
}
