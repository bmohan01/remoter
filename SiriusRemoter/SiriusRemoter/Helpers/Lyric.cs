using SiriusRemoter.com.wikia.lyrics;
using System;
using System.Text;

namespace SiriusRemoter.Helpers
{
    public static class Lyric
    {
        public static string GetLyrics(string artistName, string songName)
        {
            LyricWiki wiki = new LyricWiki();
            LyricsResult result;

            if (wiki.checkSongExists(artistName, songName))
            {
                result = wiki.getSong(artistName, songName);
                Encoding iso8859 = Encoding.GetEncoding("ISO-8859-1");
                return Encoding.UTF8.GetString(iso8859.GetBytes(result.lyrics));
            }
            else
            {
                return "Lyrics not found in database";
            }
        }
    }
}
