using OpenSource.UPnP;
using System;

namespace SiriusRemoter.Models
{
    public class TrackInfo
    {
        public uint InstanceId { get; set; }
        public uint TrackNumber { get; set; }
        public TimeSpan TrackDuration { get; set; }
        public string TrackMetaData { get; set; }
        public string TrackUri { get; set; }
        public TimeSpan RelativeTime { get; set; }
        public TimeSpan AbsoluteTime { get; set; }
        public int AbsoluteCount { get; set; }

        public TrackInfo(UPnPArgument[] trackData)
        {
            if (trackData == null)
            {
                return;
            }
            InstanceId = (uint)trackData[0].DataValue;
            TrackNumber = (uint)trackData[1].DataValue;
            TimeSpan duration;
            TimeSpan.TryParse((string)trackData[2].DataValue, out duration);
            TrackDuration = duration;
            TrackMetaData = (string)trackData[3].DataValue;
            TrackUri = (string)trackData[4].DataValue;
            TimeSpan relTime;
            TimeSpan.TryParse((string)trackData[5].DataValue, out relTime);
            RelativeTime = relTime;
            TimeSpan absTime;
            TimeSpan.TryParse((string)trackData[6].DataValue, out absTime);
            AbsoluteTime = absTime;
            AbsoluteCount = Convert.ToInt32(trackData[7].DataValue);
        }

        public TrackInfo()
        { }
    }
}
