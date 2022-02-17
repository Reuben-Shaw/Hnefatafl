#define DEBUG
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using Lidgren.Network;
using System.Diagnostics;
using System.Collections.Generic;
using static Hnefatafl.PieceType;
using static Hnefatafl.Player.InstructType;

namespace Hnefatafl
{
    public class Hnefatafl : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Player _player;
        private Server _server; 

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

            _player = new Player(_graphics, Content, 11);
            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            Console.WriteLine("Successful LoadContent");
        }

        protected override void UnloadContent()
        {
            _player._board.UnloadContent();

            Console.WriteLine("Successful UnloadContent");
        }

        MouseState previousMouse;
        KeyboardState previousKeyboard;
        protected override void Update(GameTime gameTime)
        {
            if (_server is not null)
            {
                _server.ReadMessages();
            }
            
            _player.CheckMessage();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (Keyboard.GetState().IsKeyDown(Keys.R) != previousKeyboard.IsKeyDown(Keys.R))
            {
                previousKeyboard = Keyboard.GetState();
                if (Keyboard.GetState().IsKeyDown(Keys.R))
                {
                    _player.SendMessage("hey");
                }
            }

            if (Keyboard.GetState().IsKeyDown(Keys.E) != previousKeyboard.IsKeyDown(Keys.E))
            {
                previousKeyboard = Keyboard.GetState();
                if (Keyboard.GetState().IsKeyDown(Keys.E))
                {
                    _server = new Server();
                    _server.StartServer();
                }
            }

            if (Keyboard.GetState().IsKeyDown(Keys.C) != previousKeyboard.IsKeyDown(Keys.C))
            {
                previousKeyboard = Keyboard.GetState();
                if (Keyboard.GetState().IsKeyDown(Keys.C))
                {
                    _player.EstablishConnection();
                }
            }   

            if (Keyboard.GetState().IsKeyDown(Keys.K) != previousKeyboard.IsKeyDown(Keys.K))
            {
                previousKeyboard = Keyboard.GetState();
                if (Keyboard.GetState().IsKeyDown(Keys.K))
                {
                    _server.StopServer();
                }
            } 

            if (Mouse.GetState().LeftButton != previousMouse.LeftButton)
            {
                previousMouse = Mouse.GetState();
                if (previousMouse.LeftButton == ButtonState.Pressed)
                {
                    Point mouseLoc = new Point(previousMouse.Position.X, previousMouse.Position.Y - _player._board.TileSizeY(GraphicsDevice.Viewport.Bounds) / 2);
                    HPoint point = new HPoint(
                        (mouseLoc.X - (GraphicsDevice.Viewport.Bounds.Width / 2) - ((_player._board.TileSizeX(GraphicsDevice.Viewport.Bounds) * 11) / 2)) / _player._board.TileSizeX(GraphicsDevice.Viewport.Bounds) + 10,
                        (mouseLoc.Y - (GraphicsDevice.Viewport.Bounds.Width / 2) - ((_player._board.TileSizeY(GraphicsDevice.Viewport.Bounds) * 11) / 2)) / _player._board.TileSizeY(GraphicsDevice.Viewport.Bounds) + 17
                        );
                    if (_player._board.IsPieceSelected())
                    {
                        if (_player._board.MakeMove(point))
                        {
                            _player.SendMessage(MOVE.ToString() + point.ToString());
                        }
                        else
                        {
                            _player.SendMessage(MOVEFAIL.ToString());
                        }
                    }
                    else
                    {
                        _player._board.SelectPiece(point);
                        _player.SendMessage(SELECT.ToString() + point.ToString());
                    }
                }
            }
            
            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        public static bool testBool = false;
        protected override void Draw(GameTime gameTime)
        {
            if (!testBool)
            {
                GraphicsDevice.Clear(Color.LightGray);
            }
            else
            {
                GraphicsDevice.Clear(Color.Red);
            }
            _spriteBatch.Begin();

            _player._board.Draw(gameTime, _spriteBatch, GraphicsDevice.Viewport.Bounds);

            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
