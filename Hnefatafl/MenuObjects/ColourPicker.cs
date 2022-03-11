using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections;
using System.Collections.Generic;
using static Hnefatafl.MenuObjects.MenuObject.Status;

namespace Hnefatafl.MenuObjects
{
    sealed class ColourPicker : MenuObject
    {
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
        private Texture2D _display { get; set; }
        private Texture2D[] _subDisplay { get; set; }
        private Texture2D _bevel;

        public ColourPicker(Point position, string name)
        {
            _pos = position;
            _size = new Point(512, 32);
            _name = name;
            _subDisplay = new Texture2D[3];
            R = 255;
            G = 175;
            B = 23;
        }

        public void ChangeColour(GraphicsDeviceManager graphics, Rectangle viewPort)
        {
            Color[] data;

            for (int j = 0; j < 3; j++)
            {
                data = new Color[256 * 3];
                _subDisplay[j] = new Texture2D(graphics.GraphicsDevice, 256 * 3, 1);

                int r = 0, g = 0, b = 0;

                for (int i = 0; i < 256 * 3; i++)
                {
                    data[i] = new Color(r, g, b);

                    if (j == 0)
                    {
                        r++;
                    }
                    else if (j == 1)
                    {
                        g++;
                    }
                    else
                    {
                        b++;
                    }
                }

                _subDisplay[j].SetData<Color>(data);
            }
        }

        public void LoadContent(GraphicsDeviceManager _graphics, ContentManager Content)
        {
            _bevel = Content.Load<Texture2D>("Texture/Menu/SelectBevel");
        }

        public void UnloadContent()
        {
            for (int i = 0; i < _subDisplay.Length; i++)
            {
                _subDisplay[i].Dispose();
            }

            _bevel.Dispose();
        }

        int _stage = 0;
        public void Increment()
        {
            if (_stage == 0)
            {
                R--;
                G++;
                if (G == 255)
                {
                    _stage++;
                }
            }
            else if (_stage == 1)
            {
                G--;
                B++;
                if (B == 255)
                {
                    _stage++;
                }
            }
            else
            {
                B--;
                R++;
                if (B == 0)
                {
                    _stage = 0;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, Rectangle viewPort, int gap)
        {
            for (int i = 0; i < 3; i++)
            {
                spriteBatch.Draw(_subDisplay[i], new Rectangle(_pos.X, _pos.Y + (gap * i) + (_size.Y * i), _size.X, _size.Y), Color.White);

                int bevelLoc = 0;
                switch (i)
                {
                    case 1: bevelLoc = R; break;
                    case 2: bevelLoc = G; break;
                    case 3: bevelLoc = B; break;
                }
                spriteBatch.Draw(_bevel, new Rectangle(_pos.X + ((_size.X / 256) * bevelLoc) - (_size.Y / 4), _pos.Y + (gap * i) + (_size.Y * i) + (_size.Y / 4), _size.Y / 2, _size.Y / 2), Color.White);
            }
        }
    }
}