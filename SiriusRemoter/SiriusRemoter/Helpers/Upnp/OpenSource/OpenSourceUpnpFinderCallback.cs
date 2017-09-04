using OpenSource.UPnP;

namespace SiriusRemoter.Helpers.Upnp.OpenSource
{
    public class OpenSourceUpnpFinderCallback
    {
        #region Delegates and Events

        public delegate void DeviceAddedEventHandler(UPnPSmartControlPoint cp, UPnPDevice device);

        public event DeviceAddedEventHandler OnDeviceAddedEvent;

        #endregion

        #region IUPnPDeviceFinderCallback implementation

        public void OnDeviceAdded(UPnPSmartControlPoint cp, UPnPDevice device)
        {
            if (OnDeviceAddedEvent != null)
            {
                OnDeviceAddedEvent(cp, device);
            }
        }

        #endregion
    }
}
