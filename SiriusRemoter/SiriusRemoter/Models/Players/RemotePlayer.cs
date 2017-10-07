using OpenSource.UPnP;
using SiriusRemoter.Resources;
using System;
using System.Collections.Generic;
using System.Xml;

namespace SiriusRemoter.Models.Players
{
    public class RemotePlayer : BasePlayer
    {
        public RemotePlayer(Player player) : base(player)
        {
        }

        private Stack<string> PreviousQueries = new Stack<string>();

        private string GetQueue(string query)
        {
            return Browse(query);
        }

        private static List<NavigationItem> ParseBrowseResult(string browseResultXml)
        {
            var xmlDoc = new XmlDocument(); // Create an XML document object
            xmlDoc.LoadXml(browseResultXml);
            var root = xmlDoc.FirstChild;
            //var itemNames = xmlDoc.GetElementsByTagName("dc:title");
            var children = root.ChildNodes;
            var navItems = new List<NavigationItem>();
            foreach (XmlNode item in children)
            {
                navItems.Add(GetNavigationItem(item));
            }
            return navItems;
        }

        /// <summary>
        /// Returns the right navigation item object based on the Node passed in
        /// (Like a factory pattern)
        /// </summary>
        /// <param name="node">The node to be parsed to determine the type of navigation item</param>
        private static NavigationItem GetNavigationItem(XmlNode node)
        {
            string itemClassName = node[Constants.UPNP_TYPE_TAG].InnerText;
            if (itemClassName.Contains(Constants.SONG_TAG))
            {
                return new Song(node);
            }
            else if (itemClassName.Contains(Constants.CONTAINER_TAG))
            {
                return new NavigationContainer(node);
            }
            else if (itemClassName.Contains(Constants.IMAGE_TAG))
            {
                return new Photo(node);
            }
            else if (itemClassName.Contains(Constants.VIDEO_TAG))
            {
                return new Video(node);
            }
            return null;
        }

        private string Browse(string action, uint startIndex = 0u, uint requestedCount = 100u)
        {
            try
            {
                var arguments = new UPnPArgument[10];
                arguments[0] = new UPnPArgument("ObjectID", action);
                arguments[1] = new UPnPArgument("BrowseFlag", "BrowseDirectChildren");
                arguments[2] = new UPnPArgument("Filter", "");
                arguments[3] = new UPnPArgument("StartingIndex", startIndex);
                arguments[4] = new UPnPArgument("RequestedCount", requestedCount);
                arguments[5] = new UPnPArgument("SortCriteria", "");
                arguments[6] = new UPnPArgument("Result", "");
                arguments[7] = new UPnPArgument("NumberReturned", 0u);
                arguments[8] = new UPnPArgument("TotalMatches", 0u);
                arguments[9] = new UPnPArgument("UpdateID", 0u);
                _parentPlayer.Server.ContentDirectory.InvokeSync("Browse", arguments);
                string result = arguments[6].DataValue as string;
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        public override List<NavigationItem> GetNavigationItems(string query)
        {
            string result = GetQueue(query);
            if (!String.IsNullOrWhiteSpace(result))
            {
                return ParseBrowseResult(result);
            }
            return new List<NavigationItem>();
        }

        public override void NavigateDownOneLevel(NavigationItem item)
        {
            base.NavigateDownOneLevel(item);
            PreviousQueries.Push(item.ParentId);
        }

        public override void ClearPreviousQueries()
        {
            PreviousQueries.Clear();
        }

        public override void NavigateUpOneLevel()
        {
            string previousQuery = PreviousQueries.Pop();
            _parentPlayer.SetQueue(previousQuery);
        }

        public override bool CanNavigateUp()
        {
            return PreviousQueries.Count > 0;
        }
    }
}
