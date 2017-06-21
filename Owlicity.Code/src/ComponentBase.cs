using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using VelcroPhysics.Shared;

namespace Owlicity
{
  public abstract class ComponentBase
  {
    public GameObject Owner;

    public bool IsInitializationEnabled = true;
    public bool IsPrePhysicsUpdateEnabled = true;
    public bool IsUpdateEnabled = true;
    public bool IsDrawEnabled = true;

    public bool DebugDrawingEnabled = true;

    public Action BeforeInitialize;
    public Action BeforePostInitialize;
    public Action BeforePrePhysicsUpdate;
    public Action BeforeUpdate;
    public Action BeforeDraw;
    public Action BeforeDestroy;

    public ComponentBase(GameObject owner)
    {
      Owner = owner;
      Owner.AddComponent(this);
    }

    public virtual void Initialize() { }
    public virtual void PostInitialize() { } // TODO(manu): Maybe rename. BeginPlay?
    public virtual void PrePhysicsUpdate(float deltaSeconds) { }
    public virtual void Update(float deltaSeconds) { }
    public virtual void Draw(Renderer renderer) { }
    public virtual void Destroy() { }
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
    public ISpatial DepthReference;

    public DrawComponent(GameObject owner) : base(owner)
    {
      DepthReference = this;
    }

    public override void Update(float deltaSeconds)
    {
      base.Update(deltaSeconds);

      if(DepthReference != null)
      {
        SpatialData spatial = DepthReference.GetWorldSpatialData();
        RenderDepth = Global.Game.CalcDepth(spatial, Owner.Layer);

        if(DebugDrawingEnabled)
        {
          Global.Game.DebugDrawCommands.Add(view =>
          {
            view.DrawPoint(spatial.Position, Conversion.ToMeters(3), Color.Navy);
          });
        }
      }
    }
  }
}
