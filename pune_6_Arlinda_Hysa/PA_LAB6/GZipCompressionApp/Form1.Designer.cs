namespace GZipCompressionApp
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
            button1 = new Button();
            button2 = new Button();
            button3 = new Button();
            txtFolderPath = new TextBox();
            lblStatus = new Label();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(51, 107);
            button1.Name = "button1";
            button1.Size = new Size(141, 40);
            button1.TabIndex = 0;
            button1.Text = "Zgjidh nje folder";
            button1.UseVisualStyleBackColor = true;
            button1.Click += btnSelectFolder_Click;
            // 
            // button2
            // 
            button2.Location = new Point(51, 385);
            button2.Name = "button2";
            button2.Size = new Size(107, 29);
            button2.TabIndex = 1;
            button2.Text = "Kompreso";
            button2.UseVisualStyleBackColor = true;
            button2.Click += btnCompress_Click;
            // 
            // button3
            // 
            button3.Location = new Point(187, 385);
            button3.Name = "button3";
            button3.Size = new Size(113, 29);
            button3.TabIndex = 2;
            button3.Text = "Dekompreso";
            button3.UseVisualStyleBackColor = true;
            button3.Click += btnDecompress_Click;
            // 
            // txtFolderPath
            // 
            txtFolderPath.Location = new Point(51, 33);
            txtFolderPath.Name = "txtFolderPath";
            txtFolderPath.Size = new Size(504, 27);
            txtFolderPath.TabIndex = 3;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(51, 74);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(0, 20);
            lblStatus.TabIndex = 4;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(639, 473);
            Controls.Add(lblStatus);
            Controls.Add(txtFolderPath);
            Controls.Add(button3);
            Controls.Add(button2);
            Controls.Add(button1);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button1;
        private Button button2;
        private Button button3;
        private TextBox txtFolderPath;
        private Label lblStatus;
    }
}
