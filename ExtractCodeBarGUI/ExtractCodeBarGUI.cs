using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ExtractionLibrary;

namespace ExtractCodeBarGUI
{
    /// <summary>
    /// Windows Application
    /// </summary>
    public partial class ExtractCodeBarGUI : Form
    {
        public ExtractCodeBarGUI()
        {
            InitializeComponent();
        }

        private void ExtractCodeBarGUI_Load(object sender, EventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            LabelExtraction barcode = new LabelExtraction();
            OpenFileDialog fileDialog = new OpenFileDialog();

            // allow image files only
            fileDialog.Filter = "Image Files (JPEG,GIF,BMP,PNG)|*.jpg;*.jpeg;*.gif;*.bmp;*.png|JPEG Files(*.jpg;*.jpeg)|*.jpg;*.jpeg|GIF Files(*.gif)|*.gif|BMP Files(*.bmp)|*.bmp|PNG Files(*.png)|*.png";

            // check if we opened a valid file
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                Bitmap sourceImage = new Bitmap(fileDialog.FileName);
                // Get label positions
                List<Rectangle> results = barcode.GetLabels(sourceImage);
                Bitmap processImage = barcode.Image;

                Graphics gSource = Graphics.FromImage(sourceImage);
                Graphics gProcess = Graphics.FromImage(processImage);

                // draw the rectangles of the label positions
                foreach (Rectangle rectangle in results)
                {
                    gSource.DrawRectangle(Pens.Yellow, rectangle);
                    gProcess.DrawRectangle(Pens.Yellow, rectangle);
                }

                picOriginal.Picture = null;
                picFilter.Picture = null;

                // save image for debugging
                sourceImage.Save(@"C:\Temp\labels_source.png");

                //// save image for debugging
                //processImage.Save(@"C:\Temp\labels_process.png");

                //picOriginal.Picture = @"C:\Temp\labels_source.png";
                //picFilter.Picture = @"C:\Temp\labels_process.png";

                //System.Diagnostics.Process.Start(@"C:\Temp\");
                //Close();
            }
            else
                Close();
        }
    }
}
