using System.Diagnostics;

using VideoSearcher;

namespace VideoSearcherWin32
{
    public partial class Form1 : Form
    {
        private readonly VideoRepository _repo;
        private readonly string _cacheDirectory;

        public Form1()
        {
            InitializeComponent();
            _repo = new VideoRepository(Program.Config);
            comboFields.Items.AddRange(_repo.GetFields().ToArray() as object[]);
            comboFields.SelectedIndex = 0;
            var pwd = Directory.GetCurrentDirectory();
            _cacheDirectory = Path.Combine(pwd, "cache");
            Directory.CreateDirectory(_cacheDirectory);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var data = _repo.GetSearch(textBoxSearch.Text, textBoxSearch2.Text, textBoxSearch3.Text, comboFields.Text, comboAsc.Text == "Ascending", int.Parse(textBoxLimit.Text), int.Parse(comboSize.Text));
            var f = new Font("Arial", 8);
            flowLayoutPanel1.Controls.Clear();

            foreach (var item in data)
            {
                var thumbnail = new Panel();
                thumbnail.Width = 128;
                thumbnail.Height = 160;

                var imageViewer = new PictureBox();
                var uploader = item.Uploader == "" ? "NA" : item.Uploader;
                var filename = $"Z:\\m\\{uploader.Replace(" ", "_")}\\{item.Filename}";
                var thumb = GetThumbnail(filename, 5);
                if (thumb is not null)
                {
                    try
                    {
                        imageViewer.Image = Image.FromStream(new MemoryStream(thumb));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
                imageViewer.SizeMode = PictureBoxSizeMode.Zoom;
                imageViewer.Top = 0;
                imageViewer.Left = 0;
                imageViewer.Height = 70;
                imageViewer.Width = 128;
                //imageViewer.BorderStyle = BorderStyle.FixedSingle;
                imageViewer.Click += ImageViewer_Click;
                imageViewer.Tag = filename;
                var toolTip = new ToolTip();
                var tt = item.Description == "N/A" ? item.Filename : item.Description;
                toolTip.SetToolTip(imageViewer, item.Description);
                thumbnail.Controls.Add(imageViewer);

                AddLabel(thumbnail, item.Filename, 70, f);
                AddLabel(thumbnail, item.Description, 90, f);
                AddLabel(thumbnail, FormatTime(item.Duration), 110, f);

                flowLayoutPanel1.Controls.Add(thumbnail);
            }
        }

        private void AddTextbox(Panel container, string text, int top, Font f)
        {
            var textBox = new TextBox
            {
                Text = text,
                Font = f,
                Left = 0,
                Top = top,
                Width = 128
            };
            //textBox.BorderStyle = BorderStyle.FixedSingle;
            container.Controls.Add(textBox);
        }

        private void AddLabel(Panel container, string text, int top, Font f)
        {
            var label = new Label
            {
                Text = text,
                Font = f,
                Left = 0,
                Top = top,
                Width = 128
            };
            //textBox.BorderStyle = BorderStyle.FixedSingle;
            container.Controls.Add(label);
        }

        public string FormatTime(int seconds)
        {
            var ts = TimeSpan.FromSeconds(seconds);
            return ts.ToString("hh\\:mm\\:ss");
        }

        private void ImageViewer_Click(object? sender, EventArgs e)
        {
            string filename = ((PictureBox)sender).Tag.ToString();
            Process process = new Process();
            // set the process start info
            process.StartInfo.FileName = filename;
            process.StartInfo.UseShellExecute = true;
            process.Start();
            process.WaitForExit();
        }

        private byte[] GetThumbnail(string filename, int offset)
        {
            try
            {
                var actualFilename = Path.GetFileName(filename);
                var cacheFilename = Path.Combine(_cacheDirectory, actualFilename) + ".jpg";
                if (File.Exists(cacheFilename))
                {
                    using (FileStream fs = new FileStream(cacheFilename, FileMode.Open))
                    {
                        var buf = new byte[fs.Length];
                        fs.Read(buf, 0, buf.Length);
                        fs.Close();
                        return buf;
                    }
                }
                else
                {
                    using (MemoryStream ms = new())
                    {
                        var ffMpeg = new NReco.VideoConverter.FFMpegConverter();
                        ffMpeg.GetVideoThumbnail(filename, ms, offset);
                        ms.Seek(0, SeekOrigin.Begin);
                        var buf = new byte[ms.Length];
                        ms.Read(buf, 0, buf.Length);

                        using (FileStream fs = new FileStream(cacheFilename, FileMode.Create))
                        {
                            fs.Write(buf, 0, buf.Length);
                            fs.Close();
                        }

                        ms.Close();
                        return buf;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }
    }
}