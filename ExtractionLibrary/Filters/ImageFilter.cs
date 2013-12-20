using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace ExtractionLibrary.Filters
{
    /// <summary>
    /// Filter for an image
    /// </summary>
    public abstract class ImageFilter
    {
        /// <summary>
        /// Processes a filter on the image
        /// </summary>
        /// <param name="sourceP">Source pointer</param>
        /// <param name="processP">Process pointer to apply filter on</param>
        /// <param name="sourceImage">Size of the image</param>
        public abstract unsafe void Process(uint* sourceP, uint* processP, Size sourceImage);
    }
}
