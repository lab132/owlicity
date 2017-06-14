using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelcroPhysics.Shared;

namespace Owlicity
{
  public class HealthDisplayComponent : DrawComponent
  {
    public enum DisplayOrigin
    {
      // Will place this component below this game object's bounding box.
      Bottom,

      // No attempt on re-positioning this component will be made.
      Custom,
    }

    //
    // Initialization data.
    //
    public DisplayOrigin InitialDisplayOrigin;
    public SpriteAnimationInstance HealthIcon;
    public Vector2 IconPadding = Global.ToMeters(-3, -3);
    public int NumIconsPerRow = int.MaxValue;

    //
    // Runtime data.
    //
    public HealthComponent Health;


    public HealthDisplayComponent(GameObject owner)
      : base(owner)
    {
    }

    public override void Initialize()
    {
      base.Initialize();

      Debug.Assert(NumIconsPerRow > 0);

      // TODO(manu): Does the AABB of this component count? It's just a display after all...
      //Spatial.LocalAABB = Global.ToMeters(HealthIcon.CalcAABB());

      if(Health == null)
      {
        Health = Owner.GetComponent<HealthComponent>();
        Debug.Assert(Health != null);
      }

      switch(InitialDisplayOrigin)
      {
        case DisplayOrigin.Bottom:
        {
          AABB ownerAABB = Global.CreateInvalidAABB();
          foreach(SpatialComponent sc in Owner.GetComponents<SpatialComponent>())
          {
            AABB worldAABB = sc.GetWorldSpatialData().AbsoluteAABB;
            ownerAABB.Combine(ref worldAABB);
          }

          if(ownerAABB.IsValid())
          {
            Vector2 worldPosition = new Vector2(ownerAABB.Center.X, ownerAABB.UpperBound.Y);
            worldPosition.Y += 0.5f * Spatial.LocalAABB.Height;
            this.SetWorldPosition(worldPosition);
          }
        }
        break;

        case DisplayOrigin.Custom:
        {
          // Don't do anything here.
        }
        break;

        default: throw new ArgumentException(nameof(InitialDisplayOrigin));
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

      if(Health.IsAlive)
      {
        int currentHP = Health.CurrentHealth;
        int cols = Math.Min(currentHP, NumIconsPerRow);
        Vector2 healthIconDim = Global.ToMeters(HealthIcon.ScaledDim);
        Vector2 spacing = healthIconDim + IconPadding;
        SpatialData anchor = this.GetWorldSpatialData();
        anchor.Position.X -= 0.5f * (cols * spacing.X);
        float left = anchor.Position.X;
        int hpIndex = 0;
        while(true)
        {
          for(int x = 0; x < cols; x++)
          {
            if(hpIndex++ >= currentHP)
            {
              goto DONE;
            }

            HealthIcon.Draw(renderer, anchor);
            anchor.Position.X += spacing.X;
          }
          anchor.Position.X = left;
          anchor.Position.Y += spacing.Y;
        }

        DONE:;
      }
    }
  }
}
