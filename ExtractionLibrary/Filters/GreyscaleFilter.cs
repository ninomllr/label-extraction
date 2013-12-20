using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace ExtractionLibrary.Filters
{
    /// <summary>
    /// Converts an image into a greyscale image
    /// </summary>
    public class GreyscaleFilter : ImageFilter
    {
        /// <summary>
        /// Converts the image to a greyscale
        /// </summary>
        /// <param name="sourceP">Source image pointer</param>
        /// <param name="processP">Greyscale image pointer</param>
        /// <param name="sourceImage">Size of the source image</param>
        public override unsafe void Process(uint* sourceP, uint* processP, Size sourceImage)
        {
            for (int x = 0; x < sourceImage.Width; x++)
            {
                for (int y = 0; y < sourceImage.Height; y++)
                {
                    uint r = (sourceP[x + y * sourceImage.Width] & 0xFF0000) >> 16;
                    uint g = (sourceP[x + y * sourceImage.Width] & 0xFF00) >> 8;
                    uint b = (sourceP[x + y * sourceImage.Width] & 0xFF);

                    // calculate greyscale value
                    uint greyscale = (uint)(0.299 * r + 0.587 * g + 0.114 * b);

                    processP[x + y * sourceImage.Width] = 0xFF000000 | (greyscale << 16) | (greyscale << 8) | greyscale;
                }
            }
        }
    }
}
