using System;
using System.ComponentModel;
using static SiriusRemoter.Helpers.Players.Player;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SiriusRemoter.Helpers.Players;

namespace SiriusRemoter.ViewModels
{
    public class PictureViewerViewModel : INotifyPropertyChanged
    {
        #region INotify Things

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public PictureViewerViewModel(Player player)
        {
            PlayerController = player;
            //Subscribe to Player's State engine event changes
            PlayerController.OnStateChanged += Player_OnStateChanged;
        }

        #region Members

        private ImageSource _sourceImage;
        private bool _imageLoading;

        #endregion

        #region Properties

        public Player PlayerController { get; private set; }

        public bool ImageLoading
        {
            get
            {
                return _imageLoading;
            }
            set
            {
                _imageLoading = value;
                OnPropertyChanged(nameof(ImageLoading));
            }
        }

        public ImageSource SourceImage
        {
            get
            {
                return _sourceImage;
            }
            set
            {
                if (_sourceImage == value)
                {
                    return;
                }
                _sourceImage = value;
                OnPropertyChanged(nameof(SourceImage));
            }
        }

        #endregion

        #region Methods

        private BitmapImage GetImage(string url)
        {
            var bi = new BitmapImage();
            bi.BeginInit();
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.UriSource = new Uri(url);
            bi.EndInit();
            bi.DownloadCompleted += new EventHandler(bi_DownloadCompleted);
            return bi;
        }

        #endregion

        #region Event handlers

        private void Player_OnStateChanged(PlayerState state)
        {
            try
            {
                if (state.Mode == PlayerMode.Picture)
                {
                    //Start the progressRing, busy load indicator
                    ImageLoading = true;
                    //Set the image for the control
                    SourceImage = GetImage(state.PictureSource);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        /// <summary>
        /// When image has been downloaded, stop the progressring
        /// </summary>
        void bi_DownloadCompleted(object sender, EventArgs e)
        {
            ImageLoading = false;
        }

        #endregion
    }
}
