using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Hnefatafl
{
    public sealed class Pawn
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

        private PieceType _defender;
        public PieceType defender
        {
            get
            {
                return _defender;
            }
            set
            {
                _defender = value;
            }
        }

        public Pawn(int _textInd, PieceType _defender)
        {
            textInd = _textInd;
            defender = _defender;
        }
    }
}