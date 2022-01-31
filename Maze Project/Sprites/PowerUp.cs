using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Maze_Project.Sprites
{
    public class PowerUp
    {
        public enum PowerUpTypes
        {
            Teleport,
            Speed,
            GuidenceOrb,
            Minion
        }
        public Vector2 Position, Scale;
        public int CellWidth, ScreenWidth;
        public Rectangle sourcRect;
        public RotatedCollisionRectangle Boundary;
        protected Texture2D texture;
        public int Frames;
        string GridType;
        public PowerUpTypes CurrentType;

        public PowerUp(List<Texture2D> newTextures, Vector2 GridPos, int newCellWidth, int frames, int screenWidth, string gridType, Random rnd) 
        {
            sourcRect = new Rectangle(0, 0, 81, 92);

            ScreenWidth = screenWidth;
            CellWidth = newCellWidth;
            Frames = frames;
            GridType = gridType;

            switch (rnd.Next(0, 3))
            {
                case 0:
                    CurrentType=PowerUpTypes.Teleport;
                    texture = newTextures[0];
                    break;
                case 1:
                    CurrentType = PowerUpTypes.Speed;
                    texture = newTextures[1];
                    break;
                case 2:
                    CurrentType = PowerUpTypes.GuidenceOrb;
                    texture = newTextures[2];
                    break;
                case 3:
                    CurrentType = PowerUpTypes.Minion;
                    texture = newTextures[3];
                    break;
            }
            Scale = new Vector2((float)CellWidth / (texture.Width/3)) * (0.8f + 0.2f * ((GridType == "Quad") ? 1 : 0));
            switch (gridType)
            {
                case "Quad":
                    Position = GridPos * CellWidth + new Vector2((CellWidth / 2) - (texture.Width / 12 * Scale.X), (CellWidth / 2) - (texture.Height/ 2 * Scale.Y));
                    break;
                case "Hex":
                    //Scale *= 0.8f;
                    if (GridPos.Y % 2 == 0)
                    {
                        Position = new Vector2(((int)(CellWidth * GridPos.X) + (CellWidth / 2) - (CellWidth / 5)), (int)(CellWidth / Math.Sqrt(3)) * (1 + 1.5f * (float)(GridPos.Y)) - (CellWidth / 4));
                    }
                    else
                    {
                        Position = new Vector2(((int)(CellWidth * GridPos.X)) + CellWidth - (CellWidth / 5), (int)(CellWidth / Math.Sqrt(3)) * (1 + 1.5f * (float)(GridPos.Y)) - (CellWidth / 4));
                    }
                    break;
            }

            Boundary = new RotatedCollisionRectangle(new Rectangle((int)Position.X, (int)Position.Y, (int)(texture.Width / 6 * Scale.X), (int)(texture.Height * Scale.Y)), 0f);

        }

       

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture: texture, position: Position,sourceRectangle: sourcRect,  color: Color.White, scale: Scale);
        }

    }
}
