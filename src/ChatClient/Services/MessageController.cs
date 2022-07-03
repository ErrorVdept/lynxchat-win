using ChatClient.Data;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ChatClient.Services
{
    internal sealed class MessageController : WebApiController
    {
        [Route(HttpVerbs.Post, "/cert")]
        public async Task<PublicKey> Cert()
        {
            var pbK = await HttpContext.GetRequestDataAsync<PublicKey>();

            string responseString = "";
            PublicKey message = null;
            try
            {

                Console.WriteLine(pbK.username.ToString());
                byte[] pubkey = Convert.FromBase64String(pbK.publicKey);
                Console.WriteLine(pbK.publicKey.ToString());
                string user = pbK.username.ToString();
                AppWindow.abonent.GenerateSharedKey(pubkey);
                Console.WriteLine("Shared generated");
                AppWindow.abonent.SaveKeysToFile(pbK.username.ToString() + ".lynx");
                message = new PublicKey(AppWindow.CurrentUsername, AppWindow.pbKeyString);
                var json = JsonConvert.SerializeObject(message);
                Console.WriteLine(json);
                responseString = json;
            }
            catch
            {
                responseString = "fail";
            }

            HttpContext.Response.ContentType = "application/json";
            return message;
        }


        [Route(HttpVerbs.Post, "/message")]
        public async Task<string> MessageRecieve()
        {
            var myjson = await HttpContext.GetRequestDataAsync<SendedMessage>();
            Console.Write(myjson);
            string responseString = await SaveMessage(myjson);
            return responseString;
        }
        [Route(HttpVerbs.Post, "/file")]
        public async Task<string> MessageFileRecieve()
        {
            var myjson = await HttpContext.GetRequestDataAsync<SendedFile>();
            Console.Write(myjson);
            string responseString = await SaveFileMessage(myjson);
            return responseString;
        }
        public async Task<string> SaveFileMessage(SendedFile myjson)
        {

            string responseString = "OK";
            string file = myjson.fileData;
            string user = myjson.username;
            string iv = myjson.iv;
            string filename = myjson.fileName;

            try
            {
                Abonent cl = Abonent.LoadKeysFromFile(user + ".lynx");
                var msgIv = Convert.FromBase64String(iv);
                var dataToByte = Convert.FromBase64String(file);
                AppWindow.abonent.PrivateKey = cl.PrivateKey;
                AppWindow.abonent.SharedKey = cl.SharedKey;
                AppWindow.abonent.IV = msgIv;

                string decryptedMessage = AppWindow.abonent.Receive(dataToByte);

                //byte[] filedata = Convert.FromBase64String(decryptedMessage);
                
                //File.WriteAllBytes("Files/" + filename, filedata);

                string dateMessage = DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss");
                if (AppWindow.selectedUser == user)
                {
                    //Console.WriteLine("CURRENT");
                    StorageService.SaveFileMessage(user,filename, user, iv, dateMessage, false, AppWindow.CurrentUsername, file);

                    await Application.Current.Dispatcher.BeginInvoke(
                  DispatcherPriority.Background,
                  new Action(() => updateUiFile(user, filename, dateMessage, iv, decryptedMessage)));
                }
                else
                {
                    StorageService.SaveFileMessage(user, filename, user, iv, dateMessage, true, AppWindow.CurrentUsername, file);

                    await Application.Current.Dispatcher.BeginInvoke(
                    DispatcherPriority.Background,
                    new Action(() => MessageService.UpdateNewMessage(user)));



                }
                AppWindow.player.Play();

                responseString = "OK";
            }
            catch
            {

                responseString = "Fail";
            }
            return responseString;
        }
        public async Task<string> SaveMessage(SendedMessage myjson)
        {

            string responseString = "OK";
            string message = myjson.message;
            string user = myjson.username;
            string iv = myjson.iv;

            try
            {
                Abonent cl = Abonent.LoadKeysFromFile(user + ".lynx");
                var msgIv = Convert.FromBase64String(iv);
                var dataToByte = Convert.FromBase64String(message);
                AppWindow.abonent.PrivateKey = cl.PrivateKey;
                AppWindow.abonent.SharedKey = cl.SharedKey;
                AppWindow.abonent.IV = msgIv;

                string decryptedMessage = AppWindow.abonent.Receive(dataToByte);

                string dateMessage = DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss");
                if (AppWindow.selectedUser == user)
                {
                    //Console.WriteLine("CURRENT");
                    StorageService.SaveMessage(user, message, user, iv, dateMessage, false, AppWindow.CurrentUsername);

                    await Application.Current.Dispatcher.BeginInvoke(
                  DispatcherPriority.Background,
                  new Action(() => updateUiMessage(user, decryptedMessage, dateMessage, iv)));
                }
                else
                {
                    StorageService.SaveMessage(user, message, user, iv, dateMessage, true, AppWindow.CurrentUsername);

                    await Application.Current.Dispatcher.BeginInvoke(
                    DispatcherPriority.Background,
                    new Action(() => MessageService.UpdateNewMessage(user)));



                }
                AppWindow.player.Play();

                responseString = "OK";
            }
            catch
            {

                responseString = "Fail";
            }
            return responseString;
        }
        public void updateUiMessage(string user, string decryptedMessage, string dateMessage, string iv)
        {
            AppWindow.messages.Insert(0, new MessageInList(user, decryptedMessage, dateMessage, iv));
        }
        public void updateUiFile(string user, string filename, string dateMessage, string iv, string filedata)
        {
            AppWindow.messages.Insert(0, new MessageInList(user, "", dateMessage, iv,"file",filename, filedata));
        }
        [Route(HttpVerbs.Get, "/test")]
        public async Task<string> Test()
        {
            //Console.WriteLine("AAAA");
            return "Hi beaches";
        }

    }
}
