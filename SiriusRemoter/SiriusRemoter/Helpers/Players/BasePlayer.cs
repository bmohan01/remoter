using SiriusRemoter.Models;
using System.Collections.Generic;

namespace SiriusRemoter.Helpers.Players
{
    public abstract class BasePlayer
    {
        public BasePlayer(Player player)
        {
            _parentPlayer = player;
        }

        protected Player _parentPlayer;
        protected List<NavigationItem> _currentNavItems;

        public abstract List<NavigationItem> GetNavigationItems(string path);

        public abstract bool CanNavigateUp();

        public abstract void NavigateUpOneLevel();

        public virtual void NavigateDownOneLevel(NavigationItem item)
        {
            _parentPlayer.SetQueue(item.ItemPath);
        }

        public virtual void ClearPreviousQueries()
        {
        }
    }
}
