
using SiriusRemoter.Models;
using System;

namespace SiriusRemoter.Helpers.Renderers
{

    public delegate void InfoChangedHandler(Info info);

    public interface IRenderer
    {
        TrackInfo GetActiveTrackInfo();

        TimeSpan GetCurrentSongPosition();

        double GetVolume();

        void SetVolume(double volume);

        string GetPlayMode();

        string SetPlayMode(string newPlayMode);

        bool Play(Track track);

        void Pause();

        void Stop();

        void Seek(string seekValue);

        event InfoChangedHandler OnInfoChanged;
    }
}
