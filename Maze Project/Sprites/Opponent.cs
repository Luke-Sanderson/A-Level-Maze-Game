using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Maze_Project.Sprites
{
    public class Opponent : Sprite
    {
        public int Difficulty;
        string GridType;
        Color Tint;
        public bool speedBoostBool, guidenceBool;
        public float boostElapsed, guidenceElapsed;

        public Vector2 NextPos;
        public int NextADJPos, PrevADJPos;
        int[,] ADJlist;
        public float nextDirection;
        Hex_Cell HexGoal;
        Cell QuadGoal;
        public RotatedCollisionRectangle boundary;

        double xOffset = 0, yOffset;

        public Queue<int> PathQueue;
        
        public Opponent(Texture2D newTexture, int newScale, int frames, int difficulty, string gridType,int screenWidth, int[,] newADJlist, Cell goal) : base(newTexture, newScale, frames, screenWidth)
        {
            texture = newTexture;
            cellWidth = newScale;
            Difficulty = difficulty;
            GridType = gridType;
            ADJlist = newADJlist;
            QuadGoal = goal;
            CurrentADJPos = 1;
            //Default origin in centre if sprite

            RotationVelocity = MathHelper.ToRadians(3.5f);
            LinearVelocity = 20f / (Scale/2);
            Origin = new Vector2(texture.Width / 6, texture.Height / 2);
            Position = new Vector2(cellWidth / 2, cellWidth / 2);
            boundary = new RotatedCollisionRectangle(new Rectangle(0, 0, (int)(texture.Width / 3 * (cellWidth / 250)), (int)(texture.Height * (cellWidth / 300))), 0f);

            switch (difficulty) //Switch to assign Difficulty specific attributes
            {
                case 1:

                    Tint = Color.Green;  //Swap out Tints for specific Sprites eventually
                    break;
                case 2:

                    Tint = Color.OrangeRed;

                    PathQueue = HoldLeft(ADJlist,CurrentADJPos, (int)goal.GridPos.X + 1 + (int)(Scale * goal.GridPos.Y));

                    break;
                case 3:
                    
                    Tint = Color.PaleVioletRed;

                    PathQueue = ShortestPath(ADJlist, CurrentADJPos, (int)goal.GridPos.X + 1 + (int)(Scale * goal.GridPos.Y)); 

                    break;
            }


            nextDirection = NextPosition();
            _rotation = nextDirection;
            NextADJPos = ADJlist[CurrentADJPos, (int)((nextDirection / Math.PI) * 2 + 2)];
            Direction = new Vector2((float)Math.Cos(nextDirection), (float)Math.Sin(nextDirection));

        }
        public Opponent(Texture2D newTexture, int newScale, int frames, int difficulty, string gridType, int screenWidth, int[,] newADJlist, Hex_Cell goal) : base(newTexture, newScale, frames, screenWidth )
        {
            Difficulty = difficulty;
            GridType = gridType;
            ADJlist = newADJlist;
            HexGoal = goal;
            CurrentADJPos = 1;
            //Default origin in centre if sprite

            RotationVelocity = MathHelper.ToRadians(3.5f);
            LinearVelocity = 20f / (Scale / 2)*0.8f;//TODO ADD 0.8 multiplier when Hex is reset
            Origin = new Vector2(texture.Width / frames / 2, texture.Height / 2);
            Position = new Vector2(cellWidth / 2, (int)(cellWidth / Math.Sqrt(3)));
            boundary = new RotatedCollisionRectangle(new Rectangle(0, 0, (int)((texture.Width / 3 * (cellWidth / 250) * 0.8f)), (int)((texture.Height * (cellWidth / 300)) * 0.8f)), 0f);

            yOffset = (int)(cellWidth / Math.Sqrt(3));

            switch (difficulty) //Switch to assign Difficulty specific attributes
            {
                case 1:

                    Tint = Color.Green;  //Swap out Tints for specific Sprites eventually

                    break;
                case 2:

                    Tint = Color.OrangeRed;

                    PathQueue = HoldLeft(ADJlist,CurrentADJPos, (int)goal.GridPos.X + 1 + (int)(Scale * goal.GridPos.Y));

                    break;
                case 3:

                    Tint = Color.PaleVioletRed;

                    PathQueue = ShortestPath(ADJlist, CurrentADJPos, (int)goal.GridPos.X + 1 + (int)(Scale * goal.GridPos.Y)); 
                    break;
            }


            nextDirection = NextPosition();
            _rotation = nextDirection;                     
            NextADJPos = ADJlist[CurrentADJPos, (int)((_rotation / Math.PI) * 3 + 2)];
                    
            Direction = new Vector2((float)Math.Cos(_rotation), (float)Math.Sin(_rotation));

        }
        /// <summary>
        /// Updates the logic of the Opponents such as movement and goal detection
        /// </summary>
        /// <param name="gameTime"></param>
        /// <returns></returns>
        public void Update(GameTime gameTime)
        {
            if (_rotation > 2* Math.PI)                  //Limits rotation to between 0 and 2 Pi
                _rotation -= (float)(2 * Math.PI);
            else if (_rotation < 0)
                _rotation += (float)(2 * Math.PI);

            if (nextDirection > 2 * Math.PI)                    //Limits nextDirection between 0 and 2 Pi
                nextDirection -= (float)(2 * Math.PI);
            else if (nextDirection < 0)
                nextDirection += (float)(2 * Math.PI);


            if (_rotation > nextDirection + 0.15f)
            {
                if (Math.Abs(_rotation - nextDirection) < Math.PI)
                    _rotation -= RotationVelocity;
                else
                    _rotation += RotationVelocity;
                
            }
            else if (_rotation < nextDirection - 0.15f)
            {
                if (Math.Abs(_rotation - nextDirection) < Math.PI)
                    _rotation += RotationVelocity;
                else
                    _rotation -= RotationVelocity;
            }
            else
            {
                _rotation = nextDirection;
                Direction = new Vector2((float)Math.Cos(_rotation), (float)Math.Sin(_rotation));

                Position += Direction * LinearVelocity;

                switch (GridType)
                {
                    #region case "Quad" 
                    case "Quad":
                        
                        CurrentADJPos = (int)Math.Floor(Position.X / cellWidth) + 1 + (int)(ScreenWidth / cellWidth) * (int)Math.Floor(Position.Y / cellWidth);

                        if (NextADJPos % Scale != 0)            //This stops the end column from acting like it is the first column     
                        {
                            NextPos = new Vector2(((int)(cellWidth * (NextADJPos % Scale) - (cellWidth / 2))), (int)cellWidth * (int)(NextADJPos / Scale) + (cellWidth / 2));
                        }
                        else
                        {
                            NextPos = new Vector2(((int)(cellWidth * (Scale) - (cellWidth / 2))), (int)(cellWidth * (int)(NextADJPos / Scale)) - (cellWidth / 2));
                        }

                        if (Position.X > (NextPos.X - 4) && Position.X < (NextPos.X + 4) && Position.Y > (NextPos.Y - 4) && Position.Y < (NextPos.Y + 4)) //Checks if Position is within 4 pixel tolerance of Next Position
                        {
                            Position = NextPos;                                                  //ensures the bee is at the centre of the square

                            nextDirection = NextPosition();
                            NextADJPos = ADJlist[CurrentADJPos, (int)(Math.Round((nextDirection  / Math.PI) * 2) + 2)];
                            Direction = new Vector2((float)Math.Cos(nextDirection), (float)Math.Sin(nextDirection));

                            

                        }
                        break;
                    #endregion
                    #region case "Hex"
                    case "Hex":

                        xOffset = 0;

                        if (Math.Floor(NextADJPos / (decimal)Scale) % 2 != 0 && NextADJPos % Scale != 0)   //Checks to see if its on an odd or even row
                            xOffset = cellWidth / 2;
                        else if (NextADJPos % Scale == 0 && Math.Floor(NextADJPos / (decimal)Scale) % 2 == 0)
                            xOffset = cellWidth / 2;

                        if (NextADJPos % Scale != 0)            //This stops the end column from acting like it is the first column     
                            NextPos = new Vector2(((int)(cellWidth * (NextADJPos % Scale) - (cellWidth / 2) + xOffset)), (int)yOffset * (1 + 3 * (float)Math.Floor(NextADJPos / (decimal)Scale / 2) + (((int)(NextADJPos / Scale) % 2) * 1.5f))); //Prev Y: (int)Scale * (int)(NextADJPos / cellWidth) + (int)yOffset)
                        else
                            NextPos = new Vector2(((int)(cellWidth * (Scale) - (cellWidth / 2) + xOffset)), (int)yOffset * (1 + 3 * (float)Math.Floor((NextADJPos / (decimal)Scale - 1) / 2) + (((int)(NextADJPos / Scale - 1) % 2) * 1.5f)));
                        //TODO: Anything before here can be moved to initialisation and inside the if statement so it doesnt run every update cycle
                        if (Position.X > (NextPos.X - 4) && Position.X < (NextPos.X + 4) && Position.Y > (NextPos.Y - 4) && Position.Y < (NextPos.Y + 4)) //Checks if Position is within 4 pixel tolerance of Next Position
                        {
                            PrevADJPos = CurrentADJPos;
                            CurrentADJPos = NextADJPos;

                            if (CurrentADJPos != HexGoal.ADJlistPos)
                            {
                                nextDirection = NextPosition();
                                NextADJPos = ADJlist[CurrentADJPos, (int)(Math.Round((nextDirection / Math.PI) * 3) + 2)];
                                Direction = new Vector2((float)Math.Cos(_rotation), (float)Math.Sin(_rotation));
                                Position = NextPos;      //Realligns opponent to centre of hexagon. Reduces inacurracy
                            }


                        }

                        break;
                        #endregion
                }

                boundary.ChangePosition((int)Position.X - boundary.Width / 2, (int)Position.Y - boundary.Height / 2);
                boundary.Rotation = _rotation;

                destRect = new Rectangle((int)Position.X, (int)Position.Y, 167, 150);
            }
           
        }
        /// <summary>
        /// Checks adjacent cells/path list and returns the direction of the next cell for the opponent to follow
        /// </summary>
        /// <param></param>
        /// <returns> Returns the Direction of Next cell</returns>
        public float NextPosition()
        {
            int current;
            switch (GridType)
            {
                case "Quad":
                    switch (Difficulty)
                    {
                        case 1:
                            int r;
                            var rnd = new Random();
                            do
                            {
                                r = rnd.Next(0, 4);
                            } while (ADJlist[CurrentADJPos, r + 1] == 0);
                            return ((float)Math.PI * ((r - 1) * 0.5f));

                        case 2:
                            current = PathQueue.Dequeue();
                            for (int i = 0; i < 4; i++)
                            {
                                if (ADJlist[current, i + 1] != 0 && ADJlist[current, i + 1] == PathQueue.Peek())
                                    return ((float)Math.PI * ((i - 1) * 0.5f));
                            }
                            throw new Exception("Cannot find direction to next path");
                        case 3:
                            current = PathQueue.Dequeue();
                            for (int i = 0; i < 4; i++)
                            {
                                if (ADJlist[current, i + 1] != 0 && ADJlist[current, i + 1] == PathQueue.Peek())
                                    return ((float)Math.PI * ((i - 1) * 0.5f));
                            }

                            throw new Exception("Control fell out of difficulty switch");
                    }
                    break;

                case "Hex":
                    switch (Difficulty)
                    {
                        case 1:
                            int r;
                            var rnd = new Random();
                            do
                            {
                                r = rnd.Next(0, 6);
                            } while (ADJlist[CurrentADJPos, r + 1] == 0);
                            return ((float)Math.PI * ((r - 1) / 3f));
                        case 2:
                            current = PathQueue.Dequeue();
                            for (int i = 0; i < 6; i++)
                            {
                                if (ADJlist[current, i + 1] != 0 && ADJlist[current, i + 1] == PathQueue.Peek())
                                    return ((float)Math.PI * ((i - 1) / 3f));
                            }
                            throw new Exception("Cannot find direction to next path");
                        case 3:
                            current = PathQueue.Dequeue();
                            for (int i = 0; i < 6; i++)
                            {
                                if (ADJlist[current, i + 1] != 0 && ADJlist[current, i + 1] == PathQueue.Peek())
                                    return ((float)Math.PI * ((i - 1) / 3f));
                            }
                            throw new Exception("Cannot find direction to next path");
                    }
                    throw new Exception("Control cannot fall out of difficulty switch");

                    
            }
            throw new Exception("No direction returned");
             
        }
        /// <summary>
        /// Finds the shortest path between 0,0 and the target postion. The Path found is returned as a List of Cell Positions
        /// </summary>
        /// <param name="ADJlist"></param>
        /// <param name="targetPos"></param>
        /// <returns></returns>
        public Queue<int> ShortestPath(int[,] ADJlist,int startPos, int targetPos)
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
            bool found= false;


            for (int i = 1; i < (ScreenWidth / cellWidth) * Scale + 2; i++)  
            {
                discovered.Add(false);
                parent.Add(-1);
            }
            Q.Enqueue(startPos);
            discovered[startPos] = true; //StartPos is source

            while (Q.Count > 0 && found==false)
            {
                v = Q.Dequeue();
                for (int u = 1; u < maxNeighbours+1; u++) 
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
        /// <summary>
        /// Finds the an imperfect path between 0,0 and the target postion. The path takes detours and follows current routes before hitting dead ends and returning to the main path. The full Path found is returned as a List of Cell Positions
        /// </summary>
        /// <param name="ADJlist"></param>
        /// <param name="targetPos"></param>
        /// <returns></returns>
        public Queue<int> HoldLeft(int[,] ADJlist, int startPos, int targetPos)
        {
            int maxNeighbours = 0;
            if (GridType == "Quad")
                maxNeighbours = 4;
            else if (GridType == "Hex")
                maxNeighbours = 6;

            Queue<int> Path = new Queue<int>();
            Stack<int> Stack = new Stack<int>();
            List<bool> discovered = new List<bool>();
            List<int> parent = new List<int>();
            bool found = false,moved;
            int v;
            for (int i = 1; i < (ScreenWidth / cellWidth) * Scale + 2; i++) //Change to use ADJList
            {
                discovered.Add(false);
                parent.Add(-1);
            }
            Stack.Push(startPos);
            discovered[startPos] = true;
            
            do
            {
                v = Stack.Pop();
                Path.Enqueue(v);
                moved = false;
                for (int u = 1; u < maxNeighbours+1; u++)
                {
                    if (ADJlist[v, u] != 0 && discovered[ADJlist[v, u]] == false && found == false)
                    {
                        Stack.Push(ADJlist[v, u]);
                        discovered[ADJlist[v, u]] = true;
                        parent[ADJlist[v, u]] = v;
                        moved = true;
                        //Path.Enqueue(ADJlist[v, u]);
                        if (ADJlist[v, u] == targetPos)
                            found = true;
                    }
          
                }
                if (!moved && !found)
                {
                    int prev = v;
                    do
                    {
                        prev = parent[prev];
                        Path.Enqueue(prev);
                    } while (prev != parent[Stack.Peek()]);
                }
            } while (Stack.Count > 0);

            return Path;
        }
        public void Draw(SpriteBatch spriteBatch)
        { 
            spriteBatch.Draw(texture, Position, null, sourcRect, Origin, _rotation, new Vector2(cellWidth / 200 *(0.8f+0.2f*((GridType == "Quad") ? 1 : 0))), Tint, SpriteEffects.None, 0);
        }

        public void Reset(int[,] newADJList, Cell goal, float newCellWidth, int screenWidth)
        {
            GridType = "Quad";
            cellWidth = newCellWidth;
            Scale = (int)(screenWidth / newCellWidth);
            Position = new Vector2(cellWidth / 2, cellWidth / 2);
            boundary = new RotatedCollisionRectangle(new Rectangle(0, 0, (int)(texture.Width / 3 * (cellWidth / 250)), (int)(texture.Height * (cellWidth / 300))), 0f);
            LinearVelocity = 20f / (Scale / 2);
            _rotation = 0;
            CurrentADJPos = 1;
            QuadGoal = goal;
            ADJlist = newADJList;

            speedBoostBool = false;
            guidenceBool = false;
            boostElapsed = 0f;
            guidenceElapsed = 0f;

            if (Difficulty == 3) //Difficulty 3 is the shortest path
            {
                PathQueue = ShortestPath(ADJlist, CurrentADJPos, (int)goal.GridPos.X + 1 + (int)(Scale * goal.GridPos.Y));
            }
            else if (Difficulty == 2)
            {
                PathQueue = HoldLeft(ADJlist, CurrentADJPos, (int)goal.GridPos.X + 1 + (int)(Scale * goal.GridPos.Y));
            }

            nextDirection = NextPosition();
            NextADJPos = ADJlist[CurrentADJPos, (int)(Math.Round((nextDirection / Math.PI) * 2) + 2)];
            Direction = new Vector2((float)Math.Cos(nextDirection), (float)Math.Sin(nextDirection));

        }
        public void Reset(int[,] newADJList, Hex_Cell goal, float newCellWidth, int screenWidth)
        {

            GridType = "Hex";
            cellWidth = newCellWidth;
            Scale = (int)(screenWidth / newCellWidth);
            Position = new Vector2(cellWidth / 2, (int)(cellWidth / Math.Sqrt(3)));
            boundary = new RotatedCollisionRectangle(new Rectangle(0, 0, (int)((texture.Width / 3 * (cellWidth / 250) * 0.8f)), (int)((texture.Height * (cellWidth / 300)) * 0.8f)), 0f);
            LinearVelocity = 20f / (Scale / 2);
            _rotation = 0;
            CurrentADJPos = 1;
            HexGoal = goal;
            ADJlist = newADJList;
            yOffset = (int)(cellWidth / Math.Sqrt(3));

            speedBoostBool = false;
            guidenceBool = false;
            boostElapsed = 0f;
            guidenceElapsed = 0f;

            if (Difficulty == 3) //Difficulty 3 is the shortest path
            {
                PathQueue = ShortestPath(ADJlist,CurrentADJPos, (int)goal.GridPos.X + 1 + (int)(Scale * goal.GridPos.Y));
            }
            else if (Difficulty == 2)
            {
                PathQueue = HoldLeft(ADJlist,CurrentADJPos, (int)goal.GridPos.X + 1 + (int)(Scale * goal.GridPos.Y));
            }

            nextDirection = NextPosition();
            NextADJPos = ADJlist[CurrentADJPos, (int)(Math.Round((nextDirection / Math.PI) * 2) + 2)];
            Direction = new Vector2((float)Math.Cos(_rotation), (float)Math.Sin(_rotation));

        }
    }
}
