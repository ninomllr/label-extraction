using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ExtractionLibrary;
using System.Drawing.Imaging;
using ExtractionLibrary.Filters;
using ExtractionLibrary.Detectors;
using ExtractionLibrary.Reader;
using System.Threading;

namespace ReadCodeBarGUI
{
    public partial class Form1 : Form
    {
        Bitmap originalImage;
        Bitmap stretchImage;
        Bitmap sourceImage;
        Bitmap processImage;
        Size size;
        bool scrolling = false;
        int scrollTime = 0;
        Thread scrollerThread = null;
        int trackBarValue = 0;
        string systemStatus = "idle";
        List<Rectangle> results = new List<Rectangle>();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadImage();
        }

        private void SetStatus(string status)
        {
            switch (status)
            {
                case "processing":
                    systemStatus = "processing";
                    break;
                default:
                    systemStatus = "idle";
                    break;
            }
        }

        private void LoadImage()
        {
            SetStatus("processing");
            OpenFileDialog fileDialog = new OpenFileDialog();

            // allow image files only
            fileDialog.Filter = "Image Files (JPEG,GIF,BMP,PNG)|*.jpg;*.jpeg;*.gif;*.bmp;*.png|JPEG Files(*.jpg;*.jpeg)|*.jpg;*.jpeg|GIF Files(*.gif)|*.gif|BMP Files(*.bmp)|*.bmp|PNG Files(*.png)|*.png";

            // check if we opened a valid file
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                sourceImage = new Bitmap(fileDialog.FileName);
                originalImage = new Bitmap(fileDialog.FileName);
                stretchImage = new Bitmap(fileDialog.FileName);
                processImage = new Bitmap(sourceImage);  // image for detection
                size = new Size(sourceImage.Width, sourceImage.Height);

                int grey = StretchContrast(-1);

                trackBar1.Value = grey;
                label3.Text = "Contrast: " + grey;
                label4.Text = "PredefinedContrast: " + grey;

                pictureBox1.Image = processImage;
            }

            SetStatus("idle");
            
        }

        private void buttonLoad_Click(object sender, EventArgs e)
        {
            LoadImage();
        }

        private void buttonFind_Click(object sender, EventArgs e)
        {
            SetStatus("processing");
            List<Rectangle> results = GetLabels();
            sourceImage = new Bitmap(stretchImage);
            Bitmap boxImage = new Bitmap(stretchImage);
            
            Graphics gProcess = Graphics.FromImage(boxImage);

            // draw the rectangles of the label positions
            foreach (Rectangle rectangle in results)
            {
                Brush b = new SolidBrush(Color.FromArgb(100, 255, 255, 0));
                Pen p = new Pen(new SolidBrush(Color.FromArgb(255, 255, 255, 0)),5);

                gProcess.DrawRectangle(p, rectangle);
                gProcess.FillRectangle(b, rectangle);
            }
            gProcess.Dispose();

            pictureBox1.Image = boxImage;
            SetStatus("idle");
        }

        public unsafe int StretchContrast(int stretchVal)
        {
            BitmapData sourceData = sourceImage.LockBits(new Rectangle(0, 0, sourceImage.Width, sourceImage.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            BitmapData processData = processImage.LockBits(new Rectangle(0, 0, processImage.Width, processImage.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            // set pointers
            uint* sourceP = (uint*)sourceData.Scan0;
            uint* processP = (uint*)processData.Scan0;

            GetImageContrast contrast = new GetImageContrast();
            int[] contr = contrast.Process(sourceP, processP, sourceImage.Size);

            label6.Text = "DiffContrast: " + Convert.ToString(contr[0]);
            label5.Text = "MaxDiffContrast: " + Convert.ToString(contr[1]);

            // filter the image
            ContrastStretchingFilter stretch = new ContrastStretchingFilter();
            stretch.difference = contr[0];
            stretch.Process(sourceP, processP, size, stretchVal);

            sourceImage.UnlockBits(sourceData);
            processImage.UnlockBits(processData);

            originalImage = new Bitmap(processImage);

            return Convert.ToInt32(stretch.greyOverall);
        }

        public unsafe List<Rectangle> GetLabels()
        {
            //List<Rectangle> results = new List<Rectangle>();
            results = new List<Rectangle>();

            BitmapData sourceData = sourceImage.LockBits(new Rectangle(0, 0, sourceImage.Width, sourceImage.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            BitmapData processData = processImage.LockBits(new Rectangle(0, 0, processImage.Width, processImage.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            // set pointers
            uint* sourceP = (uint*)sourceData.Scan0;
            uint* processP = (uint*)processData.Scan0;

            CopyImage(processP, sourceP, size);

            SobelFilter sobel = new SobelFilter();
            sobel.SobelValue = 50;
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
            detection.Ratio = new KeyValuePair<double, double>(3, 5);
            results = detection.Process(sourceP, processP, size);

            sourceImage.UnlockBits(sourceData);
            processImage.UnlockBits(processData);

            // save image for debugging
            sourceImage.Save(@"C:\Temp\labels_1.png");

            return results;
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

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            scrolling = true;
            scrollTime = 3;

            if (scrollerThread == null || !scrollerThread.IsAlive)
            {
                scrollerThread = new Thread(new ThreadStart(ContrastStretch));
                scrollerThread.IsBackground = true;
                scrollerThread.Start();
            }

            trackBarValue = trackBar1.Value;
            label3.Text = "Contrast: " + trackBar1.Value;
        }

        private unsafe void button1_Click(object sender, EventArgs e)
        {
            BitmapData data = originalImage.LockBits(new Rectangle(0, 0, originalImage.Width, originalImage.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            uint* pointer = (uint*)data.Scan0;

            textBoxBarcodes.Text = "";

            BarcodeReader reader = new BarcodeReader();
            textBoxBarcodes.Text += reader.ReadCode39(pointer, originalImage.Size);
            textBoxBarcodes.Text += "\n";

            originalImage.UnlockBits(data);
        }

        public void ContrastStretch()
        {
            while (scrolling)
                System.Threading.Thread.Sleep(1);

            SetStatus("processing");
            StretchContrast(trackBarValue);
            pictureBox1.Image = processImage;
            SetStatus("idle");
        }

        private void timerContrastStretch_Tick(object sender, EventArgs e)
        {
            switch (systemStatus)
            {
                case "processing":
                    toolStripStatusLabelStatus.Text = "Status: processing";
                    buttonFind.Enabled = false;
                    button1.Enabled = false;
                    buttonLoad.Enabled = false;
                    trackBar1.Enabled = false;
                    buttonReadFromResults.Enabled = false;
                    
                    break;
                default:
                    toolStripStatusLabelStatus.Text = "Status: idle";
                    buttonFind.Enabled = true;
                    button1.Enabled = true;
                    buttonLoad.Enabled = true;
                    trackBar1.Enabled = true;
                    buttonReadFromResults.Enabled = true;
                    break;
            }

            if (scrollTime == 0)
                scrolling = false;
            else
                scrollTime--;
        }

        private void buttonReadFromResults_Click(object sender, EventArgs e)
        {
            if (results != null)
            {
                SetStatus("processing");
                textBoxBarcodes.Text = "";
                int position = 0;
                foreach (Rectangle rectangle in results)
                {
                    Bitmap labelImage = new Bitmap(rectangle.Width, rectangle.Height);
                    Graphics labelGraphics = Graphics.FromImage(labelImage);
                    labelGraphics.DrawImage(sourceImage, new Rectangle(0, 0, rectangle.Width, rectangle.Height), rectangle, GraphicsUnit.Pixel);
                    labelGraphics.Dispose();

                    Bitmap testFilters = new Bitmap(labelImage);
                    Bitmap testFiltersProcess = new Bitmap(labelImage);
                    unsafe
                    {
                        BitmapData labelData = testFilters.LockBits(new Rectangle(0, 0, testFilters.Width, testFilters.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                        BitmapData labelDataProcess = testFiltersProcess.LockBits(new Rectangle(0, 0, testFiltersProcess.Width, testFiltersProcess.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                        uint* pointerSource = (uint*)labelData.Scan0;
                        uint* pointerProcess = (uint*)labelDataProcess.Scan0;

                        GreyscaleFilter grey = new GreyscaleFilter();
                        grey.Process(pointerSource, pointerProcess, labelImage.Size);

                        testFiltersProcess.Save(@"C:\Temp\labels\label_" + position + ".png");

                        testFiltersProcess.UnlockBits(labelDataProcess);
                        testFilters.UnlockBits(labelData);
                    }

                    
                    

                    unsafe
                    {
                        BitmapData data = labelImage.LockBits(new Rectangle(0, 0, labelImage.Width, labelImage.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                        uint* pointer = (uint*)data.Scan0;

                        BarcodeReader reader = new BarcodeReader();
                        textBoxBarcodes.Text += reader.ReadCode39(pointer, labelImage.Size);
                        textBoxBarcodes.Text += "\r\n";

                        labelImage.UnlockBits(data);
                    }

                    position++;

                }
                SetStatus("idle");
            }
        }
    }
}
