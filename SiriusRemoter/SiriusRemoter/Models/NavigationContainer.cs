using SiriusRemoter.Resources;
using System.Xml;

namespace SiriusRemoter.Models
{
    public class NavigationContainer : NavigationItem
    {
        public NavigationContainer(XmlNode node) : base(node)
        { }

        public NavigationContainer(string path, string name) : base(path, name) { }

        public override NavigationTypes GetEntityType()
        {
            return NavigationTypes.Folder;
        }
    }
}
