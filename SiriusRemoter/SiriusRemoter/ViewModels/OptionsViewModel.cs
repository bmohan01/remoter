using SiriusRemoter.Helpers;
using SiriusRemoter.Models.Players;
using System.ComponentModel;

namespace SiriusRemoter.ViewModels
{
    public class OptionsViewModel : INotifyPropertyChanged
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

        public OptionsViewModel()
        {
            Initialize();
            SaveCommand = new RelayCommand(SaveTokens);
        }

        #region Commands

        public RelayCommand SaveCommand { get; set; }

        #endregion

        #region Members

        private string _discogsToken;
        private string _musixToken;

        #endregion

        #region Property

        public string MusixMatchKey
        {
            get
            {
                return _musixToken;
            }
            set
            {
                _musixToken = value;
                OnPropertyChanged(nameof(MusixMatchKey));
            }
        }

        public string DiscogsKey
        {
            get
            {
                return _discogsToken;
            }
            set
            {
                _discogsToken = value;
                OnPropertyChanged(nameof(MusixMatchKey));
            }
        }

        #endregion

        private void Initialize()
        {
            DiscogsKey = ApiKeys.Instance.DiscogsKey;
            MusixMatchKey = ApiKeys.Instance.MusixMatchKey;
        }

        private void SaveTokens(object parameters)
        {
            ApiKeys.Instance.SaveKeys(DiscogsKey, MusixMatchKey);
        }
    }
}
