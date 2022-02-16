using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections;
using System.Collections.Generic;
using static Hnefatafl.PieceType;

namespace Hnefatafl
{
    public class Main
    {
        private readonly Texture2D[] _pawnTexture = new Texture2D[3];
        private Pieces _playingField = new Pieces();
        private int _boardSize = 11;
        private int _tileSizeX, _tileSizeY;
        private Point _selectedPiece = new Point(-1, -1);

        public void LoadContent(GraphicsDeviceManager graphics, Rectangle viewPort, ContentManager Content)
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

            _playingField.CreateBoard(_boardSize, BoardTypes.Regular);

            _tileSizeX = viewPort.Width / 30;
            _tileSizeY = _tileSizeX;
        }

        public void UnloadContent()
        {
            _pawnTexture[0].Dispose();
            _pawnTexture[1].Dispose();
        }

        public void Update(GameTime gameTime, MouseState mouse, Rectangle viewPort)
        {
            int x = 0, y = 0; //Assign y here to stop compilation errors
            bool clickFound = false;

            // Rectangle rect = new Rectangle(
            //             (viewPort.Width / 2) - ((_tileSizeX * _playingField.GetLength(0)) / 2), 
            //             (viewPort.Height / 2) - ((_tileSizeY * _playingField.GetLength(1)) / 2), 
            //             _tileSizeX, _tileSizeY);

            // while (x < _playingField.GetLength(0) && !clickFound)
            // {
            //     y = 0;
            //     while (y < _playingField.GetLength(1) && !clickFound)
            //     {
            //         if (_selectedPiece.X == -1)
            //         {
            //             if (_playingField[x, y].defender != Empty && ((_playingField[x, y].defender == Attacker && attackerTurn) || ((int)_playingField[x, y].defender == 1 && !attackerTurn)))
            //             {
            //                 if (rect.Contains(mouse.Position))
            //                 {
            //                     clickFound = true;
            //                     Console.WriteLine("Click");
            //                 }
            //             }
            //         }
            //         else
            //         {
            //             if (rect.Contains(mouse.Position))
            //             {
            //                 clickFound = true;

            //                 if ((x == _selectedPiece.X ^ y == _selectedPiece.Y) && ClearanceCheck(x, y))
            //                 {
            //                     _playingField[x, y] = _playingField[_selectedPiece.X, _selectedPiece.Y];
            //                     _playingField[_selectedPiece.X, _selectedPiece.Y] = new Pawn(0, Empty);
            //                     CaptureLoc(x, y, _playingField[x, y].defender);
            //                     attackerTurn = !attackerTurn;
            //                 }
            //             }
            //         }

            //         y++;
            //         rect.Y += _tileSizeY;
            //     }

            //     x++;
            //     rect.X += _tileSizeX;
            //     rect.Y = (viewPort.Height / 2) - ((_tileSizeY * _boardSize) / 2);
            // }

            if (clickFound && _selectedPiece.X == -1)
            {
                _selectedPiece = new Point(x - 1, y - 1);
            }
            else
            {
                _selectedPiece = new Point(-1, -1);
            }
        }

        // private bool ClearanceCheck(int x, int y)
        // {
        //     int start, end;

        //     if (x > _selectedPiece.X)
        //     { start = _selectedPiece.X + 1; end = x + 1; }
        //     else
        //     { start = x; end = _selectedPiece.X; }

        //     for (int i = start; i < end; i++)
        //     {
        //         if (_playingField[i, y].textInd != 0)
        //             return false;
        //     }
            
        //     if (y > _selectedPiece.Y)
        //     { start = _selectedPiece.Y + 1; end = y + 1; }
        //     else
        //     { start = y; end = _selectedPiece.Y; }

        //     for (int i = start; i < end; i++)
        //     {
        //         if (_playingField[x, i].textInd != 0)
        //             return false;
        //     }

        //     return true;
        // }

        // private void CaptureLoc(int x, int y, PieceType defender)
        // {
        //     if (y > 1 && (int)_playingField[x, y - 1].defender != (int)_playingField[x, y].defender && (int)_playingField[x, y - 2].defender == (int)_playingField[x, y].defender)
        //     {
        //         _playingField[x, y - 1] = new Pawn(0, Empty);
        //     }
        //     if (y < _boardSize - 2 && (int)_playingField[x, y + 1].defender != (int)_playingField[x, y].defender && (int)_playingField[x, y + 2].defender == (int)_playingField[x, y].defender)
        //     {
        //         _playingField[x, y + 1] = new Pawn(0, Empty);
        //     }

        //     if (x > 1 && (int)_playingField[x - 1, y].defender != (int)_playingField[x, y].defender && (int)_playingField[x - 2, y].defender == (int)_playingField[x, y].defender)
        //     {
        //         _playingField[x - 1, y] = new Pawn(0, Empty);
        //     }
        //     if (x < _boardSize - 2 && (int)_playingField[x + 1, y].defender != (int)_playingField[x, y].defender && ((int)_playingField[x + 2, y].defender == (int)_playingField[x, y].defender))
        //     {
        //         _playingField[x + 1, y] = new Pawn(0, Empty);
        //     }
        // }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, Rectangle viewPort)
        {
            Rectangle rect = new Rectangle(
                                (viewPort.Width / 2) - ((_tileSizeX * _boardSize) / 2), 
                                (viewPort.Height / 2) - ((_tileSizeY * _boardSize) / 2), 
                                _tileSizeX, _tileSizeY);
                                
            Piece iPiece;
            foreach (DictionaryEntry piece in _playingField.AllPieces())
            {
                iPiece = piece.Value as Piece;
                rect = new Rectangle(
                    (_tileSizeX * iPiece._loc.X) + (viewPort.Width / 2) - ((_tileSizeX * _boardSize) / 2), 
                    (_tileSizeY * iPiece._loc.Y) + (viewPort.Height / 2) - ((_tileSizeY * _boardSize) / 2), 
                    _tileSizeX, _tileSizeY);

                if (iPiece._pawn._type != Empty)
                {
                    spriteBatch.Draw(_pawnTexture[(int)iPiece._pawn._type], rect, Color.White);
                }
            }
        }
    }
}