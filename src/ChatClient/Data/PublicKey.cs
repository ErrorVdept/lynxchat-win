namespace ChatClient.Data
{
    public class PublicKey
    {
        public string username { get; set; }
        public string publicKey { get; set; }
        public PublicKey(string username, string publicKey)
        {
            this.username = username;
            this.publicKey = publicKey;
        }
    }
}
