using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Hnefatafl.Media;

namespace Hnefatafl.MenuObjects
{
    sealed class ColourPicker : MenuObject
    {
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
        private int _gap { get; set; }
        private Texture2D _display;
        private Rectangle _displayRect;
        private Texture2D[] _subDisplay;
        private Rectangle[] _subDisplayRect;
        private Texture2D _bevel;
        private TextureDivide _border;
        private Texture2D _exampleImage;
        private string _exampleImagePath;
        public bool _visible { get; set; }
        private int _selectedBevel;
        private ContentManager Content;

        private Point m_pos;
        public override Point _pos
        {
            get
            {
                return m_pos;
            }
            set
            {
                m_pos = value;
                DisplayRect();
            }
        }

        public ColourPicker(Point position, Rectangle viewport, string name, int gap)
        {
            int width;
            if (viewport.Width < 1024) width = (int)Math.Round((decimal)(viewport.Width / 512)) * 512;
            else if (viewport.Width < 1500) width = 768;
            else width = 1280;

            _pos = position;
            _size = new Point(width, viewport.Height / 33);
            _name = name;
            _subDisplay = new Texture2D[3];
            R = 0;
            G = 0;
            B = 0;
            _gap = gap;
            _visible = false;
            _selectedBevel = -1;

            DisplayRect();
        }

        private void DisplayRect()
        {
            _subDisplayRect = new Rectangle[3];
            for (int i = 0; i < 3; i++)
            {
                _subDisplayRect[i] = new Rectangle(_pos.X, _pos.Y + (_gap * i) + (_size.Y * i), _size.X, _size.Y);
            }

            _displayRect = new Rectangle(_pos.X + (_size.X / 4), _pos.Y + (_gap * 3) + (_size.Y * 3), _size.X / 2, _size.X / 4);
        }

        public void ChangeColour(GraphicsDeviceManager graphics, Rectangle viewPort, bool fullBuild)
        {
            if (fullBuild)
            {
                Color[] data;

                for (int j = 0; j < 3; j++)
                {
                    data = new Color[256];
                    _subDisplay[j] = new Texture2D(graphics.GraphicsDevice, 256, 1);

                    int r = 0, g = 0, b = 0;

                    for (int i = 0; i < 256; i++)
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
            
            _display = new Texture2D(graphics.GraphicsDevice, 1, 1);
            _display.SetData<Color>(new Color[] { new Color(R, G, B) } );

            if (_exampleImage is not null)
            {
                CreateExampleImage();
            }
        }

        public bool ClickEvent(MouseState mouse)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_selectedBevel == i || (_subDisplayRect[i].Contains(mouse.Position) && _selectedBevel == -1))
                {
                    int delta = mouse.Position.X - _subDisplayRect[i].X;
                    int modifyAmount = (int)((float)delta / (float)(_subDisplayRect[i].Width / 256f));

                    if (modifyAmount > 255)
                        modifyAmount = 255;
                    else if (modifyAmount < 0)
                        modifyAmount = 0;

                    switch (i)
                    {
                        case 0:
                        {
                            R = (byte)modifyAmount;
                            break;
                        }
                        case 1:
                        {
                            G = (byte)modifyAmount;
                            break;
                        }
                        case 2:
                        {
                            B = (byte)modifyAmount;
                            break;
                        }
                    }
                    
                    _selectedBevel = i;
                    return true;
                }
            }

            return false;
        }

        private void CreateExampleImage()
        {
            Color[] data; 

            Content.Unload();
            _exampleImage = Content.Load<Texture2D>(_exampleImagePath);

            data = new Color[_exampleImage.Width * _exampleImage.Height]; 
            _exampleImage.GetData<Color>(data);
            
            for (int j = 0; j < data.Length; j++)
            {
                if (data[j] != Color.Transparent && data[j] != Color.Black)
                {
                    data[j] = new Color((byte)((float)(data[j].R / 255f) * R), 
                                        (byte)((float)(data[j].G / 255f) * G), 
                                        (byte)((float)(data[j].B / 255f) * B));
                }
            }

            _exampleImage.SetData<Color>(data);
        }

        public void Deselect()
        {
            _selectedBevel = -1;
        }

        public void LoadContent(GraphicsDeviceManager graphics, ContentManager content, int tileSizeX, int tileSizeY)
        {
            Content = new ContentManager(content.ServiceProvider, content.RootDirectory);
            _bevel = content.Load<Texture2D>("Texture/Menu/SelectBevel");
            _border = new TextureDivide(graphics, content, "Texture/Menu/LabelDivide", tileSizeX, tileSizeY);
        }

        public void UnloadContent()
        {
            for (int i = 0; i < _subDisplay.Length; i++)
            {
                _subDisplay[i].Dispose();
            }

            _bevel.Dispose();

            if (_exampleImage is not null)
            {
                _exampleImage.Dispose();
            }
        }

        public void Rename(string name)
        {
            _name = name;
        }

        public void SetExampleImage(string imagePath)
        {
            if (_exampleImage is not null)
            {
                _exampleImage.Dispose();
            }

            _exampleImagePath = imagePath;
            _exampleImage = Content.Load<Texture2D>(_exampleImagePath);

            _displayRect = new Rectangle(_pos.X, _pos.Y + (_gap * 3) + (_size.Y * 3), _size.X / 2, _size.X / 4);
            CreateExampleImage();
        }

        public void UnloadExampleImage()
        {
            if (_exampleImage is not null) _exampleImage.Dispose();
            _exampleImage = null;
        }

        public void SetDisplayColour(Color colour)
        {
            R = colour.R;
            G = colour.G;
            B = colour.B;
        }

        public Color GetColour()
        {
            return new Color(R, G, B);
        }

        public Rectangle GetTextboxData(int i)
        {
            return new Rectangle(_subDisplayRect[i].X + _subDisplayRect[i].Width + _gap, _subDisplayRect[i].Y - ((_subDisplayRect[i].Height * 3) / 4), 48 * 2, _subDisplayRect[i].Height * 3);
        }

        public bool SetTextboxData(string textboxName, string text)
        {
            bool number = true;
            foreach (char charChk in text)
            {
                if (!char.IsDigit(charChk))
                    number = false;
            }

            if (number && !String.IsNullOrWhiteSpace(text))
            {
                int textInt = Convert.ToInt32(text);
                if (textInt > 255)
                    textInt = 255;
                else if (textInt < 0)
                    textInt = 0;

                switch (textboxName)
                {
                    case "r":
                        R = (byte)textInt; 
                        break;
                    case "g":
                        G = (byte)textInt;
                        break;
                    case "b":
                        B = (byte)textInt;
                        break;
                }
            }
            return number;
        }

        public void Draw(SpriteBatch spriteBatch, Rectangle viewPort)
        {
            for (int i = 0; i < 3; i++)
            {
                _border.Draw(spriteBatch, new Rectangle(_subDisplayRect[i].X - _border._tileSizeX, _subDisplayRect[i].Y - _border._tileSizeY, _subDisplayRect[i].Width + (_border._tileSizeX * 2), _subDisplayRect[i].Height + (_border._tileSizeY * 2)));
                spriteBatch.Draw(_subDisplay[i], _subDisplayRect[i], Color.White);
                
                int bevelLoc = 0;
                switch (i)
                {
                    case 0: bevelLoc = R; break;
                    case 1: bevelLoc = G; break;
                    case 2: bevelLoc = B; break;
                }
                spriteBatch.Draw(_bevel, new Rectangle(_subDisplayRect[i].X + ((_subDisplayRect[i].Width / 256) * bevelLoc) - (_subDisplayRect[i].Height / 4), _subDisplayRect[i].Y + (_subDisplayRect[i].Height / 4), _subDisplayRect[i].Height / 2, _subDisplayRect[i].Height / 2), Color.White);
            }

            if (_exampleImage is not null)
            {
                spriteBatch.Draw(_exampleImage, new Rectangle(_pos.X + (int)(_size.X / 1.5f), _pos.Y + (_gap * 3) + (_size.Y * 3), _size.X / 4, _size.X / 4), Color.White);
            }

            spriteBatch.Draw(_display, _displayRect, Color.White);
            _border.Draw(spriteBatch, new Rectangle(_displayRect.X - _border._tileSizeX, _displayRect.Y - _border._tileSizeY, _displayRect.Width + (_border._tileSizeX * 2), _displayRect.Height + (_border._tileSizeY * 2)));
        }
    }
}