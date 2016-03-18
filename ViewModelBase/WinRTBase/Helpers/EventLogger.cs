using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Popups;

namespace WinRTBase
{
    public class EventLogger : VMBase
    {
        private const string _fileName = "EventLog.txt";
        private const int _expireInDays = 7; //Expire Date will be 7 days (1 week)

        private static EventLogger _instance;
        public static EventLogger Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new EventLogger();

                return _instance;
            }
            private set
            {
                _instance = value;
            }
        }

        public EventLogger()
        {
            Instance = this;
        }

        private StorageFile _file;
        private async Task<StorageFile> getFile()
        {
            if (_file == null)
                _file = await AppDataFolder.CreateFileAsync(_fileName, CreationCollisionOption.OpenIfExists);

            return _file;
        }

        public async Task Clear()
        {
            var file = await getFile();
            await file.DeleteAsync();
            _file = null;
        }

        public async Task AppendLine(string text)
        {
            try
            {
                await checkFileExpired();

                var file = await getFile();
                await FileIO.AppendTextAsync(file, string.Format("{0} | {1}{2}", DateTime.Now.ToString(), text, LINE_BREAK));
            }
            catch (Exception) { }
        }

        public async Task<string> GetAllText()
        {
            var file = await getFile();
            return await FileIO.ReadTextAsync(file);
        }                

        private async Task checkFileExpired()
        {
            var file = await getFile();
            var diff = DateTime.Now - file.DateCreated;
            if (diff.Days > _expireInDays)
                await Clear();
        }
        private async void showMessage(string caption, string message)
        {
            MessageDialog d = new MessageDialog(message, caption);
            await d.ShowAsync();
        }
    }
}