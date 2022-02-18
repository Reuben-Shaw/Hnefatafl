using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections;
using System.Collections.Generic;
using static Hnefatafl.MenuObject.Status;

namespace Hnefatafl
{
    class TextBox : MenuObject
    {
        private char _addChar;

        public TextBox(Point position, Point size)
        {
            _pos = position;
            _size = size;
            _status = Unselected;
            _text = "";
        }
        
        public TextBox(Point position, Point size, string text)
        {
            _pos = position;
            _size = size;
            _status = Unselected;
            _text = text;
        }
    }
}