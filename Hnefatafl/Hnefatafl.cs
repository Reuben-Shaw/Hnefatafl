#define DEBUG
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Threading;
using Lidgren.Network;
using System.Diagnostics;
using System.Collections.Generic;
using static Hnefatafl.MenuObject.Status;
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

        private enum GameState { MainMenu, MultiplayerMenu, ServerMenu, InGame, EscMenu };
        private GameState m_gameState;
        private GameState _gameState
        {
            get
            {
                return m_gameState;
            }
            set
            {
                m_gameState = value;
                TransferState();
            }
        }


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
            _gameState = GameState.MainMenu;
            
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

            MouseState currentState = Mouse.GetState();
            Point mouseLoc = new Point(currentState.Position.X, currentState.Position.Y - _player._board.TileSizeY(GraphicsDevice.Viewport.Bounds) / 2);

            switch (_gameState)
            {
                case GameState.MainMenu:
                {
                    MainMenu(gameTime, currentState, mouseLoc);
                    break;
                }
                case GameState.MultiplayerMenu:
                {
                    MultiplayerMenu(gameTime, currentState, mouseLoc);
                    break;
                }
                case GameState.ServerMenu:
                {
                    ServerMenu(gameTime, currentState, mouseLoc);
                    break;
                }
                case GameState.InGame:
                {
                    InGame(gameTime, currentState, mouseLoc);
                    break;
                }
                case GameState.EscMenu:
                {
                    
                    break;
                }
            }

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            
            previousMouse = Mouse.GetState();
            base.Update(gameTime);
        }

        private void TransferState()
        {
            _button.Clear();

            Rectangle viewPorts = GraphicsDevice.Viewport.Bounds;
            Point buttonSize = new Point(viewPorts.Width / 5, viewPorts.Height / 10);

            List<string> buttonText = new List<string>();
            Point startLoc = new Point(0, 0);

            switch (_gameState)
            {
                case GameState.MainMenu:
                {
                    buttonText.Add("Multiplayer");
                    buttonText.Add("Singleplayer");
                    buttonText.Add("Options");
                    buttonText.Add("Exit");
                    startLoc = new Point(viewPorts.Width / 2 - buttonSize.X / 2, viewPorts.Height / 8 + buttonSize.Y);
                    break;
                }
                case GameState.MultiplayerMenu:
                {
                    buttonText.Add("Server");
                    buttonText.Add("Client");
                    buttonText.Add("Shutdown");
                    buttonText.Add("Back");
                    startLoc = new Point(viewPorts.Width / 2 - buttonSize.X / 2, viewPorts.Height / 8 + buttonSize.Y);
                    break;
                }
            }

            for (int i = 0; i < buttonText.Count; i++)
            {
                _button.Add(new Button(new Point(startLoc.X, startLoc.Y + ((buttonSize.X * i) / 2)), buttonSize, buttonText[i]));
                _button[i].Update(_graphics, Content);
            }
        }

        private void MainMenu(GameTime gameTime, MouseState currentState, Point mouseLoc)
        {
            string selected = ButtonCheck(currentState, mouseLoc).ToLower();

            switch (selected)
            {
                case "multiplayer":
                {
                    _gameState = GameState.MultiplayerMenu;
                    break;
                }
                case "singleplayer":
                {
                    _gameState = GameState.InGame;
                    break;
                }
                case "options":
                {
                    break;
                }
                case "exit":
                {
                    Exit();
                    break;
                }
            }
        }

        private void MultiplayerMenu(GameTime gameTime, MouseState currentState, Point mouseLoc)
        {
            string selected = ButtonCheck(currentState, mouseLoc).ToLower();

            switch (selected)
            {
                case "server":
                {
                    if (_server is null)
                    {
                        _server = new Server();
                        _server.StartServer();
                    }
                    break;
                }
                case "client":
                {
                    _player.EstablishConnection();
                    _gameState = GameState.InGame;
                    break;
                }
                case "shutdown":
                {
                    if (_server is not null)
                    {
                        _server.StopServer();
                    }
                    break;
                }
                case "back":
                {   
                    _gameState = GameState.MainMenu;
                    break;
                }
            }
        }

        private void ServerMenu(GameTime gameTime, MouseState currentState, Point mouseLoc)
        {
            
        }

        private void InGame(GameTime gameTime, MouseState currentState, Point mouseLoc)
        {
            if (currentState.LeftButton != previousMouse.LeftButton)
            {
                if (currentState.LeftButton == ButtonState.Pressed && (_player._currentTurn == true || _player._connected == false))
                {
                    HPoint point = new HPoint(
                        (mouseLoc.X - (GraphicsDevice.Viewport.Bounds.Width / 2) - ((_player._board.TileSizeX(GraphicsDevice.Viewport.Bounds) * _player._board._boardSize) / 2)) / _player._board.TileSizeX(GraphicsDevice.Viewport.Bounds) + 10,
                        (mouseLoc.Y - (GraphicsDevice.Viewport.Bounds.Width / 2) - ((_player._board.TileSizeY(GraphicsDevice.Viewport.Bounds) * _player._board._boardSize) / 2)) / _player._board.TileSizeY(GraphicsDevice.Viewport.Bounds) + 17
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
            }
        }

        private string ButtonCheck(MouseState currentState, Point mouseLoc)
        {
            string text = "";

            if (currentState.LeftButton != previousMouse.LeftButton)
            {
                foreach (Button button in _button)
                {
                    if (new Rectangle(button._pos, button._size).Contains(mouseLoc))
                    {
                        if (previousMouse.LeftButton == ButtonState.Released && currentState.LeftButton == ButtonState.Pressed)
                        {
                            button._status = Selected;
                        }
                        else if (previousMouse.LeftButton == ButtonState.Pressed && currentState.LeftButton == ButtonState.Released)
                        {
                            button._status = Unselected;
                            text = button._text;
                        }
                    }
                    else if (button._status == Selected)
                    {
                        button._status = Unselected;
                    }
                }
            }

            return text;
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.LightGray);
            
            _spriteBatch.Begin();

            foreach (Button button in _button)
            {
                button.Draw(gameTime, _spriteBatch, GraphicsDevice.Viewport.Bounds);
            }

            if (_gameState == GameState.InGame || _gameState == GameState.EscMenu)
            {
                _player._board.Draw(gameTime, _spriteBatch, GraphicsDevice.Viewport.Bounds);
            }

            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
