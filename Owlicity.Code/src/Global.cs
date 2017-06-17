using Microsoft.Xna.Framework;
using System;
using VelcroPhysics.Collision.Filtering;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Shared;

namespace Owlicity
{
  public static partial class Global
  {
    public static OwlGame Game { get; set; }

    public static readonly Vector2 OwliverScale = new Vector2(0.5f);
    public static readonly Vector2 BonbonScale = new Vector2(0.6f);
    public static readonly Vector2 SlurpScale = new Vector2(0.5f);
    public static readonly Vector2 TanktonScale = new Vector2(0.5f);

    public static readonly float OwliverDensity = 0.01f;

    public const Category LevelCollisionCategory = Category.Cat1;
    public const Category OwliverCollisionCategory = Category.Cat2;
    public const Category OwliverWeaponCollisionCategory = Category.Cat3;
    public const Category EnemyCollisionCategory = Category.Cat4;
    public const Category PickupCollisionCategory = Category.Cat5;

    public static readonly Color[] ConfettiRed = new[] { new Color(0xa3, 0x3b, 0x41), new Color(0xda, 0x67, 0x77), };
    public static readonly Color[] ConfettiPurple = new[] { new Color(0x73, 0x4c, 0x87) };
    public static readonly Color[] ConfettiBlue = new[] { new Color(0x41, 0x6d, 0x9c), new Color(0x7a, 0xaa, 0xdd), };
    public static readonly Color[] ConfettiGreen = new[] { new Color(0x5f, 0x72, 0x2d), new Color(0x9f, 0xb5, 0x63), };
    public static readonly Color[] ConfettiYellow = new[] { new Color(0xda, 0xa7, 0x44), new Color(0xf4, 0xd3, 0x92), };

    public static readonly Color[] AllConfettiColors = new[] {
      ConfettiRed[0], ConfettiRed[0],
      ConfettiPurple[0],
      ConfettiGreen[0], ConfettiGreen[0],
      ConfettiYellow[0], ConfettiYellow[0],
    };

    // TODO(manu): Better name for this one?
    public static AABB CreateInvalidAABB()
    {
      return new AABB
      {
        LowerBound = new Vector2(float.MaxValue),
        UpperBound = new Vector2(float.MinValue),
      };
    }

    public static void SpawnGameObjectsInRingFormation(Vector2 center, float radius, int numToSpawn, Random rand,
                                                       params GameObjectType[] types)
    {
      Vector2 localPosition = new Vector2(radius, 0.0f);
      Angle angle = new Angle { Radians = MathHelper.TwoPi / numToSpawn };
      for(int bonbonIndex = 0; bonbonIndex < numToSpawn; bonbonIndex++)
      {
        GameObjectType type = rand.Choose(types);
        GameObject go = GameObjectFactory.CreateKnown(type);
        go.SetWorldPosition(center + localPosition);
        Game.AddGameObject(go);

        localPosition = localPosition.GetRotated(angle);
      }
    }

    public static void HandleDefaultHit(Body hitBody, Vector2 hitterPosition, int damage, float force)
    {
      GameObject go = ((BodyComponent)hitBody.UserData).Owner;
      bool sendItToHell = true;

      // Handle health component
      HealthComponent hc = go.GetComponent<HealthComponent>();
      if(hc != null)
      {
        if(!hc.IsInvincible)
        {
          hc.Hit(damage);
        }
        else
        {
          sendItToHell = false;
        }
      }

      if(sendItToHell)
      {
        // Apply impulse
        Vector2 deltaPosition = hitBody.Position - hitterPosition;
        deltaPosition.GetDirectionAndLength(out Vector2 dir, out float distance);
        Vector2 impulse = force * dir;
        hitBody.ApplyLinearImpulse(impulse);
      }
    }
  }
}
