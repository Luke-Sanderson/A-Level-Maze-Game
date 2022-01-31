using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Maze_Project.Sprites
{
    public class Sprite
    {
        public Vector2 Direction;
        public Vector2 Position;
        public Vector2 Origin;
        public int CurrentADJPos;
        public int Scale, ScreenWidth;
        public float _rotation;
        protected float cellWidth;
        protected float RotationVelocity;
        public float LinearVelocity;

        public Rectangle sourcRect;
        public Rectangle destRect;
        protected Texture2D texture;
        public int Frames;

        public Sprite(Texture2D newTexture, int newCellWidth, int frames, int screenWidth)
        {
            texture = newTexture;

            //Default origin in centre if sprite

            //RotationVelocity = 20f / (768 / newScale);
            //LinearVelocity = 20f / (768 / newScale);
            sourcRect= new Rectangle(0, 0, 150, 167);

            Scale = (screenWidth / newCellWidth);
            ScreenWidth = screenWidth;
            cellWidth = newCellWidth;
            Origin = new Vector2(texture.Width / frames / 2, texture.Height / 2);
            Position = new Vector2(newCellWidth / 2, newCellWidth / 2);
            Frames = frames;
            
        }

       
        public void Reset(float newScale)
        {
            cellWidth = newScale;
            Scale = (int)(ScreenWidth / newScale);
            Position = new Vector2(cellWidth / 2, cellWidth / 2);
            LinearVelocity = 20f / (Scale/2);
            _rotation = 0f;
            CurrentADJPos = 1;
            sourcRect = new Rectangle(0, 0, 150, 167);

        }
    }
}
