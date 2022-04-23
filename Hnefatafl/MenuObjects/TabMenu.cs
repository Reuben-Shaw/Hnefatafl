using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Hnefatafl.Media;
using static Hnefatafl.MenuObjects.MenuObject.Status;
using System.Collections.Generic;
using System.Xml;

namespace Hnefatafl.MenuObjects
{
    sealed class TabMenu : MenuObject
    {
        private List<Button> _button = new List<Button>();
        private TextureDivide _menuBack;
        private int _tileSizeX, _tileSizeY;
        private int _indexSelected = 0;
        private const int _drawConst = 28;

        public TabMenu(Point position, Point size, int tileSizeX, int tileSizeY, XmlElement xml, Hnefatafl.GameState gameState, float fontMod, string[] buttonNames, GraphicsDeviceManager graphics, ContentManager content)
        {
            _tileSizeX = tileSizeX;
            _tileSizeY = tileSizeY;

            Point buttonSize = new Point((int)(8f * tileSizeX), (int)(3.5f * tileSizeY));

            _pos = new Point(position.X, position.Y + buttonSize.Y - _tileSizeY - (tileSizeY / 2));
            _size = new Point(size.X, size.Y - _pos.Y);


            _menuBack = new TextureDivide(graphics, content, "Texture/Menu/BackMenuDivide", _tileSizeX, _tileSizeY);

            for (int i = 0; i < buttonNames.Length; i++)
            {
                _button.Add(new Button(new Point(_pos.X + (buttonSize.X * i) + ((buttonSize.X / 8) * i) + _drawConst, 0), 
                            buttonSize, new TextureDivide(graphics, content, "Texture/Menu/TabButton", _tileSizeX, _tileSizeY), fontMod, Hnefatafl.NodeText(xml, buttonNames[i], gameState), 
                            buttonNames[i], graphics, content));
            }
            _button.Add(new Button(new Point(_size.X - _pos.X - (64 * 2), 0), 
                            new Point(96, 128), new TextureDivide(graphics, content, "Texture/Menu/TabButton", _tileSizeX, _tileSizeY), fontMod, "X", 
                            "back", graphics, content));
                    
            _button[_indexSelected]._status = Selected;
        }

        public string ButtonCheck(MouseState currentState, MouseState previousState, Point mouseLoc, bool keyboardSelect)
        {
            int i = 0;

            foreach (Button button in _button)
            {
                if (keyboardSelect || (mouseLoc.Y < _pos.Y && new Rectangle(button._pos, button._size).Contains(mouseLoc)))
                {
                    if (button._status != Selected && previousState.LeftButton == ButtonState.Pressed && currentState.LeftButton == ButtonState.Released)
                    {
                        button._status = Selected;
                        _button[_indexSelected]._status = Unselected;
                        _indexSelected = i;
                        return button._name;
                    }
                }
                i++;
            }
            return "";
        }

        public void Draw(SpriteBatch spriteBatch, float fontSize, Rectangle viewPort)
        {
            Button selectedButton = new Button();

            foreach (Button button in _button)
            {
                if (button._status != Selected)
                {
                    button.Draw(spriteBatch, fontSize, _tileSizeX, _tileSizeY, viewPort);
                }
                else
                {
                    selectedButton = button;
                }
            }

            _menuBack.Draw(spriteBatch, new Rectangle(_pos, _size));

            if (!string.IsNullOrEmpty( selectedButton._name))
            {
                selectedButton.Draw(spriteBatch, fontSize, _tileSizeX, _tileSizeY, viewPort);
            }
        }
    }
}