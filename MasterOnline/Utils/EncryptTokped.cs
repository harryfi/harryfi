extern alias BCC;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace MasterOnline.Utils
{
    public class EncryptTokped
    {
        public static byte[] decryptRsaOaep(string cipherText)
        {
            //var ret = new byte[0];
            try
            {
                byte[] cipherTextBytes = Convert.FromBase64String(cipherText);

                PemReader pr = new PemReader(
                    (StreamReader)File.OpenText("C://EncryptTokped//private_key.pem")
                );
                AsymmetricCipherKeyPair keys = (AsymmetricCipherKeyPair)pr.ReadObject();

                
                IAsymmetricBlockCipher cipher0 = new RsaBlindedEngine();
                cipher0 = new OaepEncoding(cipher0, new Sha256Digest(), new Sha256Digest(), null);
                BufferedAsymmetricBlockCipher cipher = new BufferedAsymmetricBlockCipher(cipher0);
                cipher.Init(false, keys.Private);
                cipher.ProcessBytes(cipherTextBytes, 0, cipherTextBytes.Length);
                var decryptedData = cipher.DoFinal();
                var decryptedText = System.Text.Encoding.UTF8.GetString(decryptedData);

                return decryptedData.ToArray();

            }
            catch (Exception ex)
            {

            }
            return null;
        }
        public static byte[] decryptAesGcm(byte[] encryptedMessage, byte[] key, int nonSecretPayloadLength = 0)
        {
            //User Error Checks
            //if (key == null || key.Length != KeyBitSize / 8)
            //    throw new ArgumentException(String.Format("Key needs to be {0} bit!", KeyBitSize), "key");

            if (encryptedMessage == null || encryptedMessage.Length == 0)
                throw new ArgumentException("Encrypted Message Required!", "encryptedMessage");

            using (var cipherStream = new MemoryStream(encryptedMessage))
            using (var cipherReader = new BinaryReader(cipherStream))
            {
                //Grab Payload
                var nonSecretPayload = cipherReader.ReadBytes(nonSecretPayloadLength);

                //Grab Nonce
                var nonce = cipherReader.ReadBytes(12);
                //var abc = Convert.ToBase64String(nonce);

                //var cipher = new GcmBlockCipher(new AesFastEngine());
                var cipher = new GcmBlockCipher(new AesEngine());
                var parameters = new AeadParameters(new KeyParameter(key), 128, nonce, nonSecretPayload);
                cipher.Init(false, parameters);

                //Decrypt Cipher Text
                var cipherText = cipherReader.ReadBytes(encryptedMessage.Length - nonSecretPayloadLength - nonce.Length);
                var plainText = new byte[cipher.GetOutputSize(cipherText.Length)];

                try
                {
                    var len = cipher.ProcessBytes(cipherText, 0, cipherText.Length, plainText, 0);
                    cipher.DoFinal(plainText, len);

                }
                catch (InvalidCipherTextException ex)
                {
                    //Return null if it doesn't authenticate
                    return null;
                }
                return plainText;
            }
        }
        public string DecryptOrderTokped(string secret, string content)
        {
            string ret = "";

            var secretKey = decryptRsaOaep(secret);
            if(secretKey != null)
            {
                // take out cipher text and nonce then rearrange
                var encryptedTextBased64 = Convert.FromBase64String(content);
                var chipperText = SubArray(encryptedTextBased64, 0, encryptedTextBased64.Length - 12);
                var nonce = SubArray(encryptedTextBased64, encryptedTextBased64.Length - 12, 12);

                var contentByte = new byte[encryptedTextBased64.Length];
                Array.Copy(nonce, contentByte, nonce.Length);
                Array.Copy(chipperText, 0, contentByte, nonce.Length, chipperText.Length);
                //end take out cipher text and nonce then rearrange

                var byteData = decryptAesGcm(contentByte, secretKey, 0);
                if(byteData != null)
                {
                    ret = Encoding.UTF8.GetString(byteData);
                }
            }
            return ret;
        }

        public static byte[] SubArray(byte[] data, int start, int length)
        {
            byte[] result = new byte[length];

            Array.Copy(data, start, result, 0, length);

            return result;
        }
    }
}