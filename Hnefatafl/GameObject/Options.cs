using System;

namespace Hnefatafl
{
    [Serializable] //Must be serilisable in order to transport it across a network
    public class ServerOptions //Struct as no methods will be needed in here and it will never be invoked like a method, just read from
    {
        //Made heavy use of enumeration to make code as readable as possible
        public enum PlayerTurn { Attacker = 0, Defender = 1 }
        public PlayerTurn _playerTurn { get; set; }

        public enum ThroneOp { Disabled = 0, DefenderKing = 1, King = 2 }
        public ThroneOp _throneOp { get; set; }

        public enum KingOp { Armed = 0, Unarmed = 1 }
        public KingOp _kingOp { get; set; }

        public enum SandwichMovementOp { Enabled = 0, Disabled = 1 }
        public SandwichMovementOp _sandwichMovementOp { get; set; }

        public enum CaptureOp { CornerThrone = 0, Corner = 1, Disabled = 2 } //Refers to if certain pieces on the bord can be used to capture pieces
        public CaptureOp _captureOp { get; set; }

        public enum KingCaptureOp { AllDefendersThree = 0, JustThree = 1 } //Refers to if all defenders have to be captured to allow the king to be captured with just three units on the side of the board
        public KingCaptureOp _kingCaptureOp { get; set; }

        public enum WinOp { Side = 0, Corner = 1 }
        public WinOp _winOp { get; set; }

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
        public ServerOptions(bool test)
        {
            _playerTurn = PlayerTurn.Defender;
            _throneOp = ThroneOp.Disabled;
            _kingOp = KingOp.Unarmed;
            _sandwichMovementOp = SandwichMovementOp.Enabled;
            _captureOp = CaptureOp.CornerThrone;
            _kingCaptureOp = KingCaptureOp.AllDefendersThree;
            _winOp = WinOp.Corner;
        }

        public override string ToString()
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
}