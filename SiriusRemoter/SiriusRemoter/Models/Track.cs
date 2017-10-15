using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace SiriusRemoter.Helpers
{
    public class Track
    {
        #region Properties

        public string Uri { get; set; }
        public string MetaData { get; set; }
        public string AlbumartUri { get; set; }
        public int BitRate { get; private set; }
        public string BitDepth { get; set; }
        public string SamplingFrequency { get; set; }
        public string Channels { get; set; }
        public TimeSpan Duration { get; private set; }
        public string Album { get; set; }
        public string Artist { get; set; }
        public string Title { get; set; }

        #endregion

        #region Members

        private const string URI_TAG = "res";
        private const string BIT_RATE_TAG = "bitrate";
        private const string BIT_DEPTH_TAG = "bitsPerSample";
        private const string CHANNELS_TAG = "nrAudioChannels";
        private const string SAMPLE_FREQUENCY_TAG = "sampleFrequency";
        private const string DURATION_TAG = "duration";
        private const string ALBUMART_TAG = "upnp:albumArtURI";
        private const string ALBUM_TAG = "upnp:album";
        private const string ARTIST_TAG = "upnp:artist";
        private const string TITLE_TAG = "dc:title";

        #endregion

        #region Constructors

        /// <summary>
        /// Local file track
        /// </summary>
        /// <param name="filePath"></param>
        public Track(string filePath)
        {
            var mediaFile = TagLib.File.Create(filePath);
            AlbumartUri = mediaFile.Tag.Pictures.Length > 0 ? mediaFile.Tag.Pictures[0].Description : null;
            Album = mediaFile.Tag.Album;
            Artist = mediaFile.Tag.AlbumArtists.Length > 0 ? mediaFile.Tag.AlbumArtists[0] : null;
            Title = mediaFile.Tag.Title;
            BitRate = mediaFile.Properties.AudioBitrate;
            SamplingFrequency = mediaFile.Properties.AudioSampleRate.ToString();
            BitDepth = mediaFile.Properties.BitsPerSample.ToString();
            Channels = mediaFile.Properties.AudioChannels.ToString();
            Duration = mediaFile.Properties.Duration;
            Uri = filePath;
        }

        /// <summary>
        /// Upnp media track
        /// </summary>
        /// <param name="upnpNode"></param>
        public Track(XmlNode upnpNode)
        {
            Initialize(upnpNode);
        }

        #endregion

        #region Methods

        private void Initialize(XmlNode node)
        {
            try
            {
                AlbumartUri = TryGetValue(node, ALBUMART_TAG);
                Album = TryGetValue(node, ALBUM_TAG);
                Artist = TryGetValue(node, ARTIST_TAG);
                Title = TryGetValue(node, TITLE_TAG);
                var resNode = node[URI_TAG];
                int bitRate;
                int.TryParse(resNode.GetAttribute(BIT_RATE_TAG), out bitRate);
                BitRate = bitRate / 1000;
                SamplingFrequency = resNode.GetAttribute(SAMPLE_FREQUENCY_TAG);
                BitDepth = resNode.GetAttribute(BIT_DEPTH_TAG);
                Channels = resNode.GetAttribute(CHANNELS_TAG);
                Duration = TimeSpan.Parse(resNode.GetAttribute(DURATION_TAG));
                Uri = resNode.InnerText;
                MetaData = FormDIDLMeta(node);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private string FormDIDLMeta(XmlNode rootNode)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(@"<DIDL-Lite></DIDL-Lite>");
            foreach (XmlAttribute attribute in rootNode.ParentNode.Attributes)
            {
                doc.DocumentElement.SetAttribute(attribute.Name, attribute.Value);
            }
            var va = doc.FirstChild;
            XmlNode importNode = va.OwnerDocument.ImportNode(rootNode, true);
            va.AppendChild(importNode);
            return doc.OuterXml;
        }

        private string TryGetValue(XmlNode node, string tag)
        {
            try
            {
                return node[tag].InnerText;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return "";
            }
        }

        #endregion
    }
}
