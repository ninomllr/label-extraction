using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;

namespace ExtractionLibrary.Detectors
{
    /// <summary>
    /// Detect white points and start shape detection
    /// </summary>
    public class LabelDetection : ImageDetection
    {
        /// <summary>
        /// Queue for the flood fill algorithm
        /// </summary>
        private Queue floodFillQueue = new Queue();

        /// <summary>
        /// Detect all positions of shapes
        /// </summary>
        /// <param name="sourceP">Source pointer without filters</param>
        /// <param name="processP">Process pointer with filters</param>
        /// <param name="sourceImage">Size of images</param>
        /// <returns>List of found labels as rectangles</returns>
        public unsafe List<Rectangle> Process(uint* sourceP, uint* processP, Size sourceImage)
        {
            List<Rectangle> results = new List<Rectangle>();
            ShapeDetection shape = new ShapeDetection();
            shape.Ratio = legalRatio;

            // search for white pixel
            for (int y = 1; y < sourceImage.Height - 1; y++)
            {
                for (int x = 1; x < sourceImage.Width - 1; x++)
                {
                    if ((uint)processP[y * sourceImage.Width + x] == 0xFFFFFFFF)
                    {
                        List<Rectangle> result = shape.Process(x, y, sourceP, processP, new Size(sourceImage.Width, sourceImage.Height));

                        // save result if its valid
                        if (result.Count == 1)
                            results.Add((Rectangle)result[0]);

                        // start to fill whole shape black, we don't want to find it again
                        QueueFloodFill(processP, x, y, sourceImage);

                        while (floodFillQueue.Count > 0)
                        {
                            Point coordinates = (Point)floodFillQueue.Dequeue();
                            QueueFloodFill(processP, coordinates.X, coordinates.Y, sourceImage);
                        }
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Flood fill a white area to black
        /// </summary>
        /// <param name="pointer">Image pointer</param>
        /// <param name="x">X-position of start</param>
        /// <param name="y">Y-position of start</param>
        /// <param name="imageWidth">Width of source image</param>
        /// <param name="imageHeight">Height of source image</param>
        private unsafe void QueueFloodFill(uint* pointer, int x, int y, Size size)
        {
            // dont leave bitmap image
            if ((x < 0) || (x >= size.Width))
                return;
            if ((y < 0) || (y >= size.Height))
                return;

            if (pointer[y * size.Width + x] == 0xFFFFFFFF)
            {
                // paint it black
                pointer[y * size.Width + x] = 0xFF000000;

                // add all directions to queue
                floodFillQueue.Enqueue(new Point(x + 1, y));
                floodFillQueue.Enqueue(new Point(x, y + 1));
                floodFillQueue.Enqueue(new Point(x - 1, y));
                floodFillQueue.Enqueue(new Point(x, y - 1));
            }
        }
    }
}
