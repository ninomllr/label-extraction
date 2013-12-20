using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace ExtractionLibrary.Detectors
{
    /// <summary>
    /// Detect shapes and check if they are labels
    /// </summary>
    public class ShapeDetection : ImageDetection
    {
        /// <summary>
        /// Construct
        /// </summary>
        public ShapeDetection()
        {
            legalRatio = new KeyValuePair<double, double>(3, 5);
        }

        /// <summary>
        /// Follows a shape and checks if it could be a label
        /// </summary>
        /// <param name="x">Start point x</param>
        /// <param name="y">Start point y</param>
        /// <param name="sourceP">Source pointer without filters</param>
        /// <param name="processP">Source pointer with filters</param>
        /// <param name="sourceImage">Size of the image</param>
        /// <returns>Rectangle position of the label or null if it was not a valid label</returns>
        public unsafe List<Rectangle> Process(int x, int y, uint* sourceP, uint* processP, Size sourceImage)
        {
            // store start point
            int startX = x;
            int startY = y;

            int minX = startX;
            int maxX = startX;
            int minY = startY;
            int maxY = startY;

            int direction = 0;

            do
            {
                // calculate minimum and maximum for rectangle
                if (x < minX)
                    minX = x;
                if (x > maxX)
                    maxX = x;
                if (y < minY)
                    minY = y;
                if (y > maxY)
                    maxY = y;

                // don't step out of bitmap
                if (x < 1)
                    x = 1;
                if (y < 1)
                    y = 1;

                bool validDirection = true;

                // find direction
                if ((processP[(y - 1) * sourceImage.Width + x] == (uint)0xFFFFFFFF) &&  // up
                    ((processP[y * sourceImage.Width + (x - 1)] == 0xFF000000) ||
                    (processP[(y - 1) * sourceImage.Width + (x - 1)] == 0xFF000000)))
                    direction = 0;
                else if ((processP[(y + 1) * sourceImage.Width + x] == (uint)0xFFFFFFFF) && // down
                    ((processP[y * sourceImage.Width + (x + 1)] == 0xFF000000) ||
                    (processP[(y + 1) * sourceImage.Width + (x + 1)] == 0xFF000000)))
                    direction = 1;
                else if ((processP[y * sourceImage.Width + (x + 1)] == (uint)0xFFFFFFFF) && // right
                    ((processP[(y - 1) * sourceImage.Width + x] == 0xFF000000) ||
                    (processP[(y - 1) * sourceImage.Width + (x + 1)] == 0xFF000000)))
                    direction = 2;
                else if ((processP[y * sourceImage.Width + (x - 1)] == (uint)0xFFFFFFFF) && // left
                    ((processP[(y + 1) * sourceImage.Width + x] == 0xFF000000) ||
                    (processP[(y + 1) * sourceImage.Width + (x - 1)] == 0xFF000000)))
                    direction = 3;
                else
                    validDirection = false;

                // lost track
                if (!validDirection)
                    return new List<Rectangle>();

                // move to direction
                switch (direction)
                {
                    case 0: // up
                        y--;
                        break;
                    case 1: // down
                        y++;
                        break;
                    case 2: // right
                        x++;
                        break;
                    case 3: // left
                        x--;
                        break;
                    default:
                        break;
                }

                // repaint controlled pixel
                processP[y * sourceImage.Width + x] = (uint)0xFFFF00FF;
                sourceP[y * sourceImage.Width + x] = (uint)0xFFFF00FF;


            } while (!(startX == x && startY == y));

            Rectangle result = new Rectangle(minX, minY, maxX - minX, maxY - minY);

            int higherVal = result.Width;
            if (higherVal < result.Height)
                higherVal = result.Height;

            // in case the result is too small
            if (result.Width == 0 || result.Height == 0)
                return new List<Rectangle>();
            if (higherVal < 250)
                return new List<Rectangle>();

            // calculate ratio
            double ratio;
            if (result.Width < result.Height)
                ratio = result.Height / result.Width;
            else
                ratio = result.Width / result.Height;

            List<Rectangle> resultList = new List<Rectangle>();
            resultList.Add(result);

            // determine if ratio could be label
            if (legalRatio.Key <= ratio && ratio <= legalRatio.Value)
                return resultList;
            else
                return new List<Rectangle>();
        }
    }
}