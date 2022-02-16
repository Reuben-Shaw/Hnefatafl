using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections;
using System.Collections.Generic;
using Hnefatafl;
using static Hnefatafl.PieceType;


namespace Hnefatafl
{
    public sealed class Player
    {
        public Board _board;
        private bool _currentTurn;

        public Player(GraphicsDeviceManager graphics, ContentManager Content, int boardSize)
        {
            _board = new Board(graphics, Content, boardSize);
        }
    }
}