using System;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Security.Cryptography.DataProtection;
using Windows.Storage.Streams;

namespace WinRTBase
{
    public static class Cryptology
    {
        /// <summary>
        /// .NET Framework Cryptography Model
        /// http://msdn.microsoft.com/en-us/library/0ss79b2x.aspx
        /// </summary>
        public sealed class Algorithms
        {
            /// <summary>
            /// Encrypt bytes using dual encryption method. Returns an encrypted bytes.
            /// </summary>
            /// <param name="toEncrypt">Bytes to be encrypted</param>
            /// <param name="key">Unique key for encryption/decryption</param>
            /// <param name="algorithm">Use Windows.Security.Cryptography.Core.SymmetricAlgorithmNames Class</param>
            /// <param name="hashAlgorithm">Use Windows.Security.Cryptography.Core.HashAlgorithmNames Class</param>
            /// <returns>Returns encrypted bytes.</returns>
            public static byte[] Encrypt(byte[] toEncrypt, string key, string algorithm, string hashAlgorithm)
            {
                // Get the MD5 key hash (you can as well use the binary of the key string)
                IBuffer keyHash = getHash(key, hashAlgorithm);
                // Create a buffer that contains the encoded message to be encrypted.
                IBuffer toDecryptBuffer = CryptographicBuffer.CreateFromByteArray(toEncrypt);
                // Open a symmetric algorithm provider for the specified algorithm.
                SymmetricKeyAlgorithmProvider provider = SymmetricKeyAlgorithmProvider.OpenAlgorithm(algorithm);
                // Create a symmetric key.
                CryptographicKey symetricKey = provider.CreateSymmetricKey(keyHash);
                // The input key must be securely shared between the sender of the cryptic message
                // and the recipient. The initialization vector must also be shared but does not
                // need to be shared in a secure manner. If the sender encodes a message string
                // to a buffer, the binary encoding method must also be shared with the recipient.
                IBuffer buffEncrypted = CryptographicEngine.Encrypt(symetricKey, toDecryptBuffer, null);
                // Convert the encrypted buffer to a string (for display).
                // We are using Base64 to convert bytes to string since you might get unmatched characters
                // in the encrypted buffer that we cannot convert to string with UTF8.
                byte[] retVal;
                CryptographicBuffer.CopyToByteArray(buffEncrypted, out retVal);
                return retVal;
            }
            /// <summary>
            /// Encrypt a string using dual encryption method. Returns an encrypted text.
            /// </summary>
            /// <param name="toEncrypt">String to be encrypted</param>
            /// <param name="key">Unique key for encryption/decryption</param>
            /// <param name="algorithm">Use Windows.Security.Cryptography.Core.SymmetricAlgorithmNames Class</param>
            /// <param name="hashAlgorithm">Use Windows.Security.Cryptography.Core.HashAlgorithmNames Class</param>
            /// <returns>Returns encrypted string.</returns>
            public static string Encrypt(string toEncrypt, string key, string algorithm, string hashAlgorithm)
            {
                // Get the MD5 key hash (you can as well use the binary of the key string)
                IBuffer keyHash = getHash(key, hashAlgorithm);
                // Create a buffer that contains the encoded message to be encrypted.
                IBuffer toDecryptBuffer = CryptographicBuffer.ConvertStringToBinary(toEncrypt, BinaryStringEncoding.Utf8);
                // Open a symmetric algorithm provider for the specified algorithm.
                SymmetricKeyAlgorithmProvider provider = SymmetricKeyAlgorithmProvider.OpenAlgorithm(algorithm);
                // Create a symmetric key.
                CryptographicKey symetricKey = provider.CreateSymmetricKey(keyHash);
                // The input key must be securely shared between the sender of the cryptic message
                // and the recipient. The initialization vector must also be shared but does not
                // need to be shared in a secure manner. If the sender encodes a message string
                // to a buffer, the binary encoding method must also be shared with the recipient.
                IBuffer buffEncrypted = CryptographicEngine.Encrypt(symetricKey, toDecryptBuffer, null);
                // Convert the encrypted buffer to a string (for display).
                // We are using Base64 to convert bytes to string since you might get unmatched characters
                // in the encrypted buffer that we cannot convert to string with UTF8.
                string strEncrypted = CryptographicBuffer.EncodeToBase64String(buffEncrypted);
                return strEncrypted;
            }

            /// <summary>
            /// Decrypt a string using dual encryption method. Return a Decrypted clear string
            /// </summary>
            /// <param name="cipherString">Encrypted string</param>
            /// <param name="key">Unique key for encryption/decryption</param>
            /// <returns>Returns decrypted text.</returns>
            public static string Decrypt(string cipherString, string key, string algorithm, string hashAlgorithm)
            {
                // Get the key hash (you can as well use the binary of the key string)
                IBuffer keyHash = getHash(key, hashAlgorithm);
                // Create a buffer that contains the encoded message to be decrypted.
                IBuffer toDecryptBuffer = CryptographicBuffer.DecodeFromBase64String(cipherString);
                // Open a symmetric algorithm provider for the specified algorithm.
                SymmetricKeyAlgorithmProvider provider = SymmetricKeyAlgorithmProvider.OpenAlgorithm(algorithm);
                // Create a symmetric key.
                CryptographicKey symetricKey = provider.CreateSymmetricKey(keyHash);
                IBuffer buffDecrypted = CryptographicEngine.Decrypt(symetricKey, toDecryptBuffer, null);
                string strDecrypted = CryptographicBuffer.ConvertBinaryToString(BinaryStringEncoding.Utf8, buffDecrypted);
                return strDecrypted;
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="cipherBytes">Encrypted string</param>
            /// <param name="key"></param>
            /// <param name="algorithm"></param>
            /// <param name="hashAlgorithm"></param>
            /// <returns></returns>
            public static byte[] Decrypt(byte[] cipherBytes, string key, string algorithm, string hashAlgorithm)
            {
                // Get the key hash (you can as well use the binary of the key string)
                IBuffer keyHash = getHash(key, hashAlgorithm);
                // Create a buffer that contains the encoded message to be decrypted.
                IBuffer toDecryptBuffer = CryptographicBuffer.CreateFromByteArray(cipherBytes);
                // Open a symmetric algorithm provider for the specified algorithm.
                SymmetricKeyAlgorithmProvider provider = SymmetricKeyAlgorithmProvider.OpenAlgorithm(algorithm);
                // Create a symmetric key.
                CryptographicKey symetricKey = provider.CreateSymmetricKey(keyHash);
                IBuffer buffDecrypted = CryptographicEngine.Decrypt(symetricKey, toDecryptBuffer, null);
                byte[] retVal;
                CryptographicBuffer.CopyToByteArray(buffDecrypted, out retVal);
                return retVal;
            }
        }

        #region Descriptors
        private const string _LOCAL = "LOCAL";
        private const string _WEBCREDENTIALS = "WEBCREDENTIALS";

        public static string UserDescriptor { get { return _LOCAL + "=user"; } }
        public static string MachineDescriptor { get { return _LOCAL + "=machine"; } }
        public static string WebCredentialsDescriptor(string password, string url) { return _WEBCREDENTIALS + "=" + password + "," + url; }
        public static string WebCredentialsDescriptor(string password) { return WebCredentialsDescriptor(password, string.Empty); }
        #endregion
        #region Example
        public static async void ProtectExample()
        {
            // Initialize function arguments.
            String strMsg = "This is a message to be protected.";
            String strDescriptor = "LOCAL=user";
            BinaryStringEncoding encoding = BinaryStringEncoding.Utf8;
            // Protect a message to the local user.
            IBuffer buffProtected = await SampleProtectAsync(strMsg, strDescriptor, encoding);
            // Decrypt the previously protected message.
            String strDecrypted = await SampleUnprotectData(buffProtected, encoding);
        }
        public static async Task<IBuffer> SampleProtectAsync(String strMsg, String strDescriptor, BinaryStringEncoding encoding)
        {
            // Create a DataProtectionProvider object for the specified descriptor.
            DataProtectionProvider Provider = new DataProtectionProvider(strDescriptor);
            // Encode the plaintext input message to a buffer.
            encoding = BinaryStringEncoding.Utf8;
            IBuffer buffMsg = CryptographicBuffer.ConvertStringToBinary(strMsg, encoding);
            // Encrypt the message.
            IBuffer buffProtected = await Provider.ProtectAsync(buffMsg);
            // Execution of the SampleProtectAsync function resumes here
            // after the awaited task (Provider.ProtectAsync) completes.
            return buffProtected;
        }
        public static async Task<String> SampleUnprotectData(IBuffer buffProtected, BinaryStringEncoding encoding)
        {
            // Create a DataProtectionProvider object.
            DataProtectionProvider Provider = new DataProtectionProvider();
            // Decrypt the protected message specified on input.
            IBuffer buffUnprotected = await Provider.UnprotectAsync(buffProtected);
            // Execution of the SampleUnprotectData method resumes here
            // after the awaited task (Provider.UnprotectAsync) completes
            // Convert the unprotected message from an IBuffer object to a string.
            String strClearText = CryptographicBuffer.ConvertBinaryToString(encoding, buffUnprotected);
            // Return the plaintext string.
            return strClearText;
        }
        #endregion
        #region IBuffer
        private static string iBufferToString(IBuffer buff)
        {
            //BinaryStringEncoding encoding = BinaryStringEncoding.Utf16BE;
            //return CryptographicBuffer.ConvertBinaryToString(encoding, buff);
            return CryptographicBuffer.EncodeToHexString(buff);
        }
        public static string IBufferToString(this IBuffer buff)
        {
            return iBufferToString(buff);
        }

        private static IBuffer StringToIBuffer(string str)
        {
            //BinaryStringEncoding encoding = BinaryStringEncoding.Utf16LE;
            //return CryptographicBuffer.ConvertStringToBinary(str, encoding);
            return CryptographicBuffer.DecodeFromHexString(str);
        }
        public static IBuffer ToIBuffer(this string str)
        {
            return StringToIBuffer(str);
        }
        private static IBuffer getHash(string key, string hashAlgo)
        {
            // Convert the message string to binary data.
            IBuffer buffUtf8Msg = CryptographicBuffer.ConvertStringToBinary(key, BinaryStringEncoding.Utf8);
            // Create a HashAlgorithmProvider object.
            HashAlgorithmProvider objAlgProv = HashAlgorithmProvider.OpenAlgorithm(hashAlgo);
            // Hash the message.
            IBuffer buffHash = objAlgProv.HashData(buffUtf8Msg);
            // Verify that the hash length equals the length specified for the algorithm.
            if (buffHash.Length != objAlgProv.HashLength)
                throw new Exception("There was an error creating the hash");

            return buffHash;
        }
        #endregion
    }
}