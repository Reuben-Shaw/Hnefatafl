#undef DEBUG
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Hnefatafl.MenuObjects;
using Hnefatafl.Media;
using static Hnefatafl.MenuObjects.MenuObject.Status;
using static Hnefatafl.Player.InstructType;
using static Hnefatafl.ServerOptions;
using static Hnefatafl.UserOptions.ColourButtons;

namespace Hnefatafl
{
    public class Hnefatafl : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Cursor _cursor;
        private Player _player;
        private Server _server; 
        private List<Button> _button = new List<Button>();
        private List<TextBox> _textbox = new List<TextBox>();
        private List<Label> _label  = new List<Label>();
        private ColourPicker _picker;
        private TextureDivide _buttonSelect, _buttonUnselect;
        private TextureDivide _menuBack;
        private UserOptions _userOptions;

        private enum GameState { MainMenu, OptionsMenu, ColourPickerMenu, MultiplayerMenu, ServerMenu, GameSetup, InGame, EscMenu };
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

        private enum Langauge { English }
        private Langauge _language = Langauge.English;

        public Hnefatafl()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
        }

        const bool _doFull = false;
        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = 960;
            _graphics.PreferredBackBufferHeight = 540;
            FullScreen();
            _graphics.ApplyChanges();
            
            _cursor = new Cursor(Content);

            XmlSerializer ser = new XmlSerializer(typeof(UserOptions));
            using (XmlReader reader = XmlReader.Create(AppDomain.CurrentDomain.BaseDirectory + "Options.xml"))
            {
                _userOptions = (UserOptions)ser.Deserialize(reader);
            }
            Console.WriteLine(_userOptions.ToString());

            _player = new Player(_graphics, Content, _userOptions, 11);

            _picker = new ColourPicker(new Point(20, 20), GraphicsDevice.Viewport.Bounds, "mainPicker", _player._board.TileSizeY(GraphicsDevice.Viewport.Bounds) * 2);
            _picker.ChangeColour(_graphics, GraphicsDevice.Viewport.Bounds, true);

            _gameState = GameState.MainMenu;

            // UserOptions ops = new UserOptions(new Color(255, 76, 74), new Color(54, 56, 255), new Color[] { new Color(173, 99, 63), new Color(80, 53, 30), new Color(0, 0, 0), new Color(0, 0, 0), new Color(175, 0, 0), new Color(249, 200, 24) });
            // XmlSerializer xsSubmit = new XmlSerializer(typeof(UserOptions));
            
            // using(StringWriter writerS = new StringWriter())
            // {
            //     using(XmlWriter writerX = XmlWriter.Create(writerS))
            //     {
            //         System.IO.FileStream file = System.IO.File.Create(AppDomain.CurrentDomain.BaseDirectory + "Options.xml");  
            //         xsSubmit.Serialize(file, ops);
            //         file.Close();  
            //     }
            // }

            base.Initialize();
        }

        [Conditional("DEBUG")]
        private void FullScreen()
        {
            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;
            _graphics.IsFullScreen = true;
            _graphics.HardwareModeSwitch = true;
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _buttonSelect = new TextureDivide(_graphics, Content, "Texture/Menu/ButtonDivideSelect", _player._board.TileSizeX(GraphicsDevice.Viewport.Bounds), _player._board.TileSizeY(GraphicsDevice.Viewport.Bounds));
            _buttonUnselect = new TextureDivide(_graphics, Content, "Texture/Menu/ButtonDivideUnselect", _player._board.TileSizeX(GraphicsDevice.Viewport.Bounds), _player._board.TileSizeY(GraphicsDevice.Viewport.Bounds));
            _buttonSelect.HueShiftTexture(Color.DarkGray);
            _buttonUnselect.HueShiftTexture(Color.Gray);

            _picker.LoadContent(_graphics, Content, _player._board.TileSizeX(GraphicsDevice.Viewport.Bounds), _player._board.TileSizeY(GraphicsDevice.Viewport.Bounds));

            _menuBack = new TextureDivide(_graphics, Content, "Texture/Menu/BackMenuDivide", _player._board.TileSizeX(GraphicsDevice.Viewport.Bounds), _player._board.TileSizeY(GraphicsDevice.Viewport.Bounds));

            Console.WriteLine("Successful LoadContent");
        }

        protected override void UnloadContent()
        {
            _player._board.UnloadContent();
            _cursor.UnloadContent();
            _buttonSelect.UnloadContent();
            _buttonUnselect.UnloadContent();
            _picker.UnloadContent();
            _menuBack.UnloadContent();

            Console.WriteLine("Successful UnloadContent");
        }

        KeyboardState previousKeyboard; MouseState previousMouse;
        protected override void Update(GameTime gameTime)
        {
            if (_server is not null)
            {
                _server.ReadMessages();
            }

            _player.CheckMessage();

            if (_gameState != GameState.InGame && _gameState != GameState.EscMenu && _player.IsConnected())
            {
                _player._currentTurn = false;
                _gameState = GameState.InGame;
            }

            MouseState currentMouseState = Mouse.GetState();
            KeyboardState currentKeyboardState = Keyboard.GetState();
            Point mouseLoc = new Point(currentMouseState.Position.X, currentMouseState.Position.Y);

            switch (_gameState)
            {
                case GameState.MainMenu:
                {
                    MainMenu(gameTime, currentMouseState, mouseLoc);
                    break;
                }
                case GameState.OptionsMenu:
                {
                    OptionsMenu(currentMouseState, mouseLoc);
                    break;
                }
                case GameState.ColourPickerMenu:
                {
                    ColourPickerMenu(currentMouseState, mouseLoc);
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
                case GameState.GameSetup:
                {
                    GameSetup(gameTime, currentMouseState, mouseLoc);
                    break;
                }
                case GameState.InGame:
                {
                    InGame(gameTime, currentMouseState, currentKeyboardState, mouseLoc);
                    break;
                }
                case GameState.EscMenu:
                {
                    
                    break;
                }
            }

            if (previousKeyboard != currentKeyboardState || currentKeyboardState.IsKeyDown(Keys.Back))
            {
                foreach (TextBox textbox in _textbox)
                {
                    if (textbox._status == Selected)
                    {
                        foreach (Keys key in currentKeyboardState.GetPressedKeys())
                        {
                            if (!previousKeyboard.IsKeyDown(key) || key == Keys.Back)
                            {
                                if (key != Keys.Back)
                                {
                                    if (key == Keys.Enter)
                                    {
                                        textbox._status = MenuObject.Status.Unselected;
                                        if (_picker._visible)
                                        {
                                            if (_picker.SetTextboxData(textbox._name, textbox._text)) _picker.ChangeColour(_graphics, GraphicsDevice.Viewport.Bounds, false);
                                            _textbox[0]._text = _picker.R.ToString();
                                            _textbox[1]._text = _picker.G.ToString();
                                            _textbox[2]._text = _picker.B.ToString();
                                        }
                                    }
                                    else
                                    {
                                        textbox.Add(key);
                                    }
                                }
                                else
                                {
                                    textbox.RemoveChar(gameTime.ElapsedGameTime.TotalSeconds);
                                }
                            }
                        }
                    }
                }
            }

            if (_picker._visible) PickerCheck(currentMouseState);
            
            _cursor._pos = currentMouseState.Position;

            previousMouse = currentMouseState;
            previousKeyboard = currentKeyboardState;
            base.Update(gameTime);
        }

        private void TransferState()
        {
            _button.Clear();
            _textbox.Clear();
            _cursor._state = Cursor.CursorState.Pointer;
            _picker._visible = false;

            Rectangle viewPorts = GraphicsDevice.Viewport.Bounds;
            Point buttonSize = new Point((int)((float)viewPorts.Width / 3), (int)((float)viewPorts.Height / 5.5));
            Point textboxSize = new Point((int)((float)viewPorts.Width / 4), (int)((float)viewPorts.Height / 12));
            Point labelSize = new Point(256, 48);

            List<string> buttonName = new List<string>();
            List<string> textboxName = new List<string>();
            List<string> labelName = new List<string>();
            Point startLoc = new Point(0, 0);

            switch (_gameState)
            {
                case GameState.MainMenu:
                {
                    buttonName.Add("multiplayer");
                    buttonName.Add("localCoOp");
                    buttonName.Add("options");
                    buttonName.Add("exit");
                    startLoc = new Point(buttonSize.X / 4, buttonSize.Y / 2);
                    break;
                }
                case GameState.OptionsMenu:
                {
                    buttonName.Add("pawnD");
                    buttonName.Add("pawnA");
                    buttonName.Add("board1");
                    buttonName.Add("board2");
                    buttonName.Add("throne");
                    buttonName.Add("corner");
                    buttonName.Add("highlightTrail");
                    buttonName.Add("boardD");
                    buttonName.Add("boardA");
                    buttonName.Add("back");
                    //0: Board main colour 1, 1: Board main colour 2, 2: Defender board colour, 3: Attacker board colour, 4: Throne, 5: Corner, 6: Highlight colour
                    buttonSize = new Point(viewPorts.Width / 5, (int)((float)viewPorts.Height / 8));
                    startLoc = new Point(buttonSize.X / 4, buttonSize.Y / 2);
                    break;
                }
                case GameState.ColourPickerMenu:
                {
                    _picker._visible = true;
                    buttonName.Add("default");
                    buttonName.Add("back");
                    textboxName.Add("r");
                    textboxName.Add("g");
                    textboxName.Add("b");
                    
                    buttonSize = new Point(viewPorts.Width / 5, (int)((float)viewPorts.Height / 8));
                    startLoc = new Point(viewPorts.Width - (viewPorts.Width / 15) - buttonSize.X, viewPorts.Height - (int)(buttonSize.Y * 1.5) - ((viewPorts.Width / 20) * 2));
                    break;
                }
                case GameState.MultiplayerMenu:
                {
                    buttonName.Add("host");
                    buttonName.Add("connect");
                    buttonName.Add("shutdown");
                    buttonName.Add("back");
                    startLoc = new Point(viewPorts.Width / 2 - buttonSize.X / 2, buttonSize.Y / 2);
                    break;
                }
                case GameState.ServerMenu:
                {
                    textboxName.Add("ip");
                    textboxName.Add("port");
                    buttonName.Add("connect");
                    buttonName.Add("back");
                    buttonSize = new Point(viewPorts.Width / 5, (int)((float)viewPorts.Height / 8));
                    startLoc = new Point(viewPorts.Width / 2 - textboxSize.X / 2, textboxSize.Y * 2);
                    break;
                }
                case GameState.GameSetup:
                {
                    buttonName.Add("playerTurnAttacker");
                    buttonName.Add("playerTurnDefender");
                    buttonName.Add("throneDisabled");
                    buttonName.Add("throneKing");
                    buttonName.Add("throneDefenderKing");
                    buttonName.Add("kingArmed");
                    buttonName.Add("kingUnarmed");
                    buttonName.Add("sandwichDisabled");
                    buttonName.Add("sandwichEnabled");
                    buttonName.Add("captureDisabled");
                    buttonName.Add("captureCorner");
                    buttonName.Add("captureCornerThrone");
                    buttonName.Add("kingCaptureJustThree");
                    buttonName.Add("kingCaptureAllDefendersThree");
                    buttonName.Add("winCorner");
                    buttonName.Add("winSide");
                    buttonName.Add("host");
                    textboxName.Add("port");
                    buttonSize = new Point(viewPorts.Width / 30 * 2, viewPorts.Width / 30 * 2);
                    startLoc = new Point(viewPorts.Width / 16, viewPorts.Height / 8);
                    break;
                }
                case GameState.InGame:
                {
                    buttonName.Add("back");
                    startLoc = new Point(viewPorts.Width / 30, viewPorts.Height - buttonSize.Y);
                    buttonSize = new Point(viewPorts.Width / 5, (int)((float)viewPorts.Height / 8));
                    _cursor._state = Cursor.CursorState.OpenHand;
                    break;
                }
                case GameState.EscMenu:
                {
                    break;
                }
            }
            
            XmlElement xml = MenuXmlLoad();

            if (_gameState == GameState.MainMenu || _gameState == GameState.MultiplayerMenu || _gameState == GameState.InGame)
            {
                for (int i = 0; i < buttonName.Count; i++)
                {
                    _button.Add(new Button(new Point(startLoc.X, startLoc.Y + (buttonSize.Y * i) + ((buttonSize.Y / 4) * i)), buttonSize, NodeText(xml, buttonName[i]), buttonName[i]));
                    _button[i].Update(_graphics, Content);
                }
            }
            else if (_gameState == GameState.OptionsMenu)
            {
                for (int i = 0; i < labelName.Count; i++)
                {
                    // _label.Add(new Label(new Point(startLoc.X, startLoc.Y + ((labelSize.X * i) / 2)), labelSize, NodeText(xml, labelName[i]), _player._board.));
                    // _label[i].Update(_graphics, Content);
                }

                int yMod = 0;
                for (int i = 0; i < buttonName.Count; i++)
                {
                    _button.Add(new Button(new Point(startLoc.X, startLoc.Y + (buttonSize.Y * (i - yMod)) + ((buttonSize.Y / 4) * (i - yMod))), buttonSize, NodeText(xml, buttonName[i]), buttonName[i]));
                    _button[i].Update(_graphics, Content);

                    if (i != 0 && i % 5 == 0)
                    {
                        startLoc = new Point((buttonSize.X / 4) + ((buttonSize.X + (buttonSize.X / 4)) * (i / 4)), buttonSize.Y / 2);
                        yMod += 5;
                    }
                }
            }
            else if (_gameState == GameState.ColourPickerMenu)
            {
                _picker._pos = new Point(_player._board.TileSizeX(GraphicsDevice.Viewport.Bounds) * 2, _player._board.TileSizeY(GraphicsDevice.Viewport.Bounds) * 2);
                for (int i = 0; i < buttonName.Count; i++)
                {
                    _button.Add(new Button(new Point(startLoc.X, startLoc.Y + (buttonSize.Y * i) + ((buttonSize.Y / 4) * i)), buttonSize, NodeText(xml, buttonName[i]), buttonName[i]));
                    _button[i].Update(_graphics, Content);
                }

                for (int i = 0; i < textboxName.Count; i++)
                {
                    _textbox.Add(new TextBox(_picker.GetTextboxData(i).Location, _picker.GetTextboxData(i).Size, textboxName[i]));
                    _textbox[i].Update(_graphics, Content);
                }
                _textbox[0]._text = _picker.R.ToString();
                _textbox[1]._text = _picker.G.ToString();
                _textbox[2]._text = _picker.B.ToString();
            }
            else if (_gameState == GameState.ServerMenu)
            {
                for (int i = 0; i < textboxName.Count; i++)
                {
                    _textbox.Add(new TextBox(new Point(startLoc.X, startLoc.Y + ((textboxSize.X * i) / 2)), textboxSize, NodeText(xml, textboxName[i]), textboxName[i]));
                    _textbox[i].Update(_graphics, Content);
                }

                startLoc = new Point(viewPorts.Width / 2 - buttonSize.X / 2, _textbox[textboxName.Count - 1]._pos.Y);
                for (int i = textboxName.Count; i < buttonName.Count + textboxName.Count; i++)
                {
                    _button.Add(new Button(new Point(startLoc.X, startLoc.Y + (buttonSize.Y * i) + ((buttonSize.Y / 4) * i)), buttonSize, NodeText(xml, buttonName[i - textboxName.Count]), buttonName[i - textboxName.Count]));
                    _button[(i - textboxName.Count)].Update(_graphics, Content);
                }
            }
            else if (_gameState == GameState.GameSetup)
            {
                int[] splitList = new int[] {2, 3, 2, 2, 3, 2, 2};
                int splitTrack = 0, pointer = 0;

                for (int i = 0; i < buttonName.Count - 1; i++)
                {
                    splitTrack++;

                    _button.Add(new Button(new Point(startLoc.X + ((buttonSize.X * splitTrack) + ((buttonSize.X / 4) * splitTrack)), startLoc.Y), buttonSize, Content.Load<Texture2D>("Texture/OpButton/" + buttonName[i]), buttonName[i]));
                    _button[i].Update(_graphics, Content);
                    
                    if (splitTrack == splitList[pointer])
                    {
                        pointer++;
                        splitTrack = 0;
                        if (pointer >= 3)
                            startLoc.X = (buttonSize.X * 4) + ((buttonSize.X / 4) * 4) + viewPorts.Width / 16;
                        else
                            startLoc.X = viewPorts.Width / 16;
                        
                        if (pointer != 3)
                            startLoc.Y += buttonSize.Y + (buttonSize.Y / 2);
                        else
                            startLoc.Y = viewPorts.Height / 8;
                    }
                }

                buttonSize = new Point(viewPorts.Width / 5, (int)((float)viewPorts.Height / 5.5));
                _button.Add(new Button(new Point(viewPorts.Width / 2 + viewPorts.Width / 4, viewPorts.Height / 2 + viewPorts.Height / 4), buttonSize, NodeText(xml, buttonName[buttonName.Count - 1]), buttonName[buttonName.Count - 1]));
                _button[_button.Count - 1].Update(_graphics, Content);
            }
        }

        private string NodeText(XmlElement xml, string objectName)
        {
            try
            {
                return xml.SelectSingleNode("//Language/" + _gameState.ToString() + "/" + objectName).InnerText;
            }
            catch (System.Exception)
            {
                return null;
            }
        }

        private XmlElement MenuXmlLoad()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(AppDomain.CurrentDomain.BaseDirectory + "Content/Language/" + _language.ToString() + "/Menu.xml");
            return doc.DocumentElement;
        }

        private void MainMenu(GameTime gameTime, MouseState currentState, Point mouseLoc)
        {
            string selected = ButtonCheck(currentState, mouseLoc);

            switch (selected)
            {
                case "multiplayer":
                {
                    _gameState = GameState.MultiplayerMenu;
                    break;
                }
                case "localCoOp":
                {
                    _player._board.CreateBoard();
                    _player._board._state = Board.BoardState.ActiveGame;
                    _gameState = GameState.InGame;
                    _player._side = Player.SideType.Attackers;
                    _player._currentTurn = true;
                    break;
                }
                case "options":
                {
                    _gameState = GameState.OptionsMenu;
                    break;
                }
                case "exit":
                {
                    Exit();
                    break;
                }
            }
        }

        private void OptionsMenu(MouseState currentState, Point mouseLoc)
        {
            string selected = ButtonCheck(currentState, mouseLoc);

            _picker.UnloadExampleImage();

            if (selected == "back")
            {
                XmlSerializer xsSubmit = new XmlSerializer(typeof(UserOptions));
                using(StringWriter writerS = new StringWriter())
                {
                    using(XmlWriter writerX = XmlWriter.Create(writerS))
                    {
                        System.IO.FileStream file = System.IO.File.Create(AppDomain.CurrentDomain.BaseDirectory + "Options.xml");  
                        xsSubmit.Serialize(file, _userOptions);
                        file.Close();  
                    }
                }
                _player._board.ReloadContent(_graphics, _userOptions);
                _gameState = GameState.MainMenu;
            }
            else if (!string.IsNullOrWhiteSpace(selected))
            {
                switch ((UserOptions.ColourButtons)Enum.Parse(typeof(UserOptions.ColourButtons), selected))
                {
                    case pawnD:
                    {
                        _gameState = GameState.ColourPickerMenu;
                        _picker.SetExampleImage("Texture/Pawn/PawnD");
                        _picker.SetDisplayColour(_userOptions._pawnDefender);
                        break;
                    }
                    case pawnA:
                    {
                        _gameState = GameState.ColourPickerMenu;
                        _picker.SetExampleImage("Texture/Pawn/PawnA");
                        _picker.SetDisplayColour(_userOptions._pawnAttacker);
                        break;
                    }
                    case board1:
                    {
                        _gameState = GameState.ColourPickerMenu;
                        _picker.SetDisplayColour(_userOptions._boardColours[0]);
                        break;
                    }
                    case board2:
                    {
                        _gameState = GameState.ColourPickerMenu;
                        _picker.SetDisplayColour(_userOptions._boardColours[1]);
                        break;
                    }
                    case throne:
                    {
                        _gameState = GameState.ColourPickerMenu;
                        _picker.SetDisplayColour(_userOptions._boardColours[4]);
                        break;
                    }
                    case corner:
                    {
                        _gameState = GameState.ColourPickerMenu;
                        _picker.SetDisplayColour(_userOptions._boardColours[5]);
                        break;
                    }
                    case highlightTrail:
                    {
                        _gameState = GameState.ColourPickerMenu;
                        _picker.SetDisplayColour(_userOptions._selectColours[0]);
                        break;
                    }
                    case selectPositive:
                    {
                        _gameState = GameState.ColourPickerMenu;
                        _picker.SetDisplayColour(_userOptions._selectColours[1]);
                        break;
                    }
                    case selectNegative:
                    {
                        _gameState = GameState.ColourPickerMenu;
                        _picker.SetDisplayColour(_userOptions._selectColours[2]);
                        break;
                    }
                    case boardD:
                    {
                        _gameState = GameState.ColourPickerMenu;
                        _picker.SetDisplayColour(_userOptions._boardColours[2]);
                        break;
                    }
                    case boardA:
                    {
                        _gameState = GameState.ColourPickerMenu;
                        _picker.SetDisplayColour(_userOptions._boardColours[3]);
                        break;
                    }
                }
                
                _picker.ChangeColour(_graphics, GraphicsDevice.Viewport.Bounds, false);
                _picker.Rename(selected);
            }
        }

        private void ColourPickerMenu(MouseState currentState, Point mouseLoc)
        {
            string selected = ButtonCheck(currentState, mouseLoc);

            switch (selected)
            {
                case "back":
                {
                    _userOptions.SetFromButton(_picker._name, _picker.GetColour());
                    _gameState = GameState.OptionsMenu;
                    break;
                }
                case "default":
                {
                    _picker.SetDisplayColour(_userOptions.GetDefaultColor((UserOptions.ColourButtons)Enum.Parse(typeof(UserOptions.ColourButtons), _picker._name)));
                    _picker.ChangeColour(_graphics, GraphicsDevice.Viewport.Bounds, false);
                    break;
                }
            }
        }

        private void MultiplayerMenu(GameTime gameTime, MouseState currentState, Point mouseLoc)
        {
            string selected = ButtonCheck(currentState, mouseLoc);

            switch (selected)
            {
                case "host":
                {
                    _player._board.CreateBoard();
                    _gameState = GameState.GameSetup;
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
            string selected = ButtonCheck(currentState, mouseLoc);

            switch (selected)
            {
                case "connect":
                {
                    //Console.WriteLine($"Valid IP Address: {IsValidIp(_textbox[0]._text)}\nValid Port: {IsValidPort(_textbox[1]._text)}");
                    if (IsValidIp(_textbox[0]._text) && IsValidPort(_textbox[1]._text)) //Textbox[0] is IP, Textbox[1] is port
                    {
                        _player._board.CreateBoard();
                        _player.EstablishConnection(_textbox[0]._text, Convert.ToInt32(_textbox[1]._text));
                    }
                    else if (_textbox[0]._text == "" && _textbox[1]._text == "")
                    {
                        _player._board.CreateBoard();
                        _player.EstablishConnection("localhost", Convert.ToInt32("14242"));
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

        private void GameSetup(GameTime gameTime, MouseState currentState, Point mouseLoc)
        {
            string selected = ButtonCheck(currentState, mouseLoc);

            switch (selected)
            {
                case "host":
                {
                    if (_server is null)
                    {
                        _server = new Server();
                        _server.StartServer(14242, _player._board._serverOp);
                        _player.EstablishConnection("localhost", 14242);
                    }
                    break;
                }
                case "playerTurnAttacker":
                {
                    _player._board._serverOp._playerTurn = PlayerTurn.Attacker;
                    _button[3]._status = Disabled;
                    break;
                }
                case "playerTurnDefender":
                {
                    _player._board._serverOp._playerTurn = PlayerTurn.Defender;
                    break;
                }
                case "throneDisabled":
                {
                    _player._board._serverOp._throneOp = ThroneOp.Disabled;
                    break;
                }
                case "throneKing":
                {
                    _player._board._serverOp._throneOp = ThroneOp.King;
                    break;
                }
                case "throneDefenderKing":
                {
                    _player._board._serverOp._throneOp = ThroneOp.DefenderKing;
                    break;
                }
                case "kingArmed":
                {
                    _player._board._serverOp._kingOp = KingOp.Armed;
                    break;
                }
                case "kingUnarmed":
                {
                    _player._board._serverOp._kingOp = KingOp.Unarmed;
                    break;
                }
                case "sandwichDisabled":
                {
                    _player._board._serverOp._sandwichMovementOp = SandwichMovementOp.Disabled;
                    break;
                }
                case "sandwichEnabled":
                {
                    _player._board._serverOp._sandwichMovementOp = SandwichMovementOp.Enabled;
                    break;
                }
                case "captureDisabled":
                {
                    _player._board._serverOp._captureOp = CaptureOp.Disabled;
                    break;
                }
                case "captureCorner":
                {
                    _player._board._serverOp._captureOp = CaptureOp.Corner;
                    break;
                }
                case "captureCornerThrone":
                {
                    _player._board._serverOp._captureOp = CaptureOp.CornerThrone;
                    break;
                }
                case "kingCaptureJustThree":
                {
                    _player._board._serverOp._kingCaptureOp = KingCaptureOp.JustThree;
                    break;
                }
                case "kingCaptureAllDefendersThree":
                {
                    _player._board._serverOp._kingCaptureOp = KingCaptureOp.AllDefendersThree;
                    break;
                }
                case "winCorner":
                {
                    _player._board._serverOp._winOp = WinOp.Corner;
                    break;
                }
                case "winSide":
                {
                    _player._board._serverOp._winOp = WinOp.Side;
                    break;
                }
            }
        }

        double previousInput = 0;
        private void InGame(GameTime gameTime, MouseState currentMouseState, KeyboardState currentKeyboardState, Point mouseLoc)
        {
            Rectangle viewPort = GraphicsDevice.Viewport.Bounds;

            if (currentMouseState.Position != previousMouse.Position)
                _cursor._hidden = false;

            if (currentMouseState.LeftButton != previousMouse.LeftButton)
            {
                string selected = ButtonCheck(currentMouseState, mouseLoc);

                if (currentMouseState.LeftButton == ButtonState.Released) _cursor._state = Cursor.CursorState.OpenHand;
                else _cursor._state = Cursor.CursorState.ClosedHand;

                if (selected == "back")
                {
                    _gameState = GameState.MainMenu;
                    if (_player.IsConnected())
                    {
                        _player.Disconnect();
                    }

                    if (_server is not null)
                    {
                        _server.StopServer();
                    }
                }

                if (_player._board._state == Board.BoardState.ActiveGame && _player._currentTurn)
                {
                    Movement(mouseLoc, viewPort, currentMouseState.LeftButton == ButtonState.Released);
                }
            }

            Point selectOffset = new Point(0, 0);
            bool resetPreviousInput = false;

            foreach (Keys key in currentKeyboardState.GetPressedKeys())
            {
                switch (key)
                {
                    case Keys.Up:
                    case Keys.W:
                    {
                        if (previousInput > 0.12)
                        {
                            selectOffset.Y -= _player._board.TileSizeY(viewPort);
                            _cursor._hidden = true;
                            resetPreviousInput = true;
                        }
                        break;
                    }
                    case Keys.Right:
                    case Keys.D:
                    {
                        if (previousInput > 0.12)
                        {
                            selectOffset.X += _player._board.TileSizeX(viewPort);
                            _cursor._hidden = true;
                            resetPreviousInput = true;
                        }
                        break;
                    }
                    case Keys.Down:
                    case Keys.S:
                    {
                        if (previousInput > 0.12)
                        {
                            selectOffset.Y += _player._board.TileSizeY(viewPort);
                            _cursor._hidden = true;
                            resetPreviousInput = true;
                        }
                        break;
                    }
                    case Keys.Left:
                    case Keys.A:
                    {
                        if (previousInput > 0.12)
                        {
                            selectOffset.X -= _player._board.TileSizeX(viewPort);
                            _cursor._hidden = true;
                            resetPreviousInput = true;
                        }
                        break;
                    }
                    case Keys.Enter:
                    case Keys.E:
                    {
                        if (!previousKeyboard.IsKeyDown(Keys.E) && !previousKeyboard.IsKeyDown(Keys.Enter) && _player._board._state == Board.BoardState.ActiveGame && _player._currentTurn)
                        {
                            Movement(mouseLoc, viewPort, _player._board.IsPieceSelected());
                        }
                        break;
                    }
                    case Keys.R:
                    {
                        _player._board.CreateBoard();
                        break;
                    }
                }
            }

            if (_cursor._hidden)
            {
                Point newPos = new Point(currentMouseState.X + selectOffset.X, currentMouseState.Y + selectOffset.Y);
                Rectangle boardArea = new Rectangle((viewPort.Width / 2) - ((_player._board.TileSizeX(viewPort) * _player._board._boardSize) / 2), (viewPort.Height / 2) - ((_player._board.TileSizeY(viewPort) * _player._board._boardSize) / 2), _player._board.TileSizeX(viewPort) * _player._board._boardSize, _player._board.TileSizeY(viewPort) * _player._board._boardSize);

                if (!boardArea.Contains(newPos))
                {
                    if (newPos.X < boardArea.X)
                    {
                        newPos.X = boardArea.X;
                    }
                    else if (newPos.X > boardArea.X + boardArea.Width - _player._board.TileSizeX(viewPort))
                    {
                        newPos.X = boardArea.X + boardArea.Width - _player._board.TileSizeX(viewPort);
                    }

                    if (newPos.Y < boardArea.Y)
                    {
                        newPos.Y = boardArea.Y;
                    }
                    else if (newPos.Y > boardArea.Y + boardArea.Height - _player._board.TileSizeY(viewPort))
                    {
                       newPos.Y = boardArea.Y + boardArea.Height - _player._board.TileSizeY(viewPort);
                    }
                }

                Mouse.SetPosition(newPos.X, newPos.Y);
            }
            
            if (resetPreviousInput)
            {
                previousInput = 0;
            }
            previousInput += gameTime.ElapsedGameTime.TotalSeconds;
        }

        private void Movement(Point mouseLoc, Rectangle viewPort, bool moving)
        {
            HPoint point = new HPoint(
                (mouseLoc.X - (viewPort.Width / 2) + ((_player._board.TileSizeX(viewPort) * _player._board._boardSize) / 2)) / _player._board.TileSizeX(viewPort),
                (mouseLoc.Y - (viewPort.Height / 2) + ((_player._board.TileSizeY(viewPort) * _player._board._boardSize) / 2)) / _player._board.TileSizeY(viewPort)
                );
            
            if (moving && _player._board.IsPieceSelected())
            {
                _player._board.SelectHighlightColour(null);
                if (_player._board.MakeMove(point, _player._side, !_player.IsConnected()))
                {
                    _player._repeatedMoveChk.Add(point.ToString()); //Deals with repeated moves by adding to a list
                    if (_player._repeatedMoveChk.Count > 2 && _player._repeatedMoveChk[_player._repeatedMoveChk.Count - 1] == _player._repeatedMoveChk[_player._repeatedMoveChk.Count - 3])
                    {
                        Console.WriteLine("Repeated detected");
                        if (_player._repeatedMoveChk.Count == 6)
                        {
                            Console.WriteLine("Loss due to repeat");
                            _player.SendMessage(WIN.ToString() + "," + point.ToString());
                        }
                        else
                        {
                            _player.SendMessage(MOVE.ToString() + "," + point.ToString());
                        }
                    }
                    else
                    {
                        if (_player._repeatedMoveChk.Count > 3)
                        {
                            for (int i = _player._repeatedMoveChk.Count - 1; i > 1; i--)
                            {
                                _player._repeatedMoveChk.RemoveAt(i);
                            }
                        }
                        else if (_player._repeatedMoveChk.Count == 3)
                        {
                            _player._repeatedMoveChk.RemoveAt(0);
                        }
                        _player.SendMessage(MOVE.ToString() + "," + point.ToString());
                    }

                    if (_player.IsConnected()) _player._currentTurn = false; //Turns for local co-op
                    else
                    { 
                        if (_player._side == Player.SideType.Attackers) {_player._side = Player.SideType.Defenders; _player._currentTurn = true;} 
                        else {_player._side = Player.SideType.Attackers; _player._currentTurn = true;}
                    }
                }
                else
                {
                    _player.SendMessage(MOVEFAIL.ToString());
                }
            }
            else if (!moving)
            {
                if (!_player._board.IsPieceSelected())
                {        
                    _player._board.SelectPiece(point, _player._side);
                    _player.SendMessage(SELECT.ToString() + "," + point.ToString());
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
                            text = button._name;
                        }
                    }
                    else if (button._status == Selected)
                    {
                        button._status = Unselected;
                    }
                }

                foreach (TextBox textbox in _textbox)
                {
                    bool initialSelect = (textbox._status == Selected);
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
                    else if (previousMouse.LeftButton == ButtonState.Released && currentState.LeftButton == ButtonState.Pressed)
                    {
                        textbox._status = Unselected;
                    }
                    
                    if (_picker._visible && initialSelect && textbox._status == Unselected)
                    {
                        if (_picker.SetTextboxData(textbox._name, textbox._text)) _picker.ChangeColour(_graphics, GraphicsDevice.Viewport.Bounds, false);
                        _textbox[0]._text = _picker.R.ToString();
                        _textbox[1]._text = _picker.G.ToString();
                        _textbox[2]._text = _picker.B.ToString();
                    }
                }
            }

            return text;
        }

        private void PickerCheck(MouseState currentState)
        {
            if (currentState.Position != previousMouse.Position)
            {
                if (currentState.LeftButton == ButtonState.Pressed)
                {
                    if (_picker.ClickEvent(currentState)) _picker.ChangeColour(_graphics, GraphicsDevice.Viewport.Bounds, false);
                    _textbox[0]._text = _picker.R.ToString();
                    _textbox[1]._text = _picker.G.ToString();
                    _textbox[2]._text = _picker.B.ToString();
                }
            }

            if (currentState.LeftButton == ButtonState.Released && previousMouse.LeftButton == ButtonState.Pressed)
            {
                if (_picker.ClickEvent(currentState)) _picker.ChangeColour(_graphics, GraphicsDevice.Viewport.Bounds, false);
                _textbox[0]._text = _picker.R.ToString();
                _textbox[1]._text = _picker.G.ToString();
                _textbox[2]._text = _picker.B.ToString();
                _picker.Deselect();
            }
        }

        private bool IsValidIp(string ipChk)
        {
            Regex r =  new Regex(@"^((25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[0-9][0-9]|[0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[0-9][0-9]|[0-9])$");
            if (r.IsMatch(ipChk))
            {
                return true;
            }

            return false;
        }

        private bool IsValidPort(string portChk)
        {
            Regex r =  new Regex(@"^(6[0-5]?[0-5]?[0-3]?[0-5]?|[0-5]?[0-9]?[0-9]?[0-9]?[0-9])$");
            if (r.IsMatch(portChk))
            {
                return true;
            }

            return false;
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.LightGray);
            
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise);

            Rectangle viewPort = GraphicsDevice.Viewport.Bounds;

            if (_gameState == GameState.InGame || _gameState == GameState.EscMenu)
            {
                Player.SideType? currentSide = _player._side;

                _player._board.Draw(_graphics, _spriteBatch, viewPort, currentSide, _player._currentTurn);
            }
            else if (_gameState == GameState.MainMenu)
            {
                _menuBack.Draw(_spriteBatch, 
                new Rectangle(_button[0]._size.X + (_button[0]._size.X / 3), _player._board.TileSizeX(viewPort) / 2, viewPort.Width / 2 + _player._board.TileSizeX(viewPort), viewPort.Height - _player._board.TileSizeY(viewPort)));
            
                // Label label = new Label(new Point(_button[0]._size.X + (_button[0]._size.X / 3), _player._board.TileSizeX(viewPort) / 2),
                // new Point(viewPort.Width / 2 + _player._board.TileSizeX(viewPort), viewPort.Height),
                // "test", "HNEFATAFL", 2.0f);

                // label.Update(_graphics, Content);

                // label.Draw(_spriteBatch, viewPort);
            }
            else if (_gameState == GameState.OptionsMenu || _gameState == GameState.ColourPickerMenu)
            {
                _menuBack.Draw(_spriteBatch, 
                new Rectangle(_player._board.TileSizeX(viewPort) / 2, _player._board.TileSizeY(viewPort) / 2, viewPort.Width - _player._board.TileSizeX(viewPort), viewPort.Height - _player._board.TileSizeY(viewPort)));
            }

            if (_picker._visible)
            {
                _picker.Draw(_spriteBatch, viewPort);
            }

            foreach (Button button in _button)
            {
                button.Draw(_spriteBatch, _player._board.TileSizeX(viewPort), _player._board.TileSizeY(viewPort), _buttonSelect, _buttonUnselect, viewPort);
            }

            foreach (TextBox textbox in _textbox)
            {
                textbox.Draw(gameTime, _spriteBatch, viewPort);
            }

            foreach (Label label in _label)
            {
                label.Draw(_spriteBatch, viewPort);
            }

            //_spriteBatch.Draw(_boardHighlightAtlas.GetTexture(DivideLink.UpLeft), new Rectangle(10, 10, 32, 32), Color.White);

            _cursor.Draw(_spriteBatch, viewPort);

            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
