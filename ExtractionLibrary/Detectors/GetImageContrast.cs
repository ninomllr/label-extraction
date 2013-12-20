using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace ExtractionLibrary.Detectors
{
    public class GetImageContrast : ImageDetection
    {
        public unsafe int[] Process(uint* sourceP, uint* processP, Size sourceImage)
        {
            int y1 = 0;
            int curVal = 0;
            int minContrast = 255 * 3;
            int maxContrast = 0;
            int overallContrast = 0;

            List<uint> values = new List<uint>();
            List<long> contrasts = new List<long>();
            for (int x = 1; x < sourceImage.Width - 1; x++)
            {
                for (int y = 1; y < sourceImage.Height - 1; y++)
                {
                    uint r = 0;
                    uint g = 0;
                    uint b = 0;
                    uint rgb = 0;

                    values.Clear();
                    y1 = y * sourceImage.Width;

                    r = (sourceP[y1 + x] & 0xFF0000) >> 16;
                    g = (sourceP[y1 + x] & 0xFF00) >> 8;
                    b = (sourceP[y1 + x] & 0xFF);
                    curVal = Convert.ToInt32((0.299 * r + 0.587 * g + 0.114 * b));

                    r = (sourceP[y1 - 1 + x] & 0xFF0000) >> 16;
                    g = (sourceP[y1 - 1 + x] & 0xFF00) >> 8;
                    b = (sourceP[y1 - 1 + x] & 0xFF);
                    values.Add((uint)(0.299 * r + 0.587 * g + 0.114 * b));

                    r = (sourceP[y1 - 1 + x] & 0xFF0000) >> 16;
                    g = (sourceP[y1 - 1 + x] & 0xFF00) >> 8;
                    b = (sourceP[y1 - 1 + x] & 0xFF);
                    values.Add((uint)(0.299 * r + 0.587 * g + 0.114 * b));

                    r = (sourceP[y1 - 1 + x + 1] & 0xFF0000) >> 16;
                    g = (sourceP[y1 - 1 + x + 1] & 0xFF00) >> 8;
                    b = (sourceP[y1 - 1 + x + 1] & 0xFF);
                    values.Add((uint)(0.299 * r + 0.587 * g + 0.114 * b));

                    r = (sourceP[y1 + x - 1] & 0xFF0000) >> 16;
                    g = (sourceP[y1 + x - 1] & 0xFF00) >> 8;
                    b = (sourceP[y1 + x - 1] & 0xFF);
                    values.Add((uint)(0.299 * r + 0.587 * g + 0.114 * b));

                    r = (sourceP[y1 + x + 1] & 0xFF0000) >> 16;
                    g = (sourceP[y1 + x + 1] & 0xFF00) >> 8;
                    b = (sourceP[y1 + x + 1] & 0xFF);
                    values.Add((uint)(0.299 * r + 0.587 * g + 0.114 * b));

                    r = (sourceP[y1 + 1 + x] & 0xFF0000) >> 16;
                    g = (sourceP[y1 + 1 + x] & 0xFF00) >> 8;
                    b = (sourceP[y1 + 1 + x] & 0xFF);
                    values.Add((uint)(0.299 * r + 0.587 * g + 0.114 * b));

                    r = (sourceP[y1 + 1 + x] & 0xFF0000) >> 16;
                    g = (sourceP[y1 + 1 + x] & 0xFF00) >> 8;
                    b = (sourceP[y1 + 1 + x] & 0xFF);
                    values.Add((uint)(0.299 * r + 0.587 * g + 0.114 * b));

                    r = (sourceP[y1 + 1 + x + 1] & 0xFF0000) >> 16;
                    g = (sourceP[y1 + 1 + x + 1] & 0xFF00) >> 8;
                    b = (sourceP[y1 + 1 + x + 1] & 0xFF);
                    values.Add((uint)(0.299 * r + 0.587 * g + 0.114 * b));

                    int contrastVal = 0;

                    foreach (uint val in values)
                    {
                        contrastVal += Math.Abs(curVal - Convert.ToInt32(val));    
                    }
                    contrasts.Add(contrastVal / 8);
                }
            }

            foreach (int contr in contrasts)
            {
                overallContrast += contr;
                if (minContrast > contr)
                    minContrast = contr;
                if (maxContrast < contr)
                    maxContrast = contr;
            }
            overallContrast = overallContrast / contrasts.Count;
            int maxDiffContrast = maxContrast - minContrast;

            return new int[] { overallContrast, maxDiffContrast };
        }
    }
}
