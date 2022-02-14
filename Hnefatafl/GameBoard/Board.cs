using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Hnefatafl.GameBoard
{
    public sealed class Board
    {
        private Texture2D[] _boardColours = new Texture2D[8];//new Color[4]; 
        //0: Board main colour 1, 1: Board main colour 2, 2: Defender board colour, 3: Attacker board colour, 4: Throne, 5: Corner, 6: Border

        private int _boardSize = 11;
        private int _tileSizeX, _tileSizeY;

        public void LoadContent(GraphicsDeviceManager graphics, Rectangle viewPort)
        {
            for (int i = 0; i < _boardColours.Length; i++)
            {
                _boardColours[i] = new Texture2D(graphics.GraphicsDevice, 1, 1);
            }

            _tileSizeX = viewPort.Width / 30;
            _tileSizeY = _tileSizeX;
        }

        public void UnloadContent()
        {
            for (int i = 0; i < _boardColours.Length; i++)
            {
                _boardColours[i].Dispose();
            }
        }

        public void TileGeneration(Color[] colours)
        {
            for (int i = 0; i < colours.Length; i++)
            {
                _boardColours[i].SetData(new[] { colours[i] });
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, Rectangle viewPort)
        {
            Rectangle rect;
            for (int y = 0; y < _boardSize; y++)
            {
                for (int x = 0; x < _boardSize; x++)
                {
                    rect = new Rectangle(
                        (_tileSizeX * x) + (viewPort.Width / 2) - ((_tileSizeX * _boardSize) / 2), 
                        (_tileSizeY * y) + (viewPort.Height / 2) - ((_tileSizeY * _boardSize) / 2), 
                        _tileSizeX, _tileSizeY);

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
                }
            }

            // spriteBatch.Draw(_boardColours[4], new Rectangle((GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width / 4) - ((_tileSize * _boardSize) / 2) - _tileSize, (GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height / 4) - ((_tileSize * _boardSize) / 2), (_tileSize * _boardSize) + (_tileSize * 2), _tileSize), Color.White);
            // spriteBatch.Draw(_boardColours[4], new Rectangle((GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width / 4) - ((_tileSize * _boardSize) / 2) - _tileSize, (GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height / 4) - ((_tileSize * _boardSize) / 2) + (_tileSize * _boardSize) + _tileSize, (_tileSize * _boardSize) + (_tileSize * 2), _tileSize), Color.White);

            // spriteBatch.Draw(_boardColours[4], new Rectangle((GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width / 4) - ((_tileSize * _boardSize) / 2) - _tileSize, (GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height / 4) - ((_tileSize * _boardSize) / 2), _tileSize, (_tileSize * _boardSize) + (_tileSize * 2)), Color.White);
            // spriteBatch.Draw(_boardColours[4], new Rectangle((GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width / 4) - ((_tileSize * _boardSize) / 2) + (_tileSize * _boardSize), (GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height / 4) - ((_tileSize * _boardSize) / 2), _tileSize, (_tileSize * _boardSize) + (_tileSize * 2)), Color.White);
        
        
        }
    }
}