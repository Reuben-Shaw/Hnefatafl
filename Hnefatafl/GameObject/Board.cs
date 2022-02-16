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

    public sealed class Piece
    {
        public Pawn _pawn { get; set; }
        public Point _loc  { get; set; }

        public Piece(Pawn pawn, Point loc)
        {
            _pawn = pawn;
            _loc = loc;
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
                return null;
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

    public sealed class Board
    {
        private readonly Texture2D[] _boardColours = new Texture2D[6];
        //0: Board main colour 1, 1: Board main colour 2, 2: Defender board colour, 3: Attacker board colour, 4: Throne, 5: Corner
        private readonly Texture2D[] _pawnTexture = new Texture2D[3];
        private Pieces _pieces;
        public int _boardSize;

        public Board(GraphicsDeviceManager graphics, ContentManager Content, Color[] colours, Pieces pieces, int boardSize)
        {
            CreateColours(graphics, colours);
            _pieces = pieces;
            _boardSize = boardSize;
        }

        public Board(GraphicsDeviceManager graphics, ContentManager Content, Pieces pieces, int boardSize)
        {
            CreateColours(graphics, new Color[]{new Color(173, 99, 63), new Color(80, 53, 30), new Color(0, 0, 0), new Color(0, 0, 0), new Color(175, 0, 0), new Color(249, 200, 24)});
            _pieces = pieces;
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
        }

        private int TileSizeX(Rectangle viewPort)
        {
            return viewPort.Width / 30;
        }
        
        private int TileSizeY(Rectangle viewPort)
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

                    iPiece = _pieces.GetPiece(x.ToString() + y.ToString());
                    if (iPiece._pawn._type != Empty)
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