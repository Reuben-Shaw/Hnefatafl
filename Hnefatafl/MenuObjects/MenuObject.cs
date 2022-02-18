using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hnefatafl
{
    abstract class MenuObject
    {
        public enum Status { Selected, Unselected, Hovering }
        
        protected Status m_status;
        public virtual Status _status
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

        public Point _pos { get; set; }
        public Point _size { get; set; }

        protected string m_text;
        public string _text 
        { 
            get
            {
                return m_text;
            }
            protected set
            {
                m_text = value;
            }
        }
        protected Vector2 _textPos { get; set; }
        protected SpriteFont _font;
        protected Texture2D _backColour { get; set; }
        protected Color _fontColour { get; set; }
    }
}