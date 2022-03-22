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
    class Label : MenuObject
    {
        protected float _fontSize { get; set; }
        protected Texture2D _colourDisplay { get; set; }
        protected Color _colour { get; set; }

        public Label() {  }

        public Label(Point position, Point size, string name, string text, float fontSize)
        {
            _pos = position;
            _size = size;
            _name = name;
            _text = text;
            _fontColour = Color.Black;
            _textPos = new Vector2(position.X + 36, position.Y + 16);
            _fontSize = fontSize;
        }

        public Label(Point position, Point size, string name, Color colour)
        {
            _pos = position;
            _size = size;
            _name = name;
            _colour = colour;
        }

        public void Update(GraphicsDeviceManager graphics, ContentManager Content)
        {
            _font = Content.Load<SpriteFont>("Texture/Font/PixelFont");
            if (string.IsNullOrEmpty(_text))
            {
                _colourDisplay = new Texture2D(graphics.GraphicsDevice, 1, 1);
                _colourDisplay.SetData(new[] { _colour });
            }
            else
            {
                Vector2 fontSize = _font.MeasureString(_text);
            }
        }

        public void Draw(SpriteBatch spriteBatch, Rectangle viewPort)
        {
            if (string.IsNullOrEmpty(_text))
            {
                spriteBatch.Draw(_colourDisplay, new Rectangle(_pos, _size), Color.White);
            }
            else
            {
                spriteBatch.DrawString(_font, _text, _textPos, _fontColour, 0f, new Vector2(0f, 0f), _fontSize, SpriteEffects.None, 0f);
            }   
        }
    }

    class SelectLabel : Label
    {
        public SelectLabel(Point position, Point size, string name, string text, float fontSize)
        {
            _pos = position;
            _size = size;
            _name = name;
            _text = text;
            _fontColour = Color.Black;
            _textPos = new Vector2(position.X + 36, position.Y + 16);
            _fontSize = fontSize;
            _status = Unselected;
        }

        
    }
}