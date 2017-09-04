using MahApps.Metro.Controls;
using SiriusRemoter.Helpers;
using SiriusRemoter.ViewModels;
using SiriusRemoter.Views;
using System;
using System.Windows;

namespace SiriusRemoter
{
    /// <summary>
    /// Interaction logic for RemoterMainView.xaml
    /// </summary>
    public partial class RemoterMainView : MetroWindow
    {

        #region Constructor

        public RemoterMainView()
        {
            InitializeComponent();

            var player = new Player();
            //Initialize all view models for all controls
            DataContext = new MainViewViewModel(DeviceFinderAndNavigator.ViewModel, InfoPanel.ViewModel, player);
            DeviceFinderAndNavigator.DataContext = new DeviceControllerViewModel(player);
            MediaPlayer.InitDataContext(new MediaPlayerViewModel(player));
            InfoPanel.DataContext = new ArtistInfoViewModel(player);
            //Picture viewer data context setup
            var picViewerViewModel = new PictureViewerViewModel(player);
            PicViewer.DataContext = picViewerViewModel;
            PicInfoPanel.DataContext = new PictureInfoViewModel(picViewerViewModel);
        }

        #endregion

        #region Event Handlers

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var optionsWindow = new OptionsView(Window.GetWindow(this));
                optionsWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        /// <summary>
        /// Toggles Artist Info panel visibility Control.
        /// </summary>
        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ArtistInfoContainer.Visibility == Visibility.Visible)
                {
                    ArtistInfoContainer.Visibility = Visibility.Collapsed;
                }
                else
                {
                    ArtistInfoContainer.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        #endregion
    }
}
