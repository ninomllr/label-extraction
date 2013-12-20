using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace ExtractionLibrary.Filters
{
    public class BlackWhiteFilter : ImageFilter
    {
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

                    if (greyscale > 100)
                        processP[x + y * sourceImage.Width] = 0xFFFFFFFF;
                    else
                        processP[x + y * sourceImage.Width] = 0xFF000000;
                }
            }
        }
    }
}
