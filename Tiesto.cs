using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tiesto.Podcast
{
    class Tiesto
    {
        #region JSON Podcasts
        public class PodcastList
        {
            public IList<Podcasts> podcasts { get; set; }
        }

        public class Podcasts
        {
            public Podcast podcast { get; set; }
        }

        public class Podcast
        {
            public double releaseDate { get; set; }
            public string itunesLink { get; set; }
            public int viewsCount { get; set; }
            public int likeCount { get; set; }
            public string title { get; set; }
            public int episodeNumber { get; set; }
            public int commentCount { get; set; }
            public string coverUrl { get; set; }
            public int duration { get; set; }
            public bool isExclusive { get; set; }
            public int shareCount { get; set; }
            public int hour { get; set; }
            public string artists { get; set; }
            public string id { get; set; }
            public string mp4Url { get; set; }
            public string m3u8Url { get; set; }
            public bool isFavorite { get; set; }
        }

        public class Mix
        {
            public IList<PodcastTracks> mixPodcastTracks { get; set; }
        }

        public class PodcastTracks
        {
            public string artist { get; set; }
            public IList<Tracks> tracks { get; set; }
        }

        public class Tracks
        {
            public Track track { get; set; }
        }

        public class Track
        {
            public string artist { get; set; }
            public string id { get; set; }
            public string label { get; set; }
            public int starttime { get; set; }
            public int position { get; set; }
            public string title { get; set; }
        }
        #endregion

        public static async Task<string> GetJsonData(string fromUrl)
        {
            return await Task.Run(() =>
            {
                string json_data = string.Empty;
                var webClient = new WebClient();
                webClient.Headers.Add("device", "phone");
                webClient.Headers.Add("os", "android");
                webClient.Headers.Add("authToken", "");
                //var json = webClient.DownloadString(new Uri("http://tiestoapi-env-prod.elasticbeanstalk.com/initialization"));
                webClient.Headers.Add("Access-Control-Allow-Credentials", "true");
                webClient.Headers.Add("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept,X-Auth-Token, api_key, Authorization");
                webClient.Headers.Add("Access-Control-Allow-Methods", "GET, POST, DELETE, PUT,OPTIONS");
                webClient.Headers.Add("Access-Control-Allow-Origin", "http://editor.swagger.io");
                try
                {
                    json_data = webClient.DownloadString(new Uri(fromUrl));
                }
                catch(Exception ex)
                {
                    LocalData.WriteError(ex.Message);
                    MessageBox.Show(ex.Message, "Warning! Error...", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                webClient.Dispose();
                return json_data;
            });
        }
    }
}
