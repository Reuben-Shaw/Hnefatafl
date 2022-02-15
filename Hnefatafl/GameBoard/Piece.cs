using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;

namespace Hnefatafl
{
    public class Piece
    {
        private readonly Texture2D[] _pawnTexture = new Texture2D[3];
        private Pawn[,] _playingField;
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

            _playingField = Hnefatafl.PawnRecieve();
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

            Rectangle rect = new Rectangle(
                        (viewPort.Width / 2) - ((_tileSizeX * _playingField.GetLength(0)) / 2), 
                        (viewPort.Height / 2) - ((_tileSizeY * _playingField.GetLength(1)) / 2), 
                        _tileSizeX, _tileSizeY);

            while (x < _playingField.GetLength(0) && !clickFound)
            {
                y = 0;
                while (y < _playingField.GetLength(1) && !clickFound)
                {
                    if (_selectedPiece.X == -1)
                    {
                        if (_playingField[x, y].textInd != 0)
                        {
                            if (rect.Contains(mouse.Position))
                            {
                                clickFound = true;
                                Console.WriteLine("Click");
                            }
                        }
                    }
                    else
                    {
                        if (rect.Contains(mouse.Position))
                        {
                            clickFound = true;

                            if ((x == _selectedPiece.X ^ y == _selectedPiece.Y) && ClearanceCheck(x, y))
                            {
                                _playingField[x, y] = _playingField[_selectedPiece.X, _selectedPiece.Y];
                                _playingField[_selectedPiece.X, _selectedPiece.Y] = new Pawn(0);
                            }
                        }
                    }

                    y++;
                    rect.Y += _tileSizeY;
                }

                x++;
                rect.X += _tileSizeX;
                rect.Y = (viewPort.Height / 2) - ((_tileSizeY * _boardSize) / 2);
            }

            if (clickFound && _selectedPiece.X == -1)
            {
                _selectedPiece = new Point(x - 1, y - 1);
            }
            else
            {
                _selectedPiece = new Point(-1, -1);
            }
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
                if (_playingField[i, y].textInd != 0)
                    return false;
            }
            
            if (y > _selectedPiece.Y)
            { start = _selectedPiece.Y + 1; end = y + 1; }
            else
            { start = y; end = _selectedPiece.Y; }

            for (int i = start; i < end; i++)
            {
                if (_playingField[x, i].textInd != 0)
                    return false;
            }

            return true;
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, Rectangle viewPort)
        {
            Rectangle rect = new Rectangle(
                                (viewPort.Width / 2) - ((_tileSizeX * _boardSize) / 2), 
                                (viewPort.Height / 2) - ((_tileSizeY * _boardSize) / 2), 
                                _tileSizeX, _tileSizeY);

            for (int x = 0; x < _playingField.GetLength(0); x++)
            {
                for (int y = 0; y < _playingField.GetLength(1); y++)
                {
                    if (_playingField[x, y].textInd != 0)
                    {
                        spriteBatch.Draw(_pawnTexture[_playingField[x, y].textInd - 1], rect, Color.White);
                    }
                    rect.Y += _tileSizeY;
                }
                rect.Y = (viewPort.Height / 2) - ((_tileSizeY * _boardSize) / 2);
                rect.X += _tileSizeX;
            }
        }
    }
}