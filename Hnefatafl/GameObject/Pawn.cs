using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using static Hnefatafl.PieceType;


namespace Hnefatafl
{
    public enum PieceType { Empty = -1, Attacker = 0, Defender = 1, King = 2 };
    public sealed class Pawn
    {
        public PieceType _type { get; set; }

        public Pawn(PieceType type)
        {
            _type = type;
        }
    }
    
    public sealed class Piece
    {
        public Pawn _pawn { get; set; }
        public Point _loc  { get; set; }

        public Piece(Pawn pawn, Point loc)
        {
            _pawn = pawn;
            _loc = loc;
        }

        public Piece(Pawn pawn)
        {
            _pawn = pawn;
            _loc = new Point(-1, -1);
        }
    }

    public sealed class Pieces
    {
        private Hashtable _pieceBoard;

        public Pieces()
        {
            _pieceBoard = new Hashtable();
        }

        public void AddTo(Piece newPiece)
        {
            _pieceBoard.Add(newPiece._loc.X.ToString() + newPiece._loc.Y.ToString(), newPiece);
        }

        public int Count()
        {
            return _pieceBoard.Count;
        }
        
        public Piece GetPiece(string key)
        {
            if (_pieceBoard.ContainsKey(key))
                return _pieceBoard[key] as Piece;
            else
                return new Piece(new Pawn(Empty));
        }

        public Hashtable AllPieces()
        {
            return _pieceBoard;
        }

        public void CreateBoard(int boardSize, BoardTypes type)
        {
            if (type == BoardTypes.Regular)
            {
                for (int y = 0; y < boardSize; y++)
                {
                    for (int x = 0; x < boardSize; x++)
                    {
                        if (x > 2 && x < 8)
                        {
                            if (x == boardSize / 2 && y == boardSize / 2) {
                                _pieceBoard.Add(x.ToString() + y.ToString(), new Piece(new Pawn(King), new Point(x, y))); }
                            else if (y == 0 || y == boardSize - 1) {
                                _pieceBoard.Add(x.ToString() + y.ToString(), new Piece(new Pawn(Attacker), new Point(x, y))); }
                            else if ((y > 3 && y < boardSize - 4) && (x > 3 && x < 7)){
                                _pieceBoard.Add(x.ToString() + y.ToString(), new Piece(new Pawn(Defender), new Point(x, y))); }
                            else if ((y == 1 || y == boardSize - 2) && x == boardSize / 2) {
                                _pieceBoard.Add(x.ToString() + y.ToString(), new Piece(new Pawn(Attacker), new Point(x, y))); }
                            else if ((y == 3 || y == boardSize - 4) && x == boardSize / 2) {
                                _pieceBoard.Add(x.ToString() + y.ToString(), new Piece(new Pawn(Defender), new Point(x, y))); }
                            else if (y == boardSize / 2 && (x == 3 || x == boardSize - 4)) {
                                _pieceBoard.Add(x.ToString() + y.ToString(), new Piece(new Pawn(Defender), new Point(x, y))); }
                        }
                        else if (y > 2 && y < 8)
                        {
                            if (x == 0 || x == boardSize - 1) {
                                _pieceBoard.Add(x.ToString() + y.ToString(), new Piece(new Pawn(Attacker), new Point(x, y))); }
                            else if ((x == 1 || x == boardSize - 2) && y == boardSize / 2) {
                                _pieceBoard.Add(x.ToString() + y.ToString(), new Piece(new Pawn(Attacker), new Point(x, y))); }
                        }
                    }
                }
            }
        }
    }

}