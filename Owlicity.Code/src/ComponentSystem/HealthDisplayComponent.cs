using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Owlicity
{
  public class HealthDisplayComponent : DrawComponent
  {
    //
    // Initialization data.
    //
    public HealthComponent Health;
    public SpriteAnimationInstance HealthIcon;
    public float InterHealthIconSpace = Global.ToMeters(-3);


    public HealthDisplayComponent(GameObject owner)
      : base(owner)
    {
    }

    public override void Initialize()
    {
      base.Initialize();

      if(Health == null)
      {
        Health = Owner.GetComponent<HealthComponent>();
        Debug.Assert(Health != null);
      }

      Debug.Assert(HealthIcon != null, "Give me an animation instance!");
    }

    public override void Update(float deltaSeconds)
    {
      base.Update(deltaSeconds);
      HealthIcon.Update(deltaSeconds);
    }

    public override void Draw(Renderer renderer)
    {
      base.Draw(renderer);

      float spacing = InterHealthIconSpace + Global.ToMeters(HealthIcon.ScaledDim.X);
      double sign = -1.0;
      SpatialData anchor = this.GetWorldSpatialData();
      for(int healthIndex = 0; healthIndex < Health.CurrentHealth; healthIndex++)
      {
        // 0, 1, -1, 2, -2, ...
        int truncatedIndex = (int)(Math.Ceiling(0.5 * healthIndex) * sign);
        SpatialData spatial = anchor.GetCopy();
        spatial.Position.X += truncatedIndex * spacing;
        sign = -sign;

        HealthIcon.Draw(renderer, spatial);
      }
    }
  }
}
