using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace ExtractionLibrary.Filters
{
    public class ContrastStretchingFilter : ImageFilter
    {
        public long greyOverall = 0;
        public int difference = 0;

        public unsafe int GenerateIdealValue(uint* sourceP, uint* processP, Size sourceImage)
        {
            //int x1, y1;
            //greyOverall = 0;

            //for (int x = 0; x < sourceImage.Width; x++)
            //{
            //    for (int y = 0; y < sourceImage.Height; y++)
            //    {
            //        y1 = y * sourceImage.Width;

            //        uint r = (sourceP[x + y * sourceImage.Width] & 0xFF0000) >> 16;
            //        uint g = (sourceP[x + y * sourceImage.Width] & 0xFF00) >> 8;
            //        uint b = (sourceP[x + y * sourceImage.Width] & 0xFF);

            //        // calculate greyscale value
            //        uint greyscale = (uint)(0.299 * r + 0.587 * g + 0.114 * b);
            //        greyOverall += greyscale;
            //    }
            //}

            //greyOverall = greyOverall / (sourceImage.Width * sourceImage.Height);
            greyOverall = 0;
            return Convert.ToInt32(greyOverall);
        }

        public unsafe override void Process(uint* sourceP, uint* processP, Size sourceImage)
        {
            Process(sourceP, processP, sourceImage, -1);
        }

        public unsafe void Process(uint* sourceP, uint* processP, Size sourceImage, int stretchVal)
        {
            if (stretchVal == -1)
                stretchVal = GenerateIdealValue(sourceP, processP, sourceImage);

            int x1, y1;
            byte[] lut = GenerateLookUpTable(stretchVal);
            byte originalPix;

            for (int x = 0; x < sourceImage.Width; x++)
            {
                for (int y = 0; y < sourceImage.Height; y++)
                {
                    y1 = y * sourceImage.Width;

                    uint r = (sourceP[x + y * sourceImage.Width] & 0xFF0000) >> 16;
                    uint g = (sourceP[x + y * sourceImage.Width] & 0xFF00) >> 8;
                    uint b = (sourceP[x + y * sourceImage.Width] & 0xFF);

                    // calculate greyscale value
                    uint greyscale = (uint)(0.299 * r + 0.587 * g + 0.114 * b);
                    byte grayVal = Convert.ToByte(greyscale);

                    originalPix = (byte)grayVal;
                    uint newVal = lut[originalPix];
                    processP[x + y * sourceImage.Width] = 0xFF000000 | (newVal << 16) | (newVal << 8) | newVal;
                }
            }
        }

        public byte[] GenerateLookUpTable(long value)
        {
            byte[] lookUpTable = new byte[256];
            byte b = 0;

            int r1 = 0;
            int r2 = (int)value + ((int)value / difference);
            greyOverall = r2;
            int r3 = 255;
            double s1 = 0; 
            double s2 = 0;
            double s3 = 255;

            int diffR21 = r2 - r1;
            int diffR32 = r3 - r1;
            double diffS21 = s2 - s1;
            double diffS32 = s3 - s2;

            double factor1 = 0.0; double factor2 = 0.0;
            if (diffR32 != 0) factor2 = diffS32 / diffR32;
            if (diffR21 != 0) factor1 = diffS21 / diffR21;

            for (int i = 0; i < 256; i++)
            {
                if (i <= r1)
                {
                    if (r1 == 0)
                        b = Convert.ToByte(s1);
                    else
                        b = Convert.ToByte(i * s1 / r1);
                }
                else if ((r1 < i) && (i <= r2))
                {
                    if (diffR21 == 0)
                        b = Convert.ToByte(s2);
                    else
                        b = Convert.ToByte(s1 + factor1 * (i - r1));
                }
                else
                {
                    if (diffR32 == 0)
                        b = Convert.ToByte(s3);
                    else
                        b = Convert.ToByte(s2 + factor2 * (i - r2));
                }

                lookUpTable[i] = b;
            }

            return lookUpTable;
        }
    }
}
