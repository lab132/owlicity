using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Owlicity.src;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Owlicity
{
  public class Level
  {
    private const int SCREEN_DIMENSION = 256;
    private Screen[,] _screens;
    private List<Screen> _activeScreens;
    private List<Screen> _previouslyActiveScreens;
    private Point _activeCenter;
    private ContentManager _contentManager;
    public int ScreenTileWidth { get; set; } = 1920;
    public int ScreenTileHeight { get; set; } = 1080;
    public ISpatial CullingCenter { get; set; }

    public List<GameObject> GameObjects = new List<GameObject>();
    public List<GameObject> GameObjectsPendingAdd = new List<GameObject>();
    public List<GameObject> GameObjectsPendingRemove = new List<GameObject>();

    public Level(ContentManager contentManager)
    {
      _screens = new Screen[SCREEN_DIMENSION, SCREEN_DIMENSION];
      _activeCenter = new Point(0, 0);
      _activeScreens = new List<Screen>();
      _previouslyActiveScreens = new List<Screen>();
      _contentManager = contentManager;
    }

    public void AddGameObject(GameObject go)
    {
      Debug.Assert(!GameObjects.Contains(go));
      Debug.Assert(!GameObjectsPendingAdd.Contains(go));

      if(!GameObjectsPendingRemove.Contains(go))
      {
        GameObjectsPendingAdd.Add(go);
      }
    }

    public void RemoveGameObject(GameObject go)
    {
      Debug.Assert(GameObjects.Contains(go));
      Debug.Assert(GameObjectsPendingRemove.Contains(go));

      if(!GameObjectsPendingAdd.Remove(go))
      {
        GameObjectsPendingRemove.Add(go);
      }
    }

    public void AddScreen(uint posX, uint posY, Screen screen)
    {
      _screens[posX, posY] = screen;
      screen.AbsoulutePosition = new Vector2(posX * ScreenTileWidth, posY * ScreenTileHeight);
    }

    public void Initialize()
    {
    }

    public void Update(float deltaSeconds)
    {
      _previouslyActiveScreens = _activeScreens;
      _activeScreens = GetActiveScreens();
      var becameActive = _activeScreens.Where(s => ! _previouslyActiveScreens.Contains(s));
      var becameInactive = _previouslyActiveScreens.Where(s => ! _activeScreens.Contains(s));
      
      foreach(Screen screen in becameActive)
      {
        // screen.LoadContent(_contentManager);
      }

      foreach(Screen screen in becameInactive)
      {
        // screen.UnloadContent();
      }

      foreach (Screen screen in _activeScreens)
      {
        screen.Update(deltaSeconds);
      }

      //
      // Game object simulation
      //
      GameObjects.AddRange(GameObjectsPendingAdd);
      foreach(GameObject go in GameObjectsPendingAdd)
      {
        go.Initialize();
      }
      GameObjectsPendingAdd.Clear();

      foreach(GameObject go in GameObjects)
      {
        go.Update(deltaSeconds);
      }

      GameObjects.RemoveAll(go => GameObjectsPendingRemove.Contains(go));
      foreach(GameObject go in GameObjectsPendingRemove)
      {
        go.Deinitialize();
      }
      GameObjectsPendingRemove.Clear();
    }

    public void Draw(float deltaSeconds, SpriteBatch batch)
    {
      foreach (Screen screen in _activeScreens)
      {
        screen.Draw(batch);
      }

      foreach(GameObject go in GameObjects)
      {
        go.Draw(deltaSeconds, batch);
      }
    }

    private List<Screen> GetActiveScreens()
    {

      var screenList = new List<Screen>();

      Vector2 focus = CullingCenter.Spatial.GetWorldSpatialData().Transform.p;
      int tileX = (int) focus.X / ScreenTileWidth;
      int tileY = (int) focus.Y / ScreenTileHeight;

      AddToListIfExists(tileX, tileY, ref screenList);
      AddToListIfExists(tileX, tileY + 1, ref screenList);
      AddToListIfExists(tileX, tileY - 1, ref screenList);
      AddToListIfExists(tileX + 1, tileY, ref screenList);
      AddToListIfExists(tileX + 1, tileY + 1, ref screenList);
      AddToListIfExists(tileX + 1, tileY - 1, ref screenList);
      AddToListIfExists(tileX - 1, tileY, ref screenList);
      AddToListIfExists(tileX - 1, tileY + 1, ref screenList);
      AddToListIfExists(tileX - 1, tileY - 1, ref screenList);
      return screenList;
    }

    private void AddToListIfExists(int x, int y, ref List<Screen> screenList)
    {
      if (x < SCREEN_DIMENSION && x >= 0 && y < SCREEN_DIMENSION && y >= 0)
      {
        var entry = _screens[x,y];
        if (entry != null)
        {
          screenList.Add(entry);
        }
      }
    }
  }
}
