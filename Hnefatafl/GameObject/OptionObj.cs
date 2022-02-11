using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Hnefatafl
{
    public sealed class OptionObj
    {
        private Color[] _boardColour;
        public Color[] boardColour
        {
             get
            {
                return _boardColour;
            }
            set
            {
                _boardColour = value;
            }
        }

        public OptionObj(Color[] _boardColour)
        {
            boardColour = _boardColour;
        }
    }
}