using SiriusRemoter.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace SiriusRemoter.Helpers
{
    public static class Utilities
    {
        public static string TokenFilePath => Path.Combine(Utilities.AssemblyDirectory, "token.txt");

        private static string[] mediaExtensions = {
            ".WAV", ".MID", ".MIDI", ".WMA", ".MP3", ".OGG", ".RMA", //etc
            ".AVI", ".MP4", ".DIVX", ".WMV", //etc
        };

        private static string[] photoExtensions = { ".PNG", ".JPG", ".JPEG", ".BMP", ".GIF" };

        public static bool IsMediaFile(string path)
        {
            return -1 != Array.IndexOf(mediaExtensions, Path.GetExtension(path).ToUpperInvariant());
        }

        public static bool IsPhotoFile(string path)
        {
            return -1 != Array.IndexOf(photoExtensions, Path.GetExtension(path).ToUpperInvariant());
        }

        public static string TryGetAttributeFromTrackInstance(XElement element, string attribute)
        {
            try
            {
                if (element == null)
                {
                    return "";
                }
                XNamespace NS = "urn:schemas-upnp-org:metadata-1-0/AVT/";
                var attributeElement = element.Element(NS + attribute);
                if (attributeElement == null)
                {
                    return "";
                }
                var valueAttribute = attributeElement.Attribute("val");
                if (valueAttribute == null)
                {
                    return "";
                }
                return valueAttribute.Value;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return "";
            }
        }

        public static TimeSpan ParseDuration(string value)
        {
            if (string.IsNullOrEmpty(value))
                return TimeSpan.FromSeconds(0);
            return TimeSpan.Parse(value);
        }

        public static int FindSongByUri(string uri, List<NavigationItem> navigationItems)
        {
            for (int i = 0; i < navigationItems.Count; ++i)
            {
                var song = navigationItems[i] as Song;
                if (song == null)
                {
                    continue;
                }
                if (song.SongTrack.Uri.Equals(uri, StringComparison.InvariantCultureIgnoreCase))
                {
                    return i;
                }
            }
            return -1;
        }

        public static string AssemblyDirectory
        {
            get
            {
                UriBuilder uri = new UriBuilder(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
                return Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path));
            }
        }

        public static bool IsSymbolic(string path)
        {
            FileInfo fileInfo = new FileInfo(path);
            if (fileInfo.Exists)
            {
                return fileInfo.Attributes.HasFlag(FileAttributes.ReparsePoint);
            }
            else
            {
                DirectoryInfo dirInfo = new DirectoryInfo(path);
                if (dirInfo.Exists)
                {
                    return dirInfo.Exists && dirInfo.Attributes.HasFlag(FileAttributes.ReparsePoint);
                }
            }

            return false;
        }
    }
}
