namespace ChatClient.Data
{
    public class UserRequest
    {
        public string name { get; set; }
        public string[] domain { get; set; }
        public bool online { get; set; }
        public UserRequest(string Name, string[] Domain, bool Online)
        {
            name = Name;
            domain = Domain;
            online = Online;
        }
    }
}
