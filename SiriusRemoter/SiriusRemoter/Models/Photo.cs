using SiriusRemoter.Resources;
using System.Xml;

namespace SiriusRemoter.Models
{
    public class Photo : NavigationItem
    {
        public string SourceUrl { get; private set; }
        public string SourceThumbUrl { get; private set; }

        public Photo(XmlNode node) : base(node)
        {
            SourceUrl = node["res"].InnerText;
            SourceThumbUrl = node.LastChild.InnerText;
        }

        public Photo(string path, string name) : base(path, name)
        {
            SourceUrl = path;
            //SourceThumbUrl = node.LastChild.InnerText;
        }

        public override NavigationTypes GetEntityType()
        {
            return NavigationTypes.Picture;
        }
    }
}