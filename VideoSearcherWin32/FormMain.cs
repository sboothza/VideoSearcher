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

        private string _filename;

        // private readonly EsVideoRepository _esRepo;
        private readonly bool _useMysql;
        private readonly ElasticClient _client;

        public FormMain()
        {
            InitializeComponent();
            var settings = Program.Config.GetSection("Settings");
            var pwd = Directory.GetCurrentDirectory();
            _cacheDirectory = Path.Combine(pwd, "cache");
            Directory.CreateDirectory(_cacheDirectory);
            _rootDirectory = settings.GetSection("RootFolder").Value;
            _useMysql = bool.Parse(settings.GetSection("UseMySql").Value);

            if (_useMysql)
            {
                _repo = new VideoRepository(settings.GetSection("ConnectionString").Value);
                comboFields.Items.AddRange(_repo.GetFields().ToArray());
                comboFields.SelectedIndex = 0;
                Text = "Video Searcher - using MySql DB";
            }
            else
            {
                _client = new ElasticClient(settings.GetSection("esHost").Value,
                    settings.GetSection("esUsername").Value, settings.GetSection("esPassword").Value,
                    settings.GetSection("esIndex").Value);
                Console.WriteLine(_client.Test());
                // var queryString = "{ \"query\": { \"multi_match\": { \"query\": \"braces facial\", \"fields\": [ \"tags\", \"uploader\", \"description\" ], \"operator\": \"and\" } }, \"size\": 20, \"from\": 0, \"sort\": [ { \"duration\": { \"order\": \"desc\" } } ]}";
                // var result = client.Search<EsVideo>(queryString);

                // _esRepo = new EsVideoRepository(settings.GetSection("esHost").Value, settings.GetSection("esUsername").Value, settings.GetSection("esPassword").Value, settings.GetSection("esIndex").Value);
                // var response = _esRepo.Get(1960);
                // Console.WriteLine(response?.Id);
                // var results = _esRepo.Search("panties", 10);
                // foreach (var result in results)
                // {
                //     Console.WriteLine(result.Id);
                // }
                Text = "Video Searcher - using Elasticsearch - some fields are disabled";
                textBoxSearch2.Enabled = false;
                textBoxSearch3.Enabled = false;
                comboFields.Items.AddRange(GetFields().ToArray());
                comboFields.SelectedIndex = 0;
                // comboSize.Enabled = false;
                textBoxUploader.Enabled = false;
                // comboDurationDirection.Enabled = false;
                // comboDuration.Enabled = false;
                menuDelete.Enabled = false;
            }
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
                if (_useMysql)
                {
                    data = _repo.GetSearch(textBoxSearch.Text, textBoxSearch2.Text, textBoxSearch3.Text,
                        textBoxUploader.Text, comboFields.Text, comboAsc.Text == "Ascending",
                        int.Parse(textBoxLimit.Text),
                        int.Parse(comboSize.Text), comboDurationDirection.Text, comboDuration.Text);
                }
                else
                {
                    //data = _esRepo.Search(textBoxSearch.Text.Trim(), int.Parse(textBoxLimit.Text), comboFields.Text.Trim(), comboAsc.Text == "Ascending");
                    var ascDesc = comboAsc.Text == "Ascending" ? "asc" : "desc";
                    // var queryString = "{ \"query\": { \"multi_match\": { \"query\": \"" + textBoxSearch.Text.Trim() +
                    //                   "\", \"fields\": [ \"tags\", \"uploader\", \"description\" ], \"operator\": \"and\" } }, \"size\": " +
                    //                   textBoxLimit.Text + ", \"from\": 0, \"sort\": [ { \"" + comboFields.Text +
                    //                   "\": { \"order\": \"" + ascDesc + "\" } } ]}";

                  

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


                    var queryString =
                        "{\"query\":{\"bool\":{\"must\":[{\"multi_match\":{\"query\":\"%query_terms%\",\"fields\":[\"tags\",\"uploader\",\"description\"],\"operator\":\"and\"}},{\"range\":{\"duration\":{\"%duration_op%\":%duration%}}},{\"range\":{\"height\":{\"%height_op%\":%height%}}}]}},\"size\":%count%,\"from\":0,\"sort\":[{\"%sort_term%\":{\"order\":\"%sort_order%\"}}]}";

                    queryString = queryString.Replace("%query_terms%", textBoxSearch.Text);
                    queryString = queryString.Replace("%duration_op%", durationOp);
                    queryString = queryString.Replace("%duration%", durationLimit);
                    queryString = queryString.Replace("%height_op%", "gte");
                    queryString = queryString.Replace("%height%", comboSize.Text);
                    queryString = queryString.Replace("%count%", textBoxLimit.Text);
                    queryString = queryString.Replace("%sort_term%", comboFields.Text);
                    queryString = queryString.Replace("%sort_order%", ascDesc);

                    var tempData = _client.Search<EsVideo>(queryString);
                    data = tempData.Select(v => new Video(v)).ToList();
                }

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
                    var filename = Path.Combine(Path.Combine(_rootDirectory, $"{item.NormalizedUploader}"),
                        $"{item.Filename}");
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
                    imageViewer.Tag = filename;
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
                    var filename = ((PictureBox)sender).Tag.ToString();
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
                _filename = ((PictureBox)sender).Tag.ToString();
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
            var result = MessageBox.Show($"Are you sure you want to delete the file [{_filename}]",
                "Delete File", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                var file = Path.GetFileName(_filename);
                _repo.Delete(file, _rootDirectory);
            }
        }


        private void imageMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _filename = (sender as ContextMenuStrip).SourceControl.Tag.ToString();
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
                FileName = Path.GetFileName(_filename)
            };
            var result = dlg.ShowDialog();
            if (result.Equals(DialogResult.OK))
            {
                Cursor.Current = Cursors.WaitCursor;
                labelStatus.Text = "Copying file...";
                Application.DoEvents();
                try
                {
                    File.Copy(_filename, dlg.FileName);
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