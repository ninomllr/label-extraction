using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace ExtractionLibrary.Filters
{
    /// <summary>
    /// Sobel algorithm to find borders
    /// </summary>
    public class SobelFilter : ImageFilter
    {
        /// <summary>
        /// Sobel value which indicates if pixel should be white or black
        /// </summary>
        private double sobelValue = 30;

        /// <summary>
        /// Sets the sobel value
        /// </summary>
        public double SobelValue
        {
            set { sobelValue = value; }
        }

        /// <summary>
        /// Creates gradient image with sobel
        /// </summary>
        /// <param name="sourceP">Source image pointer</param>
        /// <param name="processP">Sobel image pointer</param>
        /// <param name="sourceImage">Size of the image</param>
        public override unsafe void Process(uint* sourceP, uint* processP, Size sourceImage)
        {
            // sobel matrix
            int[,] sx = new int[,] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
            int[,] sy = new int[,] { { 1, 2, 1 }, { 0, 0, 0 }, { -1, -2, -1 } };

            for (int y = 1; y < sourceImage.Height - 1; y++) // Loop image
            {
                for (int x = 1; x < sourceImage.Width - 1; x++)
                {
                    float gx = 0;
                    float gy = 0;

                    for (int matrixH = -1; matrixH < 2; matrixH++) // Loop sobel matrix
                    {
                        for (int matrixW = -1; matrixW < 2; matrixW++)
                        {
                            // Extract RGB values
                            uint r = (sourceP[(x + matrixW) + (matrixH + y) * sourceImage.Width] & 0xFF0000) >> 16;
                            uint g = (sourceP[(x + matrixW) + (matrixH + y) * sourceImage.Width] & 0xFF00) >> 8;
                            uint b = (sourceP[(x + matrixW) + (matrixH + y) * sourceImage.Width] & 0xFF);

                            // Generate RGB value
                            float rgb = (r + g + b) / 3;

                            // Sobel
                            gx += sx[matrixH + 1, matrixW + 1] * rgb;
                            gy += sy[matrixH + 1, matrixW + 1] * rgb;
                        }
                    }

                    // Black or white, depending on contrast
                    if (Math.Sqrt(gx * gx + gy * gy) > sobelValue)
                        processP[x + y * sourceImage.Width] = 0xFFFFFFFF;
                    else
                        processP[x + y * sourceImage.Width] = 0xFF000000;
                }
            }
        }
    }
}
