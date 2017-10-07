using SiriusRemoter.Helpers;
using SiriusRemoter.Helpers.Players;
using System.ComponentModel;
using static SiriusRemoter.Helpers.Players.Player;

namespace SiriusRemoter.ViewModels
{
    public class MainViewViewModel : INotifyPropertyChanged
    {
        #region Inotify things

        public event PropertyChangedEventHandler PropertyChanged;

        // Create the OnPropertyChanged method to raise the event
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion

        #region Constructor

        public MainViewViewModel(DeviceControllerViewModel controllerViewModel, ArtistInfoViewModel artistInfoViewModel, Player player)
        {
            //Subscribe to Player's State engine event changes
            player.OnStateChanged += Player_OnStateChanged;
        }

        #endregion

        #region Members

        private bool _enableInfoIcon;
        private bool _isMediaPlayerVisible;
        private bool _isPicViewerVisible;

        #endregion

        #region Properties

        public bool EnableInfoIcon
        {
            get
            {
                return _enableInfoIcon;
            }
            set
            {
                if (EnableInfoIcon == value)
                {
                    return;
                }
                _enableInfoIcon = value;
                OnPropertyChanged(nameof(EnableInfoIcon));
            }
        }

        public bool IsMediaPlayerVisible
        {
            get
            {
                return _isMediaPlayerVisible;
            }
            set
            {
                if (_isMediaPlayerVisible == value)
                {
                    return;
                }
                _isMediaPlayerVisible = value;
                if (_isMediaPlayerVisible)
                {
                    EnableInfoIcon = true;
                }
                OnPropertyChanged(nameof(IsMediaPlayerVisible));
            }
        }

        public bool IsPicViewerVisible
        {
            get
            {
                return _isPicViewerVisible;
            }
            set
            {
                if (_isPicViewerVisible == value)
                {
                    return;
                }
                _isPicViewerVisible = value;
                if (_isPicViewerVisible)
                {
                    EnableInfoIcon = true;
                }
                OnPropertyChanged(nameof(IsPicViewerVisible));
            }
        }

        #endregion

        #region Event handlers

        private void Player_OnStateChanged(PlayerState state)
        {
            //Toggle visibility between media player and picture viewer
            if (state.Mode == PlayerMode.Picture)
            {
                //Hide media player, show picture viewer
                IsPicViewerVisible = true;
                IsMediaPlayerVisible = !IsPicViewerVisible;
            }
            else if (state.Mode == PlayerMode.Stream)
            {
                //Hide picture viewer, show media player
                IsPicViewerVisible = false;
                IsMediaPlayerVisible = !IsPicViewerVisible;
            }
        }

        #endregion
    }
}
