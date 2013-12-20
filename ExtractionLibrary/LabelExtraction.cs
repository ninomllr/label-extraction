using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using ExtractionLibrary.Filters;
using ExtractionLibrary.Detectors;
using System.Drawing.Imaging;
using System.Collections;

namespace ExtractionLibrary
{
    /// <summary>
    /// Extract labels out of an image
    /// </summary>
    public class LabelExtraction
    {
        /// <summary>
        /// ratio of allowed labels
        /// </summary>
        private KeyValuePair<double, double> legalRatio = new KeyValuePair<double, double>();

        /// <summary>
        /// Source image without filters
        /// </summary>
        private Bitmap sourceImage;
        /// <summary>
        /// Processed image with filters
        /// </summary>
        private Bitmap processImage;

        /// <summary>
        /// Sobel value which indicates if pixel should be white or black
        /// </summary>
        private double sobelValue = 100;

        /// <summary>
        /// Sets the sobel value
        /// </summary>
        public double SobelValue
        {
            set { sobelValue = value; }
        }

        /// <summary>
        /// Processed and filtered image
        /// </summary>
        public Bitmap Image
        {
            get { return sourceImage; }
        }

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

        /// <summary>
        /// Construct
        /// </summary>
        public LabelExtraction()
        {
            legalRatio = new KeyValuePair<double, double>(3, 5);
        }

        /// <summary>
        /// Detects the labels in the Bitmap
        /// </summary>
        /// <param name="source">Source Bitmap where the labels should be found</param>
        /// <returns>List of found labels</returns>
        public unsafe List<Rectangle> GetLabels(Bitmap source)
        {
            List<Rectangle> results = new List<Rectangle>();
            Size size = new Size(source.Width, source.Height);  // size of the image

            sourceImage = new Bitmap(source);   // normal image
            processImage = new Bitmap(source);  // image for detection

            BitmapData sourceData = sourceImage.LockBits(new Rectangle(0, 0, sourceImage.Width, sourceImage.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            BitmapData processData = processImage.LockBits(new Rectangle(0, 0, processImage.Width, processImage.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            // set pointers
            uint* sourceP = (uint*)sourceData.Scan0;
            uint* processP = (uint*)processData.Scan0;

            // filter the image
            ContrastStretchingFilter stretch = new ContrastStretchingFilter();
            stretch.Process(sourceP, processP, size);
            processImage.Save(@"C:\Temp\labels_stretch.png");

            CopyImage(processP, sourceP, size);

            SobelFilter sobel = new SobelFilter();
            sobel.SobelValue = sobelValue;
            sobel.Process(sourceP, processP, size);

            PixelExtractionFilter pixelExtraction = new PixelExtractionFilter();
            pixelExtraction.Process(sourceP, processP, size);

            // save image for debugging
            processImage.Save(@"C:\Temp\labels_0.png");

            DrawBlackBorder(processP, size);

            // override source bitmap with filtered bitmap
            CopyImage(processP, sourceP, size);

            // detect the labels
            LabelDetection detection = new LabelDetection();
            detection.Ratio = legalRatio;
            results = detection.Process(sourceP, processP, size);

            sourceImage.UnlockBits(sourceData);
            processImage.UnlockBits(processData);

            // save image for debugging
            sourceImage.Save(@"C:\Temp\labels_1.png");

            return results;
        }

        /// <summary>
        /// Extracts labels and saves them to a folder.
        /// </summary>
        /// <param name="path">Path to store the labels</param>
        /// <param name="source">Source image to read the labels from</param>
        /// <returns>Number of found labels</returns>
        public unsafe int SaveLabels(string path, Bitmap source)
        {
            Bitmap saveImage = new Bitmap(source);
            List<Rectangle> results = GetLabels(source);
            int position = 1;

            // save labels as images
            foreach (Rectangle rectangle in results)
            {
                Bitmap labelImage = new Bitmap(rectangle.Width, rectangle.Height);
                Graphics labelGraphics = Graphics.FromImage(labelImage);
                labelGraphics.DrawImage(saveImage, new Rectangle(0, 0, rectangle.Width, rectangle.Height), rectangle,GraphicsUnit.Pixel);
                labelImage.Save(path + "label_" + position + ".png");
                labelGraphics.Dispose();
                position++;
            }

            return results.Count;
        }

        /// <summary>
        /// Copies each pixel of an image to another
        /// </summary>
        /// <param name="from">Image pointer to copy from</param>
        /// <param name="to">Image pointer to copy to</param>
        /// <param name="sourceImage">Size of the image</param>
        private unsafe void CopyImage(uint* from, uint* to, Size sourceImage)
        {
            for (int x = 0; x < sourceImage.Width; x++)
                for (int y = 0; y < sourceImage.Height; y++)
                    to[y * sourceImage.Width + x] = from[y * sourceImage.Width + x];
        }

        /// <summary>
        /// Paint all pixel on the border black
        /// </summary>
        /// <param name="pointer">Pointer of the image</param>
        /// <param name="sourceImage">Size of the image</param>
        private unsafe void DrawBlackBorder(uint* pointer, Size sourceImage)
        {
            // paint pixels on bitmap border black
            for (int y = 0; y < sourceImage.Height; y++)
            {
                pointer[y * sourceImage.Width] = 0xFF000000;
                pointer[y * sourceImage.Width + sourceImage.Width - 1] = 0xFF000000;
            }

            for (int x = 0; x < sourceImage.Width; x++)
            {
                pointer[x] = 0xFF000000;
                pointer[(sourceImage.Height - 1) * sourceImage.Width + x] = 0xFF000000;
            }
        }
    }
}
