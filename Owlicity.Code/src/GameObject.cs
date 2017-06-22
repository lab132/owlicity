using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Owlicity
{
  public class GameObject : ISpatial
  {
    private static int _idGenerator;

    public readonly int LocalID = ++_idGenerator;

    private string _defaultName;
    public string DefaultName => _defaultName ?? (_defaultName = $"{GetType().Name}_{LocalID}");

    private string _name;
    public string Name
    {
      get => _name ?? DefaultName;
      set => _name = value;
    }

    public List<ComponentBase> Components { get; } = new List<ComponentBase>();

    public SpatialComponent RootComponent;

    private SpatialData _spatial = new SpatialData();
    public SpatialData Spatial
    {
      get
      {
        SpatialData result;
        if(RootComponent != null)
        {
          result = RootComponent.Spatial;
        }
        else
        {
          result = _spatial;
        }

        return result;
      }
    }

    public GameLayer Layer = GameLayer.Default;

    public void AddComponent(ComponentBase newComponent)
    {
      Debug.Assert(!Components.Contains(newComponent));
      Components.Add(newComponent);
    }

    public T GetComponent<T>()
      where T : ComponentBase
    {
      return Components.OfType<T>().FirstOrDefault();
    }

    public IEnumerable<T> GetComponents<T>()
      where T : ComponentBase
    {
      return Components.OfType<T>();
    }

    public virtual void Initialize()
    {
      foreach(ComponentBase component in Components.Where(c => c.IsInitializationEnabled))
      {
        component.BeforeInitialize?.Invoke();
        component.Initialize();
      }
    }

    public virtual void PostInitialize()
    {
      foreach(ComponentBase component in Components.Where(c => c.IsInitializationEnabled))
      {
        component.BeforePostInitialize?.Invoke();
        component.PostInitialize();
      }
    }

    public virtual void PrePhysicsUpdate(float deltaSeconds)
    {
      foreach(ComponentBase component in Components.Where(c => c.IsPrePhysicsUpdateEnabled))
      {
        component.BeforePrePhysicsUpdate?.Invoke();
        component.PrePhysicsUpdate(deltaSeconds);
      }
    }

    public virtual void Update(float deltaSeconds)
    {
      foreach(ComponentBase component in Components.Where(c => c.IsUpdateEnabled))
      {
        component.BeforeUpdate?.Invoke();
        component.Update(deltaSeconds);
      }
    }

    public virtual void Draw(Renderer renderer)
    {
      foreach(ComponentBase component in Components.Where(c => c.IsDrawEnabled))
      {
        component.BeforeDraw?.Invoke();
        component.Draw(renderer);
      }
    }

    public virtual void Destroy()
    {
      foreach(ComponentBase component in Components)
      {
        component.BeforeDestroy?.Invoke();
        component.Destroy();
      }
    }

    public override string ToString()
    {
      return $"{Name} ({LocalID}) @ {Spatial}";
    }
  }
}
