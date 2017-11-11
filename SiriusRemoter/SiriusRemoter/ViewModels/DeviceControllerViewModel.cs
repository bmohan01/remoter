namespace SiriusRemoter.ViewModels
{
    using CCSWE.Collections.ObjectModel;
    using OpenSource.UPnP;
    using SiriusRemoter.Helpers.Upnp;
    using SiriusRemoter.Helpers.Upnp.OpenSource;
    using System.Collections.Generic;
    using System.ComponentModel;
    using SiriusRemoter.Resources;
    using SiriusRemoter.Helpers.Renderers;
    using System.Windows.Forms;
    using SiriusRemoter.Models.Players;
    using SiriusRemoter.Helpers;
    using SiriusRemoter.Helpers.SearchFramework;
    using System.Windows.Data;
    using System;
    using SiriusRemoter.Models;

    public class DeviceControllerViewModel : INotifyPropertyChanged
    {
        #region INotify Things

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Constructors

        public DeviceControllerViewModel(Player player)
        {
            _player = player;
            _player.PropertyChanged += Player_PropertyChanged;
            SearchForDevicesCommand = new RelayCommand(SearchForDevices);
            BrowseCommand = new RelayCommand(BrowseToFolder);
            _discovery = new OpenUpnpDiscovery();
            _discovery.OnDeviceAddedEvent += DeviceAdded_EventHandler;
        }

        private void Player_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentNavigationItems")
            {
                var player = sender as Player;
                if (player != null)
                {
                    CurrentNavItems = player.CurrentNavigationItems;
                }
            }
        }

        #endregion

        #region Members

        private Player _player;
        private OpenUpnpDiscovery _discovery;
        private SynchronizedObservableCollection<UPnPDevice> _upnpDevices = new SynchronizedObservableCollection<UPnPDevice>();
        private SynchronizedObservableCollection<OpenMediaServer> _serverDevices = new SynchronizedObservableCollection<OpenMediaServer>();
        private SynchronizedObservableCollection<IRenderer> _rendererDevices = new SynchronizedObservableCollection<IRenderer>();
        private SynchronizedObservableCollection<string> _upnpDeviceNames = new SynchronizedObservableCollection<string>();
        private SynchronizedObservableCollection<string> _mediaServerNames = new SynchronizedObservableCollection<string>();
        private SynchronizedObservableCollection<string> _rendererDeviceNames = new SynchronizedObservableCollection<string>();
        private List<NavigationItem> _currentNavItems = new List<NavigationItem>();
        private CollectionView view;
        private bool _hasViewChanged;
        private string _filterText;
        private bool _isNavigable;
        private int _upnpDeviceSelectedIndex = 0;
        private int _rendererSelectedIndex = 0;
        private int _mediaServerSelectedIndex = 0;

        #endregion

        #region Properties

        public string FilterText
        {
            get
            {
                return _filterText;
            }
            set
            {
                _filterText = value;
                OnPropertyChanged(nameof(FilterText));
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

        public List<NavigationItem> CurrentNavItems
        {
            get
            {
                return _currentNavItems;
            }
            set
            {
                _currentNavItems = value;
                _hasViewChanged = true;
                OnPropertyChanged(nameof(CurrentNavItems));
            }
        }

        public int UPnpDeviceSelectedIndex
        {
            get
            {
                return _upnpDeviceSelectedIndex;
            }
            set
            {
                _upnpDeviceSelectedIndex = value;
                OnPropertyChanged(nameof(UPnpDeviceSelectedIndex));
            }
        }

        public int MediaServerSelectedIndex
        {
            get
            {
                return _mediaServerSelectedIndex;
            }
            set
            {
                _mediaServerSelectedIndex = value;
                if (_mediaServerSelectedIndex >= 0)
                {
                    SetUpPlayerServer();
                    //Update browser in playercontroller with list from queue
                    PlayerController.Server = _serverDevices[_mediaServerSelectedIndex];
                    PlayerController.SetTopQueue();
                    IsNavigable = true;
                }
                OnPropertyChanged(nameof(MediaServerSelectedIndex));
            }
        }

        public int RendererSelectedIndex
        {
            get
            {
                return _rendererSelectedIndex;
            }
            set
            {
                _rendererSelectedIndex = value;
                SetUpPlayerRenderer();
                OnPropertyChanged(nameof(RendererSelectedIndex));
            }
        }

        public SynchronizedObservableCollection<string> MediaServerNames
        {
            get
            {
                return _mediaServerNames;
            }
            set
            {
                _mediaServerNames = value;
                OnPropertyChanged(nameof(MediaServerNames));
            }
        }

        public SynchronizedObservableCollection<string> RendererDeviceNames
        {
            get
            {
                return _rendererDeviceNames;
            }
            set
            {
                _rendererDeviceNames = value;
                OnPropertyChanged(nameof(RendererDeviceNames));
            }
        }

        public SynchronizedObservableCollection<string> UPnpDeviceNames
        {
            get
            {
                return _upnpDeviceNames;
            }
            set
            {
                _upnpDeviceNames = value;
                OnPropertyChanged(nameof(UPnpDeviceNames));
            }
        }

        public bool IsNavigable
        {
            get
            {
                return _isNavigable;
            }
            set
            {
                _isNavigable = value;
                OnPropertyChanged(nameof(IsNavigable));
            }
        }

        #endregion

        #region Methods

        private List<string> GetMediaServerUrns()
        {
            var urns = new List<string>();
            urns.Add("urn:schemas-upnp-org:device:MediaServer:1");
            urns.Add("urn:schemas-upnp-org:device:ZonePlayer:1");
            return urns;
        }

        private void ClearNames()
        {
            UPnpDeviceNames.Clear();
            RendererDeviceNames.Clear();
            MediaServerNames.Clear();
        }

        //from method event callback
        private void StartSearch()
        {
            ClearNames();
            
            //local renderer setup
            RendererDeviceNames.Add("Local Renderer");
            _rendererDevices.Add(new LocalRenderer());
            SetUpPlayerRenderer();

            _discovery.StartScan();
        }

        /// <summary>
        /// Search for media servers and populate appropriate list
        /// </summary>
        private void SearchForDevices(object parameter)
        {
            StartSearch();
        }

        private void SetUpPlayerRenderer()
        {
            if (PlayerController == null)
            {
                return;
            }
            //If no item in the renderer list or invalid selection
            if (_rendererDevices.Count == 0 || _rendererSelectedIndex < 0 || _rendererSelectedIndex >= _rendererDevices.Count)
            {
                return;
            }
            var currentRenderer = _rendererDevices[_rendererSelectedIndex];
            PlayerController.Renderer = currentRenderer;
        }

        private void SetUpPlayerServer()
        {
            if (PlayerController == null)
            {
                return;
            }
            //If no item in the server list or invalid selection
            if (_serverDevices.Count == 0 || _mediaServerSelectedIndex < 0 || _mediaServerSelectedIndex >= _serverDevices.Count)
            {
                return;
            }
            var currentServer = _serverDevices[_mediaServerSelectedIndex];
            PlayerController.Server = currentServer;
        }

        /// <summary>
        /// Adds the device passed in if it is a renderer. And then, irrespective of
        /// the device type, checks if it has any embedded devices that are renderer, 
        /// and if so, add them to renderers' list.
        /// </summary>
        private void AddRenderers(UPnPDevice pDevice)
        {
            if (pDevice.DeviceURN == Constants.MEDIA_RENDERER_URN)
            {
                AddRenderer(pDevice);
            }
            foreach (var device in pDevice.EmbeddedDevices)
            {
                if (device.DeviceURN == Constants.MEDIA_RENDERER_URN)
                {
                    AddRenderer(device);
                }
            }
        }

        private void AddRenderer(UPnPDevice pDevice)
        {
            RendererDeviceNames.Add(pDevice.FriendlyName);
            _rendererDevices.Add(new OpenMediaRenderer(pDevice));
            SetUpPlayerRenderer();
        }

        /// <summary>
        /// Adds the device passed in if it is a media server. And then, irrespective of
        /// the device type, checks if it has any embedded devices that are media servers, 
        /// and if so, add them to media servers' list.
        /// </summary>
        private void AddMediaServers(UPnPDevice pDevice)
        {
            if (pDevice.DeviceURN == Constants.MEDIA_SERVER_URN)
            {
                AddMediaServer(pDevice);
            }
            foreach (var device in pDevice.EmbeddedDevices)
            {
                if (device.DeviceURN == Constants.MEDIA_SERVER_URN)
                {
                    AddMediaServer(device);
                }
            }
        }

        private void AddMediaServer(UPnPDevice pDevice)
        {
            MediaServerNames.Add(pDevice.FriendlyName);
            _serverDevices.Add(new OpenMediaServer(pDevice));
            SetUpPlayerServer();
            MediaServerSelectedIndex = 0;
        }

        /// <summary>
        /// Local device
        /// </summary>
        /// <param name="parameter"></param>
        private void BrowseToFolder(object parameter)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                var result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    PlayerController.DirectoryPath = fbd.SelectedPath;
                }
            }
            PlayerController.SetupLocalPlayerNavItems(PlayerController.DirectoryPath);
        }

        public void ExecuteFilter()
        {
            if (CurrentNavItems != null)
            {
                if (view == null || _hasViewChanged)
                {
                    _hasViewChanged = false;
                    SetupFiltering();
                }

                RefreshView();
            }
        }

        public void RefreshView()
        {
            CollectionViewSource.GetDefaultView(CurrentNavItems).Refresh();
        }

        private void SetupFiltering()
        {
            view = (CollectionView)CollectionViewSource.GetDefaultView(CurrentNavItems);
            view.Filter = SearchFilter;
        }

        private bool SearchFilter(object item)
        {
            if (String.IsNullOrEmpty(FilterText))
            {
                return true;
            }
            // Matches the beggining of each word in the name (separated by spaces)
            else
            {
                string[] fragments = (item as NavigationItem).Name.Split(null);
                foreach (var word in fragments)
                {
                    if (word.StartsWith(FilterText, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }

                return false;                
            }
        }

        #endregion

        #region Commands

        public RelayCommand SearchForDevicesCommand { get; set; }
        public RelayCommand BrowseCommand { get; set; }

        #endregion

        #region Event Handlers

        public void DeviceAdded_EventHandler(UPnPSmartControlPoint cp, UPnPDevice pDevice)
        {
            //Add to full device list
            _upnpDevices.Add(pDevice);
            UPnpDeviceNames.Add(pDevice.FriendlyName);
            //Add to MediaServer or MediaRenderer list
            AddRenderers(pDevice);
            AddMediaServers(pDevice);
        }

        #endregion
    }
}
