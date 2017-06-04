using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public bool IsDebugDrawingEnabled = true;

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
    public virtual void Draw(float deltaSeconds, SpriteBatch batch) { }
    public virtual void DebugDraw(float deltaSeconds, SpriteBatch batch) { }
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
    public SpatialData Hotspot = new SpatialData();

    public DrawComponent(GameObject owner) : base(owner)
    {
    }

    public override void Initialize()
    {
      base.Initialize();

      Hotspot.AttachTo(this);
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
