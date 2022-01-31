using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Maze_Project.Sprites
{
    public class Player : Sprite
    {
        public KeyboardState _currentKey;
        public KeyboardState _previousKey;
        Sprite Flag;
        public RotatedCollisionRectangle boundary;
        public bool speedBoostBool, guidenceBool;
        public float boostElapsed, guidenceElapsed;
 

        public string GridType;

        public Player(Texture2D newTexture, int newCellWidth, int frames, string gridType, int screenWidth, Sprite flag) : base(newTexture, newCellWidth, frames, screenWidth)
        {
            //texture = newTexture;
            //Scale = newScale;
            GridType = gridType;
            switch (GridType)
            {
                case "Quad":
                    boundary = new RotatedCollisionRectangle(new Rectangle(0, 0, (int)(texture.Width / 3 * (cellWidth / 250)), (int)(texture.Height * (cellWidth / 300))), 0f);
                    Position = new Vector2(cellWidth / 2, cellWidth / 2);                             
                    break;
                case "Hex":
                    boundary = new RotatedCollisionRectangle(new Rectangle(0, 0, (int)((texture.Width / 3 * (cellWidth / 250)*0.8f)), (int)((texture.Height * (cellWidth / 300))*0.8f)), 0f);
                    Position = new Vector2(cellWidth / 2, (int)(cellWidth / Math.Sqrt(3)));
                    break;
            }
            //Default origin in centre if sprite
            //boundary = new RotatedCollisionRectangle( new Rectangle(0, 0, (int)(texture.Width / 3 * (cellWidth/250)), (int)(texture.Height * (cellWidth/300))), 0f);
            RotationVelocity = 3.5f;
            LinearVelocity = 20f / (Scale / 2);
            Origin = new Vector2(texture.Width / frames / 2, texture.Height / 2);
            Flag = flag;
            
            CurrentADJPos = 1;
        }
        
        
        public void Draw(SpriteBatch spriteBatch) 
        {
            spriteBatch.Draw(texture, Position, null, sourcRect, Origin, _rotation, new Vector2(cellWidth / 200) * (0.8f + 0.2f * ((GridType == "Quad") ? 1 : 0)), Color.White, SpriteEffects.None, 1);
        }

        public void Update(GameTime gameTime)
        {

            Direction = new Vector2((float)Math.Cos(_rotation), (float)Math.Sin(_rotation));
            switch (GridType)
            {
                case "Quad":
                    CurrentADJPos = (int)Math.Floor(Position.X / cellWidth) + 1 + (int)(ScreenWidth / cellWidth) * (int)Math.Floor(Position.Y / cellWidth);
                    break;

                case "Hex":
                    if (Position.X < (Flag.Position.X +  (cellWidth/5)) + (cellWidth/5) && Position.X > (Flag.Position.X + (cellWidth / 5)) - (cellWidth/5) && Position.Y < (Flag.Position.Y + (cellWidth / 5)) + (cellWidth/5) && Position.Y > (Flag.Position.Y + (cellWidth / 5)) - (cellWidth/5))
                        CurrentADJPos = Flag.CurrentADJPos;
                    break;
            }

            boundary.ChangePosition((int)Position.X - boundary.Width / 2, (int)Position.Y - boundary.Height / 2);
            boundary.Rotation = _rotation;


            _previousKey = _currentKey;
            _currentKey = Keyboard.GetState();

            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                _rotation -= MathHelper.ToRadians(RotationVelocity);
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                _rotation += MathHelper.ToRadians(RotationVelocity);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                Position += Direction * LinearVelocity;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                Position -= Direction * LinearVelocity * 0.6f;
            }
            
            //destRect = new Rectangle((int)Position.X, (int)Position.Y, 167, 150);
        }
        public void Reset(float newScale,string newGridType) 
        {
            GridType = newGridType;
            cellWidth = newScale;
            Scale = (int)(ScreenWidth / newScale);
            LinearVelocity = 20f / (Scale / 2);

            speedBoostBool = false;
            guidenceBool = false;
            boostElapsed = 0f;
            guidenceElapsed = 0f;

            switch (GridType)
            {
                case "Quad":
                    Position = new Vector2(cellWidth / 2, cellWidth / 2);
                    boundary = new RotatedCollisionRectangle(new Rectangle(0, 0, (int)(texture.Width / 3 * (cellWidth / 250)), (int)(texture.Height * (cellWidth / 300))), 0f);

                    break;
                case "Hex":
                    Position = new Vector2(cellWidth / 2, (int)(cellWidth / Math.Sqrt(3)));
                    boundary = new RotatedCollisionRectangle(new Rectangle(0, 0, (int)((texture.Width / 3 * (cellWidth / 250) * 0.8f)), (int)((texture.Height * (cellWidth / 300)) * 0.8f)), 0f);

                    break;
            }

            _rotation = 0f;
            CurrentADJPos = 1;
            sourcRect = new Rectangle(0, 0, 150, 167);

        }
    }

}
