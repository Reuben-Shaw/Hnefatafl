using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;

namespace Hnefatafl.Media
{
    sealed class BoardAudio
    {
        private SoundEffect m_death;
        public SoundEffect _death 
        { 
            get
            {
                return m_death;
            }
            private set
            {
                m_death = value;
            }
        }

        private SoundEffect m_move;
        public SoundEffect _move 
        { 
            get
            {
                return m_move;
            }
            private set
            {
                m_move = value;
            }
        }

        private SoundEffect m_buttonPress;
        public SoundEffect _buttonPress 
        { 
            get
            {
                return m_buttonPress;
            }
            private set
            {
                m_buttonPress = value;
            }
        }
        private SoundEffect m_buttonUp;
        public SoundEffect _buttonUp 
        { 
            get
            {
                return m_buttonUp;
            }
            private set
            {
                m_buttonUp = value;
            }
        }

        public BoardAudio(ContentManager Content)
        {
            _death = Content.Load<SoundEffect>("Audio/Death");
            _move = Content.Load<SoundEffect>("Audio/Move");
            _buttonPress = Content.Load<SoundEffect>("Audio/ButtonPress");
            _buttonUp = Content.Load<SoundEffect>("Audio/ButtonUp");
        }
    }
}