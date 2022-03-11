using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Hnefatafl.Media;
using static Hnefatafl.MenuObjects.MenuObject.Status;
using System;

namespace Hnefatafl.MenuObjects
{
    sealed class Button : MenuObject
    {
        private Texture2D _selectBackColour { get; set; }
        private Color _selectFontColour { get; set; }
        private Texture2D _image { get; set; }

        public override Status _status
        {
            get
            {
                return m_status;
            }
            set
            {
                m_status = value;
            }
        }

        private Texture2D _disabledColour { get; set; }

        public Button(Point position, Point size, string name)
        {
            _pos = position;
            _size = size;
            _status = Unselected;
            _name = name;
            _text = name;
            _fontColour = Color.Black;
            _selectFontColour = Color.Blue;
            _textPos = new Vector2(-1, -1);
        }

        public Button(Point position, Point size, string text, string name)
        {
            _pos = position;
            _size = size;
            _status = Unselected;
            _name = name;
            if (text is null)
                _text = name;
            else
                _text = text;
            _fontColour = Color.Black;
            _selectFontColour = Color.Blue;
            _textPos = new Vector2(-1, -1);
        }

        public Button(Point position, Point size, Texture2D image, string name)
        {
            _pos = position;
            _size = size;
            _status = Unselected;
            _name = name;
            _text = "";
            _fontColour = Color.Black;
            _selectFontColour = Color.Blue;
            _textPos = new Vector2(-1, -1);
            _image = image;
        }

        public void Update(GraphicsDeviceManager graphics, ContentManager Content)
        {
            _font = Content.Load<SpriteFont>("Texture/Font/PixelFont");
            Vector2 fontSize = _font.MeasureString(_text);
            fontSize.X *= 1.5f;
            fontSize.Y *= 1.5f;
            _textPos = new Vector2((int)((float)(_size.X / 2f) - (fontSize.X / 2f)) + _pos.X, (int)((float)(_size.Y / 2f) - (fontSize.Y / 2f)) + _pos.Y);
            //Console.WriteLine($"{_textPos}, {_size}, {fontSize}");
            CreateTextures(graphics, Color.DarkGray, Color.Gray);
        }

        private void CreateTextures(GraphicsDeviceManager graphics, Color selectColour, Color regularColour)
        {
            _selectBackColour = new Texture2D(graphics.GraphicsDevice, 1, 1);
            _selectBackColour.SetData(new[] { selectColour });

            _backColour = new Texture2D(graphics.GraphicsDevice, 1, 1);
            _backColour.SetData(new[] { regularColour });

            _disabledColour = new Texture2D(graphics.GraphicsDevice, 1, 1);
            _disabledColour.SetData(new[] { new Color(20, 20, 20, 128) });
        }

        public void Draw(SpriteBatch spriteBatch, int tileSizeX, int tileSizeY, TextureDivide displaySelect, TextureDivide displayUnselect, Rectangle viewPort)
        {
            Rectangle rect = new Rectangle(_pos, _size);
            
            if (_image is null)
            {
                if (_status == Unselected || _status == Disabled)
                {
                    displayUnselect.Draw(spriteBatch, tileSizeX, tileSizeY, rect);
                    spriteBatch.DrawString(_font, _text, _textPos, _fontColour, 0f, new Vector2(0f, 0f), 1.5f, SpriteEffects.None, 0f);
                }
                else if (_status == Selected)
                {
                    displaySelect.Draw(spriteBatch, tileSizeX, tileSizeY, rect);
                    spriteBatch.DrawString(_font, _text, _textPos, _selectFontColour, 0f, new Vector2(0f, 0f), 1.5f, SpriteEffects.None, 0f);
                }
            }
            else
            {
                spriteBatch.Draw(_selectBackColour, new Rectangle(_pos.X - 4, _pos.Y - 4, _size.X + 8, _size.Y + 8), Color.White);
                spriteBatch.Draw(_image, rect, Color.White);
            }

            if (_status == Disabled)
            {
                spriteBatch.Draw(_disabledColour, new Rectangle(_pos.X - 4, _pos.Y - 4, _size.X + 8, _size.Y + 8), Color.White);
            }
        }
    }
}