using Newtonsoft.Json.Linq;
using SiriusRemoter.Helpers;
using System.ComponentModel;
using System.IO;

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
            if (File.Exists(Utilities.TokenFilePath))
            {
                var token = JObject.Parse(File.ReadAllText(Utilities.TokenFilePath).Trim());
                DiscogsKey = token["DiscogsToken"].ToString();
                MusixMatchKey = token["MusixMatchToken"].ToString();
            }
        }

        private void SaveTokens(object parameters)
        {
            var keysText = new JObject(new JProperty("DiscogsToken", _discogsToken), new JProperty("MusixMatchToken", _musixToken));
            File.WriteAllText(Utilities.TokenFilePath, keysText.ToString());
        }
    }
}
