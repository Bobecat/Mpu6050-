namespace WinFormsApp1
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            cmbPorts = new ComboBox();
            btnOpenClose = new Button();
            pbDisplay = new PictureBox();
            lblAcc = new Label();
            lblGyro = new Label();
            ((System.ComponentModel.ISupportInitialize)pbDisplay).BeginInit();
            SuspendLayout();
            // 
            // cmbPorts
            // 
            cmbPorts.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPorts.Location = new Point(12, 12);
            cmbPorts.Name = "cmbPorts";
            cmbPorts.Size = new Size(150, 25);
            cmbPorts.TabIndex = 0;
            // 
            // btnOpenClose
            // 
            btnOpenClose.Location = new Point(168, 10);
            btnOpenClose.Name = "btnOpenClose";
            btnOpenClose.Size = new Size(100, 27);
            btnOpenClose.TabIndex = 1;
            btnOpenClose.Text = "Open";
            btnOpenClose.UseVisualStyleBackColor = true;
            btnOpenClose.Click += btnOpenClose_Click;
            // 
            // pbDisplay
            // 
            pbDisplay.Location = new Point(280, 10);
            pbDisplay.Name = "pbDisplay";
            pbDisplay.Size = new Size(300, 260);
            pbDisplay.TabIndex = 4;
            pbDisplay.TabStop = false;
            pbDisplay.Click += pbDisplay_Click;
            pbDisplay.Paint += pbDisplay_Paint;
            // 
            // lblAcc
            // 
            lblAcc.AutoSize = true;
            lblAcc.Location = new Point(12, 50);
            lblAcc.Name = "lblAcc";
            lblAcc.Size = new Size(119, 17);
            lblAcc.TabIndex = 2;
            lblAcc.Text = "Acc: X:--- Y:--- Z:---";
            // 
            // lblGyro
            // 
            lblGyro.AutoSize = true;
            lblGyro.Location = new Point(12, 75);
            lblGyro.Name = "lblGyro";
            lblGyro.Size = new Size(127, 17);
            lblGyro.TabIndex = 3;
            lblGyro.Text = "Gyro: X:--- Y:--- Z:---";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(600, 300);
            Controls.Add(cmbPorts);
            Controls.Add(btnOpenClose);
            Controls.Add(lblAcc);
            Controls.Add(lblGyro);
            Controls.Add(pbDisplay);
            Name = "Form1";
            Text = "MPU6050 Viewer";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)pbDisplay).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.ComboBox cmbPorts;
        private System.Windows.Forms.Button btnOpenClose;
        private System.Windows.Forms.Label lblAcc;
        private System.Windows.Forms.Label lblGyro;
        private System.Windows.Forms.PictureBox pbDisplay;
    }
}
