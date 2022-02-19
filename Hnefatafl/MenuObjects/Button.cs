using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using static Hnefatafl.MenuObject.Status;

namespace Hnefatafl
{
    sealed class Button : MenuObject
    {
        private Texture2D _selectBackColour { get; set; }
        private Color _selectFontColour { get; set; }

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

        public Button(Point position, Point size)
        {
            _pos = position;
            _size = size;
            _status = Unselected;
            _text = "";
            _fontColour = Color.Black;
            _selectFontColour = Color.Blue;
            _textPos = new Vector2(-1, -1);
        }

        public Button(Point position, Point size, string text)
        {
            _pos = position;
            _size = size;
            _status = Unselected;
            _text = text;
            _fontColour = Color.Black;
            _selectFontColour = Color.Blue;
            _textPos = new Vector2(-1, -1);
        }

        public void Update(GraphicsDeviceManager graphics, ContentManager Content)
        {
            _font = Content.Load<SpriteFont>("PixelFont");
            Vector2 fontSize = _font.MeasureString(_text);
            //fontSize = new Vector2(fontSize.X / 2, fontSize.Y / 2);
            _textPos = new Vector2((int)((_size.X - fontSize.X) / 2) + _pos.X, (int)((_size.Y - fontSize.Y) / 2) + _pos.Y);
            CeateTextures(graphics, Color.DarkGray, Color.Gray);
        }

        private void CeateTextures(GraphicsDeviceManager graphics, Color selectColour, Color regularColour)
        {
            _selectBackColour = new Texture2D(graphics.GraphicsDevice, 1, 1);
            _selectBackColour.SetData(new[] { selectColour });

            _backColour = new Texture2D(graphics.GraphicsDevice, 1, 1);
            _backColour.SetData(new[] { regularColour });
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, Rectangle viewPort)
        {
            Rectangle rect= new Rectangle(_pos, _size);
            
            if (_status == Unselected)
            {
                spriteBatch.Draw(_backColour, rect, Color.White);
                //spriteBatch.DrawString(_font, _text, _textPos, _fontColour);
                spriteBatch.DrawString(_font, _text, _textPos, _fontColour, 0, new Vector2(0, 0), new Vector2(1f, 1f), SpriteEffects.None, 0);
            }
            else if (_status == Selected)
            {
                spriteBatch.Draw(_selectBackColour, rect, Color.White);
                //spriteBatch.DrawString(_font, _text, _textPos, _selectFontColour);
                spriteBatch.DrawString(_font, _text, _textPos, _selectFontColour, 0, new Vector2(0, 0), new Vector2(1f, 1f), SpriteEffects.None, 0);
            }
        }
    }
}