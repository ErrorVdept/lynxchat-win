using Elliptic;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ChatClient.Data
{
    public class Abonent : IDisposable
    {
        public byte[] PublicKey { get; set; }

        public byte[] PrivateKey { get; set; }

        public byte[] SharedKey { get; set; }

        public Aes Aes { get; set; }

        public byte[] IV { get; set; }
        public Curve25519 Curve25519 { get; set; }

        public Abonent()
        {
            Aes = Aes.Create();
            Curve25519 = new Curve25519();
            PrivateKey = Curve25519.CreateRandomPrivateKey();
            PublicKey = Curve25519.GetPublicKey(PrivateKey);
        }

        public static Abonent LoadKeysFromFile(string filename)
        {
            var keys = new KeyChat();
            using (StreamReader outStream = new StreamReader(File.Open("Resources/Keys/" + filename, FileMode.Open, FileAccess.Read)))
            {
                var readedString = outStream.ReadToEnd();
                keys = Newtonsoft.Json.JsonConvert.DeserializeObject<KeyChat>(readedString);
            }
            Abonent a = new Abonent();
            a.PublicKey = Convert.FromBase64String(keys.PublicKeyBase64);
            if (keys.SharedKey != null)
            {
                a.SharedKey = Convert.FromBase64String(keys.SharedKey);
            }
            if (keys.PrivateKeyBase64 != null)
            {
                a.PrivateKey = Convert.FromBase64String(keys.PrivateKeyBase64);
            }
            return a;
        }

        public void SaveKeysToFile(string filename)
        {
            var keys = new KeyChat();
            keys.PublicKeyBase64 = Convert.ToBase64String(PublicKey);
            if (PrivateKey != null)
            {
                keys.PrivateKeyBase64 = Convert.ToBase64String(PrivateKey);

            }
            if (SharedKey != null)
            {
                keys.SharedKey = Convert.ToBase64String(SharedKey);

            }

            using (StreamWriter outStream = new StreamWriter(File.Open("Resources/Keys/" + filename, FileMode.Create, FileAccess.Write)))
            {
                var serializedObject = Newtonsoft.Json.JsonConvert.SerializeObject(keys);
                outStream.Write(serializedObject);
            }
        }

        public void Dispose()
        {

        }

        public void GenerateSharedKey(byte[] anotherPublicKey)
        {
            SharedKey = Curve25519.GetSharedSecret(PrivateKey, anotherPublicKey);
        }


        public byte[] Send(string secretMessage)
        {
            byte[] encryptedMessage;
            using (Aes = Aes.Create())
            {
                Aes.Key = SharedKey;
                IV = Aes.IV;
                // Encrypt the message
                using (MemoryStream ciphertext = new MemoryStream())
                using (CryptoStream cs = new CryptoStream(ciphertext, Aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    byte[] plaintextMessage = Encoding.UTF8.GetBytes(secretMessage);
                    cs.Write(plaintextMessage, 0, plaintextMessage.Length);
                    cs.Close();
                    encryptedMessage = ciphertext.ToArray();
                }
            }
            return encryptedMessage;
        }
        


        public string Receive(byte[] encryptedMessage)
        {

            string decryptedMessage;
            using (Aes = Aes.Create())
            {
                Aes.Key = SharedKey;
                Aes.IV = IV;
                // Decrypt the message
                using (MemoryStream plaintext = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(plaintext, Aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(encryptedMessage, 0, encryptedMessage.Length);
                        cs.Close();
                        decryptedMessage = Encoding.UTF8.GetString(plaintext.ToArray());
                    }
                }
            }
            return decryptedMessage;
        }
    }
}

