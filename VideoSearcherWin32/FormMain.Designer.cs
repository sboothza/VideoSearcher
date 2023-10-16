namespace VideoSearcherWin32
{
    partial class FormMain
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            panelHeader = new Panel();
            comboDuration = new ComboBox();
            label9 = new Label();
            comboDurationDirection = new ComboBox();
            label8 = new Label();
            buttonClearCache = new Button();
            textBoxUploader = new TextBox();
            label7 = new Label();
            label6 = new Label();
            textBoxSearch3 = new TextBox();
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
            buttonSearch = new Button();
            flowLayoutPanel = new FlowLayoutPanel();
            imageMenu = new ContextMenuStrip(components);
            menuDelete = new ToolStripMenuItem();
            menuCopy = new ToolStripMenuItem();
            statusStrip = new StatusStrip();
            labelStatus = new ToolStripStatusLabel();
            panelHeader.SuspendLayout();
            imageMenu.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // panelHeader
            // 
            panelHeader.BackColor = SystemColors.Desktop;
            panelHeader.Controls.Add(comboDuration);
            panelHeader.Controls.Add(label9);
            panelHeader.Controls.Add(comboDurationDirection);
            panelHeader.Controls.Add(label8);
            panelHeader.Controls.Add(buttonClearCache);
            panelHeader.Controls.Add(textBoxUploader);
            panelHeader.Controls.Add(label7);
            panelHeader.Controls.Add(label6);
            panelHeader.Controls.Add(textBoxSearch3);
            panelHeader.Controls.Add(comboSize);
            panelHeader.Controls.Add(label5);
            panelHeader.Controls.Add(label4);
            panelHeader.Controls.Add(textBoxSearch2);
            panelHeader.Controls.Add(textBoxLimit);
            panelHeader.Controls.Add(label3);
            panelHeader.Controls.Add(comboAsc);
            panelHeader.Controls.Add(comboFields);
            panelHeader.Controls.Add(label2);
            panelHeader.Controls.Add(textBoxSearch);
            panelHeader.Controls.Add(label1);
            panelHeader.Controls.Add(buttonSearch);
            panelHeader.Dock = DockStyle.Top;
            panelHeader.Location = new Point(0, 0);
            panelHeader.Name = "panelHeader";
            panelHeader.Size = new Size(912, 110);
            panelHeader.TabIndex = 0;
            // 
            // comboDuration
            // 
            comboDuration.FormattingEnabled = true;
            comboDuration.Items.AddRange(new object[] { "N/A", "60 min", "45 min", "30 min", "15 min", "5 min" });
            comboDuration.Location = new Point(712, 42);
            comboDuration.Name = "comboDuration";
            comboDuration.Size = new Size(79, 23);
            comboDuration.TabIndex = 19;
            comboDuration.Text = "60 min";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.ForeColor = SystemColors.ControlLight;
            label9.Location = new Point(587, 15);
            label9.Name = "label9";
            label9.Size = new Size(28, 15);
            label9.TabIndex = 18;
            label9.Text = "Sort";
            // 
            // comboDurationDirection
            // 
            comboDurationDirection.FormattingEnabled = true;
            comboDurationDirection.Items.AddRange(new object[] { "<=", ">=", "N/A" });
            comboDurationDirection.Location = new Point(655, 42);
            comboDurationDirection.Name = "comboDurationDirection";
            comboDurationDirection.Size = new Size(52, 23);
            comboDurationDirection.TabIndex = 17;
            comboDurationDirection.Text = "N/A";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.ForeColor = SystemColors.ControlLight;
            label8.Location = new Point(587, 44);
            label8.Name = "label8";
            label8.Size = new Size(53, 15);
            label8.TabIndex = 16;
            label8.Text = "Duration";
            label8.Click += label8_Click;
            // 
            // buttonClearCache
            // 
            buttonClearCache.Location = new Point(655, 69);
            buttonClearCache.Name = "buttonClearCache";
            buttonClearCache.Size = new Size(96, 23);
            buttonClearCache.TabIndex = 15;
            buttonClearCache.Text = "Clear Cache";
            buttonClearCache.UseVisualStyleBackColor = true;
            buttonClearCache.Click += buttonClearCache_Click;
            // 
            // textBoxUploader
            // 
            textBoxUploader.Location = new Point(423, 70);
            textBoxUploader.Name = "textBoxUploader";
            textBoxUploader.Size = new Size(139, 23);
            textBoxUploader.TabIndex = 14;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.ForeColor = SystemColors.ControlLight;
            label7.Location = new Point(333, 76);
            label7.Name = "label7";
            label7.Size = new Size(55, 15);
            label7.TabIndex = 13;
            label7.Text = "Uploader";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.ForeColor = SystemColors.ControlLight;
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
            // comboSize
            // 
            comboSize.FormattingEnabled = true;
            comboSize.Items.AddRange(new object[] { "240", "480", "720", "1080" });
            comboSize.Location = new Point(423, 42);
            comboSize.Name = "comboSize";
            comboSize.Size = new Size(139, 23);
            comboSize.TabIndex = 10;
            comboSize.Text = "1080";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.ForeColor = SystemColors.ControlLight;
            label5.Location = new Point(333, 48);
            label5.Name = "label5";
            label5.Size = new Size(83, 15);
            label5.TabIndex = 9;
            label5.Text = "Minimum Size";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.ForeColor = SystemColors.ControlLight;
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
            label3.ForeColor = SystemColors.ControlLight;
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
            comboAsc.Size = new Size(136, 23);
            comboAsc.TabIndex = 4;
            comboAsc.Text = "Descending";
            // 
            // comboFields
            // 
            comboFields.FormattingEnabled = true;
            comboFields.Location = new Point(423, 12);
            comboFields.Name = "comboFields";
            comboFields.Size = new Size(139, 23);
            comboFields.TabIndex = 3;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.ForeColor = SystemColors.ControlLight;
            label2.Location = new Point(359, 15);
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
            label1.ForeColor = SystemColors.ControlLight;
            label1.Location = new Point(30, 15);
            label1.Name = "label1";
            label1.Size = new Size(83, 15);
            label1.TabIndex = 1;
            label1.Text = "Search Criteria";
            // 
            // buttonSearch
            // 
            buttonSearch.Location = new Point(821, 69);
            buttonSearch.Name = "buttonSearch";
            buttonSearch.Size = new Size(75, 23);
            buttonSearch.TabIndex = 1;
            buttonSearch.Text = "Search";
            buttonSearch.UseVisualStyleBackColor = true;
            buttonSearch.Click += buttonSearch_Click;
            // 
            // flowLayoutPanel
            // 
            flowLayoutPanel.AutoScroll = true;
            flowLayoutPanel.BackColor = SystemColors.Desktop;
            flowLayoutPanel.Dock = DockStyle.Fill;
            flowLayoutPanel.Location = new Point(0, 110);
            flowLayoutPanel.Name = "flowLayoutPanel";
            flowLayoutPanel.Size = new Size(912, 340);
            flowLayoutPanel.TabIndex = 1;
            // 
            // imageMenu
            // 
            imageMenu.ImageScalingSize = new Size(20, 20);
            imageMenu.Items.AddRange(new ToolStripItem[] { menuDelete, menuCopy });
            imageMenu.Name = "contextMenuStrip1";
            imageMenu.Size = new Size(108, 48);
            imageMenu.Opening += imageMenu_Opening;
            // 
            // menuDelete
            // 
            menuDelete.Name = "menuDelete";
            menuDelete.Size = new Size(107, 22);
            menuDelete.Text = "Delete";
            menuDelete.Click += test1ToolStripMenuItem_Click;
            // 
            // menuCopy
            // 
            menuCopy.Name = "menuCopy";
            menuCopy.Size = new Size(107, 22);
            menuCopy.Text = "Copy";
            menuCopy.Click += menuCopy_Click;
            // 
            // statusStrip
            // 
            statusStrip.ImageScalingSize = new Size(20, 20);
            statusStrip.Items.AddRange(new ToolStripItem[] { labelStatus });
            statusStrip.Location = new Point(0, 428);
            statusStrip.Name = "statusStrip";
            statusStrip.Padding = new Padding(1, 0, 12, 0);
            statusStrip.Size = new Size(912, 22);
            statusStrip.TabIndex = 2;
            statusStrip.Text = "statusStrip1";
            // 
            // labelStatus
            // 
            labelStatus.Name = "labelStatus";
            labelStatus.Size = new Size(39, 17);
            labelStatus.Text = "Ready";
            // 
            // FormMain
            // 
            AcceptButton = buttonSearch;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(912, 450);
            Controls.Add(statusStrip);
            Controls.Add(flowLayoutPanel);
            Controls.Add(panelHeader);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "FormMain";
            Text = "Video Searcher";
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            imageMenu.ResumeLayout(false);
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel panelHeader;
        private FlowLayoutPanel flowLayoutPanel;
        private Button buttonSearch;
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
        private ContextMenuStrip imageMenu;
        private ToolStripMenuItem menuDelete;
        private TextBox textBoxUploader;
        private Label label7;
        private Button buttonClearCache;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel labelStatus;
        private ToolStripMenuItem menuCopy;
        private Label label8;
        private Label label9;
        private ComboBox comboDurationDirection;
        private ComboBox comboDuration;
    }
}