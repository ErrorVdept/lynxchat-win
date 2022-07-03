using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.Data
{
    public class SendedFile
    {
        public string username { get; set; }
        public string fileData { get; set; }
        public string chat { get; set; }
        public string iv { get; set; }
        public string fileName { get; set; }

        public SendedFile(string user, string msg, string chatt, string ivector, string fname)
        {
            username = user;
            fileData = msg;
            chat = chatt;
            iv = ivector;
            fileName = fname;
        }
    }
}
