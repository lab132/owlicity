using Microsoft.Xna.Framework.Graphics;
using System;

namespace Owlicity
{
  public abstract class ComponentBase
  {
    public GameObject Owner;

    public bool IsInitializationEnabled = true;
    public bool IsPrePhysicsUpdateEnabled = true;
    public bool IsUpdateEnabled = true;
    public bool IsDrawEnabled = true;

    public Action BeforeInitialize;
    public Action BeforePostInitialize;

    public ComponentBase(GameObject owner)
    {
      Owner = owner;
      Owner.AddComponent(this);
    }

    public virtual void Initialize() { }
    public virtual void PostInitialize() { } // TODO(manu): Maybe rename. BeginPlay?
    public virtual void PrePhysicsUpdate(float deltaSeconds) { }
    public virtual void Update(float deltaSeconds) { }
    public virtual void Draw(SpriteBatch batch) { }
  }

  public class SpatialComponent : ComponentBase, ISpatial
  {
    public SpatialData Spatial { get; set; } = new SpatialData();

    public SpatialComponent(GameObject owner) : base(owner)
    {
    }
  }

  public abstract class DrawComponent : SpatialComponent
  {
    public float RenderDepth;

    public DrawComponent(GameObject owner) : base(owner)
    {
    }

    public override void Initialize()
    {
      base.Initialize();
    }

    public override void Update(float deltaSeconds)
    {
      base.Update(deltaSeconds);

      if(!Owner.IsStationary)
      {
        RenderDepth = Global.Game.CalcDepth(this.GetWorldSpatialData(), Owner.Layer);
      }
    }
  }
}
