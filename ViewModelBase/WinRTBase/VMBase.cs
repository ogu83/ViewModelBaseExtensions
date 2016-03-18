using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using storage = Windows.Storage;
using xml = System.Xml;
using xmlser = System.Xml.Serialization;

namespace WinRTBase
{
    public abstract class VMBase : INotifyPropertyChanged
    {
        public static string LINE_BREAK = "\r\n";

        #region Json
        public static async Task<byte[]> SerializeToCompressedJsonBytes(IEnumerable<VMBase> elements)
        {
            return await SerializeToJsonBytes(elements).CompressAsync();
        }
        public static async Task<IEnumerable<T>> DeserializeFromCompressedJsonBytes<T>(byte[] jsonBytes) where T : class
        {
            return DeserializeFromJsonBytes<T>(await jsonBytes.DeCompressAsync());
        }
        public static byte[] SerializeToJsonBytes(IEnumerable<VMBase> elements)
        {
            var jsonSerializer = new JsonSerializer();
            using (MemoryStream ms = new MemoryStream())
            {
                using (StreamWriter sw = new StreamWriter(ms))
                {
                    var jsonWriter = new JsonTextWriter(sw);
                    jsonSerializer.Serialize(jsonWriter, elements);
                    jsonWriter.Flush();
                    byte[] myBytes = ms.ToArray();
                    return myBytes;
                }
            }
        }
        public static IEnumerable<T> DeserializeFromJsonBytes<T>(byte[] jsonBytes) where T : class
        {
            var jsonSerializer = new JsonSerializer();
            jsonSerializer.NullValueHandling = NullValueHandling.Ignore;
            byte[] myBytes = jsonBytes;
            using (var ms = new MemoryStream(myBytes))
            {
                using (var sr = new StreamReader(ms))
                {
                    var jsonReader = new JsonTextReader(sr);
                    var retVal = jsonSerializer.Deserialize<IEnumerable<T>>(jsonReader);
                    return retVal;
                }
            }
        }
        public static string SerializeToJsonString(IEnumerable<VMBase> elements)
        {
            var myBytes = SerializeToJsonBytes(elements);
            string retVal = Encoding.UTF8.GetString(myBytes, 0, myBytes.Length);
            return retVal;
        }
        public static IEnumerable<T> DeserializeFromJsonString<T>(string jsonString) where T : class
        {
            var jsonSerializer = new JsonSerializer();
            byte[] myBytes = Encoding.UTF8.GetBytes(jsonString);
            return DeserializeFromJsonBytes<T>(myBytes);
        }

        public byte[] ToJsonBytes()
        {
            var jsonSerializer = new JsonSerializer();
            using (MemoryStream ms = new MemoryStream())
            {
                using (StreamWriter sw = new StreamWriter(ms))
                {
                    var jsonWriter = new JsonTextWriter(sw);
                    jsonSerializer.Serialize(jsonWriter, this);
                    jsonWriter.Flush();
                    byte[] myBytes = ms.ToArray();
                    return myBytes;
                }
            }
        }
        public static T FromJsonBytes<T>(byte[] jsonBytes) where T : VMBase
        {
            var jsonSerializer = new JsonSerializer();
            jsonSerializer.NullValueHandling = NullValueHandling.Ignore;
            byte[] myBytes = jsonBytes;
            using (var ms = new MemoryStream(myBytes))
            {
                using (var sr = new StreamReader(ms))
                {
                    var jsonReader = new JsonTextReader(sr);
                    var retVal = jsonSerializer.Deserialize<T>(jsonReader);
                    return retVal;
                }
            }
        }
        public string ToJsonString()
        {
            var myBytes = ToJsonBytes();
            string retVal = Encoding.UTF8.GetString(myBytes, 0, myBytes.Length);
            return retVal;
        }
        public static T FromJsonString<T>(string jsonString) where T : VMBase
        {
            var jsonSerializer = new JsonSerializer();
            byte[] myBytes = Encoding.UTF8.GetBytes(jsonString);
            return FromJsonBytes<T>(myBytes);
        }
        #endregion
        #region XML
        public async Task<string> ToXmlString<T>() where T : VMBase
        {
            string retVal = string.Empty;
            xmlser.XmlSerializer ser = new xmlser.XmlSerializer(typeof(T));
            using (MemoryStream stream = new MemoryStream())
            {
                ser.Serialize(stream, this);
                using (var reader = xml.XmlReader.Create(stream))
                {
                    retVal = await reader.ReadContentAsStringAsync();
                }
            }
            return retVal;
        }
        public byte[] ToXmlBytes<T>() where T : VMBase
        {
            xmlser.XmlSerializer ser = new xmlser.XmlSerializer(typeof(T));
            using (MemoryStream stream = new MemoryStream())
            {
                ser.Serialize(stream, this);
                return stream.ToArray();
            }
        }
        public static async Task<T> FromXmlString<T>(string xmlContent) where T : VMBase
        {
            T retVal = null;
            xmlser.XmlSerializer ser = new xmlser.XmlSerializer(typeof(T));
            using (MemoryStream stream = new MemoryStream())
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    await writer.WriteAsync(xmlContent);
                    using (xml.XmlReader reader = xml.XmlReader.Create(stream))
                    {
                        if (ser.CanDeserialize(reader))
                            retVal = ser.Deserialize(reader) as T;
                        else
                            throw new TypeInitializationException(typeof(T).FullName, new xml.XmlException("Cannot Deserialize"));
                    }
                }
            }
            return retVal;
        }
        public static T FromXmlBytes<T>(byte[] xmlBytes) where T : VMBase
        {
            xmlser.XmlSerializer ser = new xmlser.XmlSerializer(typeof(T));
            using (MemoryStream stream = new MemoryStream())
                return ser.Deserialize(stream) as T;
        }
        #endregion
        #region Marshaling
        public byte[] ToUnmanagedBytes()
        {
            var size = Marshal.SizeOf(this);
            // Both managed and unmanaged buffers required.
            var bytes = new byte[size];
            var ptr = Marshal.AllocHGlobal(size);
            // Copy object byte-to-byte to unmanaged memory.
            Marshal.StructureToPtr(this, ptr, false);
            // Copy data from unmanaged memory to managed buffer.
            Marshal.Copy(ptr, bytes, 0, size);
            // Release unmanaged memory.
            Marshal.FreeHGlobal(ptr);
            return bytes;
        }
        public static T FromUnmanagedBytes<T>(byte[] bytes) where T : VMBase
        {
            var size = bytes.Length;
            var ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(bytes, 0, ptr, size);
            var retVal = (T)Marshal.PtrToStructure(ptr, typeof(T));
            Marshal.FreeHGlobal(ptr);
            return retVal;
        }
        #endregion
        #region Storage
        public static storage.StorageFolder InstallationFolder { get { return Windows.ApplicationModel.Package.Current.InstalledLocation; } }
        public static storage.StorageFolder AppDataFolder { get { return Windows.Storage.ApplicationData.Current.LocalFolder; } }
        public static string AssetFolderPath { get { return "Assets"; } }

        public async Task SaveAsXmlAsync<T>(string fileName) where T : VMBase
        {
            storage.StorageFile myFile = await AppDataFolder.CreateFileAsync(fileName, storage.CreationCollisionOption.ReplaceExisting);
            await SaveAsXmlAsync<T>(myFile);
        }
        public async Task SaveAsXmlAsync<T>(storage.StorageFile file) where T : VMBase
        {
            xmlser.XmlSerializer ser = new xmlser.XmlSerializer(typeof(T));
            using (IRandomAccessStream myFileStream = await file.OpenAsync(storage.FileAccessMode.ReadWrite))
            {
                Stream wStream = myFileStream.AsStreamForWrite();
                ser.Serialize(wStream, this);
            }
        }
        public static async Task<T> LoadXmlFileAsync<T>(string fileName) where T : VMBase
        {
            storage.StorageFile myFile = await AppDataFolder.GetFileAsync(fileName);
            return await LoadXmlFileAsync<T>(myFile);
        }
        public static async Task<T> LoadXmlFileAsync<T>(storage.StorageFile file) where T : VMBase
        {
            xmlser.XmlSerializer ser = new xmlser.XmlSerializer(typeof(T));
            using (IRandomAccessStream myFileStream = await file.OpenAsync(storage.FileAccessMode.Read))
            {
                Stream rStream = myFileStream.AsStreamForRead();
                T myT = (T)ser.Deserialize(rStream);
                return myT;
            }
        }

        public static async Task SaveBytesAsFile(byte[] bytes, string fileName)
        {
            storage.StorageFile myFile = await AppDataFolder.CreateFileAsync(fileName, storage.CreationCollisionOption.ReplaceExisting);
            await SaveBytesAsFile(bytes, myFile);
        }
        public static async Task SaveBytesAsFile(byte[] bytes, storage.StorageFile file)
        {
            using (var stream = await file.OpenStreamForWriteAsync())
            {
                await stream.WriteAsync(bytes, 0, bytes.Length);
            }
        }
        public static async Task SaveStringAsFile(string content, string fileName)
        {
            storage.StorageFile myFile = await AppDataFolder.CreateFileAsync(fileName, storage.CreationCollisionOption.ReplaceExisting);
            await SaveStringAsFile(content, myFile);
        }
        public static async Task SaveStringAsFile(string content, storage.StorageFile file)
        {
            using (var stream = await file.OpenStreamForWriteAsync())
            {
                await storage.FileIO.WriteTextAsync(file, content);
            }
        }
        #endregion
        #region INotifyPropertyChanged Interface
        public event PropertyChangedEventHandler PropertyChanged;
        public async void NotifyPropertyChangedAsync(string info)
        {
            await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Low,
                new DispatchedHandler(() =>
                {
                    NotifyPropertyChanged(info);
                }));
        }
        public void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(info));
        }
        #endregion

        public virtual void Initialize() { }
        public virtual void Suspend() { }
        public virtual object Clone()
        {
            return this.MemberwiseClone();
        }

        public static bool IsVMBase(object o)
        {
            return (o as VMBase != null);
        }

        public event EventHandler Changed;

        [JsonIgnore()]
        private bool _isChanged;
        [xmlser.XmlIgnore()]
        [JsonIgnore()]
        public bool IsChanged
        {
            get { return _isChanged; }
            set
            {
                _isChanged = value;
                NotifyPropertyChanged("IsChanged");
                if (Changed != null)
                    Changed(this, null);
            }
        }
    }
}