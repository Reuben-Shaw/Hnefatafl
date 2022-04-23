using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System;
using Hnefatafl.Media;

namespace Hnefatafl.MenuObjects
{
    class TurnDisplay : MenuObject
    {
        ContentManager Content;
        Texture2D[] _colours = new Texture2D[2];
        Texture2D _cover;
        TextureDivide _displayBackground;
        private bool m_defendersTurn;
        public bool _defendersTurn
        {
            get
            {
                return m_defendersTurn; 
            }
            set
            {
                m_defendersTurn = value;
                _doTransition = true;
            }
        }
        private bool _doTransition;

        private float _velocity;
        private float _colourLoc;
        private const float _delta = 0.5f;
        private const float _defaultVelocity = 50;

        public TurnDisplay(Point position, Point size, string name)
        {
            _pos = position;
            _size = size;
            _name = name;
            _doTransition = false;
            _defendersTurn = false;
            _velocity = _defaultVelocity;
            _colourLoc = 0;
        }

        public void Update(Color[] pawnColours, ContentManager content, GraphicsDeviceManager graphics)
        {
            if (Content is not null) Content.Unload();
            else Content = new ContentManager(content.ServiceProvider, content.RootDirectory);
            
            _colours[0] = new Texture2D(graphics.GraphicsDevice, 1, 1);
            _colours[0].SetData(new[] { pawnColours[0] });

            _colours[1] = new Texture2D(graphics.GraphicsDevice, 1, 1);
            _colours[1].SetData(new[] { pawnColours[1] });

            _displayBackground = new TextureDivide(graphics, Content, "Texture/Menu/TurnDisplay", _size.X / 2, _size.Y / 2);
            _cover = Content.Load<Texture2D>("Texture/Menu/TurnDisplayCover");
        }

        public void Transition(float elapsedTime)
        {
            if (_doTransition)
            {
                if (_defendersTurn) _colourLoc -= _velocity * elapsedTime;
                else _colourLoc += _velocity * elapsedTime;
                _velocity += _delta * elapsedTime;

                if (_colourLoc > _size.X)
                {
                    _colourLoc = _size.X;
                    _doTransition = false;
                    _velocity = _defaultVelocity;
                } 
                else if (_colourLoc < 0)
                {
                    _colourLoc = 0;
                    _doTransition = false;
                    _velocity = _defaultVelocity;
                }
            }
        }

        public void Draw(GraphicsDeviceManager graphics, SpriteBatch spriteBatch)
        {
            Rectangle rect = new Rectangle((int)_colourLoc + _pos.X, _pos.Y, _size.X, _size.Y);
            _displayBackground.Draw(spriteBatch, new Rectangle(_pos.X - (_size.X / 2), _pos.Y - (_size.Y / 2), _size.X * 4, _size.Y * 2));
            spriteBatch.Draw(_colours[0], rect, Color.White);
            spriteBatch.Draw(_colours[1], new Rectangle(rect.X + _size.X, rect.Y, rect.Size.X, rect.Size.Y), Color.White);
            spriteBatch.Draw(_cover, new Rectangle(_pos.X, _pos.Y, _size.X * 3, _size.Y), Color.White);
        }
    }
}