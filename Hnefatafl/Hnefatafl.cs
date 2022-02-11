using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using Hnefatafl.GameBoard;

namespace Hnefatafl
{
    public class Hnefatafl : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        Board _gameBoard = new Board();
        OptionObj _optionObj;
        public Hnefatafl()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = 960;
            _graphics.PreferredBackBufferHeight = 540;
            _graphics.ApplyChanges();

            _optionObj = new OptionObj(new Color[]{Color.White, Color.Black, Color.Red, Color.Gold});

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _gameBoard.LoadContent(_graphics);
            _gameBoard.TileGeneration(_optionObj.boardColour);

            Console.WriteLine("Successful LoadContent");
        }

        protected override void UnloadContent()
        {
            _gameBoard.UnloadContent();

            Console.WriteLine("Successful UnloadContent");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin();

            _gameBoard.Draw(gameTime, _spriteBatch);

            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
