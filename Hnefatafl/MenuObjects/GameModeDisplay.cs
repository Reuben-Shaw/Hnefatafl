using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Hnefatafl.Media;
using static Hnefatafl.MenuObjects.MenuObject.Status;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Hnefatafl.MenuObjects
{ 
    enum Maps { Scandinavia, Ireland, Scotland }
    sealed class GameModeDisplay : MenuObject
    {
        List<Texture2D> _maps = new List<Texture2D>();
        private int _mapIndex = 0;
        private const int _borderVal = 4;
        private string _boardString;
        private string _boardDescription;
        private BoardTypes m_dropDownString;
        public BoardTypes _dropDownString
        {
            get { return m_dropDownString; }
            set 
            { 
                m_dropDownString = value;
                _boardString = Hnefatafl.NodeText(Hnefatafl.MenuXmlLoad("Menu"), m_dropDownString.ToString(), Hnefatafl.GameState.GameSetup);
                _boardDescription = Hnefatafl.NodeText(Hnefatafl.MenuXmlLoad("Menu"), m_dropDownString.ToString() + "_Description", Hnefatafl.GameState.GameSetup);
                _fontSize = _font.MeasureString(_boardString);
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

        public void Draw(SpriteBatch spriteBatch, float fontMod, Rectangle viewport)
        {
            spriteBatch.Draw(_maps[_mapIndex], new Rectangle(_pos, _size), Color.White);
            
            spriteBatch.DrawString(_font, _boardString, new Vector2((int)((float)(_size.X / 2f) - ((_fontSize.X * (fontMod * 1.5f)) / 2f)) + _pos.X, _pos.Y + _size.Y), _fontColour, 0f, new Vector2(0f, 0f), fontMod * 1.5f, SpriteEffects.None, 0f);
            spriteBatch.DrawString(_font, _boardDescription, new Vector2(_pos.X + _borderVal, _pos.Y + _size.Y + (int)((_fontSize.Y * fontMod) + (float)(_borderVal * 2))), _fontColour, 0f, new Vector2(0f, 0f), fontMod, SpriteEffects.None, 0f);
            spriteBatch.DrawString(_font, "> Attacker First", new Vector2(_pos.X + _borderVal, _pos.Y + _size.Y + (int)((_fontSize.Y * fontMod) * 2)), _fontColour, 0f, new Vector2(0f, 0f), fontMod, SpriteEffects.None, 0f);

            spriteBatch.Draw(_backColour, new Rectangle(_pos.X - _borderVal, _pos.Y - _borderVal, _size.X + (_borderVal * 2), _borderVal), Color.White);
            spriteBatch.Draw(_backColour, new Rectangle(_pos.X - _borderVal, _pos.Y + _size.Y, _size.X + (_borderVal * 2), _borderVal), Color.White);

            spriteBatch.Draw(_backColour, new Rectangle(_pos.X - _borderVal, _pos.Y - _borderVal, _borderVal, _size.Y + (_borderVal * 2) + (int)((_fontSize.Y * fontMod) * 3)), Color.White);
            spriteBatch.Draw(_backColour, new Rectangle(_pos.X + _size.X, _pos.Y - _borderVal, _borderVal, _size.Y + (_borderVal * 2) + (int)((_fontSize.Y * fontMod) * 3)), Color.White);
            spriteBatch.Draw(_backColour, new Rectangle(_pos.X - _borderVal, _pos.Y + _size.Y + (int)((_fontSize.Y * fontMod) * 3), _size.X + (_borderVal * 2), _borderVal), Color.White);


            spriteBatch.Draw(_test, new Rectangle(new Point(0 + (int)(viewport.Width / 2) - (_size.X / 2), _pos.Y), _size), Color.White);

            spriteBatch.Draw(_backColour, new Rectangle(new Point(0 + (int)(viewport.Width / 2) - (_size.X / 2) - _borderVal, _pos.Y - _borderVal), new Point(_size.X + (_borderVal * 2), _borderVal)), Color.White);
            spriteBatch.Draw(_backColour, new Rectangle(new Point(0 + (int)(viewport.Width / 2) - (_size.X / 2) - _borderVal, _pos.Y + _size.Y), new Point(_size.X + (_borderVal * 2), _borderVal)), Color.White);

            spriteBatch.Draw(_backColour, new Rectangle(new Point(0 + (int)(viewport.Width / 2) - (_size.X / 2) - _borderVal, _pos.Y - _borderVal), new Point(_borderVal, _size.Y + _borderVal)), Color.White);
            spriteBatch.Draw(_backColour, new Rectangle(new Point(0 + (int)(viewport.Width / 2) - (_size.X / 2) + _size.X, _pos.Y - _borderVal), new Point(_borderVal, _size.Y + _borderVal)), Color.White);
        }
    }
}