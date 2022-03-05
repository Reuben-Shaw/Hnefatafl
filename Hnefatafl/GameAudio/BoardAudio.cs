using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;
using System;

namespace Hnefatafl.Audio
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

        public BoardAudio(ContentManager Content)
        {
            _death = Content.Load<SoundEffect>("Audio/Death");
            _move = Content.Load<SoundEffect>("Audio/Move");
        }
    }
}