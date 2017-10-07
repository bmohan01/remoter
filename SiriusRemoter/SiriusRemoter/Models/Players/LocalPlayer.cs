using SiriusRemoter.Helpers;
using System;
using System.Collections.Generic;
using System.IO;

namespace SiriusRemoter.Models.Players
{
    public class LocalPlayer : BasePlayer
    {
        public LocalPlayer(Player player) : base(player)
        {
        }

        private string _currentPath;

        public override List<NavigationItem> GetNavigationItems(string path)
        {
            _currentPath = path;
            var result = new List<NavigationItem>();

            if (String.IsNullOrWhiteSpace(_currentPath))
            {
                return result;
            }

            foreach (var item in Directory.GetDirectories(_currentPath))
            {
                result.Add(new NavigationContainer(item, new DirectoryInfo(item).Name));
            }

            foreach (var item in Directory.GetFiles(_currentPath))
            {
                if (Utilities.IsMediaFile(item)) result.Add(new Song(item, Path.GetFileNameWithoutExtension(item)));
                if (Utilities.IsPhotoFile(item)) result.Add(new Photo(item, Path.GetFileNameWithoutExtension(item)));
            }

            return result;
        }

        public override void NavigateUpOneLevel()
        {
            _currentPath = Directory.GetParent(_currentPath).FullName;
            _parentPlayer.SetQueue(_currentPath);
        }

        public override bool CanNavigateUp()
        {
            return !String.IsNullOrWhiteSpace(_currentPath) && Directory.GetParent(_currentPath) != null;
        }
    }
}
