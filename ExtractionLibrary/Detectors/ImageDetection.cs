using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace ExtractionLibrary.Detectors
{
    /// <summary>
    /// Detect something in the image
    /// </summary>
    public abstract class ImageDetection
    {
        /// <summary>
        /// ratio of allowed labels
        /// </summary>
        protected KeyValuePair<double, double> legalRatio = new KeyValuePair<double, double>();

        /// <summary>
        /// Sets the legal ratio to find labels
        /// </summary>
        public KeyValuePair<double, double> Ratio
        {
            set { legalRatio = value; }
        }

        /// <summary>
        /// Sets the min ratio to find labels
        /// </summary>
        public double MinRatio
        {
            set
            {
                double max = legalRatio.Value;
                legalRatio = new KeyValuePair<double, double>(value, max);
            }
        }

        /// <summary>
        /// Sets the max ratio to find labels
        /// </summary>
        public double MaxRatio
        {
            set
            {
                double min = legalRatio.Key;
                legalRatio = new KeyValuePair<double, double>(min, value);
            }
        }
    }
}
