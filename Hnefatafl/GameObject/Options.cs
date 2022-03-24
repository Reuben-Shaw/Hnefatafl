using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using static Hnefatafl.UserOptions.ColourButtons;

namespace Hnefatafl
{
    [Serializable] //Must be serilisable in order to transport it across a network
    public struct ServerOptions //Struct as no methods will be needed in here and it will never be invoked like a method
    {
        //Made heavy use of enumeration to make code as readable as possible
        public enum PlayerTurn { Attacker, Defender }
        public PlayerTurn _playerTurn { get; set; } //Works

        public enum ThroneOp { Disabled, DefenderKing, King }
        public ThroneOp _throneOp { get; set; } //Works

        public enum KingOp { Armed, Unarmed }
        public KingOp _kingOp { get; set; } //Works

        public enum SandwichMovementOp { Enabled, Disabled }
        public SandwichMovementOp _sandwichMovementOp { get; set; } //Doesn't work

        public enum CaptureOp { CornerThrone, Corner, Disabled } //Refers to if certain pieces on the bord can be used to capture pieces
        public CaptureOp _captureOp { get; set; } //Works

        public enum KingCaptureOp { AllDefendersThree, JustThree  } //Refers to if all defenders have to be captured to allow the king to be captured with just three units on the side of the board
        public KingCaptureOp _kingCaptureOp { get; set; } //Works

        public enum WinOp { Side, Corner }
        public WinOp _winOp { get; set; } //Doesn't work

        public ServerOptions()
        {
            _playerTurn = PlayerTurn.Attacker;
            _throneOp = ThroneOp.King;
            _kingOp = KingOp.Armed;
            _sandwichMovementOp = SandwichMovementOp.Enabled;
            _captureOp = CaptureOp.CornerThrone;
            _kingCaptureOp = KingCaptureOp.AllDefendersThree;
            _winOp = WinOp.Corner;
        }

        public override string ToString() //Pretty debug features, I'm such a dreamy developer
        {
            string msg = "";

            if (_playerTurn == PlayerTurn.Attacker) msg += "Player is the attacker\n";
            else msg += "Player is the defender\n";

            if (_throneOp == ThroneOp.Disabled) msg += "Throne is disabled\n";
            else if (_throneOp == ThroneOp.DefenderKing) msg += "Throne is accessible only by the king\n";
            else msg += "Throne is accessible by defenders and the king\n";

            if (_kingOp == KingOp.Armed) msg += "The king is armed\n";
            else msg += "The king is unarmed\n";

            if (_sandwichMovementOp == SandwichMovementOp.Enabled) msg += "Pieces can move into a sandwich\n";
            else msg += "Pieces can't move into a sandwich\n";

            if (_captureOp == CaptureOp.Disabled) msg += "Corners and thrones can't be used for capturing\n";
            else if (_captureOp == CaptureOp.Corner) msg += "Corners only can be used for capturing\n";
            else msg += "Corners and thrones can be used for capturing\n";

            if (_kingCaptureOp == KingCaptureOp.AllDefendersThree) msg += "All defenders must be defeated for a 3 piece king capture\n";
            else msg += "A 3 piece king capture may occur anytime\n";

            if (_winOp == WinOp.Corner) msg += "The king must reach the corner to win";
            else msg += "The king must reach the side to win";

            return msg;
        }
    }

    [Serializable]
    public struct UserOptions
    {
        public enum ColourButtons { pawnA, pawnD, board1, board2, throne, corner, highlightTrail, selectPositive, selectNegative, boardD, boardA }

        public Color _pawnAttacker { get; set; }
        public Color _pawnDefender { get; set; }
        public Color[] _boardColours { get; set; }
        //0: Board main colour 1, 1: Board main colour 2, 2: Defender board colour, 3: Attacker board colour, 4: Throne, 5: Corner
        public Color[] _selectColours { get; set; }

        public UserOptions(Color pawnAttacker, Color pawnDefender, Color[] boardColours, Color[] selectColours)
        {
            _pawnAttacker = pawnAttacker;
            _pawnDefender = pawnDefender;
            _boardColours = boardColours;
            _selectColours = selectColours;
        }

        public Color GetDefaultColor(ColourButtons pickerName) => pickerName switch
        {
            pawnA => new Color(255, 76, 74),
            pawnD => new Color(54, 56, 255),
            board1 => new Color(173, 99, 63), 
            board2 => new Color(80, 53, 30), 
            throne => new Color(175, 0, 0), 
            corner => new Color(249, 200, 24),
            highlightTrail => new Color(236, 179, 19),
            selectPositive => new Color(0, 255, 0),
            selectNegative => new Color(255, 0, 0),
            boardD => new Color(255, 255, 0), 
            boardA => new Color(0, 255, 255),
            _ => new Color(0, 0, 0)
        };

        public Color GetDefaultColor(int number) => number switch
        {
            0 => new Color(255, 76, 74),
            1 => new Color(54, 56, 255),
            2 => new Color(173, 99, 63), 
            3 => new Color(80, 53, 30), 
            4 => new Color(175, 0, 0), 
            5 => new Color(249, 200, 24),
            6 => new Color(236, 179, 19),
            7 => new Color(0, 255, 0),
            8 => new Color(255, 0, 0),
            9 => new Color(255, 255, 0), 
            10 => new Color(0, 255, 255),
            _ => new Color(0, 0, 0)
        };

        public bool SetFromButton(string pickerName, Color newColour)
        {
            switch ((UserOptions.ColourButtons)Enum.Parse(typeof(UserOptions.ColourButtons), pickerName))
            {
                case (pawnA):
                    _pawnAttacker = newColour;
                    return true;
                case (pawnD):
                    _pawnDefender = newColour;
                    return true;
                case (board1):
                    _boardColours[0] = newColour;
                    return true;
                case (board2):
                    _boardColours[1] = newColour;
                    return true;
                case (throne):
                    _boardColours[4] = newColour;
                    return true;
                case (corner):
                    _boardColours[5] = newColour;
                    return true;
                case (boardA):
                    _boardColours[2] = newColour;
                    return true;
                case (boardD):
                    _boardColours[3] = newColour;
                    return true;
                case (highlightTrail):
                    _selectColours[0] = newColour;
                    return true;
                case (selectPositive):
                    _selectColours[1] = newColour;
                    return true;
                case (selectNegative):
                    _selectColours[2] = newColour;
                    return true;
            }
        
            return true;
        }

        public override string ToString()
        {
            string output = "";

            output += $"Attacker Colour: ";
            if (GetDefaultColor(0) == _pawnAttacker) output += "Default\n";
            else output += $"{_pawnAttacker.ToString()}\n";
            output += "Defender Colour: ";
            if (GetDefaultColor(1) == _pawnAttacker) output += "Default\n";
            else output += $"{_pawnDefender.ToString()}\n";

            int i = 2;
            output += "Board Colours:\n";

            foreach (Color colour in _boardColours)
            {
                if (GetDefaultColor(i) == colour) output += "Default\n";
                else output += $"{colour.ToString()}\n";
                i++;
            }

            i = 0;
            output += "Highlight Colours:\n";

            foreach (Color colour in _selectColours)
            {
                if (GetDefaultColor(i) == colour) output += "Default\n";
                else output += $"{colour.ToString()}\n";
                i++;
            }
            return output;
        }

        public string CreateFile()
        {
            return "<?xml version=\"1.0\"?><UserOptions xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><_pawnAttacker><B>74</B><G>76</G><R>255</R><A>255</A><PackedValue>4283059455</PackedValue></_pawnAttacker><_pawnDefender><B>255</B><G>56</G><R>54</R><A>255</A><PackedValue>4294916150</PackedValue></_pawnDefender><_boardColours><Color><B>63</B><G>99</G><R>173</R><A>255</A><PackedValue>4282344365</PackedValue></Color><Color><B>30</B><G>53</G><R>80</R><A>255</A><PackedValue>4280169808</PackedValue></Color><Color><B>255</B><G>255</G><R>0</R><A>255</A><PackedValue>4294967040</PackedValue></Color><Color><B>0</B><G>255</G><R>255</R><A>255</A><PackedValue>4278255615</PackedValue></Color><Color><B>0</B><G>0</G><R>175</R><A>255</A><PackedValue>4278190255</PackedValue></Color><Color><B>24</B><G>200</G><R>249</R><A>255</A><PackedValue>4279814393</PackedValue></Color></_boardColours><_selectColours><Color><B>19</B><G>179</G><R>236</R><A>255</A><PackedValue>4279481324</PackedValue></Color><Color><B>0</B><G>255</G><R>0</R><A>255</A><PackedValue>4278255360</PackedValue></Color><Color><B>0</B><G>0</G><R>255</R><A>255</A><PackedValue>4278190335</PackedValue></Color></_selectColours></UserOptions>";
        }
    }
}