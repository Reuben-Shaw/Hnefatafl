using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections;
using System.Collections.Generic;
using Hnefatafl.Audio;
using static Hnefatafl.ServerOptions;
using static Hnefatafl.PieceType;


namespace Hnefatafl
{
    public enum BoardTypes { Regular }

    sealed class Board
    {
        private readonly Texture2D[] _boardColours = new Texture2D[6];
        //0: Board main colour 1, 1: Board main colour 2, 2: Defender board colour, 3: Attacker board colour, 4: Throne, 5: Corner
        private readonly Texture2D[] _pawnTexture = new Texture2D[3];
        //0: Attacker, 1: Defender, 2: King
        private Pieces _pieces = new Pieces();
        //Generates the Piece object which contains a hashtable of all pieces, prevents need for mostly empty 2D array and better than dictionary for this method
        public int _boardSize;
        //9 or 11 depending on the users options

        public enum BoardState { ActiveGame, InactiveGame }
        public BoardState _state;

        private HPoint _selectedPiece = new HPoint(-1, -1);
        //Stores the current user selected piece, -1, -1 as expected is used as a null storing value
        public ServerOptions _serverOp;
        private BoardAudio _audioManager;

        public Board(GraphicsDeviceManager graphics, ContentManager Content, Color[] colours, int boardSize, BoardTypes boardType)
        {
            CreateColours(graphics, colours);
            CreatePawns(graphics, Content);
            _pieces.CreateBoard(boardSize, boardType);
            _audioManager = new BoardAudio(Content);
            _boardSize = boardSize;
        }

        public Board(GraphicsDeviceManager graphics, ContentManager Content, int boardSize)
        {
            CreateColours(graphics, new Color[]{new Color(173, 99, 63), new Color(80, 53, 30), new Color(0, 0, 0), new Color(0, 0, 0), new Color(175, 0, 0), new Color(249, 200, 24)});
            CreatePawns(graphics, Content);
            _audioManager = new BoardAudio(Content);
            _boardSize = boardSize;
        }

        public void CreatBoard()
        {
            _serverOp = new ServerOptions(); //Defines the options, currently empty as it will default to automatic options
            _pieces.CreateBoard(_boardSize, BoardTypes.Regular); //Responsible entirely for the creation of the pieces on the board, Board.cs doesn't contain any logic relating to this at all
            _selectedPiece = new HPoint(-1, -1);
            _state = BoardState.InactiveGame;
        }

        private void CreateColours(GraphicsDeviceManager graphics, Color[] colours)
        {
            //Creates the Texture2Ds that are used to draw the board from the user selected colours
            for (int i = 0; i < _boardColours.Length; i++)
            {
                _boardColours[i] = new Texture2D(graphics.GraphicsDevice, 1, 1);
                _boardColours[i].SetData(new[] { colours[i] });
            }
        }

        private void CreatePawns(GraphicsDeviceManager graphics, ContentManager Content)
        {
            _pawnTexture[0] = Content.Load<Texture2D>("Texture/Pawn/pawnA"); //Loading is passed by reference seemingly, this means that all sprites have to be different files to prevent the latter one being the colour of all of them, luckily I planned to do this from the start
            _pawnTexture[1] = Content.Load<Texture2D>("Texture/Pawn/pawnD");
            _pawnTexture[2] = Content.Load<Texture2D>("Texture/Pawn/king");

            Color[] userColour = new Color[]{ new Color(255, 76, 74), new Color(54, 56, 255),  new Color(54, 56, 255) }; //Pawn colours, currently hard set
            Color[] data; //Used to store a colour bitmap of the image, to be later applied once the new texture has been created in colour

            Texture2D alphaBlend; //Used to store an alpha channel that allows colours on a texture, such as the king's crown, to be drawn as the colour in the image file and not colour shifted
            for (int i = 0; i < _pawnTexture.Length; i++)
            {
                data = new Color[_pawnTexture[i].Width * _pawnTexture[i].Height]; //Sets data to the size of the sprite
                _pawnTexture[i].GetData<Color>(data); //Makes data contain a colour bitmap of the sprite
                try //Implementation of try catch due to the loading of the alphablend may fail causing the program to crash, in which case it loads as normal
                {   //I also believe this is faster than doing it with a check as it prevents and additional 1000 or so if checks and actually makes the alphablend loader faster
                    alphaBlend = Content.Load<Texture2D>(_pawnTexture[i].Name + "_a"); //_a is the appendage used to identify a texture it as an alpha layer 
                    Color[] alphaBlendArray = new Color[alphaBlend.Width * alphaBlend.Height];
                    alphaBlend.GetData<Color>(alphaBlendArray);

                    for (int j = 0; j < data.Length; j++)
                    {
                        if (alphaBlendArray[j] == Color.Black && data[j] != Color.Transparent && data[j] != Color.Black) 
                        {
                            data[j] = new Color((byte)((float)(data[j].R / 255f) * userColour[i].R), //Equation used to shift colour, 255f is important to facilitate float division otherwise it just does integer and then floats the value
                                                (byte)((float)(data[j].G / 255f) * userColour[i].G), 
                                                (byte)((float)(data[j].B / 255f) * userColour[i].B));
                        }
                    }

                    alphaBlend.Dispose(); //Disposing of alphablend to save memory like a good boy
                }
                catch (System.Exception)
                {
                    //Repeated loop as prior, but without the alphablend check, despite repeated code I still believe this to be the fastest way, CPU wise
                    for (int j = 0; j < data.Length; j++)
                    {
                        if (data[j] != Color.Transparent && data[j] != Color.Black)
                        {
                            data[j] = new Color((byte)((float)(data[j].R / 255f) * userColour[i].R), 
                                                (byte)((float)(data[j].G / 255f) * userColour[i].G), 
                                                (byte)((float)(data[j].B / 255f) * userColour[i].B));
                        }
                    }
                }

                _pawnTexture[i].SetData<Color>(data); //This line actually modifies the texture to assign it the new colouring
            }
        }

        public void UnloadContent() //Standard texture unloading, called from the main UnloadContent
        {
            for (int i = 0; i < _boardColours.Length; i++)
            {
                _boardColours[i].Dispose();
            }

            for (int i = 0; i < _pawnTexture.Length; i++)
            {
                _pawnTexture[i].Dispose();
            }
        }

        private bool DefendersRemain() //Used for reference when deciding if a three attacker takedown on a king is doable
        {
            foreach (DictionaryEntry entry in _pieces.AllPieces())
            {
                Piece piece = entry.Value as Piece; //Converts the DictionaryEntry into a Piece, as it will always be a Piece
                if (piece._pawn._type == Defender)
                    return true; //Foreach loop effectively converted into a while, I believe it's more efficient than using non-linear searches combined with sorting
            }
            return false;
        }

        private bool AttackersRemain() //Currently unused, but it will be used for victory conditions eventually
        {
            foreach (DictionaryEntry entry in _pieces.AllPieces())
            {
                Piece piece = entry.Value as Piece;
                if (piece._pawn._type == Attacker)
                    return true;
            }
            return false;
        }

        public void SelectPiece(HPoint select) //Used for selecting a piece, doesn't use PlayerSidePiece as it's used for the server selecting enemies as well
        {
            PieceType pawnChk = _pieces.GetPiece(select.ToString())._pawn._type;
            if (pawnChk != Empty && pawnChk != Throne && pawnChk != Corner)
            {
                _selectedPiece = select;
            }
            else
            {
                _selectedPiece = new HPoint(-1, -1);
            }
        }

        public bool IsPieceSelected() //Used in the main update loop to assertain if a click is to move or select
        {
            if (_selectedPiece.X != -1)
                return true;
            else
                return false;
        }

        public bool MakeMove(HPoint move, Player.SideType? side, bool doOverride) //Logic used to perform a move, and check if it is possible
        //Is a bool so it can return true if a move succeeds or false if it fails
        //doOveride is used for multiplayer to tell the game that despite being the wrong side that the move has to go through anyway
        {
            string key = move.ToString();
            if (key != _selectedPiece.ToString() && ((move.X >= 0 && move.X < _boardSize) && (move.Y >= 0 && move.Y < _boardSize)))
            {
                if (doOverride || PlayerSidePiece(_pieces.GetPiece(_selectedPiece.ToString())._pawn._type, side))
                {
                    if ((move.X == _selectedPiece.X || move.Y == _selectedPiece.Y) && ClearanceCheck(move.X, move.Y))
                    {
                        _pieces.AddTo(new Piece(_pieces.GetPiece(_selectedPiece.ToString())._pawn, move));
                        _pieces.GetPiece(key)._loc = move;

                        int b = _boardSize / 2;
                        if (_serverOp._throneOp == ThroneOp.Disabled || _selectedPiece.ToString() != b + "," + b)
                        {
                            _pieces.RemoveFrom(_selectedPiece.ToString());
                        }
                        else
                        {
                            _pieces.Replace(new Piece(new Pawn(Throne), new HPoint(_selectedPiece.ToString())));
                        }

                        _audioManager._move.Play();

                        CaptureLogic(move);
                        
                        _selectedPiece = new HPoint(-1, -1);
                        return true;
                    }
                    else if (((_pieces.GetPiece(key)._pawn._type == Throne || _pieces.GetPiece(key)._pawn._type == Corner) && _pieces.GetPiece(_selectedPiece.ToString())._pawn._type == King) ||
                            (_serverOp._throneOp == ThroneOp.DefenderKing && _pieces.GetPiece(key)._pawn._type == Throne && _pieces.GetPiece(_selectedPiece.ToString())._pawn._type == Defender))
                    {
                        _pieces.Replace(new Piece(new Pawn(_pieces.GetPiece(_selectedPiece.ToString())._pawn._type), new HPoint(key)));
                        _pieces.RemoveFrom(_selectedPiece.ToString());

                        _audioManager._move.Play();

                        CaptureLogic(move);
                        
                        _selectedPiece = new HPoint(-1, -1);
                        return true;
                    }
                }
            }
            _selectedPiece = new HPoint(-1, -1);
            return false;
        }

        private bool PlayerSidePiece(PieceType pieceType, Player.SideType? side) //Returns if a checked piece is on the same side as the player
        {
            if (side == Player.SideType.Attackers && pieceType == Attacker)
            {
                return true;
            }
            else if (side == Player.SideType.Defenders && (pieceType == Defender || pieceType == King))
            {
                return true;
            }
            return false;
        }

        private bool ClearanceCheck(int x, int y) //Used to check if a movement collides with anything on its path
        {
            int start, end;

            if (x > _selectedPiece.X)
            { start = _selectedPiece.X + 1; end = x + 1; } //+1 to account for the piece itself, otherwise it will always fail
            else
            { start = x; end = _selectedPiece.X; }

            for (int i = start; i < end; i++)
            {
                if (_pieces.GetPiece(i + "," + y)._pawn._type != Empty)
                    return false;
            }
            
            if (y > _selectedPiece.Y)
            { start = _selectedPiece.Y + 1; end = y + 1; }
            else
            { start = y; end = _selectedPiece.Y; }

            for (int i = start; i < end; i++)
            {
                if (_pieces.GetPiece(x + "," + i)._pawn._type != Empty)
                    return false;
            }

            return true;
        }

        private void CaptureLogic(HPoint loc) //Coordination centre piece for CaptureAttempt
        {
            HPoint[] locChk = new HPoint[2] { loc, loc };
            PieceType pieceMoved = _pieces.GetPiece(loc.ToString())._pawn._type;


            locChk[0] = new HPoint(loc.X + 1, loc.Y); locChk[1] = new HPoint(loc.X + 2, loc.Y); //Checks right
            CaptureAttempt(locChk[0], loc, pieceMoved, _pieces.GetPiece(locChk[0].ToString())._pawn._type, _pieces.GetPiece(locChk[1].ToString())._pawn._type);

            locChk[0] = new HPoint(loc.X - 1, loc.Y); locChk[1] = new HPoint(loc.X - 2, loc.Y); //Checks left
            CaptureAttempt(locChk[0], loc, pieceMoved, _pieces.GetPiece(locChk[0].ToString())._pawn._type, _pieces.GetPiece(locChk[1].ToString())._pawn._type);

            locChk[0] = new HPoint(loc.X, loc.Y + 1); locChk[1] = new HPoint(loc.X, loc.Y + 2); //Checks above
            CaptureAttempt(locChk[0], loc, pieceMoved, _pieces.GetPiece(locChk[0].ToString())._pawn._type, _pieces.GetPiece(locChk[1].ToString())._pawn._type);
            
            locChk[0] = new HPoint(loc.X, loc.Y - 1); locChk[1] = new HPoint(loc.X, loc.Y - 2); //Checks below
            CaptureAttempt(locChk[0], loc, pieceMoved, _pieces.GetPiece(locChk[0].ToString())._pawn._type, _pieces.GetPiece(locChk[1].ToString())._pawn._type);
        }

        private void CaptureAttempt(HPoint locChk, HPoint loc, PieceType pieceMoved, PieceType pieceChk, PieceType pieceAhead)
        {
            if (pieceChk != King)
            {
                if (pieceChk != Empty && !SameSide(pieceMoved, pieceChk) && SameSide(pieceMoved, pieceAhead))
                {
                    _pieces.RemoveFrom(locChk.ToString());
                    _audioManager._death.Play(0.7f, 0.0f, 0.0f);
                }
            }
            else
            {
                PieceType[] pieceAdditional; //Used to store the 4 tiles that will be checked for the king capturing
                if (locChk.X == loc.X - 1 || locChk.X == loc.X + 1) //Checks to see if the already checked squares are left and right of the king
                {
                    //If they are get the up and down of the king
                    pieceAdditional = new PieceType[]{_pieces.GetPiece(new HPoint(locChk.X, locChk.Y + 1).ToString())._pawn._type,
                    _pieces.GetPiece(new HPoint(locChk.X, locChk.Y - 1).ToString())._pawn._type, pieceMoved, pieceAhead};
                }
                else
                {
                    //If not get left and right of the king
                    pieceAdditional = new PieceType[]{_pieces.GetPiece(new HPoint(locChk.X + 1, locChk.Y).ToString())._pawn._type,
                    _pieces.GetPiece(new HPoint(locChk.X - 1, locChk.Y).ToString())._pawn._type, pieceMoved, pieceAhead};
                }


                int surrounding = 0; //Tally for the number of kings
                for (int i = 0; i < pieceAdditional.Length; i++)
                {
                    if (pieceAdditional[i] != Empty && !SameSide(pieceChk, pieceAdditional[i]))
                    {
                        surrounding++;
                    }
                }
                if (surrounding == 4 || 
                    (surrounding == 3 && (_serverOp._kingCaptureOp == KingCaptureOp.JustThree || !DefendersRemain()) && //Checks if either the gamerule on three capturing is true or if the king is only remaining piece
                    (locChk.X == 0 || locChk.Y == 0  || locChk.X == _boardSize - 1 || locChk.Y == _boardSize - 1))) //Checks if the king is on a bord side to allow 3 person capture
                {
                    _pieces.RemoveFrom(locChk.ToString());
                }
            }
        }

        private bool SameSide(PieceType pieceChk1, PieceType pieceChk2) //Checks if two pieces are of the same side, for capture logic
        {
            if (pieceChk1 == pieceChk2)
                return true;
            else if (((pieceChk1 == King || pieceChk2 == King) && _serverOp._kingOp == KingOp.Armed) && (pieceChk1 == Defender || pieceChk2 == Defender))
                return true;
            else if ((pieceChk1 == Corner || pieceChk2 == Corner) && _serverOp._captureOp != CaptureOp.Disabled) //Checks for not Disabled as there are two other variations where it is enabled
                return true;
            else if ((pieceChk1 == Throne || pieceChk2 == Throne) && _serverOp._captureOp == CaptureOp.CornerThrone) //Checks for is CornerThrone as it is the only variation where it is enabled
                return true;
            return false;
        }

        public int TileSizeX(Rectangle viewPort) //Used to get the tilesize across cs files
        {
            return viewPort.Width / 30;
        }
        
        public int TileSizeY(Rectangle viewPort) //May ultimately be redundant, as currently tiles are always the same size on the x and y
        {
            return viewPort.Width / 30;
        }

        public bool MovesStillPossible(Player.SideType? side)
        {
            foreach (DictionaryEntry piece in _pieces.AllPieces())
            {
                Piece iPiece = piece.Value as Piece;
                for (int i = 0; i < 4; i++)
                {
                    if (PlayerSidePiece(iPiece._pawn._type, side) && PathMaxExtent(iPiece._loc, (Direction)i).ToString() != iPiece._loc.ToString())
                    {
                        Console.WriteLine("{0} != {1}, {2}", PathMaxExtent(iPiece._loc, (Direction)i).ToString(), iPiece._loc.ToString(), PathMaxExtent(iPiece._loc, (Direction)i).ToString() != iPiece._loc.ToString());
                        return true;
                    }
                }
            }

            return false;
        }

        enum Direction { Up = 0, Down = 1, Left = 2, Right = 3 }
        private HPoint PathMaxExtent(HPoint startLocation, Direction direction)
        {
            HPoint locationChk = startLocation;
            switch (direction)
            {
                case Direction.Up:
                {
                    while (true)
                    {
                        locationChk.Y--;
                        Console.WriteLine("Checking up " + locationChk.Y);
                        if (locationChk.Y == 0 || _pieces.GetPiece(locationChk.ToString())._pawn._type != Empty)
                            return new HPoint(locationChk.X, locationChk.Y + 1);
                        else if (locationChk.Y > 0 || locationChk.Y < 0)
                            return startLocation;
                    }
                }
                case Direction.Down:
                {
                    while (true)
                    {
                        locationChk.Y++;
                        if (locationChk.Y == _boardSize || _pieces.GetPiece(locationChk.ToString())._pawn._type != Empty)
                            return new HPoint(locationChk.X, locationChk.Y - 1);
                        else if (locationChk.Y > 0 || locationChk.Y < 0)
                            return startLocation;
                    }
                }
                case Direction.Left:
                {
                    while (true)
                    {
                        locationChk.X--;
                        if (locationChk.X == 0 || _pieces.GetPiece(locationChk.ToString())._pawn._type != Empty)
                            return new HPoint(locationChk.X + 1, locationChk.Y);
                        else if (locationChk.X > 0 || locationChk.X < 0)
                            return startLocation;
                    }
                }
                case Direction.Right:
                {
                    while (true)
                    {
                        locationChk.X++;
                        if (locationChk.X == _boardSize || _pieces.GetPiece(locationChk.ToString())._pawn._type != Empty)
                            return new HPoint(locationChk.X - 1, locationChk.Y);
                        else if (locationChk.X > 0 || locationChk.X < 0)
                            return startLocation;
                    }
                }
            }
            return new HPoint(-1, -1);
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, Rectangle viewPort) //Used for drawing the board and the pieces
        {
            Rectangle rect= new Rectangle(
                        (viewPort.Width / 2) - ((TileSizeX(viewPort) * _boardSize) / 2), 
                        (viewPort.Height / 2) - ((TileSizeY(viewPort) * _boardSize) / 2), 
                        TileSizeX(viewPort), TileSizeY(viewPort));
            Piece iPiece;

            for (int y = 0; y < _boardSize; y++)
            {
                for (int x = 0; x < _boardSize; x++)
                {
                    if (_serverOp._winOp != WinOp.Side && (y == 0 || y == _boardSize - 1) && (x == 0 || x == _boardSize - 1))
                        spriteBatch.Draw(_boardColours[5], rect, Color.White);
                    else if (_serverOp._throneOp != ThroneOp.Disabled && y == (_boardSize - 1) / 2 && x == (_boardSize - 1) / 2)
                        spriteBatch.Draw(_boardColours[4], rect, Color.White);
                    else if (y % 2 == 0)
                        spriteBatch.Draw(_boardColours[x % 2], rect, Color.White);
                    else if (x % 2 > 0)
                        spriteBatch.Draw(_boardColours[0], rect, Color.White);
                    else
                        spriteBatch.Draw(_boardColours[1], rect, Color.White);


                    iPiece = _pieces.GetPiece(x.ToString() + "," + y.ToString());
                    if ((int)iPiece._pawn._type > -1 && iPiece._loc.ToString() != _selectedPiece.ToString())
                    {
                        spriteBatch.Draw(_pawnTexture[(int)iPiece._pawn._type], rect, Color.White);
                    }

                    rect.X += TileSizeX(viewPort);
                }
                rect.Y +=TileSizeY(viewPort);
                rect.X = (viewPort.Width / 2) - ((TileSizeX(viewPort) * _boardSize) / 2);
            }

             if (_selectedPiece.X != -1) //Drawn here to allow it to be above the half of the board beneath itself
            {
                spriteBatch.Draw(_pawnTexture[(int)_pieces.GetPiece(_selectedPiece.ToString())._pawn._type], new Rectangle(Mouse.GetState().Position, rect.Size), Color.White);
            }
        }
    }
}