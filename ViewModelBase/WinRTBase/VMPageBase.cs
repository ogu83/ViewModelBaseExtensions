using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.ApplicationModel;
using Windows.UI.Popups;
using System;

namespace WinRTBase
{
    public abstract class VMPageBase : VMBase
    {
        public VMPageBase()
        {
            setVersion();
        }

        protected static string __VERSION;
        [JsonIgnore()]
        [XmlIgnore()]
        public string Version { get; set; }
        protected void setVersion()
        {
            if (string.IsNullOrEmpty(__VERSION))
            {
                PackageVersion version = Package.Current.Id.Version;
                Version = __VERSION = string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
            }
            else
                Version = __VERSION;

            NotifyPropertyChanged("Version");
        }

        protected volatile bool _isBusy;
        [JsonIgnore()]
        [XmlIgnore()]
        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                if (_isBusy == value) return;
                _isBusy = value;
                NotifyPropertyChanged("IsBusy");
            }
        }

        public virtual async Task ShowMessageBox(string caption, string message)
        {
            MessageDialog d = new MessageDialog(message, caption);
            await d.ShowAsync();
        }
        public virtual async Task<bool> ShowOkCancelMessageBox(string caption, string message, string okContent = "Tamam", string cancelContent = "İptal")
        {
            MessageDialog d = new MessageDialog(message, caption);
            d.Commands.Add(new UICommand(okContent, null, 0));
            d.Commands.Add(new UICommand(cancelContent, null, 1));
            var result = await d.ShowAsync();
            return (int)result.Id == 0;
        }
    }
}
