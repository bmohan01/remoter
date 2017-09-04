using System;
using System.Text;
using UPNPLib;

namespace SiriusRemoter.Models
{
    public class RemoterUPnpDevice
    {
        #region members

        private const string MEDIA_RENDERER = "mediarenderer";
        private const string MEDIA_SERVER = "mediaserver";
        private UPnPDevice _upnpDevice;

        #endregion

        public RemoterUPnpDevice(UPnPDevice upnpDevice)
        {
            _upnpDevice = upnpDevice;
        }

        #region Properties

        public UPnPDevice UPnpCOMObject
        {
            get
            {
                return _upnpDevice;
            }
        }

        #endregion

        #region Methods

        public bool IsARenderer()
        {
            return (_upnpDevice.Type.ToLower().Contains(MEDIA_RENDERER));
        }

        public bool IsAMediaServer()
        {
            return (_upnpDevice.Type.ToLower().Contains(MEDIA_SERVER));
        }

        public string ListServices()
        {
            StringBuilder result = new StringBuilder();
            try
            {
                foreach (var item in _upnpDevice.Services)
                {
                    var service = item as IUPnPService;
                    if (service != null)
                    {
                        result.Append(service.ServiceTypeIdentifier + '\n');
                        result.Append((service.Id) + '\n');
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return result.ToString();
        }

        #endregion
    }
}
