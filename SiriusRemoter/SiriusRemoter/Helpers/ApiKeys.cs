using Newtonsoft.Json.Linq;
using System.IO;
using System.Windows;

namespace SiriusRemoter.Helpers
{
    public class ApiKeys
    {
        private const string MusixKeyName = "MusixMatchKey";
        private const string DiscogsKeyName = "DiscogsKey";
        private static ApiKeys _instance;

        public static ApiKeys Instance
        {
            get
            {
                return _instance ?? new ApiKeys();
            }
        }

        public string MusixMatchKey
        {
            get
            {
                return Application.Current.Resources[MusixKeyName].ToString();
            }
        }

        public string DiscogsKey
        {
            get
            {
                return Application.Current.Resources[DiscogsKeyName].ToString();
            }
        }

        private ApiKeys()
        {
            SetKeys();
        }

        public void SetKeys()
        {
            if (File.Exists(Utilities.TokenFilePath))
            {
                var token = JObject.Parse(File.ReadAllText(Utilities.TokenFilePath).Trim());

                Application.Current.Resources[MusixKeyName] = token["MusixMatchToken"].ToString();
                Application.Current.Resources[DiscogsKeyName] = token["DiscogsToken"].ToString();
            }
        }

        public void SaveKeys(string discogsKey, string musixKey)
        {
            var keysText = new JObject(new JProperty("DiscogsToken", discogsKey), new JProperty("MusixMatchToken", musixKey));
            File.WriteAllText(Utilities.TokenFilePath, keysText.ToString());
        }
    }
}
