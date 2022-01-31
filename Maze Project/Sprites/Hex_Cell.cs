using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Maze_Project.Sprites
{
    public class Hex_Cell : Cells
    {
        //public Vector2 GridPos;
        //public int ADJlistPos;

        public RotatedCollisionRectangle topRightRect, rightRect, botRightRect, botLeftRect, leftRect, topLeftRect;

        //public int[,] ADJlist;    
        public bool Occupied = false;                       //Shows if Powerup occupies cell 
        public bool[] walls = { true, true, true, true, true, true };

    public Hex_Cell(int x, int y, int cellWidth,Texture2D _texture, int Screenheight, Random rnd)
        {
            int wallLength = (int)Math.Round((cellWidth / 2) / (Math.Sin(Math.PI / 3)));
            int cellHeight = wallLength * 2;

            GridPos = new Vector2(x, y);
            //Equations for X coordinate: (y % 2) * (cellWidth / 2) Is the offset for every even row, + x * (cellWidth - 2) is the number of columns along it is, + (cellWidth / 2) or + cellWidth is the position the rectangle is within the cell
            //Equations for Y coordinate: 
            topRightRect = new RotatedCollisionRectangle(new Rectangle((y % 2) * (cellWidth / 2) + x * (cellWidth - 0) + (cellWidth / 2)               ,y * (cellHeight - 0) - y * ((cellHeight - wallLength) / 2) + (wallLength / 6)                                                                       ,wallLength, 3), (float)(Math.PI / 6));                //These are rectangles for each of the walls used when drawing the walls
            rightRect    = new RotatedCollisionRectangle(new Rectangle((y % 2) * (cellWidth / 2) + x * (cellWidth - 0) + cellWidth - (wallLength / 2)  ,y * (cellHeight - 0) + ((cellHeight - wallLength) / 2) - y * ((cellHeight - wallLength) / 2) + (cellWidth / 2) - (2 * wallLength / 3)               ,wallLength, 3), (float)(Math.PI / 2));
            botRightRect = new RotatedCollisionRectangle(new Rectangle((y % 2) * (cellWidth / 2) + x * (cellWidth - 0) + cellWidth - (wallLength)      ,y * (cellHeight - 0) + ((cellHeight - wallLength) / 2) + wallLength - y * ((cellHeight - wallLength) / 2) + (cellWidth / 2) - (2 * wallLength / 3)  ,wallLength, 3), (float)(Math.PI * 5 / 6));
            botLeftRect  = new RotatedCollisionRectangle(new Rectangle((y % 2) * (cellWidth / 2) + x * (cellWidth - 0) + (cellWidth / 2) - wallLength  ,y * (cellHeight - 0) + cellHeight - y * ((cellHeight - wallLength) / 2) + (cellWidth / 2) - (7 * wallLength / 6)                                    ,wallLength, 3), (float)(Math.PI * 7 / 6));
            leftRect     = new RotatedCollisionRectangle(new Rectangle((y % 2) * (cellWidth / 2) + x * (cellWidth - 0) - (wallLength / 2)              ,y * (cellHeight - 0) + ((cellHeight - wallLength) / 2) + wallLength - y * ((cellHeight - wallLength) / 2) - (cellWidth / 2) + (wallLength / 3)      ,wallLength, 3), (float)(Math.PI * 3 / 2));
            topLeftRect  = new RotatedCollisionRectangle(new Rectangle((y % 2) * (cellWidth / 2) + x * (cellWidth - 0)                                 ,y * (cellHeight - 0) + ((cellHeight - wallLength) / 2) - y * ((cellHeight - wallLength) / 2) - (cellWidth / 2) + (wallLength / 2)                   ,wallLength, 3), (float)(Math.PI * 11 / 6));
            

           

            ADJlist = new int[(Screenheight / cellWidth) * (Screenheight / cellWidth) +1, 5];//Adjacency List For tree This will be used in the AI to traverse the Maze. Columns^2 is the maximum length one path could be (One way to optimise is to add as a List rather than Array

            if (rnd.Next(0, 10) == 1 && GridPos!= new Vector2(0,0))
            {
                Occupied = true;
            }
            
        }                                                       
        public Hex_Cell GenerateMaze(List<Hex_Cell> grid,int Columns, int Rows, Random rnd)  
        {
            var current = grid[0];
            var stack = new Stack<Hex_Cell>();
                                                                         
            Hex_Cell goal = current;
            ADJlist = new int[Columns * Rows + 1, 7];
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
                    Hex_Cell next;
                    do
                    {
                        r = rnd.Next(0, 6);
                        next = neighbours[r];                                                  //Randomly picks valid neighbour
                    } while (next == null);     


                    ADJListPrevious = ADJListCurrent;                                         //Assigns Previous before changing current
                    ADJListCurrent = next.ADJlistPos;                                         //Changes current to next cell

                    ADJlist[ADJListPrevious, r + 1] = ADJListCurrent;                         //Adds Next cell to previous cell's Adjacency List
                    ADJlist[ADJListCurrent, ((r + 3) % 6) + 1] = ADJListPrevious;             //Adds Previous cell to current cells Adjacency List
                     

                    stack.Push(current);                                                       //Adds current cell to stack 

                    RemoveWalls(current, next);                                                //Removes the walls connecting the Current and Next Hex_Cell

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
        public Hex_Cell[] CheckNeighbours(Hex_Cell current, List<Hex_Cell> grid, int Columns, int Rows, Random rnd,  int ADJListCount) //Checks for Neighbours around current gridposition
        {
            Hex_Cell[] neighbours = new Hex_Cell[6];
            bool neighbourExists = false;
                                                                                                                    //Checks if the position exists
            int rowOffset = ((int)current.GridPos.Y) % 2;                                                         //rowOffset is needed as every second row is shifted to the right to keep the hexagonal pattern

            if (Index((int)current.GridPos.X + rowOffset, (int)current.GridPos.Y - 1, Columns, Rows) != -1)   //Top Right
            {
                Hex_Cell topRight = grid[Index((int)current.GridPos.X + rowOffset, (int)current.GridPos.Y - 1, Columns, Rows)]; //Position above and to the right of current cell
                if (!topRight.visited)                                                                            //Checks to see if it has been visited
                {
                    neighbours[0] = topRight;                                                                     //Adds to list of available cells
                    neighbourExists = true;
                }
            }
            if (Index((int)current.GridPos.X + 1, (int)current.GridPos.Y, Columns, Rows) != -1)   //Right
            {
                Hex_Cell right = grid[Index((int)current.GridPos.X + 1, (int)current.GridPos.Y, Columns, Rows)];
                if (!right.visited)
                {
                    neighbours[1] = right;
                    neighbourExists = true;
                }
            }
            if (Index((int)current.GridPos.X + rowOffset, (int)current.GridPos.Y + 1, Columns, Rows) != -1)   //Bottom Right
            {
                Hex_Cell botRight = grid[Index((int)current.GridPos.X + rowOffset, (int)current.GridPos.Y + 1, Columns, Rows)];
                if (!botRight.visited)
                {
                    neighbours[2] = botRight;
                    neighbourExists = true;
                }
            }
            if (Index((int)current.GridPos.X - 1 + rowOffset, (int)current.GridPos.Y + 1, Columns, Rows) != -1)   //Bottom Left
            {
                Hex_Cell botLeft = grid[Index((int)current.GridPos.X - 1 + rowOffset, (int)current.GridPos.Y + 1, Columns, Rows)]; 
                if (!botLeft.visited)
                {                                                           
                    neighbours[3] = botLeft;                                                   
                    neighbourExists = true;
                }
            }
            if (Index((int)current.GridPos.X - 1, (int)current.GridPos.Y, Columns, Rows) != -1)   //Left
            {
                Hex_Cell left = grid[Index((int)current.GridPos.X - 1, (int)current.GridPos.Y, Columns, Rows)]; 
                if (!left.visited)
                {                                                           
                    neighbours[4] = left;                                                   
                    neighbourExists = true;
                }
            }
            if (Index((int)current.GridPos.X -1 + rowOffset, (int)current.GridPos.Y - 1, Columns, Rows) != -1)   //Top Left
            {
                Hex_Cell topLeft = grid[Index((int)current.GridPos.X - 1 + rowOffset, (int)current.GridPos.Y - 1, Columns, Rows)]; 
                if (!topLeft.visited)
                {                                                         
                    neighbours[5] = topLeft;                                       
                    neighbourExists = true;
                }
            }
            if (neighbourExists)
            {
                return neighbours;
            }
            return null;
        }
        public static void RemoveWalls(Hex_Cell a, Hex_Cell b)
        {
            int rowOffset = (int)(a.GridPos.Y) % 2;                             //Odd rows will have a rown offset of 1, and even rows will have a row offset of 0
            int xDiff = (int)a.GridPos.X - (int)b.GridPos.X;                    //Finds difference in x grid positions between the two cells
            int yDiff = (int)a.GridPos.Y - (int)b.GridPos.Y;                    //Finds difference in y grid positions between the two cells
            if (yDiff == 0)
            {
                if (xDiff == 1)                                                     //xDiff of 1 means a is to the right of b
                {                                                                   //xDiff of -1 means a is to the left of b
                    a.walls[4] = false;
                    b.walls[1] = false;
                }
                else if (xDiff == -1)
                {
                    a.walls[1] = false;
                    b.walls[4] = false;
                }
            }
            else
            {
                xDiff += rowOffset;                                                  //Offset is only applied when a and b are on different rows, when yDiff!=0
            }
            if (yDiff == 1)                                                          //yDiff of 1 means a is on the botton of b
            {                                                                        //yDiff of -1 means a is on top of b
                if (xDiff == 0)                                                      //a is bottom left of b
                {
                    a.walls[0] = false;
                    b.walls[3] = false;
                }
                else if (xDiff == 1)                                                 //a is bottom right of b
                {
                    a.walls[5] = false;
                    b.walls[2] = false;
                }
            }
            else if (yDiff == -1)
            {
                if (xDiff == 1)                                                      //a is top right of b
                {
                    a.walls[3] = false;
                    b.walls[0] = false;
                }
                else if (xDiff == 0)                                                 //a is top left of b
                {
                    a.walls[2] = false;
                    b.walls[5] = false;
                }
            }
        }
        public static int Index(int x, int y, int w, int h)
        {
            if (x < 0 || y < 0 || x > w - 1 || y > h - 1)                           //w-1 being 1 less than the number of Columns
                return -1;

            return x + y * w;                                                       //w Being the number of Columns and h being number of rows
        }
        /*public void ResetGrid(List<Cell> grid, float Columns, Rows)                       //Resets grid so all cells aren't visied, have full walls and a blank adjacency list
        {
            foreach (Cell cell in grid)
            {
                cell.visited = false;
                cell.walls = new bool[4] { true, true, true, true };
                ADJlist = new int[(int)Columns * (int)Columns + 1, 5];

            }
        }*/
        public void ResetGrid(List<Hex_Cell> grid, int Columns, int Rows)                       //Resets grid so all cells aren't visied, have full walls and a blank adjacency list
        {
            foreach (Hex_Cell cell in grid)//May not be needed TODO
            {
                cell.visited = false;
                cell.walls = new bool[6] { true, true, true, true, true, true };
                ADJlist = new int[Columns * Rows + 1, 7];

            }
        }
    }
}
