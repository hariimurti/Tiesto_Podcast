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
            this.Text = "Tiësto Clublife - Podcast Grabber v" + Application.ProductVersion.Substring(0, 3);
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

            button2.Enabled = false;
            button4.Enabled = false;
            button5.Enabled = false;
            button6.Enabled = false;

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

        // refresh podcast list
        private async void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            button2.Enabled = false;
            button6.Enabled = false;
            string json_data = await Tiesto.GetJsonData(PODCAST_LIST_URL);
            if (string.IsNullOrWhiteSpace(json_data))
            {
                MessageBox.Show("Error tidak dapat mendapatkan data!\r\nCek koneksi internet anda...", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                button1.Enabled = true;
                return;
            }

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
            button1.Enabled = true;
        }

        // load podcast info & tracklist
        private async void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var data = (comboBox1.SelectedItem as ComboboxItem);
            button2.Enabled = false;
            button4.Enabled = false;
            button5.Enabled = false;
            button6.Enabled = false;
            label9.Text = data.Title;
            label8.Text = data.Release.ToString();
            label7.Text = string.Empty;
            Download.FileName = data.Title + ".m4a";
            Download.Id = data.Id;
            if (data.Url.Contains("http"))
            {
                Download.Url = (comboBox1.SelectedItem as ComboboxItem).Url;
                string json_data = await Tiesto.GetJsonData(PODCAST_TRACK_URL.Replace("$id", Download.Id));
                if (string.IsNullOrWhiteSpace(json_data))
                {
                    MessageBox.Show("Error tidak dapat mendapatkan data!\r\nCek koneksi internet anda...", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    return;
                }

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

                button2.Enabled = true;
                button4.Enabled = isListNotEmpty && (checkBox1.Checked || checkBox2.Checked);
                button5.Enabled = true;
                button6.Enabled = File.Exists("wget.exe");
            }
        }

        // idm
        private void button2_Click(object sender, EventArgs e)
        {
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
                AskToSaveTrackList();
                File.AppendAllText(localdata.HistoryLog, $"[{DateTime.Now}][IDM] {Download.FileName} - {Download.Url}\r\n");

                ProcessStartInfo exec = new ProcessStartInfo();
                exec.FileName = idm;
                exec.Arguments = $"/d \"{Download.Url}\" /p \"{Download.Folder}\" /f \"{Download.FileName}\" /n";
                Process.Start(exec);
            }
            else
            {
                MessageBox.Show("IDM tidak ditemukan! Silahkan pindah app ini ke dalam folder IDM dan jalankan kembali.",
                    this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // save folder
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

        // save tracklist
        private void button4_Click(object sender, EventArgs e)
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

        // copy clipboard
        private void button5_Click(object sender, EventArgs e)
        {
            if (Download.Url != null)
            {
                Clipboard.SetText(Download.Url);
                MessageBox.Show("Link sudah dicopy ke clipboard.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // wget
        private void button6_Click(object sender, EventArgs e)
        {
            if (File.Exists("wget.exe"))
            {
                AskToSaveTrackList();
                File.AppendAllText(localdata.HistoryLog, $"[{DateTime.Now}][WGET] {Download.FileName} - {Download.Url}\r\n");

                string pathSave = Path.Combine(Download.Folder, Download.FileName);
                string pathTemp = Path.Combine(Download.Folder, "temp-podcast.wget");
                string wgetArgs = $"--output-document=\"{pathTemp}\" --tries=0 --continue";
                if (Download.Url.ToLower().StartsWith("https"))
                    wgetArgs += " --no-check-certificate";

                ProcessStartInfo exec = new ProcessStartInfo();
                exec.FileName = "wget.exe";
                exec.Arguments =  wgetArgs + $" \"{Download.Url}\"";

                this.Hide();
                Process proc = Process.Start(exec);
                proc.WaitForExit();
                int exitCode = proc.ExitCode;
                string msgCode = "Proses download dibatalkan karena sesuatu.";
                switch (exitCode)
                {
                    case 0:
                        try
                        {
                            File.Move(pathTemp, pathSave);
                            msgCode = "Download Complete!\nSaved as " + Path.GetFileName(pathSave);
                        }
                        catch (Exception)
                        {
                            msgCode = "Download Complete!\nTapi file tidak bisa direname!\nTemp: "
                                + Path.GetFileName(pathTemp) + "\nSave: "+ Path.GetFileName(pathSave);
                        }
                        break;
                    case 1:
                        msgCode = "Generic error code.";
                        break;
                    case 2:
                        msgCode = "Parse error — for instance, when parsing command-line options, the .wgetrc or .netrc…";
                        break;
                    case 3:
                        msgCode = "File I/O error.";
                        break;
                    case 4:
                        msgCode = "Network failure.";
                        break;
                    case 5:
                        msgCode = "SSL verification failure.";
                        break;
                    case 6:
                        msgCode = "Username/password authentication failure.";
                        break;
                    case 7:
                        msgCode = "Protocol errors.";
                        break;
                    case 8:
                        msgCode = "Server issued an error response.";
                        break;
                }
                
                MessageBox.Show(msgCode, "WGet Downloader", MessageBoxButtons.OK, (exitCode == 0) ? MessageBoxIcon.Information : MessageBoxIcon.Stop);
                this.Show();
            }
            else
            {
                MessageBox.Show("wget.exe tidak ditemukan! Silahkan copy wget.exe ke dalam ini dan jalankan kembali.",
                    this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AskToSaveTrackList()
        {
            string pathCue = Path.Combine(Download.Folder, Download.FileName.Replace("m4a", "cue"));
            string pathTxt = Path.Combine(Download.Folder, Download.FileName.Replace("m4a", "txt"));
            if (isListNotEmpty)
            {
                if ((checkBox1.Checked && !File.Exists(pathCue)) || (checkBox2.Checked && !File.Exists(pathTxt)))
                {
                    DialogResult askToSave = MessageBox.Show("Simpan track list?", this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (askToSave == DialogResult.Yes)
                    {
                        if (checkBox1.Checked)
                            SaveAsCue();
                        if (checkBox2.Checked)
                            SaveAsTxt();
                    }
                }
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
            button4.Enabled = isListNotEmpty && (checkBox1.Checked || checkBox2.Checked);
        }
    }
}
