using OpenSource.UPnP;
using SiriusRemoter.Helpers.Renderers;
using SiriusRemoter.Models;
using System;
using System.IO;
using System.Xml.Linq;

namespace SiriusRemoter.Helpers.Upnp.OpenSource
{
    public class OpenMediaRenderer : IRenderer
    {
        private UPnPDevice _mediaRenderer;
        private UPnPService _renderingControl;
        private UPnPService _avTransport;

        public OpenMediaRenderer(UPnPDevice device)
        {
            _mediaRenderer = device;
            SubscribeToLastChangeEvent();
        }

        public UPnPDevice MediaRenderer
        {
            get
            {
                return _mediaRenderer;
            }
        }

        public UPnPService RenderingControl
        {
            get
            {
                if (_renderingControl != null)
                    return _renderingControl;
                if (_mediaRenderer == null)
                    return null;
                _renderingControl = _mediaRenderer.GetService("urn:upnp-org:serviceId:RenderingControl");
                return _renderingControl;
            }
        }

        public UPnPService AVTransport
        {
            get
            {
                if (_avTransport != null)
                    return _avTransport;
                if (_mediaRenderer == null)
                    return null;
                _avTransport = _mediaRenderer.GetService("urn:upnp-org:serviceId:AVTransport");
                return _avTransport;
            }
        }

        #region Methods

        /// <summary>
        /// Gets the volume of the current renderer
        /// </summary>
        public double GetVolume()
        {
            if (RenderingControl == null)
            {
                return 0;
            }
            var arguments = new UPnPArgument[3];
            arguments[0] = new UPnPArgument("InstanceID", 0u);
            arguments[1] = new UPnPArgument("Channel", "Master");
            arguments[2] = new UPnPArgument("CurrentVolume", 0u);
            RenderingControl.InvokeSync("GetVolume", arguments);
            return Convert.ToDouble(arguments[2].DataValue);
        }

        /// <summary>
        /// Sets the volume of the current renderer
        /// </summary>
        public void SetVolume(double volume)
        {
            if (RenderingControl == null)
            {
                return;
            }
            var arguments = new UPnPArgument[3];
            arguments[0] = new UPnPArgument("InstanceID", 0u);
            arguments[1] = new UPnPArgument("Channel", "Master");
            arguments[2] = new UPnPArgument("DesiredVolume", (UInt16)volume);
            RenderingControl.InvokeSync("SetVolume", arguments);
        }

        /// <summary>
        /// Gets the position (relative time) of the currently playing song
        /// </summary>
        public TimeSpan GetCurrentSongPosition()
        {
            var trackInfo = GetActiveTrackInfo();
            if (trackInfo == null)
            {
                return TimeSpan.Zero;
            }
            return trackInfo.RelativeTime;
        }

        /// <summary>
        /// Get TrackInfo by querying Renderer's AvTransport service
        /// </summary>
        public TrackInfo GetActiveTrackInfo()
        {
            if (AVTransport == null)
            {
                return null;
            }
            var arguments = new UPnPArgument[9];
            arguments[0] = new UPnPArgument("InstanceID", 0u);
            arguments[1] = new UPnPArgument("Track", 0u);
            arguments[2] = new UPnPArgument("TrackDuration", null);
            arguments[3] = new UPnPArgument("TrackMetaData", null);
            arguments[4] = new UPnPArgument("TrackURI", null);
            arguments[5] = new UPnPArgument("RelTime", null);
            arguments[6] = new UPnPArgument("AbsTime", null);
            arguments[7] = new UPnPArgument("RelCount", 0);
            arguments[8] = new UPnPArgument("AbsCount", 0);
            AVTransport.InvokeSync("GetPositionInfo", arguments);
            return new TrackInfo(arguments);
        }

        private void SetNextAVTransportURI(Track track)
        {
            var arguments = new UPnPArgument[3];
            arguments[0] = new UPnPArgument("InstanceID", 0u);
            arguments[1] = new UPnPArgument("NextURI", track.Uri);
            arguments[2] = new UPnPArgument("NextURIMetaData", track.MetaData);
            AVTransport.InvokeAsync("SetNextAVTransportURI", arguments);
        }





        /// <summary>
        /// Get current play mode
        /// </summary>
        /// <remarks>
        /// Returns one of Repeat, Shuffle, Repeat One, Random, etc
        /// </remarks>
        public string GetPlayMode()
        {
            if (AVTransport == null)
            {
                return null;
            }
            var arguments = new UPnPArgument[3];
            arguments[0] = new UPnPArgument("InstanceID", 0u);
            arguments[1] = new UPnPArgument("PlayMode", 0u);
            arguments[2] = new UPnPArgument("RecQualityMode", null);
            AVTransport.InvokeSync("GetTransportSettings", arguments);
            return arguments[1].DataValue.ToString();
        }

        /// <summary>
        /// Sets current play mode 
        /// Check if this actually changes mode or not
        /// </summary>
        /// <return>
        /// Returns the playmode after the change has been invoked
        /// </return>
        public string SetPlayMode(string newPlayMode)
        {
            try
            {
                if (AVTransport == null)
                {
                    return null;
                }
                var arguments = new UPnPArgument[2];
                arguments[0] = new UPnPArgument("InstanceID", 0u);
                arguments[1] = new UPnPArgument("NewPlayMode", newPlayMode);
                AVTransport.InvokeSync("SetPlayMode", arguments);
                //Have to set this here since changing the playmode doesnt trigger a last change state change
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return GetPlayMode();
        }

        private void SetAVTransportURI(Track track)
        {
            SetAVTransportURI(track.Uri, track.MetaData);
        }

        private void SetAVTransportURI(string uri, string metaData)
        {
            if (AVTransport == null)
            {
                return;
            }


            var arguments = new UPnPArgument[3];
            arguments[0] = new UPnPArgument("InstanceID", 0u);
            arguments[1] = new UPnPArgument("CurrentURI", uri);
            arguments[2] = new UPnPArgument("CurrentURIMetaData", metaData);
            //AVTransport.InvokeAsync("SetAVTransportURI", arguments);
            var res = AVTransport.InvokeSync("SetAVTransportURI", arguments);
        }

        public bool Play(Track track)
        {
            //Play Song
            SetAVTransportURI(track);
            return PlayCurrentTrack();
            //TODO get this to work
            //Exceptions cause huge lag, fix before uncommenting
            //Enqueue other songs in list
            //foreach (var item in _currentlyPlayingQueue)
            //{
            //    var song = item as Song;
            //    if (song == null)
            //    {
            //        continue;
            //    }
            //    Enqueue(song.SongTrack);
            //}
            //var test = GetPlaylistInfo();
        }

        private bool PlayCurrentTrack()
        {
            try
            {
                //Play on renderer
                var arguments = new UPnPArgument[2];
                arguments[0] = new UPnPArgument("InstanceID", 0u);
                arguments[1] = new UPnPArgument("Speed", "1");
                AVTransport.InvokeAsync("Play", arguments);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }

        public void Pause()
        {
            var arguments = new UPnPArgument[1];
            arguments[0] = new UPnPArgument("InstanceID", 0u);
            AVTransport.InvokeAsync("Pause", arguments);
        }

        public void Stop()
        {
            var arguments = new UPnPArgument[1];
            arguments[0] = new UPnPArgument("InstanceID", 0u);
            AVTransport.InvokeAsync("Stop", arguments);
        }

        /// <summary>
        /// Seeks to a position in the current track
        /// </summary>
        /// <param name="seekValue">the time in hh:mm:ss format</param>
        public void Seek(string seekValue)
        {
            var arguments = new UPnPArgument[3];
            arguments[0] = new UPnPArgument("InstanceID", 0u);
            //arguments[1] = new UPnPArgument("Unit", "ABS_TIME");
            arguments[1] = new UPnPArgument("Unit", "REL_TIME");
            arguments[2] = new UPnPArgument("Target", seekValue);
            AVTransport.InvokeAsync("Seek", arguments);
        }

        private XElement ParseChangeXML(string newState)
        {
            var xEvent = XElement.Parse(newState);
            XNamespace ns = xEvent.Attribute("xmlns").Value;
            XNamespace r = "urn:schemas-rinconnetworks-com:metadata-1-0/";
            var instance = xEvent.Element(ns + "InstanceID");
            // We can receive other types of change events here.
            if (instance.Element(ns + "TransportState") == null)
            {
                return null;
            }
            return instance;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// The "LastChange" event will get fired whenever anything 
        /// changes in the state of the media renderer object.
        /// This is the place where we subscribe to the event.
        /// </summary>
        public void SubscribeToLastChangeEvent()
        {
            AVTransport.Subscribe(600, (service, subscribeok) =>
            {
                if (!subscribeok)
                    return;
                var lastChangeStateVariable = service.GetStateVariableObject("LastChange");
                lastChangeStateVariable.OnModified += LastChange_Handler;
            });
        }

        private void LastChange_Handler(UPnPStateVariable sender, object value)
        {
            var newState = sender.Value;
            Console.WriteLine(newState);
            var instanceXml = ParseChangeXML((string)newState);
            
            //Update state of Renderer av-transport
            string transportState = Utilities.TryGetAttributeFromTrackInstance(instanceXml, "TransportState");
            string nextTrackMetaData = Utilities.TryGetAttributeFromTrackInstance(instanceXml, "NextTrackMetaData");
            string numberOfTracks = Utilities.TryGetAttributeFromTrackInstance(instanceXml, "NumberOfTracks");
            var info = new Info() { TransportState = transportState, NextTrackMetaData = nextTrackMetaData, NumberOfTracks = numberOfTracks };
            FireInfoChangedEvent(info);
        }

        private void FireInfoChangedEvent(Info info)
        {
            if (OnInfoChanged != null)
            {
                OnInfoChanged(info);
            }
        }

        #endregion

        #region Events

        public event InfoChangedHandler OnInfoChanged;

        #endregion


        #region TODO Items/Stubs?

        /// <summary>
        /// TODO: Does this work for any renderer?
        /// Enqueues a track in the current avTransport Queue
        /// </summary>
        public void Enqueue(Track track, bool asNext = false)
        {
            try
            {
                if (AVTransport == null)
                {
                    return;
                }
                var arguments = new UPnPArgument[8];
                arguments[0] = new UPnPArgument("InstanceID", 0u);
                arguments[1] = new UPnPArgument("EnqueuedURI", track.Uri);
                arguments[2] = new UPnPArgument("EnqueuedURIMetaData", track.MetaData);
                arguments[3] = new UPnPArgument("DesiredFirstTrackNumberEnqueued", 0u);
                arguments[4] = new UPnPArgument("EnqueueAsNext", asNext);
                arguments[5] = new UPnPArgument("FirstTrackNumberEnqueued", null);
                arguments[6] = new UPnPArgument("NumTracksAdded", null);
                arguments[7] = new UPnPArgument("NewQueueLength", null);
                AVTransport.InvokeSync("AddURIToQueue", arguments);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        /// <summary>
        /// TODO: Does this work for any renderer?
        /// </summary>
        /// <returns></returns>
        private string GetPlaylistInfo()
        {
            string playListInfo = null;
            var arguments = new UPnPArgument[3];
            arguments[0] = new UPnPArgument("InstanceID", 0u);
            arguments[1] = new UPnPArgument("PlaylistType", "Streaming");
            arguments[2] = new UPnPArgument("PlaylistInfo", playListInfo);
            AVTransport.InvokeAsync("GetPlaylistInfo", arguments);
            return arguments[2].ToString();
        }

        #endregion
    }
}
