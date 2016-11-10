namespace PuppetMasterGUI {
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.ConsoleBox = new System.Windows.Forms.TextBox();
            this.Output = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.button3 = new System.Windows.Forms.Button();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.scriptTextBox = new System.Windows.Forms.TextBox();
            this.scriptButton = new System.Windows.Forms.Button();
            this.queueCmdBox = new System.Windows.Forms.TextBox();
            this.labelQueued = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(138, 40);
            this.button1.Margin = new System.Windows.Forms.Padding(2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(111, 19);
            this.button1.TabIndex = 0;
            this.button1.Text = "Run one command";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(253, 40);
            this.button2.Margin = new System.Windows.Forms.Padding(2);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(111, 19);
            this.button2.TabIndex = 1;
            this.button2.Text = "Run all commands";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 11);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Configuration file:";
            // 
            // ConsoleBox
            // 
            this.ConsoleBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ConsoleBox.Location = new System.Drawing.Point(491, 64);
            this.ConsoleBox.Margin = new System.Windows.Forms.Padding(2);
            this.ConsoleBox.Multiline = true;
            this.ConsoleBox.Name = "ConsoleBox";
            this.ConsoleBox.ReadOnly = true;
            this.ConsoleBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.ConsoleBox.Size = new System.Drawing.Size(442, 219);
            this.ConsoleBox.TabIndex = 4;
            // 
            // Output
            // 
            this.Output.AutoSize = true;
            this.Output.Location = new System.Drawing.Point(494, 43);
            this.Output.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.Output.Name = "Output";
            this.Output.Size = new System.Drawing.Size(25, 13);
            this.Output.TabIndex = 5;
            this.Output.Text = "Log";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 43);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(122, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Previously run command";
            // 
            // textBox2
            // 
            this.textBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox2.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.textBox2.Location = new System.Drawing.Point(8, 116);
            this.textBox2.Margin = new System.Windows.Forms.Padding(2);
            this.textBox2.Multiline = true;
            this.textBox2.Name = "textBox2";
            this.textBox2.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox2.Size = new System.Drawing.Size(356, 167);
            this.textBox2.TabIndex = 7;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(8, 64);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(356, 31);
            this.textBox1.TabIndex = 8;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 99);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(83, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Next commands";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(207, 6);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 20);
            this.button3.TabIndex = 10;
            this.button3.Text = "Open file...";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(101, 7);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(100, 20);
            this.textBox3.TabIndex = 11;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(415, 11);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 13);
            this.label4.TabIndex = 12;
            this.label4.Text = "Script file:";
            // 
            // scriptTextBox
            // 
            this.scriptTextBox.Location = new System.Drawing.Point(472, 9);
            this.scriptTextBox.Name = "scriptTextBox";
            this.scriptTextBox.Size = new System.Drawing.Size(100, 20);
            this.scriptTextBox.TabIndex = 13;
            // 
            // scriptButton
            // 
            this.scriptButton.Location = new System.Drawing.Point(578, 6);
            this.scriptButton.Name = "scriptButton";
            this.scriptButton.Size = new System.Drawing.Size(75, 22);
            this.scriptButton.TabIndex = 14;
            this.scriptButton.Text = "Open file...";
            this.scriptButton.UseVisualStyleBackColor = true;
            this.scriptButton.Click += new System.EventHandler(this.scriptButton_Click);
            // 
            // queueCmdBox
            // 
            this.queueCmdBox.BackColor = System.Drawing.SystemColors.Control;
            this.queueCmdBox.Enabled = false;
            this.queueCmdBox.Location = new System.Drawing.Point(371, 64);
            this.queueCmdBox.Multiline = true;
            this.queueCmdBox.Name = "queueCmdBox";
            this.queueCmdBox.Size = new System.Drawing.Size(115, 219);
            this.queueCmdBox.TabIndex = 15;
            // 
            // labelQueued
            // 
            this.labelQueued.AutoSize = true;
            this.labelQueued.Location = new System.Drawing.Point(371, 44);
            this.labelQueued.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelQueued.Name = "labelQueued";
            this.labelQueued.Size = new System.Drawing.Size(100, 13);
            this.labelQueued.TabIndex = 16;
            this.labelQueued.Text = "Queued Commands";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(944, 298);
            this.Controls.Add(this.labelQueued);
            this.Controls.Add(this.queueCmdBox);
            this.Controls.Add(this.scriptButton);
            this.Controls.Add(this.scriptTextBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBox3);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.Output);
            this.Controls.Add(this.ConsoleBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Form1";
            this.Text = "Puppet Master";
            this.Load += new System.EventHandler(this.FormPuppetMaster_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox ConsoleBox;
        private System.Windows.Forms.Label Output;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox scriptTextBox;
        private System.Windows.Forms.Button scriptButton;
        private System.Windows.Forms.TextBox queueCmdBox;
        private System.Windows.Forms.Label labelQueued;
    }
}

