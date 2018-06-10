using System;
using UnityEngine;
using System.Collections.Generic;

/**
The MIT License (MIT)
Copyright (c) 2014 Lauri Hosio
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
 */

namespace ProtoTurtle.BitmapDrawing
{
    /// <summary>
    /// A extension class for UnityEngine.Texture2D that provides a bitmap drawing API.
    /// Provides drawing methods line Line, Circle, Rectangle etc.
    /// This class uses a convention of having the 0,0 point at the left-top corner!</summary>
    /// <remarks>
    /// Check this out for more cool middleware stuff:
    /// http://prototurtle.com 
    /// 
    /// </remarks>
    public static class BitmapDrawingExtensions
    {
        public static void Clear(this Texture2D texture)
        {
            Rect rect = new Rect(0, 0, texture.width, texture.height);
            texture.DrawFilledRectangle(rect, Color.clear);
        }


        /// <summary>
        /// Draws a pixel just like SetPixel except 0,0 is the left top corner.</summary>
        public static void DrawPixel(this Texture2D texture, int x, int y, Color color)
        {
            if (x < 0 || x > texture.width || y < 0 || y > texture.height)
            {
                return;
            }
            texture.SetPixel(x, TransformToLeftTop_y(y, texture.height), color);
        }

        /// <summary>
        /// Draws a pixel just like SetPixel except 0,0 is the left top corner.
        /// Takes the width and height as parameters - faster for calling this in a loop.
        /// </summary>
        /// <param name="width">Width of the target bitmap</param>
        /// <param name="height">Height of the target bitmap</param>
        public static void DrawPixel(this Texture2D texture, int x, int y, int width, int height, Color color)
        {
            if (x < 0 || x > width || y < 0 || y > height)
            {
                return;
            }
            texture.SetPixel(x, TransformToLeftTop_y(y, height), color);
        }

        /// <summary>
        /// Draws a circle with the midpoint being x0, x1.
        /// Implementation of Bresenham's circle algorithm
        /// </summary>
        public static void DrawCircle(this Texture2D texture, int x, int y, int radius, Color color)
        {
            Circle(texture, x, y, radius, color, false);
        }

        /// <summary>
        /// Draws a filled circle with the midpoint being x0, x1.
        /// Implementation of Bresenham's circle algorithm
        /// </summary>
        public static void DrawFilledCircle(this Texture2D texture, int x, int y, int radius, Color color)
        {
            Circle(texture, x, y, radius, color, true);
        }


        private static void Circle(Texture2D texture, int x, int y, int radius, Color color, bool filled = false)
        {
            int cx = radius;
            int cy = 0;
            int radiusError = 1 - cx;

            while (cx >= cy)
            {
                if (!filled)
                {
                    PlotCircle(texture, cx, x, cy, y, color);
                }
                else
                {
                    ScanLineCircle(texture, cx, x, cy, y, color);
                }

                cy++;

                if (radiusError < 0)
                {
                    radiusError += 2 * cy + 1;
                }
                else
                {
                    cx--;
                    radiusError += 2 * (cy - cx + 1);
                }
            }
        }

        private static void PlotCircle(Texture2D texture, int cx, int x, int cy, int y, Color color)
        {
            texture.DrawPixel(cx + x, cy + y, color); // Point in octant 1...
            texture.DrawPixel(cy + x, cx + y, color);
            texture.DrawPixel(-cx + x, cy + y, color);
            texture.DrawPixel(-cy + x, cx + y, color);
            texture.DrawPixel(-cx + x, -cy + y, color);
            texture.DrawPixel(-cy + x, -cx + y, color);
            texture.DrawPixel(cx + x, -cy + y, color);
            texture.DrawPixel(cy + x, -cx + y, color); // ... point in octant 8
        }

        // Draw scanlines from opposite sides of the circle on y-scanlines instead of just plotting pixels
        // at the right coordinates
        private static void ScanLineCircle(Texture2D texture, int cx, int x, int cy, int y, Color color)
        {
            //texture.DrawPixel(cx + x,  cy + y, color);
            //texture.DrawPixel(-cx + x, cy + y, color);
            texture.DrawLine(cx + x, cy + y, -cx + x, cy + y, color);

            //texture.DrawPixel(cy + x,  cx + y, color);
            //texture.DrawPixel(-cy + x, cx + y, color);
            texture.DrawLine(cy + x, cx + y, -cy + x, cx + y, color);

            //texture.DrawPixel(-cx + x, -cy + y, color);
            //texture.DrawPixel(cx + x,  -cy + y, color);
            texture.DrawLine(-cx + x, -cy + y, cx + x, -cy + y, color);

            //texture.DrawPixel(-cy + x, -cx + y, color);
            //texture.DrawPixel(cy + x,  -cx + y, color);
            texture.DrawLine(-cy + x, -cx + y, cy + x, -cx + y, color);
        }


        /// <summary>
        /// Starts a flood fill at point startX, startY.
		/// This is a pretty slow flood fill, biggest bottle neck is comparing two colors which happens
		/// a lot. Should be a way to make it much faster.
        /// O(n) space.  n = width*height - makes a copy of the bitmap temporarily in the memory
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="startX"></param>
        /// <param name="startY"></param>
        /// <param name="newColor"></param>
        public static void FloodFill(this Texture2D texture, int startX, int startY, Color newColor)
        {
            Point start = new Point(startX, TransformToLeftTop_y(startY, texture.height));

            Flat2DArray copyBmp = new Flat2DArray(texture.height, texture.width, texture.GetPixels());

            Color originalColor = texture.GetPixel(start.X, start.Y);
            int width = texture.width;
            int height = texture.height;


            if (originalColor == newColor)
            {
                return;
            }

            copyBmp[start.X, start.Y] = newColor;

            Queue<Point> openNodes = new Queue<Point>();
            openNodes.Enqueue(start);

            int i = 0;

            // TODO: remove this
            // emergency switch so it doesn't hang if something goes wrong
            int emergency = width * height;

            while (openNodes.Count > 0)
            {
                i++;

                if (i > emergency)
                {
                    return;
                }

                Point current = openNodes.Dequeue();
                int x = current.X;
                int y = current.Y;

                if (x > 0)
                {
                    if (copyBmp[x - 1, y] == originalColor)
                    {
                        copyBmp[x - 1, y] = newColor;
                        openNodes.Enqueue(new Point(x - 1, y));
                    }
                }
                if (x < width - 1)
                {
                    if (copyBmp[x + 1, y] == originalColor)
                    {
                        copyBmp[x + 1, y] = newColor;
                        openNodes.Enqueue(new Point(x + 1, y));
                    }
                }
                if (y > 0)
                {
                    if (copyBmp[x, y - 1] == originalColor)
                    {
                        copyBmp[x, y - 1] = newColor;
                        openNodes.Enqueue(new Point(x, y - 1));
                    }
                }
                if (y < height - 1)
                {
                    if (copyBmp[x, y + 1] == originalColor)
                    {
                        copyBmp[x, y + 1] = newColor;
                        openNodes.Enqueue(new Point(x, y + 1));
                    }
                }
            }

            texture.SetPixels(copyBmp.data);
        }

        // Could be its own file
        private class Flat2DArray
        {
            public Color[] data;
            private readonly int height;
            private readonly int width;

            public Flat2DArray(int height, int width, Color[] data)
            {
                this.height = height;
                this.width = width;

                this.data = data;
            }

            public Color this[int x, int y]
            {
                get
                {
                    return data[x + y * width];
                }
                set
                {
                    data[x + y * width] = value;
                }
            }
        }

        private struct Point
        {
            public int X;
            public int Y;

            public Point(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
        }

        /// <summary>
        /// Draws a rectangle
        /// </summary>
        public static void DrawRectangle(this Texture2D texture, Rect rectangle, Color color)
        {
            int x = (int)rectangle.x;
            int y = (int)rectangle.y;
            int height = (int)rectangle.height;
            int width = (int)rectangle.width;


            // top left to bottom left
            texture.DrawLine(x, y, x, y + height, color);

            // bottom left to bottom right
            texture.DrawLine(x, y + height, x + width, y + height, color);

            // bottom right to top right
            texture.DrawLine(x + width, y + height, x + width, y, color);

            // top right to top left
            texture.DrawLine(x + width, y, x, y, color);

        }

        /// <summary>
        /// Fills the given rectangle area with a solid color.</summary>
        public static void DrawFilledRectangle(this Texture2D texture, Rect rectangle, Color color)
        {
            Color[] colorsArray = new Color[(int)rectangle.width * (int)rectangle.height];
            for (int i = 0; i < colorsArray.Length; i++)
            {
                colorsArray[i] = color;
            }

            int transformedY = TransformToLeftTop_y(rectangle.y, texture.height) - (int)rectangle.height;
            texture.SetPixels((int)rectangle.x, transformedY,
                (int)rectangle.width, (int)rectangle.height, colorsArray);
        }



        public static void DrawLine(this Texture2D texture, Vector3 start, Vector3 end, Color color)
        {
            Line(texture, (int)start.x, (int)start.y, (int)end.x, (int)end.y, color);
        }

        public static void DrawLine(this Texture2D texture, Vector2 start, Vector2 end, Color color)
        {
            Line(texture, (int)start.x, (int)start.y, (int)end.x, (int)end.y, color);
        }

        /// <summary>
        /// Draws a line between two points. Implementation of Bresenham's line algorithm.
        /// </summary>
        /// <param name="x0">x of the start point</param>
        /// <param name="y0">y of the start point</param>
        /// <param name="x1">x of the end point</param>
        /// <param name="y1">y of the end point</param>
        public static void DrawLine(this Texture2D texture, int x0, int y0, int x1, int y1, Color color)
        {
            Line(texture, x0, y0, x1, y1, color);
        }

        private static void Line(Texture2D texture, int x0, int y0, int x1, int y1, Color color)
        {
            int width = texture.width;
            int height = texture.height;

            bool isSteep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
            if (isSteep)
            {
                Swap(ref x0, ref y0);
                Swap(ref x1, ref y1);
            }
            if (x0 > x1)
            {
                Swap(ref x0, ref x1);
                Swap(ref y0, ref y1);
            }

            int deltax = x1 - x0;
            int deltay = Math.Abs(y1 - y0);

            int error = deltax / 2;
            int ystep;
            int y = y0;

            if (y0 < y1)
                ystep = 1;
            else
                ystep = -1;

            for (int x = x0; x < x1; x++)
            {
                if (isSteep)
                    texture.DrawPixel(y, x, width, height, color);
                else
                    texture.DrawPixel(x, y, width, height, color);

                error = error - deltay;
                if (error < 0)
                {
                    y = y + ystep;
                    error = error + deltax;
                }
            }
        }


        /// <summary>
        /// Swap two ints by reference.
        /// </summary>
        private static void Swap(ref int x, ref int y)
        {
            int temp = x;
            x = y;
            y = temp;
        }

        /// <summary>
        /// Transforms a point in the texture plane so that 0,0 points at left-top corner.</summary>
        private static int TransformToLeftTop_y(int y, int height)
        {
            return height - y;
        }

        /// <summary>
        /// Transforms a point in the texture plane so that 0,0 points at left-top corner.</summary>
        private static int TransformToLeftTop_y(float y, int height)
        {
            return height - (int)y;
        }


        
    }


}