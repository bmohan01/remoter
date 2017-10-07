using SiriusRemoter.Models.Players;
using SiriusRemoter.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using static SiriusRemoter.Models.Players.Player;

namespace SiriusRemoter.Views
{
    public enum InfoPanels
    {
        Bitrate,
        SamplingFrequency
    }

    /// <summary>
    /// Interaction logic for MediaPlayerView.xaml
    /// </summary>
    public partial class MediaPlayerView : UserControl
    {

        #region Members

        private DispatcherTimer _songPositionTimer;
        private bool _isSliderDragging;
        private InfoPanels _activeInfoPanel;

        #endregion

        #region Constructors

        public MediaPlayerView()
        {
            InitializeComponent();
            InitializeSongCurrentPositionTimer();
        }

        #endregion

        #region Property

        public MediaPlayerViewModel ViewModel
        {
            get
            {
                return (DataContext as MediaPlayerViewModel);
            }
        }

        #endregion

        #region Methods

        public void InitDataContext(MediaPlayerViewModel viewModel)
        {
            DataContext = viewModel;
            ViewModel.PlayerController.OnStateChanged += Player_OnStateChanged;
        }

        /// <summary>
        /// Initialize and start the song ticker
        /// </summary>
        private void InitializeSongCurrentPositionTimer()
        {
            _songPositionTimer = new DispatcherTimer();
            _songPositionTimer.Tick += songPositionTimer_Tick;
            _songPositionTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            _songPositionTimer.Start();
        }

        private void ToggleInfoLabels(MahApps.Metro.Controls.TransitionType transitionStyle)
        {
            switch (_activeInfoPanel)
            {
                case InfoPanels.Bitrate:
                    InfoLabel.Content = ViewModel.SamplingFrequency;
                    _activeInfoPanel = InfoPanels.SamplingFrequency;
                    break;
                case InfoPanels.SamplingFrequency:
                    InfoLabel.Content = ViewModel.BitRate;
                    _activeInfoPanel = InfoPanels.Bitrate;
                    break;
            }
        }

        /// <summary>
        /// Performs play or stopped or pause operations based on teh status of the player passed in
        /// .
        /// </summary>
        /// <param name="status">The PlayerStatus passed in</param>
        private void SetupControls(PlayerStatus status)
        {
            switch (status)
            {
                case PlayerStatus.Playing:
                    PerformPlayOperations();
                    break;
                case PlayerStatus.Stopped:
                case PlayerStatus.Paused:
                    PerformStoppedOperations();
                    break;
            }
        }

        /// <summary>
        /// Do operations in the view to represent Playing state
        /// </summary>
        private void PerformPlayOperations()
        {
            //Change the play-pause image to play
            playPauseImage.Source = new BitmapImage(new Uri(@"../Resources/Icons/pause.white.png", UriKind.Relative));
            playPauseButton.Command = ViewModel.PauseCommand;
            InfoLabel.Content = ViewModel.BitRate;
        }

        /// <summary>
        /// Do operations in the view to represent Stopped state
        /// </summary>
        private void PerformStoppedOperations()
        {
            //Change the play-pause image to pause
            playPauseImage.Source = new BitmapImage(new Uri(@"../Resources/Icons/play.white.png", UriKind.Relative));
            playPauseButton.Command = ViewModel.PlayCommand;
        }

        #endregion

        #region Event Handlers

        private void Player_OnStateChanged(PlayerState state)
        {
            try
            {
                //Need to call from dispatcher context
                this.Dispatcher.Invoke(() =>
                {
                    SetupControls(state.TransportState);
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void Right_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ToggleInfoLabels(MahApps.Metro.Controls.TransitionType.Right);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void Left_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ToggleInfoLabels(MahApps.Metro.Controls.TransitionType.Left);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void Slider_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                //Set flag for dragging off
                _isSliderDragging = false;
                //get percentage
                var slider = sender as Slider;
                double distanceFromMin = (slider.Value - slider.Minimum);
                double sliderRange = (slider.Maximum - slider.Minimum);
                double sliderPercent = 100 * (distanceFromMin / sliderRange);
                ViewModel.Seek(sliderPercent);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void songSlider_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                //Set dragging flag to true
                _isSliderDragging = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void songPositionTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                //If dragging detected, do nothing
                if (_isSliderDragging)
                {
                    return;
                }
                //Update the song position
                var currentSongPosition = ViewModel.PlayerController.Renderer.GetCurrentSongPosition();
                CurrentSongPositionLabel.Content = currentSongPosition.ToString(@"hh\:mm\:ss");
                //Update Slider Value
                double songCompletionPercentage = 0;
                if (ViewModel.CurrentSongDuration.TotalSeconds > 0)
                {
                    songCompletionPercentage = (currentSongPosition.TotalSeconds / ViewModel.CurrentSongDuration.TotalSeconds) * 100;
                }
                songSlider.Value = songCompletionPercentage;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void volumeControl_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DDNTPopup.IsOpen = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        #endregion
    }
}
