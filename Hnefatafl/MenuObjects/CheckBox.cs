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
    sealed class CheckBox : MenuObject
    {
        private List<string> _options = new List<string>();
        private List<Vector2> _stringSizes = new List<Vector2>();
        private Texture2D _divideColour;
        public int _selected = 0;
        private Texture2D _tick;//, _cross;
        private const int _divide = 24;

        public CheckBox(Point position, Point size, List<string> strings, GraphicsDeviceManager graphics, ContentManager content)
        {
            _pos = position;
            _size = size;

            _font = content.Load<SpriteFont>("Texture/Font/PixelFont");
            _fontColour = Color.Black;

            _options = strings;

            foreach (string item in _options)
            {
                _stringSizes.Add(_font.MeasureString(item));
            }

            _tick = content.Load<Texture2D>("Texture/Menu/CheckTick");
            //_cross = content.Load<Texture2D>("Texture/Menu/CheckCross");

            _backColour = new Texture2D(graphics.GraphicsDevice, 1, 1);
            _backColour.SetData(new[] { new Color(255, 230, 206) });
            _divideColour = new Texture2D(graphics.GraphicsDevice, 1, 1);
            _divideColour.SetData(new[] { new Color(66, 41, 33) });
        }

        public void ButtonCheck(MouseState currentState, MouseState previousState, Point mouseLoc)
        {
            for (int i = 0; i < _options.Count; i++)
            {
                if (new Rectangle(new Point(_pos.X, _pos.Y + (_size.Y * i) + (_divide * i)), _size).Contains(mouseLoc))
                {
                    if (previousState.LeftButton == ButtonState.Pressed && currentState.LeftButton == ButtonState.Released)
                    {
                        _selected = i;
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, float fontSize)
        {
            for (int i = 0; i < _options.Count; i++)
            {
                spriteBatch.Draw(_divideColour, new Rectangle(new Point(_pos.X, _pos.Y + (_size.Y * i) + (_divide * i)), _size), Color.White);
                spriteBatch.Draw(_backColour, new Rectangle(_pos.X + 4, _pos.Y + (_size.Y * i) + (_divide * i) + 4, _size.X - 8, _size.Y - 8), Color.White);

                if (i == _selected) spriteBatch.Draw(_tick, new Rectangle(new Point(_pos.X, _pos.Y + (_size.Y * i) + (_divide * i)), _size), Color.White);
                // else spriteBatch.Draw(_cross, new Rectangle(new Point(_pos.X, _pos.Y + (_size.Y * i) + (_divide * i)), _size), Color.White);

                spriteBatch.DrawString(_font, _options[i], new Vector2(_pos.X + _size.X + _divide, (_pos.Y + (_size.Y * i) + (_divide * i)) - ((_size.Y - (_stringSizes[i].Y * fontSize)) / 2) - (_size.Y - ((_size.Y / 7) * 4))), _fontColour, 0f, new Vector2(0f, 0f), fontSize, SpriteEffects.None, 0f);
            }
        }
    }
}