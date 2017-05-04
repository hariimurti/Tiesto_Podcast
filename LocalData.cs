using System;
using System.IO;

namespace Tiesto.Podcast
{
    class LocalData
    {
        private static string LocalApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        private static string LocalApp = Path.Combine(LocalApplicationData, "Tiesto");
        public string PathIni { get; set; }
        public string CustomIdm { get; set; }
        public string HistoryLog { get; set; }
        private static string ErrorLog { get; set; }
        public string PodcastJson { get; set; }
        public string TracklistJson { get; set; }
        public bool SaveAsCue { get; set; }
        public bool SaveAsTxt { get; set; }

        public LocalData()
        {
            if (!Directory.Exists(LocalApp))
                Directory.CreateDirectory(LocalApp);

            PathIni = Path.Combine(LocalApp, "path.ini");
            CustomIdm = Path.Combine(LocalApp, "custom-idm.ini");
            HistoryLog = Path.Combine(LocalApp, "history.log");
            ErrorLog = Path.Combine(LocalApp, "error.log");
            PodcastJson = Path.Combine(LocalApp, "podcast.json");
            TracklistJson = Path.Combine(LocalApp, "tracklist.json");

            string SaveAs = Path.Combine(LocalApp, "saveas.ini");
            if (File.Exists(SaveAs))
            {
                foreach(string line in File.ReadAllLines(SaveAs))
                {
                    if (line.Contains("SaveAsCue"))
                    {
                        if (line.ToLower().Contains("true"))
                            SaveAsCue = true;
                        else
                            SaveAsCue = false;
                    }

                    if (line.Contains("SaveAsTxt"))
                    {
                        if (line.ToLower().Contains("true"))
                            SaveAsTxt = true;
                        else
                            SaveAsTxt = false;
                    }
                }
            }
            else
            {
                File.WriteAllLines(SaveAs, new string[] { "SaveAsCue=False", "SaveAsTxt=False" });
            }
        }

        public static void WriteError(string text)
        {
            File.WriteAllText(ErrorLog, text);
        }

        public static string ReadError()
        {
            string retval = string.Empty;
            if (File.Exists(ErrorLog))
                retval = File.ReadAllText(ErrorLog);
            return retval;
        }

        public string Custom(string text)
        {
            return Path.Combine(LocalApp, text);
        }

        public void SaveAs(bool cue, bool txt)
        {
            File.WriteAllLines(Path.Combine(LocalApp, "saveas.ini"), new string[] { $"SaveAsCue={cue}", $"SaveAsTxt={txt}" });
        }
    }
}
