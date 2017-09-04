using SiriusRemoter.Helpers;
using SiriusRemoter.Resources;
using System.Xml;

namespace SiriusRemoter.Models
{
    public class Song : NavigationItem
    {
        public readonly Track SongTrack;

        public Song(XmlNode node) : base(node)
        {
            SongTrack = new Track(node);
        }

        public Song(string path, string name) : base(path, name)
        {
            SongTrack = new Track(path);
        }

        public override NavigationTypes GetEntityType()
        {
            return NavigationTypes.AudioItem;
        }
    }
}
