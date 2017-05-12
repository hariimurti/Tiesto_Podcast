using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Drawing;
using Newtonsoft.Json;

namespace Tiesto.Podcast
{
    public partial class FormMain : Form
    {
        private static string PODCAST_LIST_URL = "http://tiestoapi-env-prod.elasticbeanstalk.com/podcasts?from=0&size=15";
        private static string PODCAST_TRACK_URL = "http://tiestoapi-env-prod.elasticbeanstalk.com/podcast/$id/podcastTracks";
        private static bool isListNotEmpty = false;
        private LocalData localdata;

        public FormMain()
        {
            InitializeComponent();
            InitializeListView();
            localdata = new LocalData();
        }

        private void FormMain_Shown(object sender, EventArgs e)
        {
            if (File.Exists(localdata.PathIni))
            {
                Download.Folder = File.ReadAllText(localdata.PathIni);
                textBox1.Text = Download.Folder;
            }
            else
            {
                Download.Folder = Application.StartupPath;
                textBox1.Text = Download.Folder;
            }

            checkBox1.Checked = localdata.SaveAsCue;
            checkBox2.Checked = localdata.SaveAsTxt;

            button1_Click(sender, e);
        }

        private void InitializeListView()
        {
            listView1.GridLines = true;
            listView1.BackColor = Color.SkyBlue;
            listView1.FullRowSelect = true;
            listView1.Activation = ItemActivation.Standard;
            listView1.View = View.Details;
            listView1.Columns.Add("ID", 0, HorizontalAlignment.Left);
            listView1.Columns.Add("Start", 50, HorizontalAlignment.Center);
            listView1.Columns.Add("Artist - Title", 240, HorizontalAlignment.Left);
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            button2.Enabled = false;
            string json_data = await Tiesto.GetJsonData(PODCAST_LIST_URL);
            if (json_data != string.Empty)
            {
                File.WriteAllText(localdata.PodcastJson, json_data);
                var json = JsonConvert.DeserializeObject<Tiesto.PodcastList>(json_data);
                comboBox1.Enabled = false;
                comboBox1.Items.Clear();
                foreach (var pod in json.podcasts)
                {
                    ComboboxItem item = new ComboboxItem();
                    item.Id = pod.podcast.id;
                    item.Title = Data.Normalize(pod.podcast.title);
                    item.Episode = pod.podcast.episodeNumber.ToString();
                    item.Duration = pod.podcast.duration;
                    item.Url = pod.podcast.mp4Url;
                    double drelease = Convert.ToDouble(pod.podcast.releaseDate.ToString().Substring(0, 10));
                    DateTime release = Data.UnixTimeStampToDateTime(drelease);
                    item.Release = release;
                    item.Year = release.Year.ToString();
                    comboBox1.Items.Add(item);
                }
                comboBox1.Enabled = true;
                comboBox1.SelectedIndex = 0;
                comboBox1.Focus();
            }
            else
            {
                MessageBox.Show("Error tidak dapat mendapatkan data!\r\nCek koneksi internet anda...", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
            button1.Enabled = true;
        }

        private async void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var data = (comboBox1.SelectedItem as ComboboxItem);
            button2.Enabled = false;
            label9.Text = data.Title;
            label8.Text = data.Release.ToString();
            label7.Text = string.Empty;
            Download.FileName = data.Title + ".m4a";
            Download.Id = data.Id;
            if (data.Url.Contains("http"))
            {
                Download.Url = (comboBox1.SelectedItem as ComboboxItem).Url;
                string json_data = await Tiesto.GetJsonData(PODCAST_TRACK_URL.Replace("$id", Download.Id));
                if (json_data != string.Empty)
                {
                    File.WriteAllText(localdata.TracklistJson, json_data);
                    var json = JsonConvert.DeserializeObject<Tiesto.Mix>(json_data);
                    listView1.Items.Clear();
                    int trackCount = 0;
                    foreach (var x in json.mixPodcastTracks)
                    {
                        foreach (var y in x.tracks)
                        {
                            trackCount++;
                            ListViewItem item1 = new ListViewItem(y.track.id);
                            var ts = TimeSpan.FromSeconds(y.track.starttime);
                            string durasi = string.Format("{0}:{1}", ts.Minutes.ToString("00"), ts.Seconds.ToString("00"));
                            item1.SubItems.Add(durasi);
                            item1.SubItems.Add(Data.Normalize(y.track.title));
                            listView1.Items.Add(item1);
                        }
                    }

                    button2.Enabled = true;
                    if (trackCount == 0)
                    {
                        label7.Text = "0 Track";
                        isListNotEmpty = false;
                    }
                    else
                    {
                        label7.Text = $"{trackCount.ToString()} Tracks";
                        isListNotEmpty = true;
                    }
                }
                else
                {
                    MessageBox.Show("Error tidak dapat mendapatkan data!\r\nCek koneksi internet anda...", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            File.AppendAllText(localdata.HistoryLog, $"[{DateTime.Now}] {Download.FileName} - {Download.Url}\r\n");
            string idm = null;
            if (File.Exists(localdata.CustomIdm))
            {
                idm = File.ReadAllText(localdata.CustomIdm);
            }
            else
            {
                string[] idmpath = {
                @"C:\Program Files (x86)\Internet Download Manager\IDMan.exe",
                @"C:\Program Files\Internet Download Manager\IDMan.exe",
                @"IDMan.exe" };
                foreach (string exe in idmpath)
                {
                    if (File.Exists(exe)) idm = exe;
                }
            }
            if (idm != null)
            {
                ProcessStartInfo exec = new ProcessStartInfo();
                exec.FileName = idm;
                exec.Arguments = $"/d \"{Download.Url}\" /p \"{Download.Folder}\" /f \"{Download.FileName}\" /n";
                Process.Start(exec);

                if (isListNotEmpty)
                {
                    if (checkBox1.Checked)
                        SaveAsCue();
                    if (checkBox2.Checked)
                        SaveAsTxt();
                }
            }
            else
            {
                MessageBox.Show("IDM tidak ditemukan! Silahkan pindah app ini ke dalam folder IDM dan jalankan kembali.",
                    this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowser.ShowDialog();
            if (result == DialogResult.OK)
            {
                Download.Folder = folderBrowser.SelectedPath;
                textBox1.Text = Download.Folder;
                File.WriteAllText(localdata.PathIni, Download.Folder);
            }
        }

        private void label11_Click(object sender, EventArgs e)
        {
            if (isListNotEmpty)
            {
                bool result = false;
                if (checkBox1.Checked)
                {
                    SaveAsCue();
                    result = true;
                }
                if (checkBox2.Checked)
                {
                    SaveAsTxt();
                    result = true;
                }

                if (result) MessageBox.Show("Track list berhasil disimpan.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Track list kosong!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveAsTxt()
        {
            var data = (comboBox1.SelectedItem as ComboboxItem);
            var json_data = File.ReadAllText(localdata.TracklistJson);
            var json = JsonConvert.DeserializeObject<Tiesto.Mix>(json_data);

            string savelist = Path.Combine(Download.Folder, Download.FileName.Replace("m4a", "txt"));
            if (File.Exists(savelist))
                File.Delete(savelist);

            foreach (var x in json.mixPodcastTracks)
            {
                File.AppendAllText(savelist, $"== {Data.Normalize(x.artist)} ==\r\n");
                foreach (var y in x.tracks)
                {
                    var ts = TimeSpan.FromSeconds(y.track.starttime);
                    string durasi = $"{ts.Minutes.ToString("00")}:{ts.Seconds.ToString("00")}";
                    File.AppendAllText(savelist, $"[{durasi}] {Data.Normalize(y.track.title)}\r\n");
                }
            }
        }

        private void SaveAsCue()
        {
            var data = (comboBox1.SelectedItem as ComboboxItem);
            var json_data = File.ReadAllText(localdata.TracklistJson);

            Cue.Write(data, json_data);
        }

        private void checkbox_Changed(object sender, EventArgs e)
        {
            localdata.SaveAs(checkBox1.Checked, checkBox2.Checked);
        }
    }
}
