using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using Hnefatafl.GameBoard;
using Hnefatafl.GamePiece;
using static Hnefatafl.PieceType;

namespace Hnefatafl
{
    public class Hnefatafl : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        Board _gameBoard = new Board();
        Piece _pieceBoard = new Piece();
        OptionObj _optionObj;

        static Pawn[,] _playingField = new Pawn[11, 11] 
        {
            { new Pawn(0, Empty), new Pawn(0, Empty), new Pawn(0, Empty), new Pawn(1, Attacker), new Pawn(1, Attacker), new Pawn(1, Attacker), new Pawn(1, Attacker), new Pawn(1, Attacker), new Pawn(0, Empty), new Pawn(0, Empty), new Pawn(0, Empty) },
            { new Pawn(0, Empty), new Pawn(0, Empty), new Pawn(0, Empty), new Pawn(0, Empty), new Pawn(0, Empty), new Pawn(1, Attacker), new Pawn(0, Empty), new Pawn(0, Empty), new Pawn(0, Empty), new Pawn(0, Empty), new Pawn(0, Empty) },
            { new Pawn(0, Empty), new Pawn(0, Empty), new Pawn(0, Empty), new Pawn(0, Empty), new Pawn(0, Empty), new Pawn(0, Empty), new Pawn(0, Empty), new Pawn(0, Empty), new Pawn(0, Empty), new Pawn(0, Empty), new Pawn(0, Empty) },
            { new Pawn(1, Attacker), new Pawn(0, Empty), new Pawn(0, Empty), new Pawn(0, Empty), new Pawn(0, Empty), new Pawn(2, Defender), new Pawn(0, Empty), new Pawn(0, Empty), new Pawn(0, Empty), new Pawn(0, Empty), new Pawn(1, Attacker) },
            { new Pawn(1, Attacker), new Pawn(0, Empty), new Pawn(0, Empty), new Pawn(0, Empty), new Pawn(2, Defender), new Pawn(2, Defender), new Pawn(2, Defender), new Pawn(0, Empty), new Pawn(0, Empty), new Pawn(0, Empty), new Pawn(1, Attacker) },
            { new Pawn(1, Attacker), new Pawn(1, Attacker), new Pawn(0, Empty), new Pawn(2, Defender), new Pawn(2, Defender), new Pawn(3, King), new Pawn(2, Defender), new Pawn(2, Defender), new Pawn(0, Empty), new Pawn(0, Empty), new Pawn(0, Empty) },
            { new Pawn(1, Attacker), new Pawn(0, Empty), new Pawn(0, Empty), new Pawn(0, Empty), new Pawn(2, Defender), new Pawn(2, Defender), new Pawn(2, Defender), new Pawn(0, Empty), new Pawn(0, Empty), new Pawn(0, Empty), new Pawn(1, Attacker) },
            { new Pawn(1, Attacker), new Pawn(0, Empty), new Pawn(0, Empty), new Pawn(0, Empty), new Pawn(0, Empty), new Pawn(2, Defender), new Pawn(0, Empty), new Pawn(0, Empty), new Pawn(0, Empty), new Pawn(0, Empty), new Pawn(1, Attacker) },
            { new Pawn(0, Empty), new Pawn(0, Empty), new Pawn(0, Empty), new Pawn(0, Empty), new Pawn(0, Empty), new Pawn(0, Empty), new Pawn(0, Empty), new Pawn(0, Empty), new Pawn(0, Empty), new Pawn(0, Empty), new Pawn(0, Empty) },
            { new Pawn(0, Empty), new Pawn(0, Empty), new Pawn(0, Empty), new Pawn(0, Empty), new Pawn(0, Empty), new Pawn(1, Attacker), new Pawn(0, Empty), new Pawn(0, Empty), new Pawn(0, Empty), new Pawn(0, Empty), new Pawn(0, Empty) },
            { new Pawn(0, Empty), new Pawn(0, Empty), new Pawn(0, Empty), new Pawn(1, Attacker), new Pawn(1, Attacker), new Pawn(1, Attacker), new Pawn(1, Attacker), new Pawn(1, Attacker), new Pawn(0, Empty), new Pawn(0, Empty), new Pawn(0, Empty) }
        };

        public Hnefatafl()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        const bool _doFull = false;
        protected override void Initialize()
        {
            if (!_doFull)
            {
                _graphics.PreferredBackBufferWidth = 960;
                _graphics.PreferredBackBufferHeight = 540;
                _graphics.ApplyChanges();
            }
            else
            {
                _graphics.PreferredBackBufferWidth = 1920;
                _graphics.PreferredBackBufferHeight = 1080;
                _graphics.IsFullScreen = true;
                _graphics.ApplyChanges();
            }

            _optionObj = new OptionObj(new Color[]{new Color(173, 99, 63), new Color(80, 53, 30), new Color(0, 0, 0), new Color(0, 0, 0), new Color(175, 0, 0), new Color(249, 200, 24), new Color(28, 17, 7)});

            base.Initialize();
        }

        public static Pawn[,] PawnRecieve()
        {
            return _playingField;
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _gameBoard.LoadContent(_graphics, GraphicsDevice.Viewport.Bounds);
            _gameBoard.TileGeneration(_optionObj.boardColour);
            _pieceBoard.LoadContent(_graphics, GraphicsDevice.Viewport.Bounds, Content);

            Console.WriteLine("Successful LoadContent");
        }

        protected override void UnloadContent()
        {
            _gameBoard.UnloadContent();
            _pieceBoard.UnloadContent();

            Console.WriteLine("Successful UnloadContent");
        }

        MouseState previousMouse;
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (Mouse.GetState().LeftButton != previousMouse.LeftButton)
            {
                previousMouse = Mouse.GetState();
                if (Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    _pieceBoard.Update(gameTime, Mouse.GetState(), GraphicsDevice.Viewport.Bounds);
                }
            }
            
            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.LightGray);
            _spriteBatch.Begin();

            _gameBoard.Draw(gameTime, _spriteBatch, GraphicsDevice.Viewport.Bounds);
            _pieceBoard.Draw(gameTime, _spriteBatch, GraphicsDevice.Viewport.Bounds);

            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
