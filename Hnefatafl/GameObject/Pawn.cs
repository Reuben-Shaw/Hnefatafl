using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;


namespace Hnefatafl
{
    public enum PieceType { Empty = -1, Attacker = 0, Defender = 1, King = 2 };
    public sealed class Pawn
    {
        public PieceType _type { get; set; }

        public Pawn(PieceType type)
        {
            _type = type;
        }
    }
}