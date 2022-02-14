using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Hnefatafl
{
    public class Piece
    {
        private Texture2D _pawnTemp;
        private Pawn[,] _playingField;
        private int _boardSize = 11;
        private int _tileSizeX = 32, _tileSizeY = 32;

        public void LoadContent(GraphicsDeviceManager graphics, Rectangle viewPort)
        {
            _pawnTemp = new Texture2D(graphics.GraphicsDevice, 1, 1);
            _pawnTemp.SetData(new[] { Color.Green });

            _playingField = Hnefatafl.PawnRecieve();
        }

        public void UnloadContent()
        {
            _pawnTemp.Dispose();
        }

        public void Update(GameTime gameTime, MouseState mouse, Rectangle viewPort)
        {
            int x = 0, y;
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
                    if (_playingField[x, y].textInd != 0)
                    {
                        if (rect.Contains(mouse.Position))
                            clickFound = true;
                    }

                    y++;
                    rect.Y += _tileSizeY;
                }
                x++;
                rect.X += _tileSizeX;
                rect.Y = (viewPort.Height / 2) - ((_tileSizeY * _boardSize) / 2);
            }

            Console.WriteLine($"Found square = {clickFound}");
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, Rectangle viewPort)
        {
            Rectangle rect;
            rect = new Rectangle(
                        (_tileSizeX * 5) + (viewPort.Width / 2) - ((_tileSizeX * _boardSize) / 2), 
                        (_tileSizeY * 5) + (viewPort.Height / 2) - ((_tileSizeY * _boardSize) / 2), 
                        _tileSizeX, _tileSizeY);

            spriteBatch.Draw(_pawnTemp, rect, Color.White);
        }
    }
}