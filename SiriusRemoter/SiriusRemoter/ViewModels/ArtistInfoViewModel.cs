﻿using Newtonsoft.Json.Linq;
using SiriusRemoter.Helpers;
using SiriusRemoter.Models.Players;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text;
using static SiriusRemoter.Models.Players.Player;

namespace SiriusRemoter.ViewModels
{
    public enum NextImageDirection
    {
        Next,
        Previous
    }

    public class ArtistInfoViewModel : INotifyPropertyChanged
    {
        #region INotify things

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

        public ArtistInfoViewModel(Player player)
        {
            ObtainToken();
            //Subscribe to Player's State engine event changes
            player.OnStateChanged += Player_OnStateChanged;
        }

        #region Commands

        public RelayCommand SaveTokenCommand { get; set; }

        #endregion

        #region Members

        private List<string> _imageUrls;
        private int _currentImageIndex;
        private string _activeImage;
        private string _artistName;
        private string _artistBio;
        private string _songTitle;
        private string _lyrics;
        private string _members;
        private string _nameVariations;
        private bool _artistBioBusy = true;
        private bool _membersBusy = true;
        private bool _nameVariationsBusy = true;
        private string _tokenCode;
        private const string UserAgent = "Remoter 1.0";
        private bool _expandMetadata;

        #endregion

        #region Properties

        public string Lyrics
        {
            get
            {
                return _lyrics;
            }
            set
            {
                _lyrics = value;
                OnPropertyChanged(nameof(Lyrics));
            }
        }

        public String SongTitle
        {
            get
            {
                return _songTitle;
            }
            set
            {
                _songTitle = value;
                GetLyrics(ArtistName, _songTitle);
                OnPropertyChanged(nameof(SongTitle));
            }
        }

        public bool ExpandMetadata
        {
            get
            {
                return _expandMetadata;
            }
            set
            {
                _expandMetadata = value;
                OnPropertyChanged(nameof(ExpandMetadata));
            }
        }

        public string ActiveImage
        {
            get
            {
                return _activeImage;
            }
            set
            {
                if (_activeImage == value)
                {
                    return;
                }
                _activeImage = value;
                OnPropertyChanged(nameof(ActiveImage));
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
                GetArtistInfoAsync(_artistName);
                OnPropertyChanged(nameof(ArtistName));
            }
        }

        public string ArtistNameVariations
        {
            get
            {
                return _nameVariations;
            }
            set
            {
                _nameVariations = value;
                ArtistNameVariationsBusy = false;
                OnPropertyChanged(nameof(ArtistNameVariations));
            }
        }

        public bool ArtistNameVariationsBusy
        {
            get
            {
                return _nameVariationsBusy;
            }
            set
            {
                _nameVariationsBusy = value;
                OnPropertyChanged(nameof(ArtistNameVariationsBusy));
            }
        }

        public string ArtistBio
        {
            get
            {
                return _artistBio;
            }
            set
            {
                _artistBio = value;
                ArtistBioBusy = false;
                OnPropertyChanged(nameof(ArtistBio));
            }
        }

        public bool ArtistBioBusy
        {
            get
            {
                return _artistBioBusy;
            }
            set
            {
                _artistBioBusy = value;
                OnPropertyChanged(nameof(ArtistBioBusy));
            }
        }

        public string Members
        {
            get
            {
                return _members;
            }
            set
            {
                _members = value;
                MembersBusy = false;
                OnPropertyChanged(nameof(Members));
            }
        }

        public bool MembersBusy
        {
            get
            {
                return _membersBusy;
            }
            set
            {
                _membersBusy = value;
                OnPropertyChanged(nameof(MembersBusy));
            }
        }

        #endregion

        #region Methods

        public void Initialize()
        {
            _tokenCode = ApiKeys.Instance.DiscogsKey;
            GetArtistInfoAsync(_artistName);
        }

        private void ObtainToken()
        {
            ExpandMetadata = true;
            GetArtistInfoAsync(_artistName);
        }

        public void SetNextImage(NextImageDirection imageDirection)
        {
            if (imageDirection == NextImageDirection.Next)
            {
                ++_currentImageIndex;
                if (_currentImageIndex >= _imageUrls.Count)
                {
                    _currentImageIndex = 0;
                }
            }
            else
            {
                --_currentImageIndex;
                if (_currentImageIndex < 0)
                {
                    _currentImageIndex = _imageUrls.Count - 1;
                }
            }
            ActiveImage = GetCurrentImageUrl();
        }

        private string GetCurrentImageUrl()
        {
            return _imageUrls[_currentImageIndex];
        }

        private string GetArtistNameVariations(JObject token)
        {
            try
            {
                var nameVariations = token.SelectToken("namevariations");
                string aliases = "";
                foreach (var item in nameVariations)
                {
                    aliases += item.ToString() + " ,";
                }
                return aliases;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        private string GetArtistIdFromName(string name)
        {
            try
            {
                string searchQuery = $"https://api.discogs.com/database/search?q={name}&token={_tokenCode}";
                var req = (HttpWebRequest)WebRequest.Create(searchQuery);
                req.UserAgent = UserAgent;
                var response = (HttpWebResponse)req.GetResponse();
                var responseStream = response.GetResponseStream();
                var sr = new StreamReader(responseStream, Encoding.UTF8);
                var token = JObject.Parse(sr.ReadToEnd());
                var firstSreachResult = token.SelectToken("results").First;
                return firstSreachResult.SelectToken("id").ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }            
        } 
        
        private string GetArtistBio(JObject token)
        {
            try
            {
                return token.SelectToken("profile").ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        private string GetArtistMembers(JObject token)
        {
            try
            {
                var members =  token.SelectToken("members");
                string membersStr = "";
                foreach (var member in members)
                {
                    membersStr += member.SelectToken("name").ToString() + " ,";
                }
                return membersStr.Substring(0, membersStr.Length - 1);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        private List<string> GetArtistPictures(JObject token)
        {
            try
            {
                var images = token.SelectToken("images");
                var imageUrls = new List<string>();
                foreach (var item in images)
                {
                    imageUrls.Add(item["uri"].ToString());
                }
                return imageUrls;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        private void ResetArtistInfo()
        {
            EmptyArtistInfo();
            SetBusyFlags();
        }

        private void EmptyArtistInfo()
        {
            ArtistBio = String.Empty;
            Members = String.Empty;
            ArtistNameVariations = String.Empty;
        }

        private void SetBusyFlags()
        {
            MembersBusy = true;
            ArtistBioBusy = true;
            ArtistNameVariationsBusy = true;
        }

        private void GetLyrics(string artistName, string songName)
        {
            var info = new Lyric.TrackInfo(artistName, songName);
            string urlRequest = $"{Lyric.UrlPrefix}/track.lyrics.get?apikey={ApiKeys.Instance.MusixMatchKey}&track_id={info.TrackId}&commontrack_id={info.CommonTrackId}";
            var req = (HttpWebRequest)WebRequest.Create(urlRequest);
            var state = new RequestState();
            state.Request = req;
            IAsyncResult result = req.BeginGetResponse(new AsyncCallback(LyricsAsyncCallback), state);
        }

        private void LyricsAsyncCallback(IAsyncResult result)
        {
            RequestState state = (RequestState)result.AsyncState;
            HttpWebRequest request = state.Request;
            // End the Asynchronous response and get the actual resonse object
            var webResponse = request.GetResponse();
            var infoResponseStream = webResponse.GetResponseStream();
            using (var sr = new StreamReader(infoResponseStream, Encoding.UTF8))
            {
                string lyricsBody = "";
                var token = JObject.Parse(sr.ReadToEnd());
                var status_code = token["message"]["header"]["status_code"].ToString();
                if (status_code == "200")
                {
                    //get first search result's id
                    var firstResult = token["message"]["body"]["lyrics"];
                    lyricsBody = firstResult["lyrics_body"].ToString();
                }
                Lyrics = lyricsBody;
            }
        }

        public void GetArtistInfoAsync(string name)
        {
            try
            {
                //Reset artistinfo. Set busy states
                ResetArtistInfo();
                //Fetch Artist Info Asynchronously
                var request = (HttpWebRequest)WebRequest.Create($"https://api.discogs.com/database/search?q={name}&token={_tokenCode}");
                request.UserAgent = UserAgent;
                request.Method = "GET";
                request.Proxy = null;
                var state = new RequestState();
                state.Request = request;
                IAsyncResult result = request.BeginGetResponse(new AsyncCallback(ArtistIdAsyncCallback), state);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private string GetArtistId(IAsyncResult result)
        {
            try
            {
                // Get and fill the RequestState
                RequestState state = (RequestState)result.AsyncState;
                HttpWebRequest request = state.Request;
                // End the Asynchronous response and get the actual resonse object
                var response = request.GetResponse();
                var responseStream = response.GetResponseStream();
                var sr = new StreamReader(responseStream, Encoding.UTF8);
                var token = JObject.Parse(sr.ReadToEnd());
                //get first search result's id
                var firstSreachResult = token.SelectToken("results").First;
                return firstSreachResult.SelectToken("id").ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        private void ArtistIdAsyncCallback(IAsyncResult result)
        {
            try
            {
                string artistId = GetArtistId(result);
                if (String.IsNullOrWhiteSpace(artistId))
                {
                    Console.WriteLine("No Id found for artist");
                    return;
                }
                //Get Artist Bio, Member, Aliases variations
                //make request
                var req = (HttpWebRequest)WebRequest.Create($"https://api.discogs.com/artists/{artistId}?token={_tokenCode}");
                req.UserAgent = UserAgent;
                var webResponse = (HttpWebResponse)req.GetResponse();
                var infoResponseStream = webResponse.GetResponseStream();
                using (var sr = new StreamReader(infoResponseStream, Encoding.UTF8))
                {
                    var token = JObject.Parse(sr.ReadToEnd());
                    //Populate properties from response
                    ArtistBio = GetArtistBio(token);
                    Members = GetArtistMembers(token);
                    ArtistNameVariations = GetArtistNameVariations(token);
                    _imageUrls = GetArtistPictures(token);
                }
                _currentImageIndex = 0;
                ActiveImage = null;
                ActiveImage = GetCurrentImageUrl();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                // Error handling
                RequestState state = (RequestState)result.AsyncState;
                if (state.Response != null)
                    state.Response.Close();
            }
        }

        public class RequestState
        {
            public int BufferSize { get; private set; }
            public StringBuilder ResponseContent { get; set; }
            public byte[] BufferRead { get; set; }
            public HttpWebRequest Request { get; set; }
            public HttpWebResponse Response { get; set; }
            public Stream ResponseStream { get; set; }

            public RequestState()
            {
                BufferSize = 1024;
                BufferRead = new byte[BufferSize];
                ResponseContent = new StringBuilder();
                Request = null;
                ResponseStream = null;
            }
        }

        #endregion

        #region Event handlers

        private void Player_OnStateChanged(PlayerState state)
        {
            try
            {
                string previousArtistName = ArtistName;
                ArtistName = state.ArtistName;

                //Force Song Title change if the artist changed, or if the song title changed 
                if (ArtistName != previousArtistName || SongTitle != state.CurrentTrack.Title)
                {
                    SongTitle = state.CurrentTrack.Title;
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
