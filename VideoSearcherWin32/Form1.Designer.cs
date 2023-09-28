namespace VideoSearcherWin32
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
            panel1 = new Panel();
            comboSize = new ComboBox();
            label5 = new Label();
            label4 = new Label();
            textBoxSearch2 = new TextBox();
            textBoxLimit = new TextBox();
            label3 = new Label();
            comboAsc = new ComboBox();
            comboFields = new ComboBox();
            label2 = new Label();
            textBoxSearch = new TextBox();
            label1 = new Label();
            button1 = new Button();
            flowLayoutPanel1 = new FlowLayoutPanel();
            label6 = new Label();
            textBoxSearch3 = new TextBox();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Controls.Add(label6);
            panel1.Controls.Add(textBoxSearch3);
            panel1.Controls.Add(comboSize);
            panel1.Controls.Add(label5);
            panel1.Controls.Add(label4);
            panel1.Controls.Add(textBoxSearch2);
            panel1.Controls.Add(textBoxLimit);
            panel1.Controls.Add(label3);
            panel1.Controls.Add(comboAsc);
            panel1.Controls.Add(comboFields);
            panel1.Controls.Add(label2);
            panel1.Controls.Add(textBoxSearch);
            panel1.Controls.Add(label1);
            panel1.Controls.Add(button1);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(912, 110);
            panel1.TabIndex = 0;
            // 
            // comboSize
            // 
            comboSize.FormattingEnabled = true;
            comboSize.Items.AddRange(new object[] { "480", "720", "1080" });
            comboSize.Location = new Point(491, 44);
            comboSize.Name = "comboSize";
            comboSize.Size = new Size(139, 23);
            comboSize.TabIndex = 10;
            comboSize.Text = "1080";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(402, 47);
            label5.Name = "label5";
            label5.Size = new Size(83, 15);
            label5.TabIndex = 9;
            label5.Text = "Minimum Size";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(30, 44);
            label4.Name = "label4";
            label4.Size = new Size(20, 15);
            label4.TabIndex = 8;
            label4.Text = "Or";
            // 
            // textBoxSearch2
            // 
            textBoxSearch2.Location = new Point(119, 41);
            textBoxSearch2.Name = "textBoxSearch2";
            textBoxSearch2.Size = new Size(199, 23);
            textBoxSearch2.TabIndex = 7;
            // 
            // textBoxLimit
            // 
            textBoxLimit.Location = new Point(836, 11);
            textBoxLimit.Name = "textBoxLimit";
            textBoxLimit.Size = new Size(60, 23);
            textBoxLimit.TabIndex = 6;
            textBoxLimit.Text = "20";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(796, 14);
            label3.Name = "label3";
            label3.Size = new Size(34, 15);
            label3.TabIndex = 5;
            label3.Text = "Limit";
            // 
            // comboAsc
            // 
            comboAsc.FormattingEnabled = true;
            comboAsc.Items.AddRange(new object[] { "Ascending", "Descending" });
            comboAsc.Location = new Point(655, 11);
            comboAsc.Name = "comboAsc";
            comboAsc.Size = new Size(121, 23);
            comboAsc.TabIndex = 4;
            comboAsc.Text = "Descending";
            // 
            // comboFields
            // 
            comboFields.FormattingEnabled = true;
            comboFields.Location = new Point(491, 11);
            comboFields.Name = "comboFields";
            comboFields.Size = new Size(139, 23);
            comboFields.TabIndex = 3;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(432, 14);
            label2.Name = "label2";
            label2.Size = new Size(53, 15);
            label2.TabIndex = 2;
            label2.Text = "Order By";
            // 
            // textBoxSearch
            // 
            textBoxSearch.Location = new Point(119, 12);
            textBoxSearch.Name = "textBoxSearch";
            textBoxSearch.Size = new Size(199, 23);
            textBoxSearch.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(30, 15);
            label1.Name = "label1";
            label1.Size = new Size(83, 15);
            label1.TabIndex = 1;
            label1.Text = "Search Criteria";
            // 
            // button1
            // 
            button1.Location = new Point(821, 69);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 1;
            button1.Text = "Search";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoScroll = true;
            flowLayoutPanel1.Dock = DockStyle.Fill;
            flowLayoutPanel1.Location = new Point(0, 110);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(912, 340);
            flowLayoutPanel1.TabIndex = 1;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(30, 73);
            label6.Name = "label6";
            label6.Size = new Size(20, 15);
            label6.TabIndex = 12;
            label6.Text = "Or";
            // 
            // textBoxSearch3
            // 
            textBoxSearch3.Location = new Point(119, 70);
            textBoxSearch3.Name = "textBoxSearch3";
            textBoxSearch3.Size = new Size(199, 23);
            textBoxSearch3.TabIndex = 11;
            // 
            // Form1
            // 
            AcceptButton = button1;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(912, 450);
            Controls.Add(flowLayoutPanel1);
            Controls.Add(panel1);
            Name = "Form1";
            Text = "Video Searcher";
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private FlowLayoutPanel flowLayoutPanel1;
        private Button button1;
        private TextBox textBoxSearch;
        private Label label1;
        private ComboBox comboFields;
        private Label label2;
        private ComboBox comboAsc;
        private TextBox textBoxLimit;
        private Label label3;
        private Label label4;
        private TextBox textBoxSearch2;
        private ComboBox comboSize;
        private Label label5;
        private Label label6;
        private TextBox textBoxSearch3;
    }
}