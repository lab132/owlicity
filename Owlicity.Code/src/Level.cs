using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Owlicity
{
  public enum UpdateStage
  {
    PrePhysics,
    PostPhysics,
  }

  public class Level
  {
    public const int SCREEN_DIMENSION = 256;
    private Screen[,] _screens;
    private List<Screen> _activeScreens;
    private List<Screen> _previouslyActiveScreens;
    private Point _activeCenter;
    private ContentManager _contentManager;
    public int ScreenTileWidth { get; set; } = 1920;
    public int ScreenTileHeight { get; set; } = 1080;
    public ISpatial CullingCenter { get; set; }

    public string ContentNameFormat_Ground;
    public string ContentNameFormat_Layout;
    public string ContentNameFormat_Collision;

    public Random Random = new Random();

    public Level(ContentManager contentManager)
    {
      _screens = new Screen[SCREEN_DIMENSION, SCREEN_DIMENSION];
      _activeCenter = new Point(0, 0);
      _activeScreens = new List<Screen>();
      _previouslyActiveScreens = new List<Screen>();
      _contentManager = contentManager;
    }

    public void CreateScreen(int posX, int posY)
    {
      Screen screen = new Screen
      {
        WorldPosition = Global.ToMeters(posX * ScreenTileWidth, posY * ScreenTileHeight),
        GridPosition = new Point(posX, posY),
      };
      _screens[posY, posX] = screen;
    }

    public void LoadContent()
    {
#if true
      for(int y = 0; y < SCREEN_DIMENSION; y++)
      {
        for(int x = 0; x < SCREEN_DIMENSION; x++)
        {
          Screen screen = _screens[y, x];
          if(screen != null)
          {
            screen.LoadContent(this);
          }
        }
      }
#endif
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
    }

    private List<Screen> GetActiveScreens()
    {
      var screenList = new List<Screen>();

      Vector2 focus = CullingCenter.Spatial.GetWorldSpatialData().Position;
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
        var entry = _screens[y, x];
        if (entry != null)
        {
          screenList.Add(entry);
        }
      }
    }
  }
}
