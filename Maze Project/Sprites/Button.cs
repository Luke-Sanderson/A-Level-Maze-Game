using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Maze_Project.Sprites
{
    public class Button
    {
        Vector2 Position;
        Texture2D Texture;
        Rectangle rec;
        SpriteFont Font;
        readonly int AverageLetterWidth = 10;
        public string Name;
        string Text;
        int Width, Height;

        public Button(Vector2 newPos, int width, int height, Texture2D newTexture, string name, string text, SpriteFont font)
        {
            Position = newPos;
            Width = width;
            Height = height;
            Texture = newTexture;
            Name = name;
            Text = text;
            rec = new Rectangle((int)Position.X, (int)Position.Y, Width, Height);
            Font = font;
        }

        public bool IsPressedCheck(MouseState prevMouse)
        {
            if (IsMouseHovered(prevMouse) &&
            Mouse.GetState().LeftButton == ButtonState.Released &&  //Only goes through once the user unclick to prevent missclicks as often
            prevMouse.LeftButton == ButtonState.Pressed) 
            {
                return true;
            }
            return false;
        }
        public bool IsMouseHovered(MouseState prevMouse)
        {
            if(                                                  
            Mouse.GetState().Position.X < Position.X + Width &&    //Checks to see if the mouse is inside the button area 
            Mouse.GetState().Position.X > Position.X &&          //Values 200 and 50 can later be changed to texure.Width/Height once Texures are finalised
            Mouse.GetState().Position.Y < Position.Y + Height &&
            Mouse.GetState().Position.Y > Position.Y)
            {
                return true;
            }
            return false;
        }

        public void Draw(SpriteBatch spriteBatch, MouseState prevMouse)
        {
            if (IsMouseHovered(prevMouse))
                spriteBatch.Draw(Texture, rec, Color.Gray);
            else
                spriteBatch.Draw(Texture, rec, Color.White);

            spriteBatch.DrawString(Font, Text, new Vector2(Position.X + (Width / 2) - (Text.Length / 2 * AverageLetterWidth), Position.Y + 5), Color.Black);
        }

        public string NewText
        {
            set { Text = value; }
        }
    }
}
