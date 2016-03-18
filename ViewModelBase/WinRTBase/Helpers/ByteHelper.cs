using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using sStream = Windows.Storage.Streams;
using storage = Windows.Storage;

namespace WinRTBase
{
    public static class ByteHepler
    {
        #region Compression
        private async static Task<byte[]> compress(byte[] raw)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(memory, CompressionMode.Compress, true))
                {
                    await gzip.WriteAsync(raw, 0, raw.Length);
                }
                await memory.FlushAsync();
                return memory.ToArray();
            }
        }
        public static async Task<byte[]> CompressAsync(this byte[] raw) { return await compress(raw); }

        private async static Task<byte[]> decompress(byte[] gzip)
        {
            // Create a GZIP stream with decompression mode.
            // ... Then create a buffer and write into while reading from the GZIP stream.
            using (GZipStream stream = new GZipStream(new MemoryStream(gzip), CompressionMode.Decompress))
            {
                const int size = 4096;
                byte[] buffer = new byte[size];
                using (MemoryStream memory = new MemoryStream())
                {
                    int count = 0;
                    do
                    {
                        count = await stream.ReadAsync(buffer, 0, size);
                        if (count > 0)
                            await memory.WriteAsync(buffer, 0, count);
                    }
                    while (count > 0);
                    return memory.ToArray();
                }
            }
        }
        public static async Task<byte[]> DeCompressAsync(this byte[] raw) { return await decompress(raw); }
        #endregion
        #region Byte
        public static byte[] SubBytes(this byte[] bytes, int start, int lenght)
        {
            byte[] retVal = new byte[lenght];
            for (int i = start; i < bytes.Length; i++)
                retVal[i - start] = bytes[i];
            return retVal;
        }
        public static IEnumerable<byte[]> SplitWithBuffer(this byte[] bytes, int buffer = 65535)
        {
            for (var i = 0; i < bytes.Length; i += buffer)
                yield return bytes.SubBytes(i, Math.Min(buffer, bytes.Length - i));
        }
        public static byte[] CombineFromBuffer(this IEnumerable<byte[]> byteBufferCollection)
        {
            List<byte> retVal = new List<byte>();
            foreach (var bytes in byteBufferCollection)
                retVal.AddRange(bytes);
            return retVal.ToArray();
        }
        #endregion
        #region Hex
        public static string ToHex(this byte[] bytes)
        {
            char[] c = new char[bytes.Length * 2];
            byte b;
            for (int bx = 0, cx = 0; bx < bytes.Length; ++bx, ++cx)
            {
                b = ((byte)(bytes[bx] >> 4));
                c[cx] = (char)(b > 9 ? b + 0x37 + 0x20 : b + 0x30);

                b = ((byte)(bytes[bx] & 0x0F));
                c[++cx] = (char)(b > 9 ? b + 0x37 + 0x20 : b + 0x30);
            }
            return new string(c);
        }
        public static IEnumerable<string> ToHexWithBuffer(this byte[] bytes, int buffer = 65535)
        {
            int maxStrLen = buffer / 2;
            string hexStr = bytes.ToHex();
            IEnumerable<string> retVal = hexStr.SplitInParts(maxStrLen);
            return retVal;
        }

        public static byte[] HexToBytes(this string str)
        {
            if (str.Length == 0 || str.Length % 2 != 0)
                return new byte[0];

            byte[] buffer = new byte[str.Length / 2];
            char c;
            for (int bx = 0, sx = 0; bx < buffer.Length; ++bx, ++sx)
            {
                // Convert first half of byte
                c = str[sx];
                buffer[bx] = (byte)((c > '9' ? (c > 'Z' ? (c - 'a' + 10) : (c - 'A' + 10)) : (c - '0')) << 4);

                // Convert second half of byte
                c = str[++sx];
                buffer[bx] |= (byte)(c > '9' ? (c > 'Z' ? (c - 'a' + 10) : (c - 'A' + 10)) : (c - '0'));
            }

            return buffer;
        }
        public static byte[] BufferedHexToBytes(this IEnumerable<string> str, int buffer = 65535)
        {
            string myHexStr = string.Concat(str.ToArray());
            return myHexStr.HexToBytes();
        }
        #endregion
        #region Cryptology
        /// <summary>
        /// <param name="str">Unique key for encryption/decryption</param>
        /// <param name="key"></param>
        /// <param name="algorithm">Use Windows.Security.Cryptography.Core.SymmetricAlgorithmNames Class</param>
        /// <param name="hashAlgorithm">Use Windows.Security.Cryptography.Core.HashAlgorithmNames Class</param>
        /// <returns>Returns encrypted bytes.</returns>
        public static string Encrypt(this string str, string key, string algorithm, string hashAlgorithm)
        {
            return Cryptology.Algorithms.Encrypt(str, key, algorithm, hashAlgorithm);
        }
        /// <summary>
        /// <param name="str">Unique key for encryption/decryption</param>
        /// <param name="key"></param>
        /// <param name="algorithm">Use Windows.Security.Cryptography.Core.SymmetricAlgorithmNames Class</param>
        /// <param name="hashAlgorithm">Use Windows.Security.Cryptography.Core.HashAlgorithmNames Class</param>
        /// <returns>Returns decrypted bytes.</returns>
        public static string Decrypt(this string str, string key, string algorithm, string hashAlgorithm)
        {
            return Cryptology.Algorithms.Decrypt(str, key, algorithm, hashAlgorithm);
        }
        /// <summary>
        /// <param name="str"></param>
        /// <param name="key">Unique key for encryption/decryption</param>
        /// <param name="algorithm">Use Windows.Security.Cryptography.Core.SymmetricAlgorithmNames Class</param>
        /// <param name="hashAlgorithm">Use Windows.Security.Cryptography.Core.HashAlgorithmNames Class</param>
        /// <returns>Returns encrypted bytes.</returns>
        public static byte[] Encrypt(this byte[] bytes, string key, string algorithm, string hashAlgorithm)
        {
            return Cryptology.Algorithms.Encrypt(bytes, key, algorithm, hashAlgorithm);
        }
        /// <summary>
        /// <param name="str"></param>
        /// <param name="key">Unique key for encryption/decryption</param>
        /// <param name="algorithm">Use Windows.Security.Cryptography.Core.SymmetricAlgorithmNames Class</param>
        /// <param name="hashAlgorithm">Use Windows.Security.Cryptography.Core.HashAlgorithmNames Class</param>
        /// <returns>Returns decrypted bytes.</returns>
        public static byte[] Decrypt(this byte[] bytes, string key, string algorithm, string hashAlgorithm)
        {
            return Cryptology.Algorithms.Decrypt(bytes, key, algorithm, hashAlgorithm);
        }
        #endregion
        #region String
        public static IEnumerable<String> SplitInParts(this String s, Int32 partLength)
        {
            if (s == null)
                throw new ArgumentNullException("s");
            if (partLength <= 0)
                throw new ArgumentException("Part length has to be positive.", "partLength");

            for (var i = 0; i < s.Length; i += partLength)
                yield return s.Substring(i, Math.Min(partLength, s.Length - i));
        }
        public static int ToInt(this string str)
        {
            int retVal;
            int.TryParse(str, out retVal);
            return retVal;
        }
        public static double ToDouble(this string str)
        {
            double retVal;
            double.TryParse(str, out retVal);
            return retVal;
        }
        public static bool IsNumeric(this string str)
        {
            foreach (char c in str)
                if (!char.IsNumber(c))
                    return false;

            return true;
        }
        public static bool IsDouble(this string str)
        {
            double retVal;
            return double.TryParse(str, out retVal);
        }
        public static string TrimLast(this string str, int length)
        {
            return str.Length > length ? str.Substring(0, length) + "..." : str;
        }
        #endregion
        #region File
        public static async Task<byte[]> GetBytesAsync(this storage.StorageFile file)
        {
            sStream.IRandomAccessStream fileStream = await file.OpenAsync(storage.FileAccessMode.Read);
            var reader = new sStream.DataReader(fileStream.GetInputStreamAt(0));
            await reader.LoadAsync((uint)fileStream.Size);
            byte[] bytes = new byte[fileStream.Size];
            reader.ReadBytes(bytes);
            return bytes;
        }
        #endregion
    }
}