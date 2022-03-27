using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections;
using System.Collections.Generic;
using Hnefatafl.Media;

namespace Hnefatafl.MenuObjects
{
    enum EditorObject { ButtonObj, TextboxObj, None }
    class Editor
    {
        private TextBox _selectedTextbox;
        private Button _selectedButton;
        public EditorObject _editorObject;
        private bool _inMovement;
        private bool _sizeChange;
        private double _lastMove = 1;

        public bool _readyToReceive = false;


        public Editor(GraphicsDeviceManager graphics, ContentManager Content)
        {
            _selectedButton = new Button(new Point(0, 0), new Point(0, 0), "");
            _selectedButton.Update(graphics, Content);
            _selectedTextbox = new TextBox(new Point(0, 0), new Point(0, 0), "");
            _selectedTextbox.Update(graphics, Content);

            _editorObject = EditorObject.None;
            _inMovement = true;
            _sizeChange = false;
        }

        public void KeyPress(KeyboardState currentKeyboard, MouseState currentMouse)
        {
            int delta = 1;
            Point objChange = new Point(0, 0);

            if (_lastMove > 0.1)
            {
                if (currentKeyboard.IsKeyDown(Keys.LeftShift) || currentKeyboard.IsKeyDown(Keys.RightShift))
                {
                    delta = 10;
                }
                else if (currentKeyboard.IsKeyDown(Keys.LeftControl) || currentKeyboard.IsKeyDown(Keys.RightControl))
                {
                    delta = 100;
                }

                if (currentKeyboard.IsKeyDown(Keys.Enter))
                {
                    _readyToReceive = true;
                }
                else if (currentKeyboard.IsKeyDown(Keys.Space))
                {
                    _inMovement = !_inMovement;
                    _sizeChange = !_sizeChange;
                }
                else if (_editorObject == EditorObject.None && currentKeyboard.IsKeyDown(Keys.B))
                {
                    _editorObject = EditorObject.ButtonObj;
                }
                else if (_editorObject == EditorObject.None && currentKeyboard.IsKeyDown(Keys.T))
                {
                    _editorObject = EditorObject.TextboxObj;
                }
                else if (currentKeyboard.IsKeyDown(Keys.O))
                {
                    _selectedButton = new Button(new Point(0, 0), new Point(0, 0), "");
                    _selectedTextbox = new TextBox(new Point(0, 0), new Point(0, 0), "");
                }

                if (currentKeyboard.IsKeyDown(Keys.Up))
                {
                    objChange.Y -= delta;
                }
                else if (currentKeyboard.IsKeyDown(Keys.Down))
                {
                    objChange.Y += delta;
                }

                if (currentKeyboard.IsKeyDown(Keys.Left))
                {
                    objChange.X -= delta;
                }
                else if (currentKeyboard.IsKeyDown(Keys.Right))
                {
                    objChange.X += delta;
                }

                if (_editorObject == EditorObject.ButtonObj)
                {
                    if (_inMovement) _selectedButton._pos = new Point(_selectedButton._pos.X + objChange.X, _selectedButton._pos.Y + objChange.Y);
                    else _selectedButton._size = new Point(_selectedButton._size.X + objChange.X, _selectedButton._size.Y + objChange.Y);
                }
                else if (_editorObject == EditorObject.TextboxObj)
                {
                    if (_inMovement) _selectedTextbox._pos = new Point(_selectedTextbox._pos.X + objChange.X, _selectedTextbox._pos.Y + objChange.Y);
                    else _selectedTextbox._size = new Point(_selectedTextbox._size.X + objChange.X, _selectedTextbox._size.Y + objChange.Y);
                }
            }
        }

        public void AddToTime(double timeSinceLast)
        {
            _lastMove += timeSinceLast;
        }

        public TextBox ReceiveTextbox()
        {
            _readyToReceive = false;
            return _selectedTextbox;
        }

        public Button ReceiveButton()
        {
            _readyToReceive = false;
            return _selectedButton;
        }

        public void Draw(SpriteBatch spriteBatch, int tileSizeX, int tileSizeY, TextureDivide buttonSelect, TextureDivide buttonUnselect, Rectangle viewPort)
        {
            _selectedButton.Draw(spriteBatch, tileSizeX, tileSizeY,  buttonSelect, buttonUnselect, viewPort);
            _selectedTextbox.Draw(spriteBatch, viewPort);
        }
    }
}