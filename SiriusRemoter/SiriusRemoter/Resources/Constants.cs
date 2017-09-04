namespace SiriusRemoter.Resources
{
    public enum NavigationTypes
    {
        Folder,
        AudioItem,
        Picture,
        Video
    }

    public static class Constants
    {
        public const string CONTAINER_TAG = "object.container";
        public const string SONG_TAG = "object.item.audioItem.musicTrack";
        public const string IMAGE_TAG = "object.item.imageItem.photo";
        public const string VIDEO_TAG = "object.item.videoItem";
        public const string UPNP_ROOT_DIRECTORY = "0";
        public const string UPNP_TYPE_TAG = "upnp:class";
        public const string MEDIA_SERVER_URN = "urn:schemas-upnp-org:device:MediaServer:1";
        public const string MEDIA_RENDERER_URN = "urn:schemas-upnp-org:device:MediaRenderer:1";
    }

    public static class PlayModes
    {
        public const string NORMAL = "NORMAL";
        public const string SHUFFLE = "SHUFFLE";
        public const string REPEAT = "REPEAT";
    }

    public static class TransportStates
    {
        public const string STOPPED = "STOPPED";
        public const string PLAYING = "PLAYING";
        public const string TRANSITIONING = "TRANSITIONING";
        public const string PAUSED_PLAYBACK = "PAUSED_PLAYBACK";
        public const string PAUSED_RECORDING = "PAUSED_RECORDING";
        public const string RECORDING = "RECORDING";
        public const string NO_MEDIA_PRESENT = "NO_MEDIA_PRESENT";
    }
}