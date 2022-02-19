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
        private List<TextBox> _textbox = new List<TextBox>();

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

            MouseState currentMouseState = Mouse.GetState();
            KeyboardState currentKeyboardState = Keyboard.GetState();
            Point mouseLoc = new Point(currentMouseState.Position.X + 10, currentMouseState.Position.Y - _player._board.TileSizeY(GraphicsDevice.Viewport.Bounds) / 2 + 17);

            switch (_gameState)
            {
                case GameState.MainMenu:
                {
                    MainMenu(gameTime, currentMouseState, mouseLoc);
                    break;
                }
                case GameState.MultiplayerMenu:
                {
                    MultiplayerMenu(gameTime, currentMouseState, mouseLoc);
                    break;
                }
                case GameState.ServerMenu:
                {
                    ServerMenu(gameTime, currentMouseState, mouseLoc);
                    break;
                }
                case GameState.InGame:
                {
                    InGame(gameTime, currentMouseState, mouseLoc);
                    break;
                }
                case GameState.EscMenu:
                {
                    
                    break;
                }
            }

            if (previousKeyboard != currentKeyboardState)
            {
                foreach (TextBox textbox in _textbox)
                {
                    if (textbox._status == Selected)
                    {
                        foreach (Keys key in currentKeyboardState.GetPressedKeys())
                        {
                            if (!previousKeyboard.IsKeyDown(key))
                            {
                                if (key != Keys.Back)
                                {
                                    textbox.Add(key);
                                }
                                else
                                {
                                    textbox.RemoveChar();
                                }
                            }
                        }
                    }
                }
            }

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            
            previousMouse = currentMouseState;
            previousKeyboard = currentKeyboardState;
            base.Update(gameTime);
        }

        private void TransferState()
        {
            _button.Clear();
            _textbox.Clear();

            Rectangle viewPorts = GraphicsDevice.Viewport.Bounds;
            Point buttonSize = new Point(viewPorts.Width / 5, viewPorts.Height / 10);
            Point textboxSize = new Point(256, 48);

            List<string> buttonText = new List<string>();
            List<string> textboxText = new List<string>();
            Point startLoc = new Point(0, 0);

            switch (_gameState)
            {
                case GameState.MainMenu:
                {
                    buttonText.Add("MULTIPLAYER");
                    buttonText.Add("SINGLEPLAYER");
                    buttonText.Add("OPTIONS");
                    buttonText.Add("EXIT");
                    startLoc = new Point(viewPorts.Width / 2 - buttonSize.X / 2, viewPorts.Height / 8 + buttonSize.Y);
                    break;
                }
                case GameState.MultiplayerMenu:
                {
                    buttonText.Add("HOST");
                    buttonText.Add("CONNECT");
                    buttonText.Add("SHUTDOWN");
                    buttonText.Add("BACK");
                    startLoc = new Point(viewPorts.Width / 2 - buttonSize.X / 2, viewPorts.Height / 8 + buttonSize.Y);
                    break;
                }
                case GameState.ServerMenu:
                {
                    textboxText.Add("IP");
                    textboxText.Add("PORT");
                    buttonText.Add("CONNECT");
                    buttonText.Add("BACK");
                    startLoc = new Point(viewPorts.Width / 2 - textboxSize.X / 2, textboxSize.Y * 2);
                    break;
                }
                case GameState.InGame:
                {
                    buttonText.Add("BACK");
                    startLoc = new Point(viewPorts.Width / 30, viewPorts.Height - buttonSize.Y - viewPorts.Width / 30);
                    _player._board.CreatBoard();
                    break;
                }
                case GameState.EscMenu:
                {
                    break;
                }
            }

            if (_gameState == GameState.MainMenu || _gameState == GameState.MultiplayerMenu || _gameState == GameState.InGame)
            {
                for (int i = 0; i < buttonText.Count; i++)
                {
                    _button.Add(new Button(new Point(startLoc.X, startLoc.Y + ((buttonSize.X * i) / 2)), buttonSize, buttonText[i]));
                    _button[i].Update(_graphics, Content);
                }
            }
            else if (_gameState == GameState.ServerMenu)
            {
                for (int i = 0; i < textboxText.Count; i++)
                {
                    _textbox.Add(new TextBox(new Point(startLoc.X, startLoc.Y + ((textboxSize.X * i) / 2)), textboxSize, textboxText[i]));
                    _textbox[i].Update(_graphics, Content);
                }

                startLoc = new Point(viewPorts.Width / 2 - buttonSize.X / 2, _textbox[textboxText.Count - 1]._pos.Y + _textbox[textboxText.Count - 1]._size.Y + viewPorts.Width / 30);
                for (int i = textboxText.Count; i < buttonText.Count + textboxText.Count; i++)
                {
                    _button.Add(new Button(new Point(startLoc.X, startLoc.Y + ((buttonSize.X * (i - textboxText.Count)) / 2)), buttonSize, buttonText[(i - textboxText.Count)]));
                    _button[(i - textboxText.Count)].Update(_graphics, Content);
                }
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
                case "host":
                {
                    if (_server is null)
                    {
                        _server = new Server();
                        _server.StartServer();
                    }
                    break;
                }
                case "connect":
                {
                    _gameState = GameState.ServerMenu;
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
            string selected = ButtonCheck(currentState, mouseLoc).ToLower();

            switch (selected)
            {
                case "connect":
                {
                    if (_textbox[0]._text != "" && _textbox[1]._text != "")
                    {
                        _player.EstablishConnection(_textbox[0]._text, Convert.ToInt32(_textbox[1]._text));
                        _gameState = GameState.InGame;
                    }
                    break;
                }
                case "back":
                {
                    _gameState = GameState.MultiplayerMenu;
                    break;
                }
            }
        }

        private void InGame(GameTime gameTime, MouseState currentState, Point mouseLoc)
        {
            if (currentState.LeftButton != previousMouse.LeftButton)
            {
                string selected = ButtonCheck(currentState, mouseLoc).ToLower();

                if (selected == "back")
                {
                    _gameState = GameState.MainMenu;
                    if (_player._connected)
                    {
                        _player.Disconnect();
                    }
                }

                if (selected == "" && currentState.LeftButton == ButtonState.Pressed && (_player._currentTurn == true || _player._connected == false))
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
                        else if (button._status == Selected && previousMouse.LeftButton == ButtonState.Pressed && currentState.LeftButton == ButtonState.Released)
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

                foreach (TextBox textbox in _textbox)
                {
                    if (new Rectangle(textbox._pos, textbox._size).Contains(mouseLoc))
                    {
                        if (previousMouse.LeftButton == ButtonState.Released && currentState.LeftButton == ButtonState.Pressed)
                        {
                            for (int i = 0; i < _textbox.Count; i++)
                            {
                                _textbox[i]._status = Unselected;
                            }
                            textbox._status = Selected;
                        }
                    }
                }
            }

            return text;
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.LightGray);
            
            _spriteBatch.Begin(SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            null, null, null, null);

            Rectangle viewPort = GraphicsDevice.Viewport.Bounds;

            if (_gameState == GameState.InGame || _gameState == GameState.EscMenu)
            {
                _player._board.Draw(gameTime, _spriteBatch, viewPort);
            }

            foreach (Button button in _button)
            {
                button.Draw(gameTime, _spriteBatch, viewPort);
            }

            foreach (TextBox textbox in _textbox)
            {
                textbox.Draw(gameTime, _spriteBatch, viewPort);
            }

            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
