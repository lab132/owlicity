using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace Owlicity
{
  public struct ScreenLayoutInfo
  {
    public Vector2 OffsetInMeters;
    public GameObjectType ObjectType;
  }

  public class Screen
  {
    public Point GridPosition;
    public Vector2 WorldPosition;
    private GameObject _screenGameObject;
    private List<GameObject> _decorationObjects = new List<GameObject>();

    public void LoadContent(Level level)
    {
      string groundTextureName = string.Format(level.ContentNameFormat_Ground, GridPosition.Y, GridPosition.X);
      string collisionContentName = string.Format(level.ContentNameFormat_Collision, GridPosition.Y, GridPosition.X);
      string layoutContentName = string.Format(level.ContentNameFormat_Layout, GridPosition.Y, GridPosition.X);

      //
      // Screen game object
      //
      {
        var go = GameObjectFactory.CreateKnown(GameObjectType.BackgroundScreen);
        go.Spatial.Position += WorldPosition;

        var bc = go.Components.OfType<BodyComponent>().Single();
        bc.ShapeContentName = collisionContentName;

        var sc = go.Components.OfType<SpriteComponent>().Single();
        sc.SpriteContentName = groundTextureName;

        _screenGameObject = go;
        Global.Game.AddGameObject(_screenGameObject);
      }

      List<ScreenLayoutInfo> layoutInfos = Global.Game.Content.Load<List<ScreenLayoutInfo>>(layoutContentName);
      foreach(ScreenLayoutInfo layout in layoutInfos)
      {
        var deco = GameObjectFactory.CreateKnown(layout.ObjectType);
        SpriteAnimationComponent sa = deco.GetComponent<SpriteAnimationComponent>();
#if false
        // Choose random initial frame
        sa.OnPostInitialize += delegate ()
        {
          int startFrame = level.Random.Next(3);
          for(int frameIndex = 0; frameIndex < startFrame; frameIndex++)
          {
            sa.ActiveAnimation.AdvanceFrameIndex();
          }
        };
#endif
        deco.Spatial.Position += layout.OffsetInMeters;
        deco.AttachTo(_screenGameObject);

        Global.Game.AddGameObject(deco);

        _decorationObjects.Add(deco);
      }
    }

    public void Unload()
    {
      Global.Game.RemoveGameObject(_screenGameObject);

      foreach(GameObject deco in _decorationObjects)
      {
        Global.Game.RemoveGameObject(deco);
      }
      _decorationObjects.Clear();
    }
  }
}
