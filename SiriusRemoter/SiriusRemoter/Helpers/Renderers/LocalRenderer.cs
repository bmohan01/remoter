using System;
using SiriusRemoter.Models;
using System.Windows.Media;
using SiriusRemoter.Resources;

namespace SiriusRemoter.Helpers.Renderers
{
    public class LocalRenderer : IRenderer
    {
        #region Members

        private MediaPlayer _mediaPlayer = new MediaPlayer();
        private string _state = TransportStates.STOPPED;

        #endregion

        #region Constructor

        public LocalRenderer()
        {
            _mediaPlayer.MediaOpened += _mediaPlayer_MediaOpened;
        }

        #endregion

        #region Methods

        public TrackInfo GetActiveTrackInfo()
        {
            return new TrackInfo()
            {
                TrackDuration = (_mediaPlayer.NaturalDuration.HasTimeSpan ? _mediaPlayer.NaturalDuration.TimeSpan : TimeSpan.Zero),
                AbsoluteTime = _mediaPlayer.Position,
                TrackNumber = 1,
                TrackUri = _mediaPlayer.Source.ToString()
            };
        }

        public TimeSpan GetCurrentSongPosition()
        {
            return _mediaPlayer.Position;
        }

        public string GetPlayMode()
        {
            return PlayModes.NORMAL;
        }

        public double GetVolume()
        {
            return _mediaPlayer.Volume * 100;
        }

        public void Pause()
        {
            _mediaPlayer.Pause();
            _state = TransportStates.PAUSED_PLAYBACK;
            FireInfoStateChange();
        }

        public bool Play(Track track)
        {
            //if new stream, play it
            if (_mediaPlayer.Source == null || !track.Uri.Equals(_mediaPlayer.Source.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                _mediaPlayer.Open(new Uri(track.Uri));
            }
            _mediaPlayer.Play();
            _state = TransportStates.PLAYING;
            FireInfoStateChange();
            return true;
        }

        public void Seek(string seekValue)
        {
            var position = TimeSpan.Parse(seekValue);
            _mediaPlayer.Position = position;
        }

        /// <summary>
        /// Sets the volume, values needs to be converted from 0 - 100 value range to 0 - 1 range.
        /// </summary>
        /// <param name="volume">Volume value passed in the rage of 0 - 100</param>
        public void SetVolume(double volume)
        {
            _mediaPlayer.Volume = volume / 100;
        }

        public void Stop()
        {
            _mediaPlayer.Stop();
            _state = TransportStates.STOPPED;
            //change state
            FireInfoStateChange();
        }

        string IRenderer.SetPlayMode(string newPlayMode)
        {
            return _state;
        }

        #endregion

        #region Event Handling

        public event InfoChangedHandler OnInfoChanged;

        /// <summary>
        /// Duration and some other properties are updated when the media opened
        /// event is finished and the handler is fired.
        /// </summary>
        private void _mediaPlayer_MediaOpened(object sender, EventArgs e)
        {
            try
            {
                FireInfoStateChange();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void FireInfoChangedEvent(Info info)
        {
            if (OnInfoChanged != null)
            {
                OnInfoChanged(info);
            }
        }

        private void FireInfoStateChange()
        {
            //change state
            var info = new Info()
            {
                TransportState = _state,
                NumberOfTracks = "1"
            };
            FireInfoChangedEvent(info);
        }

        #endregion
    }
}
