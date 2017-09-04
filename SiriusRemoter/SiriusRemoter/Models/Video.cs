using SiriusRemoter.Resources;
using System.Xml;

namespace SiriusRemoter.Models
{
    public class Video : NavigationItem
    {
        public string SourceUrl { get; private set; }

        public Video(XmlNode node) : base(node)
        {
            SourceUrl = node["res"].InnerText;
        }

        public override NavigationTypes GetEntityType()
        {
            return NavigationTypes.Video;
        }
    }
}