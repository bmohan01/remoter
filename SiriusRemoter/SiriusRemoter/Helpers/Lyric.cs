using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace SiriusRemoter.Helpers
{
    public static class Lyric
    {
        public const string UrlPrefix = "http://api.musixmatch.com/ws/1.1";

        internal class TrackInfo
        {
            public string TrackId;
            public string CommonTrackId;

            public TrackInfo(string artistName, string songName)
            {
                artistName = Uri.EscapeUriString(artistName).ToLower();
                songName = Uri.EscapeUriString(songName).ToLower();

                string urlRequest = $"{UrlPrefix}/track.search?apikey={ApiKeys.Instance.MusixMatchKey}&q_artist={artistName}&q_track={songName}";
                var req = (HttpWebRequest)WebRequest.Create(urlRequest);
                var webResponse = (HttpWebResponse)req.GetResponse();
                var infoResponseStream = webResponse.GetResponseStream();
                using (var sr = new StreamReader(infoResponseStream, Encoding.UTF8))
                {
                    var token = JObject.Parse(sr.ReadToEnd());
                    //get first search result's id
                    var results = token["message"]["body"]["track_list"];
                    var first = results.First["track"];
                    TrackId = first["track_id"].ToString();
                    CommonTrackId = first["commontrack_id"].ToString();
                }
            }

        }
    }
}
