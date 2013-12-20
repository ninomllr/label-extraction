using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace ExtractionLibrary.Filters
{
    /// <summary>
    /// Increase the size of the edges
    /// </summary>
    public class PixelExtractionFilter : ImageFilter
    {
        /// <summary>
        /// Increases the size of eacht white pixel
        /// </summary>
        /// <param name="sourceP">Source image pointer</param>
        /// <param name="processP">Extract image pointer</param>
        /// <param name="sourceImage">Size of the image</param>
        public override unsafe void Process(uint* sourceP, uint* processP, Size sourceImage)
        {
            int pixelSize = 3;  // Size of pixel enlargement

            // find a pixel size that fits the image
            while (sourceImage.Height % pixelSize != 0 && sourceImage.Width % pixelSize != 0)
            {
                if (pixelSize > 10)
                    return;

                pixelSize++;
            }

            for (int y = 0; y <= sourceImage.Height - pixelSize; y += pixelSize) // Loop image
            {
                for (int x = 0; x <= sourceImage.Width - pixelSize; x += pixelSize)
                {
                    bool foundWhite = false;
                    for (int yMatrix = 0; yMatrix < pixelSize; yMatrix++) // Loop enlargement area
                    {
                        for (int xMatrix = 0; xMatrix < pixelSize; xMatrix++)
                        {
                            // check if any pixel in the enlargment area is white
                            if ((processP[(y + yMatrix) * sourceImage.Width + (x + xMatrix)] & (uint)0xFF) > (uint)0xE0)
                            {
                                foundWhite = true;
                                break;
                            }
                        }
                    }

                    // draw enlargement area white or black
                    for (int yMatrixDraw = 0; yMatrixDraw < pixelSize; yMatrixDraw++)
                    {
                        for (int xMatrixDraw = 0; xMatrixDraw < pixelSize; xMatrixDraw++)
                            processP[(y + yMatrixDraw) * sourceImage.Width + (x + xMatrixDraw)] = (foundWhite ? (uint)0xFFFFFFFF : (uint)0xFF000000);
                    }
                }
            }
        }
    }
}
