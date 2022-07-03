using System;
namespace ChatClient.Data
{
    [Serializable]
    public class KeyChat
    {
        public string PublicKeyBase64 { get; set; }
        public string PrivateKeyBase64 { get; set; }

        public string SharedKey { get; set; }


    }
}
