using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelcroPhysics.Shared;

namespace Owlicity
{
  public struct ScreenLayoutInfo
  {
    public Vector2 Offset;
    public GameObjectType ObjectType;
  }

  public class Screen
  {
    public Point GridPosition;
    public Vector2 WorldPosition;
    public Texture2D GroundTexture;
    private GameObject _screenGameObject;
    private List<GameObject> _decorationObjects = new List<GameObject>();

    public void LoadContent(Level level)
    {
      string groundTextureName = string.Format(level.ContentNameFormat_Ground, GridPosition.Y, GridPosition.X);
      string collisionVerticesName = string.Format(level.ContentNameFormat_Collision, GridPosition.Y, GridPosition.X);
      string layoutName = string.Format(level.ContentNameFormat_Layout, GridPosition.Y, GridPosition.X);

      GroundTexture = Global.Game.Content.Load<Texture2D>(groundTextureName);

      //
      // Screen game object
      //
      {
        var go = new GameObject();
        var bc = new BodyComponent(go)
        {
          InitMode = BodyComponentInitMode.FromContent,
          ShapeContentName = collisionVerticesName,
        };
        go.RootComponent = bc;
        go.Spatial.Transform.p += WorldPosition;
        Global.Game.AddGameObject(go);

        _screenGameObject = go;
      }

      List<ScreenLayoutInfo> layoutInfos = Global.Game.Content.Load<List<ScreenLayoutInfo>>(layoutName);
      foreach(ScreenLayoutInfo layout in layoutInfos)
      {
        var deco = GameObjectFactory.CreateKnown(layout.ObjectType);
        deco.Spatial.Transform.p += layout.Offset;
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

    public void Draw(SpriteBatch batch)
    {
      Debug.Assert(GroundTexture != null);

      SpatialData spatial = _screenGameObject.GetWorldSpatialData();

      batch.Draw(GroundTexture,
        position: spatial.Transform.p,
        sourceRectangle: null,
        color: Color.White,
        rotation: spatial.Transform.q.GetAngle(),
        origin: Vector2.Zero,
        scale: 1,
        effects: SpriteEffects.None,
        layerDepth: 1.0f);
    }
  }
}
