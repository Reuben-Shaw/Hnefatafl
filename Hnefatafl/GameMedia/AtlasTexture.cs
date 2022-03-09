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
    sealed class AtlasTexture
    {
        private Texture2D[,] _texture;

        private Dictionary<string, Point> _locator = new Dictionary<string, Point>();
        
        public AtlasTexture(GraphicsDeviceManager graphics, ContentManager Content, string texturePath)
        {
            Texture2D fullAtlas;
            Color[] atlasColourArray, data;

            try
            {
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
                    Console.WriteLine($"Data size: {data.Length}");

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

        public AtlasTexture(GraphicsDeviceManager graphics, ContentManager Content, string texturePath, string[] names)
        {
            Texture2D fullAtlas;
            Color[] atlasColourArray, data;

            try
            {
                fullAtlas = Content.Load<Texture2D>(texturePath);

                atlasColourArray = new Color[fullAtlas.Width * fullAtlas.Height];
                data = new Color[16 * 16];

                fullAtlas.GetData<Color>(atlasColourArray);

                _texture = new Texture2D[fullAtlas.Width / 16, fullAtlas.Height / 16];

                int x = 0, y = 0;
                for (int name = 0; name < (fullAtlas.Width / 16) * (fullAtlas.Height / 16); name++)
                {
                    _locator.Add(names[name], new Point(x, y));

                    for (int i = y * 16; i < (y + 1) * 16; i++)
                    {
                        for (int j = x * 16; j < (x + 1) * 16; j++)
                        {
                            data[((i - (y * 16)) * 16) + (j - (x * 16))] = atlasColourArray[(j) + (i * fullAtlas.Width)];
                        }
                    }

                    _texture[x, y] = new Texture2D(graphics.GraphicsDevice, 16, 16);
                    _texture[x, y].SetData<Color>(data);
                    Console.WriteLine($"Data size: {data.Length}");

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
}