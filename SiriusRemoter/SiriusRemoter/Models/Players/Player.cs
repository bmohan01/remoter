using SiriusRemoter.Helpers;
using SiriusRemoter.Helpers.Renderers;
using SiriusRemoter.Helpers.Upnp.OpenSource;
using SiriusRemoter.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;

namespace SiriusRemoter.Models.Players
{
    public enum PlayerStatus
    {
        Stopped,
        Playing,
        Paused
    }

    /// <summary>
    /// Singleton class, so that its shared during lifetime of app.
    /// </summary>
    public class Player : INotifyPropertyChanged
    {
        #region Constructors

        public Player()
        {
            NavigateUpCommand = new RelayCommand(NavigateUpOneLevel, CanNavigateUp);
            _remotePlayer = new RemotePlayer(this);
            _localPlayer = new LocalPlayer(this);
        }

        #endregion

        #region Members

        private List<NavigationItem> _currentNavigationItems = new List<NavigationItem>();
        private List<NavigationItem> _currentlyPlayingQueue = new List<NavigationItem>();
        private int _currentPlayingIndex;
        private IRenderer _renderer;
        private int _currentNavigationIndex;
        private RemotePlayer _remotePlayer;
        private LocalPlayer _localPlayer;

        //State related
        private PlayerState _state = new PlayerState();

        #endregion

        #region Properties

        public BasePlayer ActivePlayer
        {
            get
            {
                //weird, https://stackoverflow.com/questions/9810471/ternary-expression-with-interfaces-as-a-base-class
                return (_state.TypeOfPlayer == PlayerType.Remote ? (BasePlayer)_remotePlayer : _localPlayer);
            }
        }

        public IRenderer Renderer
        {
            get
            {
                return _renderer;
            }
            set
            {
                _renderer = value;
                _renderer.OnInfoChanged += LastChange_Handler;
                OnPropertyChanged(nameof(Renderer));
            }
        }

        public OpenMediaServer Server { get; set; }

        public Song CurrentlyPlaying
        {
            get
            {
                if (_currentlyPlayingQueue.Count > 0 && CurrentPlayingIndex >= 0 && CurrentPlayingIndex < _currentlyPlayingQueue.Count)
                {
                    return _currentlyPlayingQueue[CurrentPlayingIndex] as Song;
                }
                return null;                
            }
        }

        public Song NextSong
        {
            get
            {
                int nextIndex = CurrentPlayingIndex + 1;
                if (nextIndex >= _currentlyPlayingQueue.Count)
                {
                    nextIndex = 0;
                }
                if (_currentlyPlayingQueue.Count > 0 && nextIndex >= 0)
                {
                    return _currentlyPlayingQueue[nextIndex] as Song;
                }
                return null;
            }
        }

        public string PlayMode
        {
            get
            {
                return _state.PlayMode;
            }
            set
            {
                _state.PlayMode = value;
                OnPropertyChanged(nameof(PlayMode));
            }
        }

        public string CurrentTrackMetaData
        {
            get
            {
                return _state.CurrentTrackMetadata;
            }
            set
            {
                _state.CurrentTrackMetadata = value;
                OnPropertyChanged(nameof(CurrentTrackMetaData));
            }
        }

        public TimeSpan RelativeTrackDuration
        {
            get
            {
                return _state.RelativeTrackDuration;
            }
            set
            {
                _state.RelativeTrackDuration = value;
                OnPropertyChanged(nameof(RelativeTrackDuration));
            }
        }

        public string NextTrackMetaData
        {
            get
            {
                return _state.NextTrackMetaData;
            }
            set
            {
                _state.NextTrackMetaData = value;
                OnPropertyChanged(nameof(NextTrackMetaData));
            }
        }

        public string NumberOfTracks
        {
            get
            {
                return _state.NumberOfTracks;
            }
            set
            {
                if (_state.NumberOfTracks == value)
                {
                    return;
                }
                _state.NumberOfTracks = value;
                OnPropertyChanged(nameof(NumberOfTracks));
            }
        }

        public uint CurrentTrackNumber
        {
            get
            {
                return _state.CurrentTrackNumber;
            }
            set
            {
                if (_state.CurrentTrackNumber == value)
                {
                    return;
                }
                _state.CurrentTrackNumber = value;
                OnPropertyChanged(nameof(CurrentTrackNumber));
            }
        }

        public int CurrentPlayingIndex
        {
            get
            {
                return _currentPlayingIndex;
            }
            private set
            {
                _currentPlayingIndex = GetValidPlayingIndex(value);
                OnPropertyChanged(nameof(CurrentPlayingIndex));
            }
        }

        public int CurrentNavigationIndex
        {
            get
            {
                return _currentNavigationIndex;
            }
            set
            {
                _currentNavigationIndex = value;
                OnPropertyChanged(nameof(CurrentNavigationIndex));
            }
        }

        public List<NavigationItem> CurrentNavigationItems
        {
            get
            {
                return _currentNavigationItems;
            }
            set
            {
                _currentNavigationItems = value;
                OnPropertyChanged(nameof(CurrentNavigationItems));
            }
        }

        #endregion

        #region Commands

        public RelayCommand NavigateUpCommand { get; set; }

        #endregion

        #region Methods

        public void SetupLocalPlayerNavItems(string path)
        {
            _state.SetPlayerType(PlayerType.Local);
            SetQueue(path);
        }

        public NavigationItem GetSelectedItem()
        {
            if (_currentNavigationItems.Count > 0 && _currentNavigationIndex >= 0 && _currentNavigationIndex < _currentNavigationItems.Count)
            {
                return _currentNavigationItems[_currentNavigationIndex];
            }
            return null;
        }

        public void ActivateNavigationItem()
        {
            var selectedItem = GetSelectedItem();
            if (selectedItem == null)
            {
                Console.WriteLine("No Item selected.");
                return;
            }
            if (selectedItem.Type == NavigationTypes.Folder)
            {
                ActivePlayer.NavigateDownOneLevel(selectedItem);
            }
            else if (selectedItem.Type == NavigationTypes.Picture)
            {
                //Stop currently playing item on Renderer
                Stop();
                //Show photo
                _state.SetPictureMode((selectedItem as Photo).SourceUrl);
                //Fire signal to broadcast state has changed
                FireStateChangedEvent();
            }
            else
            {
                Play(selectedItem as Song);
            }
        }

        public void NavigateUpOneLevel(object parameter)
        {
            ActivePlayer.NavigateUpOneLevel();
        }

        public bool CanNavigateUp(object parameter)
        {
            return ActivePlayer.CanNavigateUp();
        }

        public void SetQueue(string query)
        {
            CurrentNavigationItems = ActivePlayer.GetNavigationItems(query);
        }

        public void SetTopQueue()
        {
            //Clear previous media items
            ActivePlayer.ClearPreviousQueries();
            CurrentNavigationItems = null;
            //Navigate to root
            SetQueue(Constants.UPNP_ROOT_DIRECTORY);
        }

        private void UpdateAlbumArtUriAndPlayingInfo(string trackMetadata = null)
        {
            Track track = null;
            if (String.IsNullOrWhiteSpace(trackMetadata))
            {
                if (CurrentlyPlaying != null && CurrentlyPlaying.SongTrack != null)
                {
                    track = CurrentlyPlaying.SongTrack;
                }
            }
            else
            {
                var doc = new XmlDocument();
                doc.LoadXml(trackMetadata);
                var newNode = doc.DocumentElement["item"];
                track = new Track(newNode);
            }
            if (track == null)
            {
                return;
            }
            _state.CurrentTrack = track;
        }

        #region Media Controls Methods

        public void Shuffle()
        {
            PlayMode = Renderer.SetPlayMode("SHUFFLE");
        }

        public void RepeatAll()
        {
            PlayMode = Renderer.SetPlayMode("REPEAT_ALL");
        }

        private int GetValidPlayingIndex(int currentIndex)
        {
            if (currentIndex < 0)
            {
                return _currentlyPlayingQueue.Count - 1;
            }
            else if (currentIndex >= _currentlyPlayingQueue.Count)
            {
                return 0;
            }
            return currentIndex;
        }

        public void PlayPrevious()
        {
            --CurrentPlayingIndex;
            //get selected song from selected item in queue and play it
            var song = _currentlyPlayingQueue[CurrentPlayingIndex] as Song;
            if (song == null)
            {
                //Show Message
                return;
            }
            Play(song, false);
        }

        public void PlayNext()
        {
            ++CurrentPlayingIndex;
            //get selected song from selected item in queue and play it
            var song = _currentlyPlayingQueue[CurrentPlayingIndex] as Song;
            if (song == null)
            {
                //Show Message
                return;
            }
            Play(song, false);
        }

        public void Play()
        {
            if (_currentlyPlayingQueue.Count == 0 || CurrentPlayingIndex < 0 || CurrentPlayingIndex >= _currentlyPlayingQueue.Count)
            {
                return;
            }
            //get selected song from selected item in queue and play it
            var song = _currentlyPlayingQueue[CurrentPlayingIndex] as Song;
            if (song == null)
            {
                //Show Message
                return;
            }
            Play(song);
        }

        public void Play(Song song, bool updatePlayingQueue = true)
        {
            var succeeded = Renderer.Play(song.SongTrack);
            if (!succeeded)
            {
                return;
            }
            if (updatePlayingQueue)
            {
                CurrentPlayingIndex = _currentNavigationIndex;
                _currentlyPlayingQueue = _currentNavigationItems;
            }
            UpdateAlbumArtUriAndPlayingInfo();
            _state.Mode = PlayerMode.Stream;
        }

        public void Pause()
        {
            if (Renderer == null)
            {
                return;
            }
            Renderer.Pause();
        }

        public void Stop()
        {
            if (Renderer == null)
            {
                return;
            }
            Renderer.Stop();
        }

        /// <summary>
        /// Seeks to a position in the current track
        /// </summary>
        /// <param name="seekValue">the time in hh:mm:ss format</param>
        public void Seek(string seekValue)
        {
            if (Renderer == null)
            {
                return;
            }
            Renderer.Seek(seekValue);
        }

        #endregion

        #endregion

        #region Events

        public delegate void StateChangedHandler(PlayerState transportState);
        public event StateChangedHandler OnStateChanged;

        #endregion

        #region Event Handlers

        /// <summary>
        /// All the changes to properties to the Player class should happen here
        /// </summary>
        private void LastChange_Handler(Helpers.Info sender)
        {
            var currentTrack = CurrentlyPlaying == null ? null : CurrentlyPlaying.SongTrack;
            //Update state of Renderer av-transport
            _state._transportState = sender.TransportState;
            PlayMode = Renderer.GetPlayMode();
            //Get Track Info and update associated properties
            var trackInfo = Renderer.GetActiveTrackInfo();
            _state.CurrentSongDuration = trackInfo.TrackDuration;
            CurrentTrackMetaData = trackInfo.TrackMetaData;
            RelativeTrackDuration = trackInfo.RelativeTime;
            NextTrackMetaData = sender.NextTrackMetaData;
            NumberOfTracks = sender.NumberOfTracks;
            CurrentTrackNumber = trackInfo.TrackNumber;

            string currentSongUri = null;
            if (CurrentlyPlaying != null && CurrentlyPlaying.SongTrack != null)
            {
                currentSongUri = CurrentlyPlaying.SongTrack.Uri;
            }
            if (!String.IsNullOrEmpty(currentSongUri))
            {
                if (!trackInfo.TrackUri.Equals(currentSongUri, StringComparison.OrdinalIgnoreCase))
                {
                    CurrentPlayingIndex = Utilities.FindSongByUri(trackInfo.TrackUri, _currentlyPlayingQueue);
                    //TODO: This works but is slow, can this be on a background thread???
                    //Set next song
                    //if (NextSong != null && _transportState == "PLAYING")
                    //{
                    //    SetNextAVTransportURI(NextSong.SongTrack);
                    //}
                }
            }
            //Do not update unless the state is in "Playing" mode
            //or seamless transition
            if (_state._transportState == "PLAYING" || _state._transportState == "")
            {
                UpdateAlbumArtUriAndPlayingInfo(CurrentTrackMetaData);
            }
            //Update volume
            _state.CurrentVolume = Renderer.GetVolume();
            //Fire signal to broadcast state has changed
            FireStateChangedEvent();
        }

        private void FireStateChangedEvent()
        {
            if (OnStateChanged != null)
            {
                OnStateChanged(_state);
            }
        }

        #endregion

        #region Internals

        public enum PlayerMode
        {
            Stream,
            Picture
        }

        public enum PlayerType
        {
            Local,
            Remote
        }

        public class PlayerState
        {
            public PlayerState()
            {
                TypeOfPlayer = PlayerType.Remote;
            }

            #region Members
            
            //AudioItem members
            public TimeSpan CurrentSongDuration;
            public string CurrentTrackMetadata;
            public string NextTrackMetaData;
            public string NumberOfTracks;
            public uint CurrentTrackNumber;
            public TimeSpan RelativeTrackDuration;
            public double CurrentVolume;
            public Track CurrentTrack;

            //Picture members
            public string PictureSource = null;

            //State related
            public string _transportState;
            public string PlayMode;
            public PlayerMode Mode = PlayerMode.Stream;
            public PlayerType TypeOfPlayer { get; private set; }

            #endregion

            #region Properties

            //track related
            public string ArtistName
            {
                get { return CurrentTrack.Artist; }
            }

            public string Title
            {
                get { return CurrentTrack.Title; }
            }

            public string Album
            {
                get { return CurrentTrack.Album; }
            }

            public string AlbumartUri
            {
                get { return CurrentTrack.AlbumartUri; }
            }

            public int Bitrate
            {
                get { return CurrentTrack.BitRate; }
            }

            public string SamplingFrequency
            {
                get { return CurrentTrack.SamplingFrequency; }
            }

            public PlayerStatus TransportState
            {
                get
                {
                    switch (_transportState)
                    {
                        case "PLAYING":
                            return PlayerStatus.Playing;
                        case "PAUSED":
                            return PlayerStatus.Paused;
                        default:
                            return PlayerStatus.Stopped;
                    }
                }
            }

            #endregion

            #region Methods

            public void SetPlayerType(PlayerType playerType)
            {
                TypeOfPlayer = playerType;
            }

            public void SetPictureMode(string url)
            {
                Mode = PlayerMode.Picture;
                PictureSource = url;
            }

            #endregion
        }

        #endregion

        #region INotify Things

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
