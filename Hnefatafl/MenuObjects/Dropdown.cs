using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections;
using System.Collections.Generic;
using static Hnefatafl.MenuObjects.MenuObject.Status;

namespace Hnefatafl.MenuObjects
{
    class Dropdown : MenuObject
    {
        private List<string> m_items = new List<string>();
        public List<string> _items
        {
            get
            {
                return m_items;
            }
            set
            {
                m_items = value;
                m_items.Sort();
            }
        }
        private int _selected;
        private Texture2D _divideColour;
        private const int _borderVal = 6;

        public Dropdown(Point position, Point size, string name)
        {
            _pos = position;
            _size = size;
            _name = name;
            _status = Unselected;
            _selected = -1;
            _items = new List<string>();
        }

        public void LoadContent(ContentManager Content, GraphicsDeviceManager graphics)
        {
            _font = Content.Load<SpriteFont>("Texture/Font/PixelFont");

            _backColour = new Texture2D(graphics.GraphicsDevice, 1, 1);
            _backColour.SetData(new[] { new Color(255, 230, 206) });
            _divideColour = new Texture2D(graphics.GraphicsDevice, 1, 1);
            _divideColour.SetData(new[] { new Color(66, 41, 33) });

            _fontColour = Color.Black;
        }

        public string GetSelected()
        {
            return _items[_selected];
        }

        public void ClickEvent(Point mouse)
        {
            if (_items.Count > 0)
            {
                Rectangle extendedRect = new Rectangle(_pos.X, _pos.Y, _size.X, _size.Y * (_items.Count + 1));

                if (extendedRect.Contains(mouse))
                { 
                    Rectangle rect = new Rectangle(_pos, _size);
                    int i = 0;
                    while (i < _items.Count)
                    {
                        rect.Y += rect.Height;
                        if (rect.Contains(mouse))
                        {
                            _selected = i;
                            i = _items.Count;
                            _status = Unselected;
                        }

                        i++;
                    }
                }
                else
                {
                    _status = Unselected;
                }
            }
            else
            {
                _status = Unselected;
            }
        }

        public void Draw(GraphicsDeviceManager graphics, SpriteBatch spriteBatch)
        {
            Rectangle rect = new Rectangle(_pos, _size);

            spriteBatch.Draw(_backColour, rect, Color.White);
            if (_selected != -1)
                spriteBatch.DrawString(_font, _items[_selected], new Vector2(rect.X + 8, rect.Y), _fontColour, 0, new Vector2(0, 0), new Vector2(1f, 1f), SpriteEffects.None, 0);
            
            spriteBatch.Draw(_divideColour, new Rectangle(rect.X, rect.Y - (_borderVal / 2), rect.Width, _borderVal), Color.White);
            spriteBatch.Draw(_divideColour, new Rectangle(rect.X, rect.Y + rect.Height - (_borderVal / 2), rect.Width, _borderVal), Color.White);

            if (_status == Selected)
            {
                int i = 0;
                foreach (string item in _items)
                {
                    rect.Y += rect.Height;
                    spriteBatch.Draw(_backColour, rect, Color.White);
                    spriteBatch.DrawString(_font, _items[i], new Vector2(rect.X + 8, rect.Y), _fontColour, 0, new Vector2(0, 0), new Vector2(1f, 1f), SpriteEffects.None, 0);

                    spriteBatch.Draw(_divideColour, new Rectangle(rect.X, rect.Y - (_borderVal / 2), rect.Width, _borderVal), Color.White);

                    i++;
                }

                spriteBatch.Draw(_divideColour, new Rectangle(rect.X, rect.Y + rect.Height - 2, rect.Width, _borderVal), Color.White);
            }

            spriteBatch.Draw(_divideColour, new Rectangle(_pos.X - (_borderVal / 2), _pos.Y, _borderVal, rect.Y - _pos.Y + _size.Y), Color.White);
            spriteBatch.Draw(_divideColour, new Rectangle(_pos.X - (_borderVal / 2) + _size.X, _pos.Y, _borderVal, rect.Y - _pos.Y + _size.Y), Color.White);
        }
    }
}