using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System;

namespace Hnefatafl.MenuObjects
{
    sealed class Cursor
    {
        public enum CursorState { Pointer, OpenHand, ClosedHand }

        public Texture2D _pointer { get; set; }
        public Texture2D _openHand { get; set; }
        public Texture2D _closedHand { get; set; }
        public Point _pos { get; set; }
        public bool _hidden { get; set; }

        public CursorState _state;

        public Cursor(ContentManager Content)
        {
            _state = CursorState.Pointer;
            _pointer = Content.Load<Texture2D>("Texture/Menu/PointerCursor");
            _openHand = Content.Load<Texture2D>("Texture/Menu/OpenHandCursor");
            _closedHand = Content.Load<Texture2D>("Texture/Menu/CloseHandCursor");
            _hidden = false;
        }

        public void UnloadContent()
        {
            _pointer.Dispose();
            _openHand.Dispose();
            _closedHand.Dispose();
        }

        public void Draw(SpriteBatch spriteBatch, Rectangle viewPort)
        {
            if (viewPort.Contains(_pos) && !_hidden)
            {
                switch (_state)
                {
                    case CursorState.Pointer:
                    {
                        spriteBatch.Draw(_pointer, new Rectangle(_pos, new Point(32, 32)), Color.White);
                        break;
                    }
                    case CursorState.OpenHand:
                    {
                        spriteBatch.Draw(_openHand, new Rectangle(_pos, new Point(32, 32)), Color.White);
                        break;
                    }
                    case CursorState.ClosedHand:
                    {
                        spriteBatch.Draw(_closedHand, new Rectangle(_pos, new Point(32, 32)), Color.White);
                        break;
                    }
                }
            }
        }
    }
}