using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Maze_Project.Sprites
{
    class Guidance_Orb 
    {
        private string GridType;
        float rotation=0f;
        Queue<int> Path;
        int NextADJPos,CurrentADJPos, ScreenWidth, Scale, CellWidth;
        Rectangle destRect;
        int[,] ADJlist;
        Texture2D Texture;
        Sprite Flag;

        public Guidance_Orb(Texture2D texture, int cellWidth, int screenWidth,string gridType,int[,] newADJlist, int newADJPos, int ParentNumber, Sprite flag)
        {
            Parent = ParentNumber; //Parent Number is assciated with the difficulty of the AI, 1 being easy, 2 med etc. Player has 0
            Texture = texture;
            ScreenWidth = screenWidth;
            CellWidth = cellWidth;
            Scale = (screenWidth / CellWidth);
            GridType = gridType;
            ADJlist = newADJlist;
            Flag = flag;

            CurrentADJPos = newADJPos;
            Path = ShortestPath(ADJlist, newADJPos, flag.CurrentADJPos);
            NextADJPos = Path.Dequeue();
            if (GridType == "Quad")
            {
                for (int i = 0; i < 4; i++)
                {
                    if (ADJlist[newADJPos, i + 1] != 0 && ADJlist[newADJPos, i + 1] == Path.Peek())
                        rotation = ((float)Math.PI * ((i - 1) / 2f));
                }
            }
            else //Else Gridtype is Hex so for loop to 6
            {
                for (int i = 0; i < 6; i++)
                {
                    if (ADJlist[newADJPos, i + 1] != 0 && ADJlist[newADJPos, i + 1] == Path.Peek())
                        rotation = ((float)Math.PI * ((i - 1) / 3f));
                }
            }
        }

        public void Update(int playerADJPos, Vector2 playerPos)
        {
            if (CurrentADJPos == NextADJPos && Path.Count != 0)
            {
                NextADJPos = Path.Dequeue();
                if (Path.Count != 0)
                {
                    if (GridType == "Quad")
                    {

                        for (int i = 0; i < 4; i++)
                        {
                            if (ADJlist[CurrentADJPos, i + 1] != 0 && ADJlist[CurrentADJPos, i + 1] == Path.Peek())
                                rotation = ((float)Math.PI * ((i - 1) / 2f));
                        }
                    }
                    else //Hex
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            if (ADJlist[CurrentADJPos, i + 1] != 0 && ADJlist[CurrentADJPos, i + 1] == Path.Peek())
                                rotation = ((float)Math.PI * ((i - 1) / 3f));
                        }
                    }
                }
            }
            else if (CurrentADJPos != playerADJPos)
            {
                CurrentADJPos = playerADJPos;
                Path = ShortestPath(ADJlist, CurrentADJPos, Flag.CurrentADJPos);
                if (Path.Count != 0)
                {
                    NextADJPos = Path.Dequeue();
                    if (GridType == "Quad")
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            if (ADJlist[CurrentADJPos, i + 1] != 0 && ADJlist[CurrentADJPos, i + 1] == Path.Peek())
                                rotation = ((float)Math.PI * ((i - 1) / 2f));
                        }
                    }
                    else //Hex
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            if (ADJlist[CurrentADJPos, i + 1] != 0 && ADJlist[CurrentADJPos, i + 1] == Path.Peek())
                                rotation = ((float)Math.PI * ((i - 1) / 3f));
                        }
                    }
                }

            }

            destRect = new Rectangle((int)playerPos.X, (int)playerPos.Y, CellWidth, 3);
        }

        public void Draw (SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture: Texture, destinationRectangle: destRect, rotation: rotation, color: Color.Goldenrod);
        } 
        public Queue<int> ShortestPath(int[,] ADJlist, int startPos, int targetPos)
        {
            int maxNeighbours = 0;
            if (GridType == "Quad")
                maxNeighbours = 4;
            else if (GridType == "Hex")
                maxNeighbours = 6;
            Queue<int> Q = new Queue<int>();
            Queue<int> Path = new Queue<int>();
            List<int> revPath = new List<int>();
            List<int> parent = new List<int>();
            List<bool> discovered = new List<bool>();
            int v, c;
            bool found = false;


            for (int i = 1; i < (ScreenWidth / CellWidth) * Scale + 2; i++)
            {
                discovered.Add(false);
                parent.Add(-1);
            }
            Q.Enqueue(startPos);
            discovered[startPos] = true; //StartPos is source

            while (Q.Count > 0 && found == false)
            {
                v = Q.Dequeue();
                for (int u = 1; u < maxNeighbours + 1; u++)
                {
                    if (ADJlist[v, u] != 0 && discovered[ADJlist[v, u]] == false && found == false)
                    {
                        Q.Enqueue(ADJlist[v, u]);
                        discovered[ADJlist[v, u]] = true;
                        parent[ADJlist[v, u]] = v;
                        if (ADJlist[v, u] == targetPos)
                            found = true;
                    }
                }
            }
            if (found == true)
            {
                c = targetPos;
                revPath.Add(targetPos);
                do
                {
                    c = parent[c];
                    revPath.Add(c);
                } while (c != startPos); //1 Can be replaced with source pos

                revPath.Reverse();
                foreach (var cell in revPath)
                {
                    Path.Enqueue(cell);
                }
                Path.Enqueue(targetPos);
            }
            return Path;
        }

        public int Parent { get; }
    }
}
