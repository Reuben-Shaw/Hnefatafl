#define DEBUG
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Threading;
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
        private List<Button> _button = new List<Button>();

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
            _button.Add(new Button(new Point(0, 0), new Point(100, 50), "Longer"));
            _button[0].Update(_graphics, Content);
            
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

            
            MouseState currentState = Mouse.GetState();
            if (currentState.LeftButton != previousMouse.LeftButton)
            {
                Point mouseLoc = new Point(currentState.Position.X, currentState.Position.Y - _player._board.TileSizeY(GraphicsDevice.Viewport.Bounds) / 2);
            
                bool buttonReached = false;
                foreach (Button button in _button)
                {
                    if (new Rectangle(button._pos, button._size).Contains(mouseLoc))
                    {
                        if (previousMouse.LeftButton == ButtonState.Released && currentState.LeftButton == ButtonState.Pressed)
                        {
                            button._status = MenuObject.Status.Selected;
                            buttonReached = false;
                        }
                        else if (previousMouse.LeftButton == ButtonState.Pressed && currentState.LeftButton == ButtonState.Released)
                        {
                            button._status = MenuObject.Status.Unselected;
                            buttonReached = false;
                        }
                    }
                    else if (button._status == MenuObject.Status.Selected)
                    {
                        button._status = MenuObject.Status.Unselected;
                    }
                }
                
                if (!buttonReached && currentState.LeftButton == ButtonState.Pressed && (_player._currentTurn == true || _player._connected == false))
                {
                    HPoint point = new HPoint(
                        (mouseLoc.X - (GraphicsDevice.Viewport.Bounds.Width / 2) - ((_player._board.TileSizeX(GraphicsDevice.Viewport.Bounds) * 11) / 2)) / _player._board.TileSizeX(GraphicsDevice.Viewport.Bounds) + 10,
                        (mouseLoc.Y - (GraphicsDevice.Viewport.Bounds.Width / 2) - ((_player._board.TileSizeY(GraphicsDevice.Viewport.Bounds) * 11) / 2)) / _player._board.TileSizeY(GraphicsDevice.Viewport.Bounds) + 17
                        );
                    
                    if (_player._board.IsPieceSelected())
                    {
                        if (_player._board.MakeMove(point, _player._side, !_player._connected))
                        {
                            _player.SendMessage(MOVE.ToString() + "," + point.ToString());
                            _player._currentTurn = false;
                        }
                        else
                        {
                            _player.SendMessage(MOVEFAIL.ToString());
                        }
                    }
                    else
                    {
                        _player._board.SelectPiece(point);
                        _player.SendMessage(SELECT.ToString() + "," + point.ToString());
                    }
                }   

                previousMouse = Mouse.GetState();
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
           
            GraphicsDevice.Clear(Color.LightGray);
            
            _spriteBatch.Begin();

            foreach (Button button in _button)
            {
                button.Draw(gameTime, _spriteBatch, GraphicsDevice.Viewport.Bounds);
            }

            _player._board.Draw(gameTime, _spriteBatch, GraphicsDevice.Viewport.Bounds);

            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
