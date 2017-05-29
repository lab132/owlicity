using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Owlicity.src;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Owlicity
{
  class Level
  {
    private const int SCREEN_DIMENSION = 256;
    public Level(ContentManager contentManager)
    {
      _screens = new Screen[SCREEN_DIMENSION, SCREEN_DIMENSION];
      _activeCenter = new Point(0, 0);
      _activeScreens = new List<Screen>();
      _previouslyActiveScreens = new List<Screen>();
      _contentManager = contentManager;
    }

    private Screen[,] _screens;
    private List<Screen> _activeScreens;
    private List<Screen> _previouslyActiveScreens;
    private Point _activeCenter;
    private ContentManager _contentManager;
    public int ScreenTileWidth { get; set; } = 1920;
    public int ScreenTileHeight { get; set; } = 1080;
    public ITransformable CullingCenter { get; set; }

    public void addScreen(uint posX, uint posY, Screen screen) {
      _screens[posX, posY] = screen;
      screen.AbsoulutePosition = new Vector2(posX * ScreenTileWidth, posY * ScreenTileHeight);
    }

    public void Update(GameTime gameTime)
    {
      _previouslyActiveScreens = _activeScreens;
      _activeScreens = getActiveScreens();
      var becameActive = _activeScreens.Where(S => ! _previouslyActiveScreens.Contains(S));
      var becameInactive = _previouslyActiveScreens.Where(S => ! _activeScreens.Contains(S));
      
      foreach(Screen screen in becameActive)
      {
     //   screen.LoadContent(_contentManager);
      }

      foreach(Screen screen in becameInactive)
      {
      //  screen.UnloadContent();
      }
      
      foreach (Screen screen in _activeScreens)
      {
        screen.Update(gameTime);
      }
    }

    public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
      foreach (Screen screen in _activeScreens)
      {
        screen.Draw(gameTime, spriteBatch);
      }
    }

    private List<Screen> getActiveScreens()
    {

      var screenList = new List<Screen>();

      Vector2 focus = CullingCenter.GetWorldTransform().Position;
      int tileX = (int) focus.X / ScreenTileWidth;
      int tileY = (int) focus.Y / ScreenTileHeight;

      addToListIfExists(tileX, tileY, ref screenList);
      addToListIfExists(tileX, tileY + 1, ref screenList);
      addToListIfExists(tileX, tileY - 1, ref screenList);
      addToListIfExists(tileX + 1, tileY, ref screenList);
      addToListIfExists(tileX + 1, tileY + 1, ref screenList);
      addToListIfExists(tileX + 1, tileY - 1, ref screenList);
      addToListIfExists(tileX - 1, tileY, ref screenList);
      addToListIfExists(tileX - 1, tileY + 1, ref screenList);
      addToListIfExists(tileX - 1, tileY - 1, ref screenList);
      return screenList;
    }

    private void addToListIfExists(int x, int y, ref List<Screen> screenList)
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
