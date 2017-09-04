using OpenSource.UPnP;
using System.Linq;

namespace SiriusRemoter.Helpers.Upnp.OpenSource
{
    public class OpenMediaServer
    {
        private UPnPDevice _mediaServer;
        private UPnPService _contentDirectory;

        public OpenMediaServer(UPnPDevice device)
        {
            _mediaServer = device;
        }

        public UPnPDevice MediaServer
        {
            get
            {
                return _mediaServer;
            }
        }

        public UPnPService ContentDirectory
        {
            get
            {
                if (_contentDirectory != null)
                    return _contentDirectory;
                if (_mediaServer == null)
                    return null;
                _contentDirectory = _mediaServer.GetService("urn:upnp-org:serviceId:ContentDirectory");                
                return _contentDirectory;
            }
        }
    }
}
