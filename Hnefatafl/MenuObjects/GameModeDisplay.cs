using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Hnefatafl.Media;
using static Hnefatafl.MenuObjects.MenuObject.Status;
using System.Collections.Generic;
using System.Xml;

namespace Hnefatafl.MenuObjects
{
    sealed class GameModeDisplay : MenuObject
    {
        enum Maps: int { Scandinavia, Ireland, Scotland }
        List<Texture2D> _maps = new List<Texture2D>();

        public GameModeDisplay(Point position, Point size, ContentManager content)
        {
            _pos = position;
            _size = size;

            _maps.Add(content.Load<Texture2D>("Texture/Map/Scandinavia"));
        }

        public void Draw(SpriteBatch spriteBatch, Rectangle viewport)
        {
            spriteBatch.Draw(_maps[0], new Rectangle(_pos, _size), Color.White);
        }
    }
}