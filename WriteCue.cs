using System.IO;
using System.Text.RegularExpressions;

namespace Tiesto.Podcast
{
    class WriteCue
    {
        public string Location { get; set; }

        public WriteCue(string path, string year, string episode, string filename)
        {
            this.Location = path;
            File.WriteAllText(this.Location,
                $"REM GENRE Electronic Dance Music\r\n" +
                $"REM DATE {year}\r\n" +
                $"PERFORMER \"Tiësto\"\r\n" +
                $"TITLE \"Club Life {episode}\"\r\n" +
                $"FILE \"{filename}\" M4A\r\n");
        }

        public void AddTrack(string trackNumber, string trackName, string startTime, string guest = null)
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
            File.AppendAllText(this.Location,
                $"  TRACK {trackNumber} AUDIO\r\n" +
                $"    TITLE \"{title}\"\r\n" + 
                $"    PERFORMER \"{artist}\"\r\n" +
                $"    INDEX 01 {startTime}\r\n");
        }
    }
}
