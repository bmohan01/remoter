using CCSWE.Collections.ObjectModel;
using SiriusRemoter.Helpers;
using OpenSource.UPnP;
using SiriusRemoter.Helpers.Upnp;
using SiriusRemoter.Helpers.Upnp.OpenSource;
using System.Collections.Generic;
using System.ComponentModel;
using SiriusRemoter.Resources;
using SiriusRemoter.Helpers.Renderers;
using System.Windows.Forms;
using System.IO;

namespace SiriusRemoter.ViewModels
{
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
            SearchForDevicesCommand = new RelayCommand(SearchForDevices);
            BrowseCommand = new RelayCommand(BrowseToFolder);
            _discovery = new OpenUpnpDiscovery();
            _discovery.OnDeviceAddedEvent += DeviceAdded_EventHandler;
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
        private int _upnpDeviceSelectedIndex = 0;
        private int _rendererSelectedIndex = 0;
        private int _mediaServerSelectedIndex = 0;
        private bool _isNavigable;
        private string _directoryPath;

        #endregion

        #region Properties

        public string DirectoryPath
        {
            get
            {
                return _directoryPath;
            }
            set
            {
                _directoryPath = value;
                OnPropertyChanged(nameof(DirectoryPath));
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
                    DirectoryPath = fbd.SelectedPath;
                }
            }
            PlayerController.SetupLocalPlayerNavItems(DirectoryPath);
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
