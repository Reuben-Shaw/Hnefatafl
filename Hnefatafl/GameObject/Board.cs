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
    public enum BoardTypes { Regular }
    sealed class Board
    {
        private readonly Texture2D[] _boardColours = new Texture2D[6];
        //0: Board main colour 1, 1: Board main colour 2, 2: Defender board colour, 3: Attacker board colour, 4: Throne, 5: Corner
        private readonly Texture2D[] _pawnTexture = new Texture2D[3];
        private Pieces _pieces = new Pieces();
        public int _boardSize;
        private HPoint _selectedPiece = new HPoint(-1, -1);

        public Board(GraphicsDeviceManager graphics, ContentManager Content, Color[] colours, int boardSize, BoardTypes boardType)
        {
            CreateColours(graphics, colours);
            CreatePawns(graphics, Content);
            _pieces.CreateBoard(boardSize, boardType);
            _boardSize = boardSize;
        }

        public Board(GraphicsDeviceManager graphics, ContentManager Content, int boardSize)
        {
            CreateColours(graphics, new Color[]{new Color(173, 99, 63), new Color(80, 53, 30), new Color(0, 0, 0), new Color(0, 0, 0), new Color(175, 0, 0), new Color(249, 200, 24)});
            CreatePawns(graphics, Content);
            _pieces.CreateBoard(boardSize, BoardTypes.Regular);
            _boardSize = boardSize;
        }

        private void CreateColours(GraphicsDeviceManager graphics, Color[] colours)
        {
            for (int i = 0; i < _boardColours.Length; i++)
            {
                _boardColours[i] = new Texture2D(graphics.GraphicsDevice, 1, 1);
                _boardColours[i].SetData(new[] { colours[i] });
            }
        }

        private void CreatePawns(GraphicsDeviceManager graphics, ContentManager Content)
        {
            _pawnTexture[0] = Content.Load<Texture2D>("pawnE");
            _pawnTexture[1] = Content.Load<Texture2D>("pawnD");
            _pawnTexture[2] = Content.Load<Texture2D>("king");

            Color[] userColour = new Color[]{ new Color(255, 0, 0), new Color(0, 0, 255),  new Color(0, 0, 255) };
            Color[] data;


            for (int i = 0; i < _pawnTexture.Length; i++)
            {
                data = new Color[_pawnTexture[i].Width * _pawnTexture[i].Height];
                _pawnTexture[i].GetData<Color>(data);

                for (int j = 0; j < data.Length; j++)
                {
                    if (data[j] != Color.Transparent && data[j] != Color.Black)
                    {
                        data[j] = new Color((byte)(data[j].R * (userColour[i].R / 255)), (byte)(data[j].G * (userColour[i].G / 255)), (byte)(data[j].B  * (userColour[i].B / 255)));
                    }
                }

                _pawnTexture[i].SetData<Color>(data);
            }
        }

        public void UnloadContent()
        {
            for (int i = 0; i < _boardColours.Length; i++)
            {
                _boardColours[i].Dispose();
            }

            for (int i = 0; i < _pawnTexture.Length; i++)
            {
                _pawnTexture[i].Dispose();
            }
        }

        private bool DefendersRemain()
        {
            foreach (DictionaryEntry s in _pieces.AllPieces())
            {
                Piece piece = s.Value as Piece;
                if (piece._pawn._type == Defender)
                    return true;
            }
            return false;
        }

        private bool AttackersRemain()
        {
            foreach (DictionaryEntry s in _pieces.AllPieces())
            {
                Piece piece = s.Value as Piece;
                if (piece._pawn._type == Attacker)
                    return true;
            }
            return false;
        }

        public void SelectPiece(HPoint select)
        {
            if (_pieces.GetPiece(select.ToString())._pawn._type != Empty)
            {
                _selectedPiece = select;
            }
            else
            {
                _selectedPiece = new HPoint(-1, -1);
            }
        }

        public bool IsPieceSelected()
        {
            if (_selectedPiece.X != -1)
                return true;
            else
                return false;
        }

        public bool MakeMove(HPoint move, Player.SideType side, bool doOverride)
        {
            string key = move.ToString();

            if ((move.X >= 0 && move.X < _boardSize) && (move.Y >= 0 && move.Y < _boardSize))
            {
                if (doOverride || PlayerSidePiece(_pieces.GetPiece(_selectedPiece.ToString())._pawn._type, side))
                {
                    if ((move.X == _selectedPiece.X || move.Y == _selectedPiece.Y) && ClearanceCheck(move.X, move.Y))
                    {

                        _pieces.AddTo(new Piece(_pieces.GetPiece(_selectedPiece.ToString())._pawn, move));
                        _pieces.GetPiece(key)._loc = move;

                        int b = _boardSize / 2;
                        if (_selectedPiece.ToString() != b + "," + b)
                        {
                            _pieces.RemoveFrom(_selectedPiece.ToString());
                        }
                        else
                        {
                            _pieces.Replace(new Piece(new Pawn(Throne), new HPoint(_selectedPiece.ToString())));
                        }

                        CaptureLogic(move);
                        
                        _selectedPiece = new HPoint(-1, -1);
                        return true;
                    }
                    else if ((_pieces.GetPiece(key)._pawn._type == Throne || _pieces.GetPiece(key)._pawn._type == Corner) && _pieces.GetPiece(_selectedPiece.ToString())._pawn._type == King)
                    {
                        _pieces.Replace(new Piece(new Pawn(King), new HPoint(key)));
                        _pieces.RemoveFrom(_selectedPiece.ToString());

                        CaptureLogic(move);
                        
                        _selectedPiece = new HPoint(-1, -1);
                        return true;
                    }
                    else
                    {
                        _selectedPiece = new HPoint(-1, -1);
                        return false;
                    }
                }
                else
                {
                    _selectedPiece = new HPoint(-1, -1);
                    return false;
                }
            }
            else
            {
                _selectedPiece = new HPoint(-1, -1);
                return false;
            }
        }

        private bool PlayerSidePiece(PieceType pieceType, Player.SideType side)
        {
            if (side == Player.SideType.Attackers && pieceType == Attacker)
            {
                return true;
            }
            else if (side == Player.SideType.Defenders && (pieceType == Defender || pieceType == King))
            {
                return true;
            }
            return false;
        }

        private bool ClearanceCheck(int x, int y)
        {
            int start, end;

            if (x > _selectedPiece.X)
            { start = _selectedPiece.X + 1; end = x + 1; }
            else
            { start = x; end = _selectedPiece.X; }

            for (int i = start; i < end; i++)
            {
                if (_pieces.GetPiece(i + "," + y)._pawn._type != Empty)
                    return false;
            }
            
            if (y > _selectedPiece.Y)
            { start = _selectedPiece.Y + 1; end = y + 1; }
            else
            { start = y; end = _selectedPiece.Y; }

            for (int i = start; i < end; i++)
            {
                if (_pieces.GetPiece(x + "," + i)._pawn._type != Empty)
                    return false;
            }

            return true;
        }

        private void CaptureLogic(HPoint loc)
        {
            HPoint[] locChk = new HPoint[2] { loc, loc };
            PieceType pieceMoved = _pieces.GetPiece(loc.ToString())._pawn._type;


            locChk[0] = new HPoint(loc.X + 1, loc.Y); locChk[1] = new HPoint(loc.X + 2, loc.Y);
            CaptureAttempt(locChk[0], loc, pieceMoved, _pieces.GetPiece(locChk[0].ToString())._pawn._type, _pieces.GetPiece(locChk[1].ToString())._pawn._type);

            locChk[0] = new HPoint(loc.X - 1, loc.Y); locChk[1] = new HPoint(loc.X - 2, loc.Y);
            CaptureAttempt(locChk[0], loc, pieceMoved, _pieces.GetPiece(locChk[0].ToString())._pawn._type, _pieces.GetPiece(locChk[1].ToString())._pawn._type);

            locChk[0] = new HPoint(loc.X, loc.Y + 1); locChk[1] = new HPoint(loc.X, loc.Y + 2);
            CaptureAttempt(locChk[0], loc, pieceMoved, _pieces.GetPiece(locChk[0].ToString())._pawn._type, _pieces.GetPiece(locChk[1].ToString())._pawn._type);
            
            locChk[0] = new HPoint(loc.X, loc.Y - 1); locChk[1] = new HPoint(loc.X, loc.Y - 2);
            CaptureAttempt(locChk[0], loc, pieceMoved, _pieces.GetPiece(locChk[0].ToString())._pawn._type, _pieces.GetPiece(locChk[1].ToString())._pawn._type);
        }

        private void CaptureAttempt(HPoint locChk, HPoint loc, PieceType pieceMoved, PieceType pieceChk, PieceType pieceAhead)
        {
            if (pieceChk != King)
            {
                if (!SameSide(pieceMoved, pieceChk) && SameSide(pieceMoved, pieceAhead))
                {
                    _pieces.RemoveFrom(locChk.ToString());
                }
            }
            else
            {
                PieceType[] pieceAdditional;
                if (locChk.X == loc.X - 1 || locChk.X == loc.X + 1)
                {
                    pieceAdditional = new PieceType[]{_pieces.GetPiece(new HPoint(locChk.X, locChk.Y + 1).ToString())._pawn._type,
                     _pieces.GetPiece(new HPoint(locChk.X, locChk.Y - 1).ToString())._pawn._type, pieceMoved, pieceAhead};
                }
                else
                {
                    pieceAdditional = new PieceType[]{_pieces.GetPiece(new HPoint(locChk.X + 1, locChk.Y).ToString())._pawn._type,
                     _pieces.GetPiece(new HPoint(locChk.X - 1, locChk.Y).ToString())._pawn._type, pieceMoved, pieceAhead};
                }


                int surrounding = 0;
                for (int i = 0; i < pieceAdditional.Length; i++)
                {
                    if (pieceAdditional[i] != Empty && !SameSide(pieceChk, pieceAdditional[i]))
                    {
                        surrounding++;
                    }
                }
                if (surrounding == 4 || (surrounding == 3 && !DefendersRemain() && (locChk.X == 0 || locChk.Y == 0  || locChk.X == _boardSize - 1 || locChk.Y == _boardSize - 1)))
                {
                    _pieces.RemoveFrom(locChk.ToString());
                }
            }
        }

        private bool SameSide(PieceType pieceChk1, PieceType pieceChk2)
        {
            if (pieceChk1 == pieceChk2)
                return true;
            else if ((pieceChk1 == King || pieceChk2 == King) && (pieceChk1 == Defender || pieceChk2 == Defender))
                return true;
            else if (pieceChk1 == Corner || pieceChk1 == Throne || pieceChk2 == Corner || pieceChk2 == Throne)
                return true;
            return false;
        }

        public int TileSizeX(Rectangle viewPort)
        {
            return viewPort.Width / 30;
        }
        
        public int TileSizeY(Rectangle viewPort)
        {
            return viewPort.Width / 30;
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, Rectangle viewPort)
        {
            Rectangle rect= new Rectangle(
                        (viewPort.Width / 2) - ((TileSizeX(viewPort) * _boardSize) / 2), 
                        (viewPort.Height / 2) - ((TileSizeY(viewPort) * _boardSize) / 2), 
                        TileSizeX(viewPort), TileSizeY(viewPort));
            Piece iPiece;

            for (int y = 0; y < _boardSize; y++)
            {
                for (int x = 0; x < _boardSize; x++)
                {
                    if ((y == 0 || y == _boardSize - 1) && (x == 0 || x == _boardSize - 1))
                        spriteBatch.Draw(_boardColours[5], rect, Color.White);
                    else if (y == (_boardSize - 1) / 2 && x == (_boardSize - 1) / 2)
                        spriteBatch.Draw(_boardColours[4], rect, Color.White);
                    else if (y % 2 == 0)
                        spriteBatch.Draw(_boardColours[x % 2], rect, Color.White);
                    else if (x % 2 > 0)
                        spriteBatch.Draw(_boardColours[0], rect, Color.White);
                    else
                        spriteBatch.Draw(_boardColours[1], rect, Color.White);

                    iPiece = _pieces.GetPiece(x.ToString() + "," + y.ToString());
                    if ((int)iPiece._pawn._type > -1)
                    {
                        spriteBatch.Draw(_pawnTexture[(int)iPiece._pawn._type], rect, Color.White);
                    }

                    rect.X += TileSizeX(viewPort);
                }
                rect.Y +=TileSizeY(viewPort);
                rect.X = (viewPort.Width / 2) - ((TileSizeX(viewPort) * _boardSize) / 2);
            }
        }
    }
}