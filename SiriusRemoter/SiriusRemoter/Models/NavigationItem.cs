using SiriusRemoter.Resources;
using System.ComponentModel;
using System.IO;
using System.Xml;

namespace SiriusRemoter.Models
{
    public abstract class NavigationItem : INotifyPropertyChanged
    {
        #region Members

        private const string PARENT_ID_TAG = "parentID";
        private const string ID_TAG = "id";
        private const string NAME_TAG = "dc:title";
        public readonly string ItemPath;
        public readonly string ParentId;
        private string _rawType;
        private NavigationTypes _type;
        public string _name;

        #endregion

        #region Constructors

        public NavigationItem()
        {
            Type = GetEntityType();
        }

        public NavigationItem(XmlNode node) : this()
        {
            ItemPath = node.Attributes[ID_TAG].Value;
            ParentId = node.Attributes[PARENT_ID_TAG].Value;
            _rawType = node[Constants.UPNP_TYPE_TAG].InnerText;
            Name = node[NAME_TAG].InnerText;
        }

        public NavigationItem(string path, string name) : this()
        {
            Name = name;
            ItemPath = path;
        }

        #endregion

        #region properties

        public NavigationTypes Type
        {
            get
            {
                return _type;
            }
            set
            {
                if (_type == value)
                {
                    return;
                }
                _type = value;
                OnPropertyChanged(nameof(Type));
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (_name == value)
                {
                    return;
                }
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        #endregion

        #region Methods

        public abstract NavigationTypes GetEntityType();

        #endregion

        #region INotify Things

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
