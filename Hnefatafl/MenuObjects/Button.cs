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
        
        private TextureDivide _buttonSelect, _buttonUnselect;
        
        private TextureDivide _tabButtonTexture { get; set; }
        private ContentManager Content { get; set; }

        public Button() {}
        public Button(Button button) //Unfortunately required for the editor, kind of weird but it fixes the bug I was having about assingments not working as expected, so....
        {
            Content = button.Content;
            _pos = button._pos;
            _size = button._size;
            _status = button._status;
            _name = button._name;
            _text = button._text;
            _font = button._font;
            _fontColour = button._fontColour;
            _selectFontColour = button._selectFontColour;
            _backColour = button._backColour;
            _textPos = button._textPos;
            _image = button._image;
            _disabledColour = button._disabledColour;
            _tabButtonTexture = button._tabButtonTexture;
        }

        public Button(Point position, Point size, float fontMod, string name, GraphicsDeviceManager graphics, ContentManager content)
        {
            Content = new ContentManager(content.ServiceProvider, content.RootDirectory);
            _font = content.Load<SpriteFont>("Texture/Font/PixelFont");
            _pos = position;
            _size = size;
            _status = Unselected;
            _name = name;
            _text = name;
            _fontColour = Color.Black;
            _selectFontColour = Color.Blue;
            _textPos = new Vector2(-1, -1);
            SetGraphics(graphics, fontMod, null);
        }

        public Button(Point position, Point size, string name, Texture2D image, GraphicsDeviceManager graphics, ContentManager content)
        {
            Content = new ContentManager(content.ServiceProvider, content.RootDirectory);
            _pos = position;
            _size = size;
            _status = Unselected;
            _name = name;
            _image = image;
            _fontColour = Color.Black;
            _selectFontColour = Color.Blue;
            _textPos = new Vector2(-1, -1);
            SetGraphics(graphics, 1f, null);
        }

        public Button(Point position, Point size, Color[] textColours, Color[] backColours, float fontMod, string text, string name, GraphicsDeviceManager graphics, ContentManager content)
        {
            Content = new ContentManager(content.ServiceProvider, content.RootDirectory);
            _font = content.Load<SpriteFont>("Texture/Font/PixelFont");
            _pos = position;
            _size = size;
            _status = Unselected;
            _name = name;
            if (string.IsNullOrWhiteSpace(text))
                _text = name;
            else
                _text = text;
            _fontColour = textColours[0];
            _selectFontColour = textColours[1];
            _textPos = new Vector2(-1, -1);
            SetGraphics(graphics, fontMod, backColours);
        }

        public Button(Point position, Point size, TextureDivide tabButton, float fontMod, string text, string name, GraphicsDeviceManager graphics, ContentManager content)
        {
            Content = new ContentManager(content.ServiceProvider, content.RootDirectory);
            _font = content.Load<SpriteFont>("Texture/Font/PixelFont");
            _pos = position;
            _size = size;
            _status = Unselected;
            _name = name;
            if (string.IsNullOrWhiteSpace(text))
                _text = name;
            else
                _text = text;
            _fontColour = Color.Black;
            _selectFontColour = Color.Blue;
            _textPos = new Vector2(-1, -1);
            _tabButtonTexture = tabButton;
            SetGraphics(graphics, fontMod, null);
        }

        public void SetGraphics(GraphicsDeviceManager graphics, float fontMod, Color[] backColours)
        {
            if (_image is null)
            {    
                Content.Dispose();
                Vector2 fontSize = _font.MeasureString(_text);
                fontSize.X *= fontMod;
                fontSize.Y *= fontMod;
                if (_tabButtonTexture is null)
                {
                    _textPos = new Vector2((int)((float)(_size.X / 2f) - (fontSize.X / 2f)) + _pos.X, (int)((float)(_size.Y / 2f) - (fontSize.Y / 2f)) + _pos.Y);
                }
                else
                {
                    _textPos = new Vector2((int)((float)(_size.X / 2f) - (fontSize.X / 2f)) + _pos.X, (int)((float)(_size.Y / 2f) - (fontSize.Y / 1.5f)) + _pos.Y);
                }
                //Console.WriteLine($"{_textPos}, {_size}, {fontSize}");
            }
            _buttonUnselect = new TextureDivide(graphics, Content, "Texture/Menu/ButtonDivideUnselect", 32, 32);
            _buttonSelect = new TextureDivide(graphics, Content, "Texture/Menu/ButtonDivideSelect", 32, 32);
            if (backColours == null) 
            {
                _buttonUnselect.HueShiftTexture(Color.Gray);
                _buttonSelect.HueShiftTexture(Color.DarkGray);
            }
            else
            {
                _buttonUnselect.HueShiftTexture(backColours[0]);
                _buttonSelect.HueShiftTexture(backColours[1]);
            }
        }


        public void Draw(SpriteBatch spriteBatch, float fontSize, int tileSizeX, int tileSizeY, Rectangle viewPort)
        {
            Rectangle rect = new Rectangle(_pos, _size);
            
            if (_image is not null)
            {
                //spriteBatch.Draw(_selectBackColour, new Rectangle(_pos.X - 4, _pos.Y - 4, _size.X + 8, _size.Y + 8), Color.White);
                spriteBatch.Draw(_image, rect, Color.White);
            }
            else if (_tabButtonTexture is not null)
            {
                _tabButtonTexture.Draw(spriteBatch, rect);
                spriteBatch.DrawString(_font, _text, _textPos, _fontColour, 0f, new Vector2(0f, 0f), fontSize, SpriteEffects.None, 0f);
            }
            else
            {
                if (_status == Unselected || _status == Disabled)
                {
                    _buttonUnselect.Draw(spriteBatch, rect);
                    spriteBatch.DrawString(_font, _text, new Vector2(_textPos.X, _textPos.Y - (tileSizeY / 4)), _fontColour, 0f, new Vector2(0f, 0f), fontSize, SpriteEffects.None, 0f);
                }
                else if (_status == Selected)
                {
                    _buttonSelect.Draw(spriteBatch, rect);
                    spriteBatch.DrawString(_font, _text, _textPos, _selectFontColour, 0f, new Vector2(0f, 0f), fontSize, SpriteEffects.None, 0f);
                }
            }

            if (_status == Disabled)
            {
                spriteBatch.Draw(_disabledColour, new Rectangle(_pos.X - 4, _pos.Y - 4, _size.X + 8, _size.Y + 8), Color.White);
            }
        }

        public override string ToString()
        {
            return ($"Positon: {_pos}\nSize: {_size}\nName: {_name}");
        }
    }
}