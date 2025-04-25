namespace HoneyOS
{
    partial class WelcomeScreen
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WelcomeScreen));
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.TransparentButton = new System.Windows.Forms.Button();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // timer1
            // 
            this.timer1.Interval = 3500;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // TransparentButton
            // 
            this.TransparentButton.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
            this.TransparentButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.TransparentButton.AutoSize = true;
            this.TransparentButton.BackColor = System.Drawing.Color.Transparent;
            this.TransparentButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.TransparentButton.FlatAppearance.BorderSize = 0;
            this.TransparentButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.TransparentButton.ForeColor = System.Drawing.SystemColors.Control;
            this.TransparentButton.Image = ((System.Drawing.Image)(resources.GetObject("TransparentButton.Image")));
            this.TransparentButton.Location = new System.Drawing.Point(371, 359);
            this.TransparentButton.Margin = new System.Windows.Forms.Padding(0);
            this.TransparentButton.Name = "TransparentButton";
            this.TransparentButton.Size = new System.Drawing.Size(86, 88);
            this.TransparentButton.TabIndex = 1;
            this.TransparentButton.UseCompatibleTextRendering = true;
            this.TransparentButton.UseVisualStyleBackColor = false;
            this.TransparentButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // pictureBox2
            // 
            this.pictureBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox2.Image = global::HoneyOS.Properties.Resources.Welcome_Screen_Static_;
            this.pictureBox2.InitialImage = global::HoneyOS.Properties.Resources.Welcome_Screen1;
            this.pictureBox2.Location = new System.Drawing.Point(0, 0);
            this.pictureBox2.Margin = new System.Windows.Forms.Padding(2);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(817, 469);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox2.TabIndex = 2;
            this.pictureBox2.TabStop = false;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Image = global::HoneyOS.Properties.Resources.Welcome_Screen1;
            this.pictureBox1.InitialImage = global::HoneyOS.Properties.Resources.Welcome_Screen1;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(2);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(817, 469);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 5;
            this.pictureBox1.TabStop = false;
            // 
            // WelcomeScreen
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(817, 469);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.TransparentButton);
            this.Controls.Add(this.pictureBox2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "WelcomeScreen";
            this.Text = "WelcomeScreen";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.Form2_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button TransparentButton;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}