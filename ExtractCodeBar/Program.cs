using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using ExtractionLibrary;

namespace ExtractCodeBar
{
    /// <summary>
    /// Console application
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            if (CheckParameters(args))
            {
                // start stop watch to measure execution time
                System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Start();
                
                LabelExtraction barcode = new LabelExtraction();

                if (args[1].Substring(args[1].Length - 1) != "\\")
                    args[1] = args[1] + "\\";

                Console.WriteLine("Label Extraction");
                Console.WriteLine("-------------------------");

                int numberOfLabelsFound = barcode.SaveLabels(args[1], new Bitmap(args[0]));

                stopwatch.Stop();
                TimeSpan timeElapsed = TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds);

                Console.WriteLine("Successfull!");
                Console.WriteLine("Found " + numberOfLabelsFound + " labels. Elapsed time: " + timeElapsed.ToString());

                // open the folder
                System.Diagnostics.Process.Start(args[1]);

                Console.ReadLine();
            }
            else
            {
                Console.WriteLine("Label Extraction");
                Console.WriteLine("-------------------------");
                Console.WriteLine("Usage:");
                Console.WriteLine("ExtractCodeBar.exe <source.img> <path to extract>");
                Console.ReadLine();
            }
        }

        /// <summary>
        /// Checks if the parameters are correct
        /// </summary>
        /// <param name="args">Passed arguments</param>
        /// <returns>True if valid parameters</returns>
        private static bool CheckParameters(string [] args)
        {
            // check number of arguments
            if (args.Length != 2)
                return false;

            // check if first argument is a file
            if (!File.Exists(args[0]))
                return false;

            // check if first file is an image
            try
            {
                Bitmap bitmap = new Bitmap(args[0]);
            }
            catch (Exception exception)
            {
                return false;
            }

            // check if second argument is a folder
            if (!((File.GetAttributes(args[1]) & FileAttributes.Directory) == FileAttributes.Directory))
                return false;

            return true;
        }
    }
}
