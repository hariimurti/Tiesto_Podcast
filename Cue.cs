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
            Header(data.Year, data.Episode, Download.FileName);

            int tracknumber = 1;
            int lasttime = 0;
            foreach (var x in json.mixPodcastTracks)
            {
                bool guest = false;
                if (tracknumber == 1)
                {
                    AddTrack(tracknumber.ToString("00"), $"{Data.Normalize(x.artist)} - Intro", "00:00:00");
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
                        var intro_ts = TimeSpan.FromSeconds(lasttime);
                        string intro_durasi = $"{intro_ts.Minutes.ToString("00")}:{intro_ts.Seconds.ToString("00")}:{intro_ts.Milliseconds.ToString("00")}";
                        AddTrack(tracknumber.ToString("00"), Data.Normalize(Data.GetArtist(x.artist) + " - Guest Mix"), intro_durasi);
                        tracknumber++;
                        lasttime += 30;
                        guest = false;
                    }

                    var ts = TimeSpan.FromSeconds(lasttime);
                    string durasi = $"{ts.Minutes.ToString("00")}:{ts.Seconds.ToString("00")}:{ts.Milliseconds.ToString("00")}";
                    AddTrack(tracknumber.ToString("00"), Data.Normalize(y.track.title), durasi, x.artist);
                }
            }
        }

        private static void Header(string year, string episode, string filename)
        {
            File.WriteAllText(pathToSave,
                $"REM GENRE Electronic Dance Music\r\n" +
                $"REM DATE {year}\r\n" +
                $"PERFORMER \"Tiësto\"\r\n" +
                $"TITLE \"Club Life {episode}\"\r\n" +
                $"FILE \"{filename}\" M4A\r\n");
        }

        private static void AddTrack(string trackNumber, string trackName, string startTime, string guest = null)
        {
            Regex regex = new Regex(@"([\s\S]+)-([\s\S]+)");
            Match match = regex.Match(trackName);
            string title, artist;
            if (match.Success)
            {
                artist = match.Groups[1].Value.Trim();
                title = match.Groups[2].Value.Trim();
            }
            else
            {
                artist = (guest != null) ? guest : "VA" ;
                title = trackName;
            }
            File.AppendAllText(pathToSave,
                $"  TRACK {trackNumber} AUDIO\r\n" +
                $"    TITLE \"{title}\"\r\n" + 
                $"    PERFORMER \"{artist}\"\r\n" +
                $"    INDEX 01 {startTime}\r\n");
        }
    }
}
