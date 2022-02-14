using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Hnefatafl
{
    public class Pawn
    {
        private int _textInd;
        public int textInd
        {
             get
            {
                return _textInd;
            }
            set
            {
                _textInd = value;
            }
        }

        public Pawn(int _textInd)
        {
            textInd = _textInd;
        }
    }
}