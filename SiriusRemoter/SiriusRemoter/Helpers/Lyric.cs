using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace SiriusRemoter.Helpers
{
    public static class Lyric
    {
        private const string UrlPrefix = "http://api.musixmatch.com/ws/1.1";

        public static string GetLyrics(string artistName, string songName)
        {
            var info = new TrackInfo(artistName, songName);

            string urlRequest = $"{UrlPrefix}/track.lyrics.get?apikey={ApiKeys.Instance.MusixMatchKey}&track_id={info.TrackId}&commontrack_id={info.CommonTrackId}";
            var req = (HttpWebRequest)WebRequest.Create(urlRequest);
            var webResponse = (HttpWebResponse)req.GetResponse();
            var infoResponseStream = webResponse.GetResponseStream();
            using (var sr = new StreamReader(infoResponseStream, Encoding.UTF8))
            {
                var token = JObject.Parse(sr.ReadToEnd());
                //get first search result's id
                var firstResult = token["message"]["body"]["lyrics"];
                return firstResult["lyrics_body"].ToString();
            }
        }


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
