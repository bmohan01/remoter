using SiriusRemoter.Helpers;
using SiriusRemoter.Models.Players;
using System;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static SiriusRemoter.Models.Players.Player;

namespace SiriusRemoter.ViewModels
{
    public class MediaPlayerViewModel : INotifyPropertyChanged
    { 
        #region INotify Things

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Contructor

        public MediaPlayerViewModel(Player player)
        {
            _player = player;
            //Subscribe to Player's State engine event changes
            _player.OnStateChanged += Player_OnStateChanged;
            //set commands
            PlayCommand = new RelayCommand(Play);
            PauseCommand = new RelayCommand(Pause);
            NextCommand = new RelayCommand(NextSong);
            PreviousCommand = new RelayCommand(PreviousSong);
            RepeatCommand = new RelayCommand(RepeatCurrentQueue);
            RepeatOneCommand = new RelayCommand(RepeatSong);
            ShuffleCommand = new RelayCommand(Shuffle);
        }

        #endregion

        #region Commands

        public RelayCommand PlayCommand { get; set; }
        public RelayCommand PauseCommand { get; set; }
        public RelayCommand NextCommand { get; set; }
        public RelayCommand PreviousCommand { get; set; }
        public RelayCommand RepeatCommand { get; set; }
        public RelayCommand RepeatOneCommand { get; set; }
        public RelayCommand ShuffleCommand { get; set; }

        #endregion

        #region Members

        private Player _player;
        private string _albumartUri;
        private string _artistName;
        private string _album;
        private string _title;
        private string _bitrate;
        private string _samplingFrequency;
        private TimeSpan _currentSongDuration;
        private double _currentVolume;
        private ImageSource _volumeImage;
        private string _currentSongDurationText;

        #endregion

        #region Properties

        public string BitRate
        {
            get
            {
                return _bitrate;
            }
            set
            {
                if (_bitrate == value)
                {
                    return;
                }
                _bitrate = value;
                OnPropertyChanged(nameof(BitRate));
            }
        }

        public string SamplingFrequency
        {
            get
            {
                return _samplingFrequency;
            }
            set
            {
                if (_samplingFrequency == value)
                {
                    return;
                }
                _samplingFrequency = value;
                OnPropertyChanged(nameof(SamplingFrequency));
            }
        }

        public string AlbumArtUri
        {
            get
            {
                return _albumartUri;
            }
            set
            {
                if (_albumartUri == value)
                {
                    return;
                }
                _albumartUri = value;
                OnPropertyChanged(nameof(AlbumArtUri));
            }
        }

        public string ArtistName
        {
            get
            {
                return _artistName;
            }
            set
            {
                if (_artistName == value)
                {
                    return;
                }
                _artistName = value;
                OnPropertyChanged(nameof(ArtistName));
            }
        }

        public string Album
        {
            get
            {
                return _album;
            }
            set
            {
                if (_album == value)
                {
                    return;
                }
                _album = value;
                OnPropertyChanged(nameof(Album));
            }
        }

        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                if (_title == value)
                {
                    return;
                }
                _title = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        public TimeSpan CurrentSongDuration
        {
            get
            {
                return _currentSongDuration;
            }
            private set
            {
                if (_currentSongDuration == value)
                {
                    return;
                }
                _currentSongDuration = value;
                CurrentSongDurationText = _currentSongDuration.ToString(@"hh\:mm\:ss");
                OnPropertyChanged(nameof(CurrentSongDuration));
            }
        }

        public string CurrentSongDurationText
        {
            get
            {
                return _currentSongDurationText;
            }
            private set
            {
                if (_currentSongDurationText == value)
                {
                    return;
                }
                _currentSongDurationText = value;
                OnPropertyChanged(nameof(CurrentSongDurationText));
            }
        }

        public double CurrentVolume
        {
            get
            {
                return _currentVolume;
            }
            set
            {
                if (_currentVolume == value)
                {
                    return;
                }
                _currentVolume = value;
                SetupOnVolumeChange();
                OnPropertyChanged(nameof(CurrentVolume));
            }
        }

        public ImageSource VolumeImage
        {
            get
            {
                return _volumeImage;
            }
            set
            {
                _volumeImage = value;
                OnPropertyChanged(nameof(VolumeImage));
            }
        }

        public Player PlayerController
        {
            get
            {
                return _player;
            }
            set
            {
                _player = value;
                OnPropertyChanged(nameof(PlayerController));
            }
        }

        #endregion

        #region Methods

        private void SetupOnVolumeChange()
        {
            PlayerController.Renderer.SetVolume(CurrentVolume);
            SetVolumeLoudnessImage(_currentVolume);
        }

        private void SetVolumeLoudnessImage(double volume)
        {
            BitmapImage volumeImage;
            if (volume > 0 && volume <= 33)
            {
                volumeImage = GetImage("../../Resources/Icons/Volume-whisper.white.png");
            }
            else if (volume > 33 && volume <= 66)
            {
                volumeImage = GetImage("../../Resources/Icons/Volume-quiet.white.png");
            }
            else if (volume > 66 && volume <= 100)
            {
                volumeImage = GetImage("../../Resources/Icons/Volume-intermediate.white.png");
            }
            else
            {
                volumeImage = GetImage("../../Resources/Icons/Volume-mute.white.png");
            }
            //Need to freeze image otherwise there is a threading error:
            //http://stackoverflow.com/questions/26361020/error-must-create-dependencysource-on-same-thread-as-the-dependencyobject-even
            volumeImage.Freeze();
            VolumeImage = volumeImage;
        }

        private BitmapImage GetImage(string uri)
        {
            return new BitmapImage(new Uri(uri, UriKind.Relative));
        }

        public void Play(object parameter)
        {
            _player.Play();
        }

        public void Pause(object parameter)
        {
            _player.Pause();
        }

        public void NextSong(object parameter)
        {
            _player.PlayNext();
        }

        public void PreviousSong(object parameter)
        {
            _player.PlayPrevious();
        }

        public void RepeatCurrentQueue(object parameter)
        {
            _player.RepeatAll();
        }

        public void RepeatSong(object parameter)
        {
            throw new NotImplementedException();
        }

        public void Shuffle(object parameter)
        {
            _player.Shuffle();
        }

        public void Seek(double seekPercentage)
        {
            //convert seekPercentage into hh:mm:ss format
            var currentlyPlayingSong = _player.CurrentlyPlaying;
            if (currentlyPlayingSong == null)
            {
                return;
            }
            var currentlyPlayingTrack = currentlyPlayingSong.SongTrack;
            var duration = currentlyPlayingTrack.Duration;
            int secondsToAdd = (int) (duration.TotalSeconds * seekPercentage / 100);
            var seekTime = new TimeSpan();
            seekTime = seekTime.Add(new TimeSpan(0, 0, secondsToAdd));
            _player.Seek(seekTime.ToString(@"hh\:mm\:ss"));
        }

        #endregion

        #region Event handlers

        private void Player_OnStateChanged(PlayerState state)
        {
            try
            {
                AlbumArtUri = state.AlbumartUri;
                ArtistName = state.ArtistName;
                Album = state.Album;
                Title = state.Title;
                BitRate = state.Bitrate + " kbps";
                SamplingFrequency = state.SamplingFrequency + " Hz";
                CurrentSongDuration = state.CurrentSongDuration;
                CurrentVolume = state.CurrentVolume;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        #endregion
    }
}
