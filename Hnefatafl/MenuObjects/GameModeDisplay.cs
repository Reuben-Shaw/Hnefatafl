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
    sealed class GameModeDisplay : MenuObject
    {
        List<Texture2D> _maps = new List<Texture2D>();
        private int _mapIndex = 0;
        private const int _borderVal = 4;
        private string _boardString;
        private string[] _boardDescription = new string[2];
        private BoardTypes m_dropDownType;
        public BoardTypes _dropDownType
        {
            get { return m_dropDownType; }
            set 
            { 
                m_dropDownType = value;
                _boardString = Hnefatafl.NodeText(Hnefatafl.MenuXmlLoad("Menu"), m_dropDownType.ToString(), Hnefatafl.GameState.GameSetup);
                _boardDescription[0] = Hnefatafl.NodeText(Hnefatafl.MenuXmlLoad("Menu"), m_dropDownType.ToString() + "_Description", Hnefatafl.GameState.GameSetup);
                _boardDescription[1] = Hnefatafl.NodeText(Hnefatafl.MenuXmlLoad("Menu"), m_dropDownType.ToString() + "_Description2", Hnefatafl.GameState.GameSetup);
                _fontSize = _font.MeasureString(_boardString);
            }
        }
        private Vector2 _fontSize;
        private Texture2D _board, _throne, _corners, _pieces;
        public bool[] _visibleArr = new bool[] { true, true };

        public GameModeDisplay(Point position, Point size, GraphicsDeviceManager graphics, ContentManager content)
        {
            _pos = position;
            _size = size;

            _backColour = new Texture2D(graphics.GraphicsDevice, 1, 1);
            _backColour.SetData(new[] { new Color(66, 41, 33) });

            _board = content.Load<Texture2D>("Texture/BoardDisplay/11_Base");
            _throne = content.Load<Texture2D>("Texture/BoardDisplay/11_Throne");
            _corners = content.Load<Texture2D>("Texture/BoardDisplay/11_Corners");
            _pieces = content.Load<Texture2D>("Texture/BoardDisplay/Pieces_Hnefatafl");

            _font = content.Load<SpriteFont>("Texture/Font/PixelFont");
            _fontColour = Color.Black;

            _maps.Add(content.Load<Texture2D>("Texture/Map/Scandinavia"));
            _maps.Add(content.Load<Texture2D>("Texture/Map/Sapmi"));
            _maps.Add(content.Load<Texture2D>("Texture/Map/Ireland"));
            _maps.Add(content.Load<Texture2D>("Texture/Map/Scotland"));
            _maps.Add(content.Load<Texture2D>("Texture/Map/Wales"));
        }

        public void ChangeBoard(ContentManager content)
        {
            int boardInt = Hnefatafl.ToInt(_dropDownType);
            _board = content.Load<Texture2D>($"Texture/BoardDisplay/{boardInt}_Base");
            _throne = content.Load<Texture2D>($"Texture/BoardDisplay/{boardInt}_Throne");
            _corners = content.Load<Texture2D>($"Texture/BoardDisplay/{boardInt}_Corners");
            _pieces = content.Load<Texture2D>($"Texture/BoardDisplay/Pieces_{_dropDownType.ToString()}");

            _mapIndex = ChangeMap(_dropDownType);
        }

        public int ChangeMap(BoardTypes type) => type switch
        {
            BoardTypes.Hnefatafl => 0,
            BoardTypes.Tawlbwrdd => 4,
            BoardTypes.Tablut => 1,
            BoardTypes.Brandubh => 2,
            BoardTypes.ArdRi => 3,
            _ => 0
        };

        public void Draw(SpriteBatch spriteBatch, float fontMod, Rectangle viewport)
        {
            spriteBatch.Draw(_maps[_mapIndex], new Rectangle(_pos, _size), Color.White);
            
            spriteBatch.DrawString(_font, _boardString, new Vector2((int)((float)(_size.X / 2f) - ((_fontSize.X * (fontMod * 1.5f)) / 2f)) + _pos.X, _pos.Y + _size.Y), _fontColour, 0f, new Vector2(0f, 0f), fontMod * 1.5f, SpriteEffects.None, 0f);
            spriteBatch.DrawString(_font, _boardDescription[0], new Vector2(_pos.X + _borderVal, _pos.Y + _size.Y + (int)((_fontSize.Y * fontMod) + (float)(_borderVal * 2))), _fontColour, 0f, new Vector2(0f, 0f), fontMod, SpriteEffects.None, 0f);
            spriteBatch.DrawString(_font, _boardDescription[1], new Vector2(_pos.X + _borderVal, _pos.Y + _size.Y + (int)((_fontSize.Y * fontMod) * 2)), _fontColour, 0f, new Vector2(0f, 0f), fontMod, SpriteEffects.None, 0f);

            spriteBatch.Draw(_backColour, new Rectangle(_pos.X - _borderVal, _pos.Y - _borderVal, _size.X + (_borderVal * 2), _borderVal), Color.White);
            spriteBatch.Draw(_backColour, new Rectangle(_pos.X - _borderVal, _pos.Y + _size.Y, _size.X + (_borderVal * 2), _borderVal), Color.White);

            spriteBatch.Draw(_backColour, new Rectangle(_pos.X - _borderVal, _pos.Y - _borderVal, _borderVal, _size.Y + (_borderVal * 2) + (int)((_fontSize.Y * fontMod) * 3)), Color.White);
            spriteBatch.Draw(_backColour, new Rectangle(_pos.X + _size.X, _pos.Y - _borderVal, _borderVal, _size.Y + (_borderVal * 2) + (int)((_fontSize.Y * fontMod) * 3)), Color.White);
            spriteBatch.Draw(_backColour, new Rectangle(_pos.X - _borderVal, _pos.Y + _size.Y + (int)((_fontSize.Y * fontMod) * 3), _size.X + (_borderVal * 2), _borderVal), Color.White);


            spriteBatch.Draw(_board, new Rectangle(new Point((int)(viewport.Width / 2) - (_size.X / 2), _pos.Y), _size), Color.White);
            if (_visibleArr[0]) spriteBatch.Draw(_throne, new Rectangle(new Point((int)(viewport.Width / 2) - (_size.X / 2), _pos.Y), _size), Color.White);
            if (_visibleArr[1]) spriteBatch.Draw(_corners, new Rectangle(new Point((int)(viewport.Width / 2) - (_size.X / 2), _pos.Y), _size), Color.White);
            spriteBatch.Draw(_pieces, new Rectangle(new Point((int)(viewport.Width / 2) - (_size.X / 2), _pos.Y), _size), Color.White);


            spriteBatch.Draw(_backColour, new Rectangle(new Point((int)(viewport.Width / 2) - (_size.X / 2) - _borderVal, _pos.Y - _borderVal), new Point(_size.X + (_borderVal * 2), _borderVal)), Color.White);
            spriteBatch.Draw(_backColour, new Rectangle(new Point((int)(viewport.Width / 2) - (_size.X / 2) - _borderVal, _pos.Y + _size.Y), new Point(_size.X + (_borderVal * 2), _borderVal)), Color.White);

            spriteBatch.Draw(_backColour, new Rectangle(new Point((int)(viewport.Width / 2) - (_size.X / 2) - _borderVal, _pos.Y - _borderVal), new Point(_borderVal, _size.Y + _borderVal)), Color.White);
            spriteBatch.Draw(_backColour, new Rectangle(new Point((int)(viewport.Width / 2) - (_size.X / 2) + _size.X, _pos.Y - _borderVal), new Point(_borderVal, _size.Y + _borderVal)), Color.White);
        }
    }
}