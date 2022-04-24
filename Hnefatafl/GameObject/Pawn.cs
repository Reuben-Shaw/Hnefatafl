using System.Collections;
using System.Collections.Generic;
using System;
using static Hnefatafl.PieceType;

namespace Hnefatafl
{
    public enum PieceType { Empty = -1, Attacker = 0, Defender = 1, King = 2, Throne = -2, Corner = -3 };
    public struct Pawn
    {
        public PieceType _type { get; set; }

        public Pawn(PieceType type)
        {
            _type = type;
        }

        public override string ToString()
        {
            switch (_type)
            {
                case Attacker:
                    return "Attacker";
                case Defender:
                    return "Defender";
                case King:
                    return "King";
                default:
                    return "Empty";
            }
        }
    }
    
    [Serializable()]
    public sealed class Piece
    {
        public Pawn _pawn { get; set; }
        public HPoint _loc  { get; set; }

        public Piece() { } //For serialising

        public Piece(Pawn pawn, HPoint loc)
        {
            _pawn = pawn;
            _loc = loc;
        }

        public Piece(Pawn pawn)
        {
            _pawn = pawn;
            _loc = new HPoint(-1, -1);
        }

        public override string ToString()
        {
            return _pawn + " " + _loc.ToString();
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
            _pieceBoard.Add(newPiece._loc.ToString(), newPiece);
        }

        public void Replace(Piece replacePiece)
        {
            _pieceBoard[replacePiece._loc.ToString()] = replacePiece;
        }

        public void RemoveFrom(string key)
        {
            if (_pieceBoard.ContainsKey(key))
                _pieceBoard.Remove(key);
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

        public bool Exists(string key)
        {
            return _pieceBoard.ContainsKey(key);
        }

        public bool Changed(List<Piece> pieces)
        {
            foreach (Piece piece in pieces)
            {
                if (!_pieceBoard.Contains(piece._loc.ToString()) || _pieceBoard[piece._loc.ToString()] == piece)
                {
                    return true;
                }
            }

            return false;
        }

        public void CreateBoard(List<Piece> pieces)
        {
            _pieceBoard.Clear();
            foreach (Piece piece in pieces)
            {
                _pieceBoard.Add(piece._loc.ToString(), piece);
            }
        }

        public void CreateBoard(BoardTypes type)
        {
            _pieceBoard.Clear();
            int boardSize = Hnefatafl.ToInt(type);

            if (type == BoardTypes.Hnefatafl)
            {
                HPoint point;

                for (int y = 0; y < boardSize; y++)
                {
                    for (int x = 0; x < boardSize; x++)
                    {
                        point = new HPoint(x, y);

                        if ((x == 0 || x == boardSize - 1) && (y == 0 || y == boardSize - 1))
                        {
                            _pieceBoard.Add(point.ToString(), new Piece(new Pawn(Corner), point)); 
                        }
                        else if (x > 2 && x < 8)
                        {
                            if (x == boardSize / 2 && y == boardSize / 2) {
                                _pieceBoard.Add(point.ToString(), new Piece(new Pawn(King), point)); }
                            else if (y == 0 || y == boardSize - 1) {
                                _pieceBoard.Add(point.ToString(), new Piece(new Pawn(Attacker), point)); }
                            else if ((y > 3 && y < boardSize - 4) && (x > 3 && x < boardSize - 4)) {
                                _pieceBoard.Add(point.ToString(), new Piece(new Pawn(Defender), point)); }
                            else if ((y == 1 || y == boardSize - 2) && x == boardSize / 2) {
                                _pieceBoard.Add(point.ToString(), new Piece(new Pawn(Attacker), point)); }
                            else if ((y == 3 || y == boardSize - 4) && x == boardSize / 2) {
                                _pieceBoard.Add(point.ToString(), new Piece(new Pawn(Defender), point)); }
                            else if (y == boardSize / 2 && (x == 3 || x == boardSize - 4)) {
                                _pieceBoard.Add(point.ToString(), new Piece(new Pawn(Defender), point)); }
                        }
                        else if (y > 2 && y < boardSize - 3)
                        {
                            if (x == 0 || x == boardSize - 1) {
                                _pieceBoard.Add(point.ToString(), new Piece(new Pawn(Attacker), point)); }
                            else if ((x == 1 || x == boardSize - 2) && y == boardSize / 2) {
                                _pieceBoard.Add(point.ToString(), new Piece(new Pawn(Attacker), point)); }
                        }
                    }
                }
            }
            else if (type == BoardTypes.Tawlbwrdd)
            {
                HPoint point;

                for (int y = 0; y < boardSize; y++)
                {
                    for (int x = 0; x < boardSize; x++)
                    {
                        point = new HPoint(x, y);

                        if ((x == 0 || x == boardSize - 1) && (y == 0 || y == boardSize - 1))
                        {
                            _pieceBoard.Add(point.ToString(), new Piece(new Pawn(Corner), point)); 
                        }
                        else if (x > 2 && x < 8)
                        {
                            if (x == boardSize / 2 && y == boardSize / 2) {
                                _pieceBoard.Add(point.ToString(), new Piece(new Pawn(King), point)); }
                            else if ((y == 0 || y == boardSize - 1) && (x > 3 && x < boardSize - 4)) {
                                _pieceBoard.Add(point.ToString(), new Piece(new Pawn(Attacker), point)); }
                            else if ((y > 3 && y < boardSize - 4) && (x > 3 && x < boardSize - 4)) {
                                _pieceBoard.Add(point.ToString(), new Piece(new Pawn(Defender), point)); }
                            else if ((y == 2 || y == boardSize - 3) && x == boardSize / 2) {
                                _pieceBoard.Add(point.ToString(), new Piece(new Pawn(Attacker), point)); }
                            else if (x != boardSize / 2 && (y == 1 || y == boardSize - 2) && (x != 3 && x != boardSize - 4)) {
                                _pieceBoard.Add(point.ToString(), new Piece(new Pawn(Attacker), point)); }
                            else if ((y == 3 || y == boardSize - 4) && x == boardSize / 2) {
                                _pieceBoard.Add(point.ToString(), new Piece(new Pawn(Defender), point)); }
                            else if (y == boardSize / 2 && (x == 3 || x == boardSize - 4)) {
                                _pieceBoard.Add(point.ToString(), new Piece(new Pawn(Defender), point)); }
                        }
                        else if (y > 3 && y < boardSize - 4)
                        {
                            if (x == 0 || x == boardSize - 1) {
                                _pieceBoard.Add(point.ToString(), new Piece(new Pawn(Attacker), point)); }
                            else if (y != boardSize / 2 && (x == 1 || x == boardSize - 2)) {
                                _pieceBoard.Add(point.ToString(), new Piece(new Pawn(Attacker), point)); }
                            else if ((x == 2 || x == boardSize - 3) && y == boardSize / 2) {
                                _pieceBoard.Add(point.ToString(), new Piece(new Pawn(Attacker), point)); }
                            
                        }
                    }
                }
            }
            else if (type == BoardTypes.Tablut)
            {
                HPoint point;

                for (int y = 0; y < boardSize; y++)
                {
                    for (int x = 0; x < boardSize; x++)
                    {
                        point = new HPoint(x, y);

                        if ((x == 0 || x == boardSize - 1) && (y == 0 || y == boardSize - 1))
                        {
                            _pieceBoard.Add(point.ToString(), new Piece(new Pawn(Corner), point)); 
                        }
                        else if (x >= 2 && x <= boardSize - 3)
                        {
                            if (x == boardSize / 2 && y == boardSize / 2) {
                                _pieceBoard.Add(point.ToString(), new Piece(new Pawn(King), point)); }
                            else if ((y == 0 || y == boardSize - 1) && (x > 2 && x < boardSize - 3)) {
                                _pieceBoard.Add(point.ToString(), new Piece(new Pawn(Attacker), point)); }
                            else if ((y == 1 || y == boardSize - 2) && x == boardSize / 2) {
                                _pieceBoard.Add(point.ToString(), new Piece(new Pawn(Attacker), point)); }
                            else if (y == boardSize / 2) {
                                _pieceBoard.Add(point.ToString(), new Piece(new Pawn(Defender), point)); }
                            else if (x == boardSize / 2) {
                                _pieceBoard.Add(point.ToString(), new Piece(new Pawn(Defender), point)); }
                        }
                        else if (y > 2 && y < boardSize - 3)
                        {
                            if (x == 0 || x == boardSize - 1) {
                                _pieceBoard.Add(point.ToString(), new Piece(new Pawn(Attacker), point)); }
                            else if ((x == 1 || x == boardSize - 2) && y == boardSize / 2) {
                                _pieceBoard.Add(point.ToString(), new Piece(new Pawn(Attacker), point)); }
                        }
                    }
                }
            }
            else if (type == BoardTypes.Brandubh)
            {
                HPoint point;

                for (int y = 0; y < boardSize; y++)
                {
                    for (int x = 0; x < boardSize; x++)
                    {
                        point = new HPoint(x, y);

                        if ((x == 0 || x == boardSize - 1) && (y == 0 || y == boardSize - 1))
                        {
                            _pieceBoard.Add(point.ToString(), new Piece(new Pawn(Corner), point)); 
                        }
                        else if (x >= 2 && x <= boardSize - 3)
                        {
                            if (x == boardSize / 2 && y == boardSize / 2) {
                                _pieceBoard.Add(point.ToString(), new Piece(new Pawn(King), point)); }
                            else if ((y == 0 || y == boardSize - 1) && (x > 1 && x < boardSize - 2)) {
                                _pieceBoard.Add(point.ToString(), new Piece(new Pawn(Attacker), point)); }
                            else if ((y == 1 || y == boardSize - 2) && x == boardSize / 2) {
                                _pieceBoard.Add(point.ToString(), new Piece(new Pawn(Attacker), point)); }
                            else if (y == boardSize / 2) {
                                _pieceBoard.Add(point.ToString(), new Piece(new Pawn(Defender), point)); }
                            else if (x == boardSize / 2) {
                                _pieceBoard.Add(point.ToString(), new Piece(new Pawn(Defender), point)); }
                        }
                        else if (y >= 2 && y <= boardSize - 3)
                        {
                            if (x == 0 || x == boardSize - 1) {
                                _pieceBoard.Add(point.ToString(), new Piece(new Pawn(Attacker), point)); }
                            else if ((x == 1 || x == boardSize - 2) && y == boardSize / 2) {
                                _pieceBoard.Add(point.ToString(), new Piece(new Pawn(Attacker), point)); }
                        }
                    }
                }
            }
            else if (type == BoardTypes.ArdRi)
            {
                HPoint point;

                for (int y = 0; y < boardSize; y++)
                {
                    for (int x = 0; x < boardSize; x++)
                    {
                        point = new HPoint(x, y);

                        if ((x == 0 || x == boardSize - 1) && (y == 0 || y == boardSize - 1))
                        {
                            _pieceBoard.Add(point.ToString(), new Piece(new Pawn(Corner), point)); 
                        }
                        else if (x >= 2 && x <= boardSize - 3)
                        {
                            if (x == boardSize / 2 && y == boardSize / 2) {
                                _pieceBoard.Add(point.ToString(), new Piece(new Pawn(King), point)); }
                            else if ((y == 0 || y == boardSize - 1) && (x > 1 && x < boardSize - 2)) {
                                _pieceBoard.Add(point.ToString(), new Piece(new Pawn(Attacker), point)); }
                            else if ((y == 1 || y == boardSize - 2) && x == boardSize / 2) {
                                _pieceBoard.Add(point.ToString(), new Piece(new Pawn(Attacker), point)); }
                            else if (((y == boardSize / 2) || (x > 1 && x < boardSize - 2)) && y != 1 && y != boardSize - 2) {
                                _pieceBoard.Add(point.ToString(), new Piece(new Pawn(Defender), point)); }
                            else if (x == boardSize / 2) {
                                _pieceBoard.Add(point.ToString(), new Piece(new Pawn(Defender), point)); }
                        }
                        else if (y >= 2 && y <= boardSize - 3)
                        {
                            if (x == 0 || x == boardSize - 1) {
                                _pieceBoard.Add(point.ToString(), new Piece(new Pawn(Attacker), point)); }
                            else if ((x == 1 || x == boardSize - 2) && y == boardSize / 2) {
                                _pieceBoard.Add(point.ToString(), new Piece(new Pawn(Attacker), point)); }
                        }
                    }
                }
            }
        }

        public override string ToString()
        {
            return $"There is {_pieceBoard.Count} on the board";
        }
    }
}