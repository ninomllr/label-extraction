namespace ExtractCodeBarGUI
{
    partial class ExtractCodeBarGUI
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.button1 = new System.Windows.Forms.Button();
            this.picOriginal = new PictureBoxCtrl.PictureBox();
            this.picFilter = new PictureBoxCtrl.PictureBox();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(9, 361);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 5;
            this.button1.Text = "Load Image";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // picOriginal
            // 
            this.picOriginal.Border = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picOriginal.Location = new System.Drawing.Point(9, 13);
            this.picOriginal.Name = "picOriginal";
            this.picOriginal.Picture = "";
            this.picOriginal.Size = new System.Drawing.Size(501, 333);
            this.picOriginal.TabIndex = 4;
            // 
            // picFilter
            // 
            this.picFilter.Border = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picFilter.Location = new System.Drawing.Point(518, 12);
            this.picFilter.Name = "picFilter";
            this.picFilter.Picture = "";
            this.picFilter.Size = new System.Drawing.Size(501, 333);
            this.picFilter.TabIndex = 3;
            // 
            // ExtractCodeBarGUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1026, 393);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.picOriginal);
            this.Controls.Add(this.picFilter);
            this.Name = "ExtractCodeBarGUI";
            this.Text = "ExtractCodeBarGUI";
            this.Load += new System.EventHandler(this.ExtractCodeBarGUI_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private PictureBoxCtrl.PictureBox picFilter;
        private PictureBoxCtrl.PictureBox picOriginal;
        private System.Windows.Forms.Button button1;
    }
}

