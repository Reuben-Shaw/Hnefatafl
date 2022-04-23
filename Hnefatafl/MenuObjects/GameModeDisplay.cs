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
    enum Maps { Scandinavia, Ireland, Scotland }
    sealed class GameModeDisplay : MenuObject
    {
        List<Texture2D> _maps = new List<Texture2D>();
        private int _mapIndex = 0;
        private const int _borderVal = 4;
        private string m_dropDownString;
        public string _dropDownString
        {
            get { return m_dropDownString; }
            set 
            { 
                m_dropDownString = value;
                _fontSize = _font.MeasureString(m_dropDownString);
            }
        }
        Vector2 _fontSize;
        Texture2D _test;

        public GameModeDisplay(Point position, Point size, GraphicsDeviceManager graphics, ContentManager content)
        {
            _pos = position;
            _size = size;

            _backColour = new Texture2D(graphics.GraphicsDevice, 1, 1);
            _backColour.SetData(new[] { new Color(66, 41, 33) });

            _test = content.Load<Texture2D>("Texture/Board/11x11BaseBoard");

            _font = content.Load<SpriteFont>("Texture/Font/PixelFont");
            _fontColour = Color.Black;
            _maps.Add(content.Load<Texture2D>("Texture/Map/Scandinavia"));
        }

        public void ChangeMap(Maps map)
        {
            switch (map)
            {
                case (Maps.Scandinavia): _mapIndex = 0; break;
                case (Maps.Ireland): _mapIndex = 1; break;
                case (Maps.Scotland): _mapIndex = 2; break;
            }
        }

        public void Draw(SpriteBatch spriteBatch, Rectangle viewport)
        {
            spriteBatch.Draw(_maps[_mapIndex], new Rectangle(_pos, _size), Color.White);
            
            spriteBatch.DrawString(_font, _dropDownString, new Vector2(_pos.X + (int)(_fontSize.X / 4f), _pos.Y + _size.Y), _fontColour, 0f, new Vector2(0f, 0f), 1.5f, SpriteEffects.None, 0f);
            spriteBatch.DrawString(_font, "> 11x11 Board", new Vector2(_pos.X + _borderVal, _pos.Y + _size.Y + (int)(_fontSize.Y + (float)(_borderVal * 2))), _fontColour, 0f, new Vector2(0f, 0f), 1f, SpriteEffects.None, 0f);
            spriteBatch.DrawString(_font, "> Attacker First", new Vector2(_pos.X + _borderVal, _pos.Y + _size.Y + (int)(_fontSize.Y * 2)), _fontColour, 0f, new Vector2(0f, 0f), 1f, SpriteEffects.None, 0f);

            spriteBatch.Draw(_backColour, new Rectangle(_pos.X - _borderVal, _pos.Y - _borderVal, _size.X + (_borderVal * 2), _borderVal), Color.White);
            spriteBatch.Draw(_backColour, new Rectangle(_pos.X - _borderVal, _pos.Y + _size.Y, _size.X + (_borderVal * 2), _borderVal), Color.White);

            spriteBatch.Draw(_backColour, new Rectangle(_pos.X - _borderVal, _pos.Y - _borderVal, _borderVal, _size.Y + (_borderVal * 2) + (int)(_fontSize.Y * 3)), Color.White);
            spriteBatch.Draw(_backColour, new Rectangle(_pos.X + _size.X, _pos.Y - _borderVal, _borderVal, _size.Y + (_borderVal * 2) + (int)(_fontSize.Y * 3)), Color.White);
            spriteBatch.Draw(_backColour, new Rectangle(_pos.X - _borderVal, _pos.Y + _size.Y + (int)(_fontSize.Y * 3), _size.X + (_borderVal * 2), _borderVal), Color.White);


            spriteBatch.Draw(_test, new Rectangle(new Point(0 + (int)(viewport.Width / 2) - (_size.X / 2), _pos.Y), _size), Color.White);

            spriteBatch.Draw(_backColour, new Rectangle(new Point(0 + (int)(viewport.Width / 2) - (_size.X / 2) - _borderVal, _pos.Y - _borderVal), new Point(_size.X + (_borderVal * 2), _borderVal)), Color.White);
            spriteBatch.Draw(_backColour, new Rectangle(new Point(0 + (int)(viewport.Width / 2) - (_size.X / 2) - _borderVal, _pos.Y + _size.Y), new Point(_size.X + (_borderVal * 2), _borderVal)), Color.White);

            spriteBatch.Draw(_backColour, new Rectangle(new Point(0 + (int)(viewport.Width / 2) - (_size.X / 2) - _borderVal, _pos.Y - _borderVal), new Point(_borderVal, _size.Y + _borderVal)), Color.White);
            spriteBatch.Draw(_backColour, new Rectangle(new Point(0 + (int)(viewport.Width / 2) - (_size.X / 2) + _size.X, _pos.Y - _borderVal), new Point(_borderVal, _size.Y + _borderVal)), Color.White);
        }
    }
}