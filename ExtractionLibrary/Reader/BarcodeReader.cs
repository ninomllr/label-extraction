using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;
using System.Text.RegularExpressions;

namespace ExtractionLibrary.Reader
{
    public class BarcodeReader
    {
        public struct BarcodeHistogram
        {
            public float[] histogram;
            public float min;
            public float max;
        }

        public unsafe string ReadCode39(uint* pointer, Size size)
        {
            bool firstRun = true;
            Dictionary<string, int> results = new Dictionary<string, int>();

            for (int y = 0; y < size.Height; y++)
            {
                if (firstRun)
                    firstRun = false;
                else
                    firstRun = true;

                BarcodeHistogram histogram = HorizontalHistogram(pointer, y, size.Width);

                int firstBar = 0;
                int lastBar = 0;

                // Get the threshold between black and white
                float threshold = histogram.min + ((histogram.max - histogram.min) / 2);
                threshold = threshold / 100 * 108;
                //float threshold = histogram.min + ((histogram.max - histogram.min) / 2) - (histogram.max - histogram.min) / 10;

                // Find first black bar
                for (int i = 0; i < histogram.histogram.Length; i++)
                {
                    if (histogram.histogram[i] < threshold)
                    {
                        firstBar = i;
                        break;
                    }
                }

                // Find last black bar
                for (int i = histogram.histogram.Length - 1; i > 0 ; i--)
                {
                    if (histogram.histogram[i] < threshold)
                    {
                        lastBar = i;
                        break;
                    }
                }

                if (lastBar == 0)
                    continue;
                Bitmap bmp = new Bitmap(lastBar - firstBar + 1, 50);
                Graphics g = Graphics.FromImage(bmp);

                Bitmap second = new Bitmap(lastBar - firstBar + 1, (int)histogram.max);
                Graphics gSecond = Graphics.FromImage(second);

                // Clear the whole image black
                g.Clear(Color.Red);
                gSecond.Clear(Color.White);

                // Set up the values for histogram
                int x = -1;
                char[] code = new char[histogram.histogram.Length];
                char[] dirs = new char[lastBar - firstBar + 1];
                if (y == size.Height / 3 * 2)
                { }
                // Create a graphical histogram, with a better quality
                foreach (float value in histogram.histogram)
                {
                    x++;

                    if (x < firstBar)
                        continue;
                    if (x > lastBar)
                        break;

                    if (x == firstBar)
                    {
                        dirs[x - firstBar] = 'u';
                    }
                    else if (value < histogram.histogram[x - 1])
                        dirs[x - firstBar] = 'u';
                    else
                        dirs[x - firstBar] = 'd';

                    gSecond.DrawLine(Pens.Green, new Point(0, (int)threshold), new Point(lastBar - firstBar + 1, (int)threshold));
                    gSecond.DrawLine(Pens.Black, new Point(x - firstBar, (int)value), new Point(x - firstBar, (int)histogram.max));

                    if (value < threshold)
                    {
                        g.DrawLine(Pens.Black, new Point(x - firstBar, 50), new Point(x - firstBar, 0));
                    }
                    else
                    {
                        g.DrawLine(Pens.White, new Point(x - firstBar, 50), new Point(x - firstBar, 0));
                    }

                    
                }
                g.Dispose();
                //gSecond.Dispose();
                if (y == size.Height / 3 * 2)
                {
                    second.Save(@"C:\Temp\histogram.png", ImageFormat.Png);
                    bmp.Save(@"C:\Temp\barcode.png", ImageFormat.Png);
                }



                BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

                uint* improvedPointer = (uint*)data.Scan0;
                BarcodeHistogram improvedHistogram = HorizontalHistogram(improvedPointer, 0, size.Width);
                threshold = improvedHistogram.min + ((improvedHistogram.max - improvedHistogram.min) / 2);
                //threshold = threshold / 10 * 7;
                x = 0;
                foreach (float value in improvedHistogram.histogram)
                {
                    if (value < threshold)
                    {
                        code[x] = 'b';
                    }
                    else
                    {
                        code[x] = 'w';
                    }

                    x++;
                }

                bmp.UnlockBits(data);

                int narrowWith = 0;
                string patternString = "";
                int startPos = 0;
                List<int> barSizes = new List<int>();
                char currentChar = 'b';
                int numberOfSameColor = 0;
                int changes = 0;
                int colors = 0;

                foreach (char value in code)
                {
                    if (value == currentChar)
                        numberOfSameColor++;
                    else
                    {
                        changes++;
                        barSizes.Add(numberOfSameColor);
                        colors += numberOfSameColor;
                        
                        numberOfSameColor = 1;
                                                
                        if (currentChar == 'b')
                            currentChar = 'w';
                        else
                            currentChar = 'b';
                    }
                }
                changes++;


                narrowWith = colors / barSizes.Count;
                narrowWith = code.Length / changes;
                if (narrowWith != 1 && firstRun)
                    narrowWith++;

                currentChar = 'b';
                numberOfSameColor = 0;

                for (int i = 0; i < code.Length; i++)
                {
                    if (code[i] == currentChar)
                        numberOfSameColor++;
                    else
                    {
                        if (narrowWith >= numberOfSameColor) // its a narrow bar
                        {
                            patternString += "n";
                        }
                        else // its a wide bar
                        {
                            patternString += "w";
                        }

                        currentChar = code[i];
                        numberOfSameColor = 1;
                    }
                }

                if (narrowWith >= numberOfSameColor) // its a narrow bar
                {
                    patternString += "n";
                }
                else // its a wide bar
                {
                    patternString += "w";
                }

                if (patternString.Length > 3)
                {
                    if (patternString.Substring(patternString.Length - 3, 3) == "wwn")
                        patternString = patternString.Substring(0, patternString.Length - 3) + "n";
                }

                string dataString = "";

                try
                {
                    // Each pattern within code 39 is nine bars with one white bar between each pattern
                    for (int i = 0; i < patternString.Length - 1; i += 10)
                    {
                        if (i + 9 > patternString.Length - 1)
                            break;

                        // Create an array of charachters to hold the pattern to be tested
                        char[] pattern = new char[9];
                        // Stuff the pattern with data from the pattern string
                        patternString.CopyTo(i, pattern, 0, 9);

                        dataString += parsePattern(new string(pattern));
                    }
                }
                catch (Exception err)
                {
                }
                

                if (String.IsNullOrEmpty(dataString))
                    continue;

                if (dataString.Substring(dataString.Length - 1, 1) != "*")
                    dataString += "*";
                if (dataString.Substring(0, 1) != "*")
                    dataString = "*" + dataString;

                Regex exp = new Regex(
                        @"^\*[A-Z]{2,4}[0-9]{3,4}.\*$",
                        RegexOptions.IgnoreCase);

                Match match = exp.Match(dataString);

                if (match.Success)
                {
                    string barcode = CheckBarcode(dataString);
                    if (barcode != null)
                    {
                        if (results.ContainsKey(barcode))
                        {
                            results[barcode]++;
                        }
                        else
                        {
                            results.Add(barcode, 1);
                        }
                    }
                }

                if (!firstRun)
                    y--;
            }

            int max = 0;
            string result = "";

            foreach (KeyValuePair<string, int> kvp in results)
            {
                if (kvp.Value > max)
                {
                    max = kvp.Value;
                    result = kvp.Key;
                }
            }

            return result;
        }

        private string CheckBarcode(string barcode)
        {
            barcode = barcode.Substring(1, barcode.Length - 1);
            barcode = barcode.Substring(0, barcode.Length - 1);

            char checksum = Convert.ToChar(barcode.Substring(barcode.Length - 1, 1));

            barcode = barcode.Substring(0, barcode.Length - 1);

            if (checksum == Checksum39(barcode))
                return barcode;
            else
                return null;
        }

        private char Checksum39(string barcode)
        {
            string charSet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ-.!$/+%";

            char[] textArray = barcode.ToCharArray();
            int checkSum = 0;

            foreach (char letter in textArray)
            {
                if (charSet.IndexOf(letter) != -1)
                {
                    checkSum += charSet.IndexOf(letter);
                }
                else
                {
                    //throw (new Exception("Barcode is not valid"));
                }
            }

            checkSum = (checkSum % 43);

            return charSet[checkSum];
        }

        private static string parsePattern(string pattern)
        {

            switch (pattern)
            {
                case "wnnwnnnnw":
                    return "1";
                case "nnwwnnnnw":
                    return "2";
                case "wnwwnnnnn":
                    return "3";
                case "nnnwwnnnw":
                    return "4";
                case "wnnwwnnnn":
                    return "5";
                case "nnwwwnnnn":
                    return "6";
                case "nnnwnnwnw":
                    return "7";
                case "wnnwnnwnn":
                    return "8";
                case "nnwwnnwnn":
                    return "9";
                case "nnnwwnwnn":
                    return "0";
                case "wnnnnwnnw":
                    return "A";
                case "nnwnnwnnw":
                    return "B";
                case "wnwnnwnnn":
                    return "C";
                case "nnnnwwnnw":
                    return "D";
                case "wnnnwwnnn":
                    return "E";
                case "nnwnwwnnn":
                    return "F";
                case "nnnnnwwnw":
                    return "G";
                case "wnnnnwwnn":
                    return "H";
                case "nnwnnwwnn":
                    return "I";
                case "nnnnwwwnn":
                    return "J";
                case "wnnnnnnww":
                    return "K";
                case "nnwnnnnww":
                    return "L";
                case "wnwnnnnwn":
                    return "M";
                case "nnnnwnnww":
                    return "N";
                case "wnnnwnnwn":
                    return "O";
                case "nnwnwnnwn":
                    return "P";
                case "nnnnnnwww":
                    return "Q";
                case "wnnnnnwwn":
                    return "R";
                case "nnwnnnwwn":
                    return "S";
                case "nnnnwnwwn":
                    return "T";
                case "wwnnnnnnw":
                    return "U";
                case "nwwnnnnnw":
                    return "V";
                case "wwwnnnnnn":
                    return "W";
                case "nwnnwnnnw":
                    return "X";
                case "wwnnwnnnn":
                    return "Y";
                case "nwwnwnnnn":
                    return "Z";
                case "nwnnnnwnw":
                    return "-";
                case "wwnnnnwnn":
                    return ".";
                case "nwwnnnwnn":
                    return " ";
                case "nwnnwnwnn":
                    return "*";
                case "nwnwnwnnn":
                    return "$";
                case "nwnwnnnwn":
                    return "/";
                case "nwnnnwnwn":
                    return "+";
                case "nnnwnwnwn":
                    return "%";
                default:
                    return null;
            }
        }

        private static List<string> GetSimilar(string pattern)
        {
            List<string> values = new List<string>();
            char[] patternArray = new char[pattern.Length];

            for (int i = 0; i < pattern.Length; i++)
                patternArray[i] = Convert.ToChar(pattern.Substring(i, 1));

            values.Add("wnnwnnnnw");
            values.Add("nnwwnnnnw");
            values.Add("wnwwnnnnn");
            values.Add("nnnwwnnnw");
            values.Add("wnnwwnnnn");
            values.Add("nnwwwnnnn");
            values.Add("nnnwnnwnw");
            values.Add("wnnwnnwnn");
            values.Add("nnwwnnwnn");
            values.Add("nnnwwnwnn");
            values.Add("wnnnnwnnw");
            values.Add("nnwnnwnnw");
            values.Add("wnwnnwnnn");
            values.Add("nnnnwwnnw");
            values.Add("wnnnwwnnn");
            values.Add("nnwnwwnnn");
            values.Add("nnnnnwwnw");
            values.Add("wnnnnwwnn");
            values.Add("nnwnnwwnn");
            values.Add("nnnnwwwnn");
            values.Add("wnnnnnnww");
            values.Add("nnwnnnnww");
            values.Add("wnwnnnnwn");
            values.Add("nnnnwnnww");
            values.Add("wnnnwnnwn");
            values.Add("nnwnwnnwn");
            values.Add("nnnnnnwww");
            values.Add("wnnnnnwwn");
            values.Add("nnwnnnwwn");
            values.Add("nnnnwnwwn");
            values.Add("wwnnnnnnw");
            values.Add("nwwnnnnnw");
            values.Add("wwwnnnnnn");
            values.Add("nwnnwnnnw");
            values.Add("wwnnwnnnn");
            values.Add("nwwnwnnnn");
            values.Add("nwnnnnwnw");
            values.Add("wwnnnnwnn");
            values.Add("nwwnnnwnn");
            values.Add("nwnnwnwnn");
            values.Add("nwnwnwnnn");
            values.Add("nwnwnnnwn");
            values.Add("nwnnnwnwn");
            values.Add("nnnwnwnwn");

            int[] valueCount = new int[values.Count];

            int pos = 0;
            foreach (string bar in values)
            {
                for (int i = 0; i < pattern.Length; i++)
                {
                    if (Convert.ToChar(bar.Substring(i, 1)) == patternArray[i])
                    {
                        if (valueCount[pos] == null)
                            valueCount[pos] = 0;
                        valueCount[pos]++;
                    }
                }
                pos++;
            }

            int max = 0;
            foreach (int val in valueCount)
            {
                if (max < val)
                    max = val;
            }

            List<string> possibilities = new List<string>();
            for (pos = 0; pos < valueCount.Length; pos++)
            {
                if (max == valueCount[pos])
                {
                    possibilities.Add(parsePattern(values[pos]));
                }
            }
            

            return possibilities;
        }


        public unsafe BarcodeHistogram HorizontalHistogram(uint* pointer, int y, int maxX)
        {
            BarcodeHistogram histogram = new BarcodeHistogram();
            histogram.histogram = new float[maxX];

            for (int x = 0; x < maxX; x++)
            {
                uint r = (pointer[y * maxX + x] & 0xFF0000) >> 16;
                uint g = (pointer[y * maxX + x] & 0xFF00) >> 8;
                uint b = (pointer[y * maxX + x] & 0xFF);
                float rgb = ((r + g + b) / 3);

                histogram.histogram[x] = rgb;

                if (rgb > histogram.max)
                    histogram.max = rgb;

                if (rgb < histogram.min)
                    histogram.min = rgb;
            }

            return histogram;
        }
    }
}
