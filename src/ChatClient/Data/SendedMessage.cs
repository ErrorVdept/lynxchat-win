namespace ChatClient.Data
{
    public class SendedMessage
    {
        public string username { get; set; }
        public string message { get; set; }
        public string chat { get; set; }
        public string iv { get; set; }

        public SendedMessage(string user, string msg, string chatt, string ivector)
        {
            username = user;
            message = msg;
            chat = chatt;
            iv = ivector;
        }
    }
}
