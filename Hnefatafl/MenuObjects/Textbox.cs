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
    sealed class TextBox : MenuObject
    {
        private Texture2D _borderColour { get; set; }
        private Color _defaultFontColour { get; set; }
        private string m_defaultText;
        public string _defaultText 
        { 
            get
            {
                return m_defaultText;
            }
            private set
            {
                m_defaultText = value;
            }
        }
        public new string _text 
        { 
            get
            {
                return m_text;
            }
            set
            {
                m_text = value;
            }
        }
        private double m_lastPress;
        public double _lastPress 
        { 
            get
            {
                return m_lastPress;
            }
            private set
            {
                m_lastPress = value;
            }
        }
        private bool _full = false;

        public TextBox(Point position, Point size, string name)
        {
            _pos = position;
            _size = size;
            _status = Unselected;
            _name = name;
            _text = "";
            _defaultText = "";
            _fontColour = Color.Black;
            _defaultFontColour = Color.Gray;
            _textPos = new Vector2(-1, -1);
            _lastPress = 0;
        }
        
        public TextBox(Point position, Point size, string defaultText, string name)
        {
            _pos = position;
            _size = size;
            _status = Unselected;
            _name = name;
            _text = "";
            _defaultText = defaultText;
            _fontColour = Color.Black;
            _defaultFontColour = Color.Gray;
            _textPos = new Vector2(-1, -1);
            _lastPress = 0;
        }

        public void Update(GraphicsDeviceManager graphics, ContentManager Content)
        {
            _font = Content.Load<SpriteFont>("Texture/Font/PixelFont");
            Vector2 fontSize = _font.MeasureString("l");
            //fontSize = new Vector2(fontSize.X / 2, fontSize.Y / 2);
            _textPos = new Vector2(12 + _pos.X, (int)((_size.Y - fontSize.Y) / 2) + _pos.Y);
            CreateTextures(graphics, Color.Black, Color.White);
        }

        private void CreateTextures(GraphicsDeviceManager graphics, Color borderColour, Color backColour)
        {
            _borderColour = new Texture2D(graphics.GraphicsDevice, 1, 1);
            _borderColour.SetData(new[] { borderColour });

            _backColour = new Texture2D(graphics.GraphicsDevice, 1, 1);
            _backColour.SetData(new[] { backColour });
        }

        public void Add(Keys key)
        {
            if (!_full)
            {
                string keyString = key.ToString().ToLower();
                
                if (keyString.Length == 1)
                {
                    _text += keyString;
                }
                else if (keyString.Length == 2)
                {
                    keyString = keyString.TrimStart('d');
                    _text += keyString;
                }
                else if (keyString.Length > 5 && keyString.Substring(0, 6) == "numpad")
                {
                    keyString = keyString.Substring(6, 1);
                    _text += keyString;
                }
                else if (keyString == "oemperiod" || keyString == "decimal")
                {
                    keyString = ".";
                    _text += keyString;
                }
                else if (keyString == "enter")
                {
                    _status = Unselected;
                }

                TextboxFull();
            }
        }

        public void Add(string addChar)
        {
            if (!_full)
            {
                _text += addChar;

                TextboxFull();
            }
        }

        private void TextboxFull()
        {
            if (_font.MeasureString(_text + "D").X >= _size.X - 10)
            {
                RemoveChar(0.2);
                _full = true;
            }
        }

        public void RemoveChar(double time)
        {
            if (_lastPress >= 0.1)
            {
                _full = false;
                _lastPress = 0;
                _text = string.IsNullOrEmpty(_text) ? _text : _text.Remove(_text.Length - 1);
            }
            else
            {
                _lastPress += time;
            }
        }

        public void DeleteText()
        {
            _text = "";
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, Rectangle viewPort)
        {
            Rectangle rectBody = new Rectangle(new Point(_pos.X + 5, _pos.Y + 5), new Point(_size.X - 10, _size.Y - 10));
            Rectangle rectBack = new Rectangle(_pos, _size);
            
            
            spriteBatch.Draw(_borderColour, rectBack, Color.White);
            spriteBatch.Draw(_backColour, rectBody, Color.White);
            if (string.IsNullOrWhiteSpace(_text))
            {
                spriteBatch.DrawString(_font, _defaultText, _textPos, _defaultFontColour, 0, new Vector2(0, 0), new Vector2(1f, 1f), SpriteEffects.None, 0);
            }
            else
            {
                spriteBatch.DrawString(_font, _text, _textPos, _fontColour, 0, new Vector2(0, 0), new Vector2(1f, 1f), SpriteEffects.None, 0);
            }
        }
    }
}