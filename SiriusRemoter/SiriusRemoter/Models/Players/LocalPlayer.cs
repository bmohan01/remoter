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

        private const string Root = "Root";
        private string _currentPath;

        public override List<NavigationItem> GetNavigationItems(string path)
        {
            _currentPath = path;
            var result = new List<NavigationItem>();
            if (String.IsNullOrWhiteSpace(_currentPath))
            {
                return result;
            }

            //If at root level
            if (_currentPath == Root)
            {
                var drives = GetLogicalDrives();
                foreach (var driveName in drives)
                {
                    result.Add(new NavigationContainer(driveName, driveName));
                }
            }
            else
            {
                foreach (var item in Directory.GetDirectories(_currentPath))
                {
                    result.Add(new NavigationContainer(item, new DirectoryInfo(item).Name));
                }

                foreach (var item in Directory.GetFiles(_currentPath))
                {
                    if (Utilities.IsMediaFile(item)) result.Add(new Song(item, Path.GetFileNameWithoutExtension(item)));
                    if (Utilities.IsPhotoFile(item)) result.Add(new Photo(item, Path.GetFileNameWithoutExtension(item)));
                }
            }

            return result;
        }

        public override void NavigateUpOneLevel()
        {
            var dirInfo = new DirectoryInfo(_currentPath);
            _currentPath = (dirInfo.Parent == null) ? Root : Directory.GetParent(_currentPath).FullName;
            _parentPlayer.SetQueue(_currentPath);
        }

        public override bool CanNavigateUp()
        {
            if (_currentPath == Root)
            {
                return false;
            }
            var dirInfo = new DirectoryInfo(_currentPath);
            //If a drive Root
            if (dirInfo.Parent == null)
            {
                return true;
            }
            return !String.IsNullOrWhiteSpace(_currentPath) && Directory.GetParent(_currentPath) != null;
        }

        // Print out all logical drives on the system.
        private List<string> GetLogicalDrives()
        {
            List<string> result = new List<string>();
            try
            {
                result = new List<string>(System.IO.Directory.GetLogicalDrives());
            }
            catch (System.IO.IOException)
            {
                System.Console.WriteLine("An I/O error occurs.");
            }
            catch (System.Security.SecurityException)
            {
                System.Console.WriteLine("The caller does not have the " +
                    "required permission.");
            }
            return result;
        }
    }
}
