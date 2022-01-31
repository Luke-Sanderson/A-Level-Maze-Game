using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Maze_Project.Sprites
{
    public class Cell : Cells
    {

        public RotatedCollisionRectangle topRect;
        public RotatedCollisionRectangle rightRect;
        public RotatedCollisionRectangle botRect;
        public RotatedCollisionRectangle leftRect;

        public bool Occupied = false;                       //Shows if Powerup occupies cell 
        public bool[] walls = { true, true, true, true };

        public Cell(int x, int y, int width,Texture2D _texture, int ScreenHeight, Random rnd)
        {
            int height = width;

            GridPos = new Vector2(x, y);
            topRect   = new RotatedCollisionRectangle( new Rectangle(x * width         , y * height            , width  , 2     ), 0);                   //These are rectangles for each of the walls used when drawing the walls
            botRect   = new RotatedCollisionRectangle( new Rectangle(x * width         , y * height + height   , width  , 2     ), 0);
            leftRect  = new RotatedCollisionRectangle( new Rectangle(x * width         , y * height            , 2      , height), 0);
            rightRect = new RotatedCollisionRectangle( new Rectangle(x * width + width , y * height            , 2      , height), 0);



            ADJlist = new int[(ScreenHeight / width) * (ScreenHeight / width) + 1, 5];//Adjacency List For tree This will be used in the AI to traverse the Maze. Columns^2 is the maximum length one path could be (One way to optimise is to add as a List rather than Array
            if (rnd.Next(0, 10) == 1 /*&& Occupied == false*/)
            {
                Occupied = true;
            }
        }                                                         //The array is used as Current, Up, Left, Down, Right
        
        public Cell GenerateMaze(List<Cell> grid,int Columns,int Rows, Random rnd)  
        {
            var current = grid[0];
            var stack = new Stack<Cell>();
                                                                         
            Cell goal = current;
            ADJlist = new int[Columns * Rows + 1, 5];
            int ADJListPrevious;
            int deepest = 0;
            int ADJListCount = 1;                                        //Starts at 1 as the 0 is reserved to indicate no move is possible in that direction
            int ADJListCurrent = 1;

            do
            {
                current.visited = true;
                ADJListPrevious = ADJListCurrent;
                ADJListCurrent = current.ADJlistPos;

                ADJlist[ADJListCurrent, 0] = ADJListCurrent;

                var neighbours = CheckNeighbours(current, grid, Columns, Rows, rnd, ADJListCount);    //Looks for unvisted neighbours of current cell
                if (neighbours != null)
                {
                    int r;
                    Cell next;
                    do
                    {
                        r = rnd.Next(0, 4);
                        next = neighbours[r];                                                  //Randomly picks valid neighbour
                    } while (next == null);     


                    ADJListPrevious = ADJListCurrent;                                         //Assigns Previous before changing current
                    ADJListCurrent = next.ADJlistPos;                                         //Changes current to next cell

                    ADJlist[ADJListPrevious, r + 1] = ADJListCurrent;                         //Adds Next cell to previous cell's Adjacency List
                    ADJlist[ADJListCurrent, ((r + 2) % 4) + 1] = ADJListPrevious;             //Adds Previous cell to current cells Adjacency List
                    
  

                    stack.Push(current);                                                       //Adds current cell to stack 

                    RemoveWalls(current, next);                                                //Removes the walls connecting the Current and Next Cell

                    current = next;                                                            //Moves current to the next cell
                }
                else if (stack.Count > 0)                                                
                {
                    if (stack.Count > deepest)                                                  //If current cell is the deepest, it is set to deepest
                    {
                        deepest = stack.Count;
                        goal = current;
                    }
                    current = stack.Pop();                                                      //Moves current back a cell
                }

                
            } while (stack.Count > 0);
            return goal;
        }
        public Cell[] CheckNeighbours(Cell current, List<Cell> grid, int Columns, int Rows, Random rnd,  int ADJListCount) //Checks for Neighbours around current gridposition
        {
            Cell[] neighbours = new Cell[4];
            bool neighbourExists = false;
                                                                                           //Checks if the position exists
            if (Index((int)current.GridPos.X, (int)current.GridPos.Y - 1, Columns, Rows) != -1)   //Top
            {
                Cell top = grid[Index((int)current.GridPos.X, (int)current.GridPos.Y - 1, Columns, Rows)]; //position above current
                if (!top.visited)
                {                                                           //checks if it has been visted
                    neighbours[0] = top;                                                   //Adds to list that are availiable
                    neighbourExists = true;
                }
            }
            if (Index((int)current.GridPos.X + 1, (int)current.GridPos.Y, Columns, Rows) != -1)   //Right
            {
                Cell right = grid[Index((int)current.GridPos.X + 1, (int)current.GridPos.Y, Columns, Rows)];
                if (!right.visited)
                {
                    neighbours[1] = right;
                    neighbourExists = true;
                }
            }
            if (Index((int)current.GridPos.X, (int)current.GridPos.Y + 1, Columns, Rows) != -1)   //Bottom
            {
                Cell bot = grid[Index((int)current.GridPos.X, (int)current.GridPos.Y + 1, Columns, Rows)];
                if (!bot.visited)
                {
                    neighbours[2] = bot;
                    neighbourExists = true;
                }
            }
            if (Index((int)current.GridPos.X - 1, (int)current.GridPos.Y, Columns, Rows) != -1)   //Left
            {
                Cell left = grid[Index((int)current.GridPos.X - 1, (int)current.GridPos.Y, Columns, Rows)];
                if (!left.visited)
                {
                    neighbours[3] = left;
                    neighbourExists = true;
                }
            }
            if (neighbourExists)
            {
                return neighbours;
            }
            return null;
        }
        public static void RemoveWalls(Cell a, Cell b)
        {
            int xDiff = (int)a.GridPos.X - (int)b.GridPos.X;                    //Finds difference in x grid positions between the two cells
            if (xDiff == 1)                                                     //xDiff of 1 means a is to the right of b
            {                                                                   //xDiff of -1 means a is to the left of b
                a.walls[3] = false;
                b.walls[1] = false;
            }
            else if (xDiff == -1)
            {
                a.walls[1] = false;
                b.walls[3] = false;
            }

            int yDiff = (int)a.GridPos.Y - (int)b.GridPos.Y;                    //Finds difference in y grid positions between the two cells
            if (yDiff == 1)                                                     //yDiff of 1 means a is on the botton of b
            {                                                                   //yDiff of -1 means a is on top of b
                a.walls[0] = false;
                b.walls[2] = false;
            }
            else if (yDiff == -1)
            {
                a.walls[2] = false;
                b.walls[0] = false;
            }
        }
        public static int Index(int x, int y, int w, int h)                     
        {
            if (x < 0 || y < 0 || x > w - 1 || y > h - 1)                           //w-1 being 1 less than the number of Columns
                return -1;

            return x + y * w;                                                       //w Being the number of Columns and h being number of rows
        }
        public void ResetGrid(List<Cell> grid, int Columns, int Rows)                       //Resets grid so all cells aren't visied, have full walls and a blank adjacency list
        {
            foreach (Cell cell in grid)
            {
                cell.visited = false;                
                cell.walls = new bool[4] { true, true, true, true };
                ADJlist = new int[Columns * Rows + 1, 5];

            }
        }
    }
}
