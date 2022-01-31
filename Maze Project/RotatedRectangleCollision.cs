using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Maze_Project
{
    public class RotatedCollisionRectangle
    {
        public Rectangle CollisionRectangle;
        public float Rotation;
        public Vector2 Origin;

        public RotatedCollisionRectangle(Rectangle rect, float rotation)
        {
            CollisionRectangle = rect;
            Rotation = rotation;

            //Calculate the Rectangles origin. We assume the center of the Rectangle will
            //be the point that we will be rotating around and we use that for the origin
            Origin = new Vector2((int)rect.Width / 2, (int)rect.Height / 2);
        }

        /// <summary>
        /// Used for changing the X and Y position of the RotatedRectangle
        /// </summary>
        /// <param name="newXPos"></param>
        /// <param name="newYPos"></param>
        public void ChangePosition(int newXPos, int newYPos)
        {
            CollisionRectangle.X = newXPos;
            CollisionRectangle.Y = newYPos;
        }

        /// <summary>
        /// This intersects method can be used to check a standard XNA framework Rectangle
        /// object and see if it collides with a Rotated Rectangle object
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public bool Intersects(Rectangle rect)
        {
            return Intersects(new RotatedCollisionRectangle(rect, 0.0f));
        }

        /// <summary>
        /// Check to see if two Rotated Rectangls have collided
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public bool Intersects(RotatedCollisionRectangle rect)
        {
            //Calculate the Axis we will use to determine if a collision has occurred
            //Since the objects are rectangles, we only have to generate 4 Axis (2 for
            //each rectangle) since we know the other 2 on a rectangle are parallel.
            List<Vector2> rectAxises = new List<Vector2>();
            rectAxises.Add(UpperRightCorner() - UpperLeftCorner());
            rectAxises.Add(UpperRightCorner() - LowerRightCorner());
            rectAxises.Add(rect.UpperLeftCorner() - rect.LowerLeftCorner());
            rectAxises.Add(rect.UpperLeftCorner() - rect.UpperRightCorner());
            
            //Cycle through all of the Axis we need to check. If a collision does not occur
            //on ALL of the Axis, then a collision is NOT occurring. We can then exit out 
            //immediately and notify the calling function that no collision was detected. If
            //a collision DOES occur on ALL of the Axis, then there is a collision occurring
            //between the rotated rectangles. We know this to be true by the Seperating Axis Theorem
            foreach (Vector2 axis in rectAxises)
            {
                if (!IsAxisCollision(rect, axis))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Determines if a collision has occurred on an Axis of one of the
        /// planes parallel to the Rectangle
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="axis"></param>
        /// <returns></returns>
        private bool IsAxisCollision(RotatedCollisionRectangle rect, Vector2 axis)
        {
            //Project the corners of the Rectangle we are checking on to the Axis and
            //get a scalar value of that project we can then use for comparison
            List<int> rectAScalars = new List<int>();
            rectAScalars.Add(GenerateScalar(rect.UpperLeftCorner(), axis));
            rectAScalars.Add(GenerateScalar(rect.UpperRightCorner(), axis));
            rectAScalars.Add(GenerateScalar(rect.LowerLeftCorner(), axis));
            rectAScalars.Add(GenerateScalar(rect.LowerRightCorner(), axis));

            //Project the corners of the current Rectangle on to the Axis and
            //get a scalar value of that project we can then use for comparison
            List<int> rectBScalars = new List<int>();
            rectBScalars.Add(GenerateScalar(UpperLeftCorner(), axis));
            rectBScalars.Add(GenerateScalar(UpperRightCorner(), axis));
            rectBScalars.Add(GenerateScalar(LowerLeftCorner(), axis));
            rectBScalars.Add(GenerateScalar(LowerRightCorner(), axis));

            //Get the Maximum and Minium Scalar values for each of the Rectangles
            int rectAMin = rectAScalars.Min();
            int rectAMax = rectAScalars.Max();
            int rectBMin = rectBScalars.Min();
            int rectBMax = rectBScalars.Max();

            //If we have overlaps between the Rectangles (i.e. Min of B is less than Max of A)
            //then we are detecting a collision between the rectangles on this Axis
            if (rectBMin <= rectAMax && rectBMax >= rectAMax)
            {
                return true;
            }
            else if (rectAMin <= rectBMax && rectAMax >= rectBMax)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Generates a scalar value that can be used to compare where corners of 
        /// a rectangle have been projected onto a particular axis. 
        /// </summary>
        /// <param name="corner"></param>
        /// <param name="axis"></param>
        /// <returns></returns>
        private int GenerateScalar(Vector2 corner, Vector2 axis)
        {
            //Using the formula for Vector projection. Take the corner being passed in
            //and project it onto the given Axis
            float aNumerator = (corner.X * axis.X) + (corner.Y * axis.Y);
            float aDenominator = (axis.X * axis.X) + (axis.Y * axis.Y);
            float aDivisionResult = aNumerator / aDenominator;
            Vector2 projectedCorner = new Vector2(aDivisionResult * axis.X, aDivisionResult * axis.Y);

            //Now that we have our projected Vector, calculate a scalar of that projection
            //that can be used to more easily do comparisons
            float aScalar = (axis.X * projectedCorner.X) + (axis.Y * projectedCorner.Y);
            return (int)aScalar;
        }

        /// <summary>
        /// Rotate a point from a given location and adjust using the Origin we
        /// are rotating around
        /// </summary>
        /// <param name="point"></param>
        /// <param name="origin"></param>
        /// <param name="rotation"></param>
        /// <returns></returns>
        private Vector2 RotatePoint(Vector2 point, Vector2 origin, float rotation)
        {
            Vector2 newPoint = new Vector2();
            newPoint.X = (float)(origin.X + (point.X - origin.X) * Math.Cos(rotation)
                - (point.Y - origin.Y) * Math.Sin(rotation));
            newPoint.Y = (float)(origin.Y + (point.Y - origin.Y) * Math.Cos(rotation)
                + (point.X - origin.X) * Math.Sin(rotation));
            return newPoint;
        }

        public Vector2 UpperLeftCorner()
        {
            Vector2 upperLeft = new Vector2(CollisionRectangle.Left, CollisionRectangle.Top);
            upperLeft = RotatePoint(upperLeft, upperLeft + Origin, Rotation);
            return upperLeft;
        }

        public Vector2 UpperRightCorner()
        {
            Vector2 upperRight = new Vector2(CollisionRectangle.Right, CollisionRectangle.Top);
            upperRight = RotatePoint(upperRight, upperRight + new Vector2(-Origin.X, Origin.Y), Rotation);
            return upperRight;
        }

        public Vector2 LowerLeftCorner()
        {
            Vector2 lowerLeft = new Vector2(CollisionRectangle.Left, CollisionRectangle.Bottom);
            lowerLeft = RotatePoint(lowerLeft, lowerLeft + new Vector2(Origin.X, -Origin.Y), Rotation);
            return lowerLeft;
        }

        public Vector2 LowerRightCorner()
        {
            Vector2 lowerRight = new Vector2(CollisionRectangle.Right, CollisionRectangle.Bottom);
            lowerRight = RotatePoint(lowerRight, lowerRight + new Vector2(-Origin.X, -Origin.Y), Rotation);
            return lowerRight;
        }

        public int X
        {
            get { return CollisionRectangle.X; }
        }

        public int Y
        {
            get { return CollisionRectangle.Y; }
        }

        public int Width
        {
            get { return CollisionRectangle.Width; }
        }

        public int Height
        {
            get { return CollisionRectangle.Height; }
        }

    }
}
