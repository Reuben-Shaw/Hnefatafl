using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Hnefatafl.Media
{
    public enum TextureAtlasLink: int { Seperated, UpDown, LeftRight, UpEnd, RightEnd, DownEnd, LeftEnd, Cross, UpFork, RightFork, DownFork, LeftFork, UpCorner, RightCorner, DownCorner, LeftCorner };
    public enum DivideLink: int { UpLeft, UpMid, UpRight, MidLeft, MidMid, MidRight, DownLeft, DownMid, DownRight}
    
    class AtlasTexture
    {
        protected Texture2D[,] _texture;
        protected string _texturePath;
        protected ContentManager Content;
        
        protected Dictionary<string, Point> _locator = new Dictionary<string, Point>();

        protected AtlasTexture() {  } //Used for inheretance
        
        public AtlasTexture(GraphicsDeviceManager graphics, ContentManager content, string texturePath)
        { 
           Content = new ContentManager(content.ServiceProvider, content.RootDirectory);
           LoadTexture(graphics, texturePath, null);
        }

        // public AtlasTexture(GraphicsDeviceManager graphics, ContentManager Content, string texturePath, string[] names)
        // {
        //     _texturePath = texturePath;
        //     Texture2D fullAtlas;
        //     Color[] atlasColourArray, data;
        //     Content = new ContentManager(content.ServiceProvider, content.RootDirectory);

        //     try
        //     {
        //         fullAtlas = Content.Load<Texture2D>(texturePath);

        //         atlasColourArray = new Color[fullAtlas.Width * fullAtlas.Height];
        //         data = new Color[16 * 16];

        //         fullAtlas.GetData<Color>(atlasColourArray);

        //         _texture = new Texture2D[fullAtlas.Width / 16, fullAtlas.Height / 16];

        //         int x = 0, y = 0;
        //         for (int name = 0; name < (fullAtlas.Width / 16) * (fullAtlas.Height / 16); name++)
        //         {
        //             _locator.Add(names[name], new Point(x, y));

        //             for (int i = y * 16; i < (y + 1) * 16; i++)
        //             {
        //                 for (int j = x * 16; j < (x + 1) * 16; j++)
        //                 {
        //                     data[((i - (y * 16)) * 16) + (j - (x * 16))] = atlasColourArray[(j) + (i * fullAtlas.Width)];
        //                 }
        //             }

        //             _texture[x, y] = new Texture2D(graphics.GraphicsDevice, 16, 16);
        //             _texture[x, y].SetData<Color>(data);
        //             Console.WriteLine($"Data size: {data.Length}");

        //             x++;
        //             if (x * 16 == fullAtlas.Width) { x = 0; y++; }
        //         }

        //         fullAtlas.Dispose();
        //     }
        //     catch (System.Exception)
        //     {
        //         Console.WriteLine("There was an exception loading the texture");
        //     }
        // }

        private void LoadTexture(GraphicsDeviceManager graphics, string texturePath, string[] names)
        {
            _texturePath = texturePath;
            Texture2D fullAtlas;
            Color[] atlasColourArray, data;

            fullAtlas = Content.Load<Texture2D>(texturePath);

            atlasColourArray = new Color[fullAtlas.Width * fullAtlas.Height];
            data = new Color[16 * 16];

            fullAtlas.GetData<Color>(atlasColourArray);

            _texture = new Texture2D[fullAtlas.Width / 16, fullAtlas.Height / 16];

            int x = 0, y = 0;
            for (int orientation = 0; orientation < (fullAtlas.Width / 16) * (fullAtlas.Height / 16); orientation++)
            {
                _locator.Add(((TextureAtlasLink)orientation).ToString(), new Point(x, y));

                //Console.WriteLine($"Location: {x}, {y} which is {x * 16}, {y * 16}");
                for (int i = y * 16; i < (y + 1) * 16; i++)
                {
                    for (int j = x * 16; j < (x + 1) * 16; j++)
                    {
                        data[((i - (y * 16)) * 16) + (j - (x * 16))] = atlasColourArray[(j) + (i * fullAtlas.Width)];
                    }
                }

                _texture[x, y] = new Texture2D(graphics.GraphicsDevice, 16, 16);
                _texture[x, y].SetData<Color>(data);

                x++;
                if (x * 16 == fullAtlas.Width) { x = 0; y++; }
            }

            fullAtlas.Dispose();
        }

        public Texture2D GetTexture(TextureAtlasLink key)
        {
            Point dictionaryPoint = _locator[key.ToString()];
            return _texture[dictionaryPoint.X, dictionaryPoint.Y];
        }

        public Texture2D GetTexture(string key)
        {
            Point dictionaryPoint = _locator[key];
            return _texture[dictionaryPoint.X, dictionaryPoint.Y];
        }

        public void ReloadContent(GraphicsDeviceManager graphics, Color userColour)
        {
            Content.Unload();

            _locator.Clear();
            LoadTexture(graphics, _texturePath, null);
            HueShiftTexture(userColour);
        }

        public void HueShiftTexture(Color userColour)
        {
            for (int x = 0; x < _texture.GetLength(0); x++)
            {
                for (int y = 0; y < _texture.GetLength(1); y++)
                {
                    Color[] data;
                    data = new Color[_texture[x, y].Width * _texture[x, y].Height];
                    _texture[x, y].GetData<Color>(data);

                    for (int i = 0; i < data.Length; i++)
                    {
                        if (data[i] != Color.Transparent && data[i] != Color.Black)
                        {
                            data[i] = new Color((byte)((float)(data[i].R / 255f) * userColour.R), 
                                                (byte)((float)(data[i].G / 255f) * userColour.G), 
                                                (byte)((float)(data[i].B / 255f) * userColour.B));
                        }
                    }
                    
                    _texture[x, y].SetData<Color>(data);
                }
            }
        }

        public void UnloadContent()
        {
            if (_texture is not null)
            {
                for (int x = 0; x < _texture.GetLength(0); x++)
                {
                    for (int y = 0; y < _texture.GetLength(1); y++)
                    {
                        _texture[x, y].Dispose();
                    }
                }
            }
        }
    }

    sealed class TextureDivide : AtlasTexture
    {
        public int _tileSizeX { get; set; }
        public int _tileSizeY { get; set; }
        
        public TextureDivide(GraphicsDeviceManager graphics, ContentManager content, string texturePath, int tileSizeX, int tileSizeY)
        {
            _tileSizeX = tileSizeX;
            _tileSizeY = tileSizeY;
            
            _texturePath = texturePath;
            Texture2D fullAtlas;
            Color[] atlasColourArray, data;
            Content = new ContentManager(content.ServiceProvider, content.RootDirectory);

            try
            {
                fullAtlas = Content.Load<Texture2D>(texturePath);

                atlasColourArray = new Color[fullAtlas.Width * fullAtlas.Height];
                data = new Color[16 * 16];

                fullAtlas.GetData<Color>(atlasColourArray);

                this._texture = new Texture2D[fullAtlas.Width / 16, fullAtlas.Height / 16];

                int x = 0, y = 0;
                for (int piece = 0; piece < (fullAtlas.Width / 16) * (fullAtlas.Height / 16); piece++)
                {
                    _locator.Add(((DivideLink)piece).ToString(), new Point(x, y));

                    //Console.WriteLine($"Location: {x}, {y} which is {x * 16}, {y * 16}");
                    for (int i = y * 16; i < (y + 1) * 16; i++)
                    {
                        for (int j = x * 16; j < (x + 1) * 16; j++)
                        {
                            data[((i - (y * 16)) * 16) + (j - (x * 16))] = atlasColourArray[(j) + (i * fullAtlas.Width)];
                        }
                    }

                    _texture[x, y] = new Texture2D(graphics.GraphicsDevice, 16, 16);
                    _texture[x, y].SetData<Color>(data);

                    x++;
                    if (x * 16 == fullAtlas.Width) { x = 0; y++; }
                }

                fullAtlas.Dispose();
            }
            catch (System.Exception)
            {
                Console.WriteLine("There was an exception loading the texture");
            }
        }

        private Texture2D GetTexture(DivideLink key)
        {
            Point dictionaryPoint = _locator[key.ToString()];
            return _texture[dictionaryPoint.X, dictionaryPoint.Y];
        }

        public void Draw(SpriteBatch spriteBatch, Rectangle rect)
        {
            spriteBatch.Draw(GetTexture(DivideLink.MidMid), new Rectangle(rect.X + _tileSizeX, rect.Y + _tileSizeY, rect.Width - (_tileSizeX * 2), rect.Height - (_tileSizeY * 2)), Color.White);

            spriteBatch.Draw(GetTexture(DivideLink.UpMid), new Rectangle(rect.X + _tileSizeX, rect.Y, rect.Width - (_tileSizeX * 2), _tileSizeY), Color.White);
            spriteBatch.Draw(GetTexture(DivideLink.DownMid), new Rectangle(rect.X + _tileSizeX, rect.Y + rect.Height - _tileSizeY, rect.Width - (_tileSizeX * 2), _tileSizeY), Color.White);

            spriteBatch.Draw(GetTexture(DivideLink.MidLeft), new Rectangle(rect.X, rect.Y + _tileSizeY, _tileSizeX, rect.Height - (_tileSizeY * 2)), Color.White);
            spriteBatch.Draw(GetTexture(DivideLink.MidRight), new Rectangle(rect.X + rect.Width - _tileSizeX, rect.Y + _tileSizeY, _tileSizeX, rect.Height - (_tileSizeY * 2)), Color.White);

            spriteBatch.Draw(GetTexture(DivideLink.UpLeft), new Rectangle(rect.X, rect.Y, _tileSizeX, _tileSizeY), Color.White);
            spriteBatch.Draw(GetTexture(DivideLink.DownRight), new Rectangle(rect.X + rect.Width - _tileSizeX, rect.Y + rect.Height - _tileSizeY, _tileSizeX, _tileSizeY), Color.White);
            spriteBatch.Draw(GetTexture(DivideLink.DownLeft), new Rectangle(rect.X, rect.Y + rect.Height - _tileSizeY, _tileSizeX, _tileSizeY), Color.White);
            spriteBatch.Draw(GetTexture(DivideLink.UpRight), new Rectangle(rect.X + rect.Width - _tileSizeX, rect.Y, _tileSizeX, _tileSizeY), Color.White);
        }
    }
}