using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections;
using System.Collections.Generic;
using Hnefatafl.Media;
using Hnefatafl.MenuObjects;
using static Hnefatafl.ServerOptions;
using static Hnefatafl.PieceType;
using static Hnefatafl.Media.TextureAtlasLink;

namespace Hnefatafl
{
    public enum BoardTypes { Hnefatafl = 11, Tablut = 9, TablutCentre = 9, Brandubh = 7, ArdRi = 7 }

    sealed class Board
    {
        ContentManager Content;
        private readonly Texture2D[] _boardColours = new Texture2D[6];
        //0: Board main colour 1, 1: Board main colour 2, 2: Defender board colour, 3: Attacker board colour, 4: Throne, 5: Corner
        private readonly Texture2D[] _pawnTexture = new Texture2D[3];
        //0: Attacker, 1: Defender, 2: King
        private AtlasTexture _boardHighlightAtlas;
        private TextureDivide _boarder; //BOARDer!!!!!!! XDXDXDXDXD Oh the fun I have with programming
        private Texture2D _selectHighlight;
        private bool? _redrawSelect = null;
        private Color[] _selectColours = new Color[3];
        //0: Highlight colour, 1: Positive select colour, 2: Negative select colour
        private Pieces _pieces = new Pieces();
        //Generates the Piece object which contains a hashtable of all pieces, prevents need for mostly empty 2D array and better than dictionary for this method
        public int _boardSize;
        //9 or 11 depending on the users options
        private int _tileSizeX;
        private int _tileSizeY;

        public enum BoardState { ActiveGame, InactiveGame }
        public BoardState _state;

        private HPoint _selectedPiece = new HPoint(-1, -1);
        //Stores the current user selected piece, -1, -1 as expected is used as a null storing value
        public ServerOptions _serverOp;
        private BoardAudio _audioManager;
        public TurnDisplay _turnDisplay;

        public Board(GraphicsDeviceManager graphics, ContentManager content, Color[] boardColours, Color[] pawnColours, Color[] selectColours)
        {
            Content = new ContentManager(content.ServiceProvider, content.RootDirectory);
            CreateBoardColours(graphics, boardColours);
            CreatePawns(graphics, pawnColours);
            CreateSelectColours(selectColours);
            _audioManager = new BoardAudio(content);

            _tileSizeX = TileSizeX(graphics.GraphicsDevice.Viewport.Bounds);
            _tileSizeY = TileSizeY(graphics.GraphicsDevice.Viewport.Bounds);

            _turnDisplay = new TurnDisplay(new Point((graphics.PreferredBackBufferWidth / 2) - ((_tileSizeX * 3) / 2), _tileSizeY), new Point(_tileSizeX, _tileSizeY), "display");

            _boardHighlightAtlas = new AtlasTexture(graphics, Content, "Texture/Board/HighlightTrail");
            _boardHighlightAtlas.HueShiftTexture(_selectColours[0]);
            _boarder = new TextureDivide(graphics, Content, "Texture/Board/BorderDivide", _tileSizeX, _tileSizeY);
            SelectHighlightColour(null);
            _turnDisplay.Update(new Color[]{pawnColours[0], pawnColours[1]}, Content, graphics);
        }

        public void ReloadContent(GraphicsDeviceManager graphics, UserOptions options)
        {
            if (Content is not null)
            {
                Content.Unload();
            }
            CreateBoardColours(graphics, options._boardColours);
            CreatePawns(graphics, new Color[] {options._pawnAttacker, options._pawnDefender, options._pawnDefender});
            _boardHighlightAtlas.ReloadContent(graphics, options._selectColours[0]);
            CreateSelectColours(options._selectColours);
            SelectHighlightColour(null);
            _turnDisplay.Update(new Color[]{options._pawnAttacker, options._pawnDefender}, Content, graphics);
        }

        public void ReceivePawns(List<Piece> pieces)
        {
            _pieces.CreateBoard(pieces);
        }

        public bool CheckPawns(List<Piece> pieces)
        {
            return _pieces.Changed(pieces);
        }

        public Pieces GetPieces()
        {
            return _pieces;
        }

        public void WritePieces()
        {
            Console.WriteLine(_pieces.ToString());
        }

        public void CreateBoard(BoardTypes type)
        {
            _serverOp = new ServerOptions(); //Defines the options, currently empty as it will default to automatic options
            _boardSize = (int)type;
            _pieces.CreateBoard(type); //Responsible entirely for the creation of the pieces on the board, Board.cs doesn't contain any logic relating to this at all
            _selectedPiece = new HPoint(-1, -1);
            _state = BoardState.InactiveGame;
        }

        private void CreateBoardColours(GraphicsDeviceManager graphics, Color[] colours)
        {
            //Creates the Texture2Ds that are used to draw the board from the user selected colours
            for (int i = 0; i < colours.Length; i++)
            {
                _boardColours[i] = new Texture2D(graphics.GraphicsDevice, 1, 1);
                _boardColours[i].SetData(new[] { colours[i] });
            }
        }
        
        private void CreateSelectColours(Color[] colours)
        {
            for (int i = 0; i < colours.Length; i++)
            {
                _selectColours[i] = colours[i];
            }
        }

        private void CreatePawns(GraphicsDeviceManager graphics, Color[] colours)
        {
            _pawnTexture[0] = Content.Load<Texture2D>("Texture/Pawn/pawnA"); //Loading is passed by reference seemingly, this means that all sprites have to be different files to prevent the latter one being the colour of all of them, luckily I planned to do this from the start
            _pawnTexture[1] = Content.Load<Texture2D>("Texture/Pawn/pawnD");
            _pawnTexture[2] = Content.Load<Texture2D>("Texture/Pawn/king");

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
                            data[j] = new Color((byte)((float)(data[j].R / 255f) * colours[i].R), //Equation used to shift colour, 255f is important to facilitate float division otherwise it just does integer and then floats the value
                                                (byte)((float)(data[j].G / 255f) * colours[i].G), 
                                                (byte)((float)(data[j].B / 255f) * colours[i].B));
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
                            data[j] = new Color((byte)((float)(data[j].R / 255f) * colours[i].R), 
                                                (byte)((float)(data[j].G / 255f) * colours[i].G), 
                                                (byte)((float)(data[j].B / 255f) * colours[i].B));
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
                if (_boardColours[i] is not null)
                    _boardColours[i].Dispose();
            }

            for (int i = 0; i < _pawnTexture.Length; i++)
            {
                if (_pawnTexture[i] is not null)
                    _pawnTexture[i].Dispose();
            }

            _boardHighlightAtlas.UnloadContent();
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

        public void SelectPiece(HPoint select, SideType? side) //Used for selecting a piece, doesn't use PlayerSidePiece as it's used for the server selecting enemies as well
        {
            PieceType pawnChk = _pieces.GetPiece(select.ToString())._pawn._type;
            if (PlayerSidePiece(pawnChk, side))
            {
                _selectedPiece = select;
            }
            else
            {
                _selectedPiece = new HPoint(-1, -1);
            }
        }

        public HPoint GetSelectPiece()
        {
            return _selectedPiece;
        }

        public bool IsPieceSelected() //Used in the main update loop to assertain if a click is to move or select
        {
            if (_selectedPiece.X != -1)
                return true;
            else
                return false;
        }

        public bool MakeMove(HPoint move, SideType? side, bool doOverride) //Logic used to perform a move, and check if it is possible
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

        private bool PlayerSidePiece(PieceType pieceType, SideType? side) //Returns if a checked piece is on the same side as the player
        {
            if (side == SideType.Attackers && pieceType == Attacker)
            {
                return true;
            }
            else if (side == SideType.Defenders && (pieceType == Defender || pieceType == King))
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

        public bool MovesStillPossible(SideType? side) //CODE DOES NOT WORK, NEED FIX
        {
            foreach (DictionaryEntry piece in _pieces.AllPieces())
            {
                bool[] connections = new bool[4];
                Piece iPiece = piece.Value as Piece;
                if (PlayerSidePiece(iPiece._pawn._type, side))
                {
                    for (int i = 0; i < 4; i++)
                    {
                        HPoint[] maxLocations = new HPoint[4];
                        int start, end;
                        
                        maxLocations[i] = PathMaxExtent(iPiece._loc, (Direction)i);

                        if ((Direction)i == Direction.Left || (Direction)i == Direction.Right)
                        {
                            if ((Direction)i == Direction.Right)
                            { start = iPiece._loc.X + 1; end = maxLocations[i].X; }
                            else
                            { start = maxLocations[i].X; end = iPiece._loc.X - 1; }
                        }
                        else
                        {
                            if ((Direction)i == Direction.Down)
                            { start = iPiece._loc.Y + 1; end = maxLocations[i].Y; }
                            else
                            { start = maxLocations[i].Y; end = iPiece._loc.Y - 1; }
                        }

                        if (start != end)
                        {
                            connections[i] = true;
                        }
                    }
                    if (PositiveDirections(connections) > 0)
                    {
                        return true;
                    }
                }
            }
            
            Console.WriteLine("NO MOVES");
            return false;
        }

        enum Direction { Up = 0, Right = 1, Down = 2, Left = 3 }
        private HPoint PathMaxExtent(HPoint startLocation, Direction direction)
        {
            int start, end, increment;
            bool isKing = false, defenderThrone = false;
            PieceType pieceChk;

            if (_pieces.GetPiece(_selectedPiece.ToString())._pawn._type == King)
                isKing = true;
            else if (_pieces.GetPiece(_selectedPiece.ToString())._pawn._type == Defender && _serverOp._throneOp == ThroneOp.DefenderKing)
                defenderThrone = true;

            if (direction == Direction.Down || direction == Direction.Up)
            {
                if (direction == Direction.Down)
                { 
                    start = startLocation.Y + 1; end = _boardSize; increment = 1; 
                    for (int i = start; i < end; i += increment)
                    {
                        pieceChk = _pieces.GetPiece(startLocation.X + "," + i)._pawn._type;
                        if (pieceChk != Empty)
                        {
                            if ((isKing && (pieceChk == Corner || pieceChk == Throne)) || (defenderThrone && pieceChk == Throne)) return new HPoint(startLocation.X, i + increment);
                            else return new HPoint(startLocation.X, i);
                        }
                    }
                }
                else
                { 
                    start = startLocation.Y - 1; end = -1; increment = -1; 
                    for (int i = start; i > end; i += increment)
                    {
                        pieceChk = _pieces.GetPiece(startLocation.X + "," + i)._pawn._type;
                        if (pieceChk != Empty)
                        {
                            if ((isKing && (pieceChk == Corner || pieceChk == Throne)) || (defenderThrone && pieceChk == Throne)) return new HPoint(startLocation.X, i + increment);
                            else return new HPoint(startLocation.X, i);
                        }
                    }
                }
                return new HPoint(startLocation.X, end);
            }
            else
            {
                if (direction == Direction.Right)
                { 
                    start = startLocation.X + 1; end = _boardSize; increment = 1; 
                    for (int i = start; i < end; i += increment)
                    {
                        pieceChk = _pieces.GetPiece(i + "," + startLocation.Y)._pawn._type;
                        if (pieceChk != Empty)
                        {
                            if ((isKing && (pieceChk == Corner || pieceChk == Throne)) || (defenderThrone && pieceChk == Throne)) return new HPoint(i + increment, startLocation.Y);
                            else return new HPoint(i, startLocation.Y);
                        }
                    }
                }
                else
                { 
                    start = startLocation.X - 1; end = -1; increment = -1; 
                    for (int i = start; i > end; i += increment)
                    {
                        pieceChk = _pieces.GetPiece(i + "," + startLocation.Y)._pawn._type;
                        if (pieceChk != Empty)
                        {
                            if ((isKing && (pieceChk == Corner || pieceChk == Throne)) || (defenderThrone && pieceChk == Throne)) return new HPoint(i + increment, startLocation.Y);
                            else return new HPoint(i, startLocation.Y);
                        }
                    }
                }
                return new HPoint(end, startLocation.Y);
            }
        }

        private void ConstructBoardHighlight(GraphicsDeviceManager graphics, SpriteBatch spriteBatch, Rectangle startRect, Rectangle selectRect)
        {
            HPoint[] maxLocations = new HPoint[4];
            Rectangle rect;
            bool[] connections = new bool[4];

            bool foundSelect = false;

            for (int i = 0; i < 4; i++)
            {
                int start, end;
                rect = startRect;
                
                maxLocations[i] = PathMaxExtent(_selectedPiece, (Direction)i);

                if ((Direction)i == Direction.Left || (Direction)i == Direction.Right)
                {
                    if ((Direction)i == Direction.Right)
                    { start = _selectedPiece.X + 1; end = maxLocations[i].X; }
                    else
                    { start = maxLocations[i].X; end = _selectedPiece.X - 1; }

                    for (int j = start; j < end; j++)
                    {
                        if ((Direction)i == Direction.Left)
                            rect.X -= _tileSizeX;
                        else
                            rect.X += _tileSizeX;

                        if (j != end - 1)
                        {
                            spriteBatch.Draw(_boardHighlightAtlas.GetTexture(LeftRight), rect, Color.White);
                        }
                        else if ((Direction)i == Direction.Left && end - 1 != _selectedPiece.X)
                        {
                            spriteBatch.Draw(_boardHighlightAtlas.GetTexture(RightEnd), rect, Color.White);
                        }
                        else if (end - 1 != _selectedPiece.X)
                        {
                            spriteBatch.Draw(_boardHighlightAtlas.GetTexture(LeftEnd), rect, Color.White);
                        }

                        //if (new Rectangle(startRect.X - start, startRect.Y - end, start * TileSizeX(viewPort) + startRect.Width, start * TileSizeY(viewPort) + startRect.Height).Contains(selectRect)) foundSelect = true;
                        
                        if ((Direction)i == Direction.Left)
                        {
                            Rectangle test = new Rectangle(startRect.X - (end - start) * _tileSizeX, startRect.Y, (end - start) * _tileSizeX + _tileSizeX, startRect.Height);
                            if (test.Contains(selectRect)) foundSelect = true;
                            //Console.WriteLine($"OLD RECT {startRect.ToString()} NEW RECT: {test.ToString()}, SELECT: {selectRect.ToString()}");
                        }
                        else
                        {
                            Rectangle test = new Rectangle(startRect.X, startRect.Y, (end - start) * _tileSizeX + _tileSizeX, startRect.Height);
                            if (test.Contains(selectRect)) foundSelect = true;
                        }
                    }
                }
                else
                {
                    if ((Direction)i == Direction.Down)
                    { start = _selectedPiece.Y + 1; end = maxLocations[i].Y; }
                    else
                    { start = maxLocations[i].Y; end = _selectedPiece.Y - 1; }

                    for (int j = start; j < end; j++)
                    {
                        if ((Direction)i == Direction.Up)
                            rect.Y -= _tileSizeY;
                        else
                            rect.Y += _tileSizeY;

                        if (j != end - 1)
                        {
                            spriteBatch.Draw(_boardHighlightAtlas.GetTexture(UpDown), rect, Color.White);
                        }
                        else if ((Direction)i == Direction.Up && end - 1 != _selectedPiece.Y)
                        {
                            spriteBatch.Draw(_boardHighlightAtlas.GetTexture(DownEnd), rect, Color.White);
                        }
                        else if (end - 1 != _selectedPiece.Y)
                        {
                            spriteBatch.Draw(_boardHighlightAtlas.GetTexture(UpEnd), rect, Color.White);
                        }

                        if ((Direction)i == Direction.Up)
                        {
                            Rectangle test = new Rectangle(startRect.X, startRect.Y - (end - start) * _tileSizeY, startRect.Width, (end - start) * _tileSizeY + _tileSizeY);
                            if (test.Contains(selectRect)) foundSelect = true;
                            //Console.WriteLine($"OLD RECT {startRect.ToString()} NEW RECT: {test.ToString()}, SELECT: {selectRect.ToString()}");
                        }
                        else
                        {
                            Rectangle test = new Rectangle(startRect.X, startRect.Y, startRect.Width, (end - start) * _tileSizeY + _tileSizeY);
                            if (test.Contains(selectRect)) foundSelect = true;
                        }
                    }
                }

                if (start != end)
                {
                    connections[i] = true;
                }
            }

            int amountOfConnects = PositiveDirections(connections);
            switch (amountOfConnects)
            {
                case 0:
                {
                    spriteBatch.Draw(_boardHighlightAtlas.GetTexture(Seperated), startRect, Color.White);
                    break;
                }
                case 1:
                {
                    spriteBatch.Draw(_boardHighlightAtlas.GetTexture((TextureAtlasLink)Enum.Parse(typeof(TextureAtlasLink), ((Direction)ConnectionLocation(connections, true)).ToString() + "End")), startRect, Color.White);
                    break;
                }
                case 2:
                {
                    if (connections[0] && connections[2])
                    {
                        spriteBatch.Draw(_boardHighlightAtlas.GetTexture(UpDown), startRect, Color.White);
                    }
                    else if (connections[1] && connections[3])
                    {
                        spriteBatch.Draw(_boardHighlightAtlas.GetTexture(LeftRight), startRect, Color.White);
                    }
                    else if (connections[0] && connections[3])
                    {
                        spriteBatch.Draw(_boardHighlightAtlas.GetTexture(LeftCorner), startRect, Color.White);
                    }
                    else
                    {
                        spriteBatch.Draw(_boardHighlightAtlas.GetTexture((TextureAtlasLink)Enum.Parse(typeof(TextureAtlasLink), ((Direction)ConnectionLocation(connections, true)).ToString() + "Corner")), startRect, Color.White);
                    }
                    
                    break;
                }
                case 3:
                {
                    spriteBatch.Draw(_boardHighlightAtlas.GetTexture((TextureAtlasLink)Enum.Parse(typeof(TextureAtlasLink), ((Direction)ConnectionLocation(connections, false)).ToString() + "Fork")), startRect, Color.White);
                    break;
                }
                case 4:
                {
                    spriteBatch.Draw(_boardHighlightAtlas.GetTexture(Cross), startRect, Color.White);
                    break;
                }
            }

            if (foundSelect != _redrawSelect) SelectHighlightColour(foundSelect);
            //Console.WriteLine($"{(Direction)0}: {maxLocations[0]}, {(Direction)1}: {maxLocations[1]}, {(Direction)2}: {maxLocations[2]}, {(Direction)3}: {maxLocations[3]}");
        }

        private int PositiveDirections(bool[] boolArr)
        {
            int amount = 0;
            for (int i = 0; i < boolArr.Length; i++)
            {
                if (boolArr[i] == true)
                    amount++;
            }
            return amount;
        }

        private int ConnectionLocation(bool[] boolArr, bool checkForConnection)
        {
            for (int i = 0; i < boolArr.Length; i++)
            {
                if (boolArr[i] == checkForConnection)
                    return i;
            }
            return 0;
        }

        public void SelectHighlightColour(bool? positive)
        {
            _redrawSelect = positive;

            Color[] data;
            _selectHighlight = _boardHighlightAtlas.GetTexture(0);
            data = new Color[_selectHighlight.Width * _selectHighlight.Height];
            _selectHighlight.GetData<Color>(data);

            Color hueColour;
            if (positive == true) hueColour = _selectColours[1];
            else if (positive == false) hueColour = _selectColours[2];
            else hueColour = _selectColours[0];

            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] != Color.Transparent && data[i] != Color.Black)
                {
                    data[i] = hueColour;
                }
            }
            
            _selectHighlight.SetData<Color>(data);
        }

        public void Draw(GraphicsDeviceManager graphics, SpriteBatch spriteBatch, Rectangle viewPort, SideType? currentSide, bool currentTurn) //Used for drawing the board and the pieces
        {
            Rectangle rect = new Rectangle(
                        (viewPort.Width / 2) - ((_tileSizeX * _boardSize) / 2), 
                        (viewPort.Height / 2) - ((_tileSizeY * _boardSize) / 2), 
                        _tileSizeX, _tileSizeY);

            Rectangle highlightRect = new Rectangle(
                        (viewPort.Width / 2) - ((_tileSizeX * _boardSize) / 2) + (_selectedPiece.X * _tileSizeX), 
                        (viewPort.Height / 2) - ((_tileSizeY * _boardSize) / 2) + (_selectedPiece.Y * _tileSizeY), 
                        _tileSizeX, _tileSizeY);
            Piece iPiece;

            _boarder.Draw(spriteBatch, new Rectangle(rect.X - _tileSizeX, rect.Y - _tileSizeY, _tileSizeY * _boardSize + (_tileSizeX * 2), _tileSizeY  * _boardSize + (_tileSizeY * 2)));

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

                    if ((int)iPiece._pawn._type > -1 && (iPiece._loc.ToString() != _selectedPiece.ToString() || !currentTurn))
                    {
                        spriteBatch.Draw(_pawnTexture[(int)iPiece._pawn._type], rect, Color.White);
                    }
                
                    rect.X += _tileSizeX;
                }
                rect.Y += _tileSizeY;
                rect.X = (viewPort.Width / 2) - ((_tileSizeX * _boardSize) / 2);
            }

            Rectangle selectRect = new Rectangle(
                (viewPort.Width / 2) - ((_tileSizeX * _boardSize) / 2) + ((Mouse.GetState().Position.X - (_tileSizeX * _boardSize) + (int)(_tileSizeX * 1.5)) / _tileSizeX * _tileSizeX) - (_tileSizeX * (int)((11 - _boardSize) * 1.5)), 
                (viewPort.Height / 2) - ((_tileSizeY * _boardSize) / 2) + ((Mouse.GetState().Position.Y - (_tileSizeY * _boardSize) + (_tileSizeY * 8 + (_tileSizeY / 16))) / _tileSizeY * _tileSizeY) - (_tileSizeY * (int)((11 - _boardSize) * 1.5)),
                _tileSizeX, _tileSizeY);

            if (currentTurn && _selectedPiece.X != -1)
            {
                ConstructBoardHighlight(graphics, spriteBatch, highlightRect, selectRect);
            }

            if (new Rectangle((viewPort.Width / 2) - ((_tileSizeX * _boardSize) / 2), (viewPort.Height / 2) - ((_tileSizeY * _boardSize) / 2), _tileSizeX * _boardSize, _tileSizeY * _boardSize).Contains(Mouse.GetState().Position))
            {
                spriteBatch.Draw(_selectHighlight, selectRect, Color.White);
            }

            if (currentTurn && _selectedPiece.X != -1) //Drawn here to allow it to be above the half of the board beneath itself
            {
                spriteBatch.Draw(_pawnTexture[(int)_pieces.GetPiece(_selectedPiece.ToString())._pawn._type], new Rectangle(Mouse.GetState().Position, rect.Size), Color.White);
            }

            _turnDisplay.Draw(graphics, spriteBatch);
        }
    }
}