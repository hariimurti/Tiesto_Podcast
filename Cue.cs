using Newtonsoft.Json;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Tiesto.Podcast
{
    class Cue
    {
        static string pathToSave;

        public static void Write(ComboboxItem data, string json_data)
        {
            var json = JsonConvert.DeserializeObject<Tiesto.Mix>(json_data);

            pathToSave = Path.Combine(Download.Folder, Download.FileName.Replace("m4a", "cue"));
            WriteHeader(data.Year, data.Episode, Download.FileName);

            int tracknumber = 1;
            int lasttime = 0;
            foreach (var x in json.mixPodcastTracks)
            {
                bool guest = false;
                if (tracknumber == 1)
                {
                    AddIntro(tracknumber, Data.Normalize(x.artist), "Intro", 0);
                }
                else
                {
                    guest = true;
                }

                foreach (var y in x.tracks)
                {
                    tracknumber++;
                    if (y.track.starttime != 0)
                    {
                        lasttime = y.track.starttime;
                    }
                    else
                    {
                        lasttime += 60;
                    }

                    if (guest)
                    {
                        AddIntro(tracknumber, Data.Normalize(x.artist), "Guest Mix", lasttime);
                        tracknumber++;
                        lasttime += 30;
                        guest = false;
                    }
                                        
                    AddTrack(tracknumber, x.artist, Data.Normalize(y.track.title), lasttime);
                }
            }
        }

        private static void WriteHeader(string year, string episode, string filename)
        {
            File.WriteAllText(pathToSave,
                $"REM GENRE Electronic Dance Music\r\n" +
                $"REM DATE {year}\r\n" +
                $"PERFORMER \"Tiësto\"\r\n" +
                $"TITLE \"Club Life {episode}\"\r\n" +
                $"FILE \"{filename}\" M4A\r\n");
        }

        private static void WriteTrack(int trackNumber, string performer, string title, int startTime)
        {
            var ts = TimeSpan.FromSeconds(startTime);
            string duration = $"{ts.Minutes.ToString("00")}:{ts.Seconds.ToString("00")}:{ts.Milliseconds.ToString("00")}";

            File.AppendAllText(pathToSave,
                $"  TRACK {trackNumber.ToString("00")} AUDIO\r\n" +
                $"    TITLE \"{title}\"\r\n" +
                $"    PERFORMER \"{performer}\"\r\n" +
                $"    INDEX 01 {duration}\r\n");
        }

        private static void AddIntro(int trackNumber, string artist, string trackName, int startTime)
        {
            string performer = Data.GetPerformer(artist);
            WriteTrack(trackNumber, performer, trackName, startTime);
        }

        private static void AddTrack(int trackNumber, string artist, string trackName, int startTime)
        {
            Regex regex = new Regex(@"([\s\S]+)-([\s\S]+)");
            Match match = regex.Match(trackName.Replace("–", "-"));
            string title, performer;
            if (match.Success)
            {
                string value1 = match.Groups[1].Value.Trim();
                string value2 = match.Groups[2].Value.Trim();
                if (!value1.ToLower().Contains("guest"))
                {
                    performer = value1;
                    title = value2;
                }
                else
                {
                    performer = value2;
                    title = value1;
                }
            }
            else
            {
                if (!trackName.ToLower().Contains("guest"))
                {
                    performer = trackName;
                    title = "Mixed Track";
                }
                else
                {
                    performer = Data.GetPerformer(artist);
                    title = "Mixed Track";
                }
            }

            WriteTrack(trackNumber, performer, title, startTime);
        }
    }
}
