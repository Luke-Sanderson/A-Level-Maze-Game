using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Maze_Project.Sprites
{
    public class Cells
    {
        public Vector2 GridPos;
        public int ADJlistPos;
        public int[,] ADJlist;
        public bool visited = false;

    }
}
