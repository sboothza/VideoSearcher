using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Google.Protobuf.WellKnownTypes;
using VideoCommon;
using VideoSearcher;

namespace VideoSearcherWin32
{
    public partial class FormMain : Form
    {
        private readonly VideoRepository _repo;
        private readonly string _cacheDirectory;
        private readonly string _rootDirectory;

        private Video _video;

        // private readonly EsVideoRepository _esRepo;
        // private readonly bool _useMysql;
        private readonly ElasticClient _client;

        public FormMain()
        {
            InitializeComponent();
            var settings = Program.Config.GetSection("Settings");
            var pwd = Directory.GetCurrentDirectory();
            _cacheDirectory = Path.Combine(pwd, "cache");
            Directory.CreateDirectory(_cacheDirectory);
            _rootDirectory = settings.GetSection("RootFolder").Value;

            _repo = new VideoRepository(settings.GetSection("ConnectionString").Value);

            _client = new ElasticClient(settings.GetSection("esHost").Value,
                settings.GetSection("esUsername").Value, settings.GetSection("esPassword").Value,
                settings.GetSection("esIndex").Value);
            Console.WriteLine(_client.Test());

            Text = "Video Searcher - using Elasticsearch and PostgreSql";

            comboFields.Items.AddRange(GetFields().ToArray());
            comboFields.SelectedIndex = 0;
        }

        private List<string> GetFields()
        {
            return new List<string>()
            {
                "size",
                "duration",
                "width",
                "height",
                "insertdate",
                "downloaddate"
            };
        }

        private void buttonSearch_Click(object sender, EventArgs e)
        {
            labelStatus.Text = "Starting Search...";
            Application.DoEvents();
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                var start = DateTime.Now;

                List<Video> data;
                var ascDesc = comboAsc.Text == "Ascending" ? "asc" : "desc";

                var durationOp = comboDurationDirection.Text switch
                {
                    "N/A" => "lte",
                    "<=" => "lte",
                    ">=" => "gte",
                    _ => "lte"
                };

                if (comboDurationDirection.Text == "N/A")
                    comboDuration.Text = "N/A";

                var durationLimit = comboDuration.Text switch
                {
                    "60 min" => (60 * 60).ToString(),
                    "45 min" => (45 * 60).ToString(),
                    "30 min" => (30 * 60).ToString(),
                    "15 min" => (15 * 60).ToString(),
                    "5 min" => (5 * 60).ToString(),
                    _ => (60 * 60 * 24).ToString()
                };

                var queryText = "";
                if (!string.IsNullOrWhiteSpace(textBoxSearch.Text))
                    queryText = "{\"multi_match\":{\"query\":\"%query_terms%\",\"fields\":[\"tags\",\"uploader\",\"description\"],\"operator\":\"and\"}},";

                var queryString =
                    "{\"query\":{\"bool\":{\"must\":[%query_text%{\"range\":{\"duration\":{\"%duration_op%\":%duration%}}},{\"range\":{\"height\":{\"%height_op%\":%height%}}}%uploader%]}},\"size\":%count%,\"from\":0,\"sort\":[{\"%sort_term%\":{\"order\":\"%sort_order%\"}}]}";

                queryString = queryString.Replace("%query_text%", queryText);
                queryString = queryString.Replace("%query_terms%", textBoxSearch.Text);
                queryString = queryString.Replace("%duration_op%", durationOp);
                queryString = queryString.Replace("%duration%", durationLimit);
                queryString = queryString.Replace("%height_op%", "gte");
                queryString = queryString.Replace("%height%", comboSize.Text);
                queryString = queryString.Replace("%count%", textBoxLimit.Text);
                queryString = queryString.Replace("%sort_term%", comboFields.Text);
                queryString = queryString.Replace("%sort_order%", ascDesc);

                var uploader = "";
                if (!string.IsNullOrWhiteSpace(textBoxUploader.Text))
                {
                    uploader = ",{\"match\": {\"uploader\": {\"query\": \"%uploader%\"}}}".Replace("%uploader%", textBoxUploader.Text);
                }
                queryString = queryString.Replace("%uploader%", uploader);

                var tempData = _client.Search<EsVideo>(queryString);
                data = tempData.Select(v => new Video(v)).ToList();

                var searchTime = DateTime.Now - start;
                labelStatus.Text = $"Search: {searchTime:mm\\:ss\\:ff}, Populating thumbnails - 0 of {data.Count}";
                Application.DoEvents();
                var f = new Font("Arial", 8);
                flowLayoutPanel.Controls.Clear();

                start = DateTime.Now;
                int counter = 0;
                foreach (var item in data)
                {
                    counter++;
                    var thumbnail = new Panel();
                    thumbnail.Width = 128;
                    thumbnail.Height = 160;

                    var imageViewer = new PictureBox();
                    var filename = Path.Combine(Path.Combine(_rootDirectory, $"{item.NormalizedUploader}"), $"{item.Filename}");
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
                    imageViewer.Click += ImageViewer_Click;
                    imageViewer.ContextMenuStrip = imageMenu;
                    imageViewer.Tag = item;
                    var toolTip = new ToolTip();
                    var tt = item.Description == "N/A" ? item.Filename : item.Description;
                    toolTip.SetToolTip(imageViewer, item.Description);
                    thumbnail.Controls.Add(imageViewer);

                    AddLabel(thumbnail, item.Uploader, 70, f);
                    AddLabel(thumbnail, item.Description, 90, f);
                    AddLabel(thumbnail, FormatTime(item.Duration), 110, f);

                    flowLayoutPanel.Controls.Add(thumbnail);
                    labelStatus.Text =
                        $"Search: {searchTime:mm\\:ss\\:ff}, Populating thumbnails - {counter} of {data.Count}";
                    Application.DoEvents();
                }

                var populateTime = DateTime.Now - start;
                labelStatus.Text =
                    $"Ready Search: {searchTime:mm\\:ss\\:ff}, Populating {data.Count} thumbnails: {populateTime:mm\\:ss\\:ff}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                labelStatus.Text = $"Ready";
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }


        private void AddLabel(Panel container, string text, int top, Font f)
        {
            var label = new Label
            {
                Text = text,
                Font = f,
                Left = 0,
                Top = top,
                Width = 128,
                ForeColor = Color.White
            };
            container.Controls.Add(label);
        }

        public string FormatTime(int seconds)
        {
            var ts = TimeSpan.FromSeconds(seconds);
            return ts.ToString("hh\\:mm\\:ss");
        }

        private void ImageViewer_Click(object? sender, EventArgs e)
        {
            var args = e as MouseEventArgs;
            if (args.Button == MouseButtons.Left)
            {
                try
                {
                    var video = ((PictureBox)sender).Tag as Video;
                    var filename = Path.Combine(Path.Combine(_rootDirectory, $"{video.NormalizedUploader}"), $"{video.Filename}");
                    var process = new Process();
                    process.StartInfo.FileName = filename;
                    process.StartInfo.UseShellExecute = true;
                    process.Start();
                    process.WaitForExit();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            else if (args.Button == MouseButtons.Right)
            {
                _video = ((PictureBox)sender).Tag as Video;
            }
        }

        private byte[]? GetThumbnail(string filename, int offset)
        {
            try
            {
                var actualFilename = Path.GetFileName(filename);
                var cacheFilename = Path.Combine(_cacheDirectory, actualFilename) + ".jpg";
                if (File.Exists(cacheFilename))
                {
                    using (var fs = new FileStream(cacheFilename, FileMode.Open))
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

        private void test1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show($"Are you sure you want to delete the file [{_video.Filename}]",
                "Delete File", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                Cursor.Current = Cursors.WaitCursor;
                labelStatus.Text = "Deleting file from disk and db...";
                _repo.Delete(_video.Filename, _rootDirectory);
                _client.Delete(_video.Id.ToString());
                Cursor.Current= Cursors.Default;
                labelStatus.Text = "Deleting file from disk and db - Done";
            }
        }


        private void imageMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _video = (sender as ContextMenuStrip).SourceControl.Tag as Video;
        }

        private void buttonClearCache_Click(object sender, EventArgs e)
        {
            var files = Directory.GetFiles(_cacheDirectory);
            DialogResult result = MessageBox.Show(
                $"{files.Length} cache files found in {_cacheDirectory}.  Would you like to delete them all?",
                "Confirm Delete", MessageBoxButtons.YesNo);
            var errors = new StringBuilder(1024);
            if (result == DialogResult.Yes)
            {
                foreach (var file in files)
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch
                    {
                        errors.AppendLine(file);
                    }
                }

                var message = errors.Length > 0
                    ? $"The following files failed delete: {errors.ToString()}"
                    : "No errors found";
                MessageBox.Show(message, "Cache Delete Result", MessageBoxButtons.OK);
            }
        }

        private void menuCopy_Click(object sender, EventArgs e)
        {
            var dlg = new SaveFileDialog
            {
                CheckPathExists = true,
                OverwritePrompt = true,
                FileName = Path.GetFileName(_video.Filename)
            };
            var result = dlg.ShowDialog();
            if (result.Equals(DialogResult.OK))
            {
                Cursor.Current = Cursors.WaitCursor;
                labelStatus.Text = "Copying file...";
                Application.DoEvents();
                try
                {
                    File.Copy(_video.Filename, dlg.FileName);
                    labelStatus.Text = "Copy successful";
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }
            }
        }

        private void label8_Click(object sender, EventArgs e)
        {
        }
    }
}