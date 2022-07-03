using ChatClient.Data;
using EmbedIO;
using EmbedIO.WebApi;
using MihaZupan;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ChatClient.Services
{
    public class MessageService
    {



        public WebServer CreateWebServer(string url)
        {

#pragma warning disable CA2000
            var server = new WebServer(o => o
                    .WithUrlPrefix(url)
                    .WithMode(HttpListenerMode.EmbedIO))
                .WithLocalSessionManager()
                .WithWebApi("/api", m => m
                    .WithController<MessageController>());

            // Listen for state changes.

            
            return server;
#pragma warning restore CA2000
        }
        public async Task RunWebServerAsync(string url, CancellationToken cancellationToken)
        {

            var server = CreateWebServer(url);
            await server.RunAsync(cancellationToken).ConfigureAwait(false);
            //await server.RunAsync().ConfigureAwait(false);
        }








        public static async Task UpdateNewMessage(string name)
        {
            foreach (UserController usr in AppWindow.users)
            {
                if (usr.Username == name)
                {
                    usr.HasMessage = true;
                }
            }
        }

        public static async Task<string> SendMessage(string msg, string chat)
        {
            string response = "Fail";


            if (File.Exists("Resources/Keys/" + AppWindow.selectedUser + ".lynx"))
            {
                
                response = await SendOnlyMsg(msg, chat);
            }
            else
            {
                response = await RequestMsg(msg, chat);

            }
            return response;
        }

        public static async Task<string> SendOnlyMsg(string msg, string chat)
        {
            var proxy = new HttpToSocks5Proxy("127.0.0.1", 9050);

            var handler = new HttpClientHandler
            {
                Proxy = proxy
            };
            Abonent cl = null;
            string dateMessage = DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss");
            string result = "";
            using (var httpClient = new HttpClient(handler))
            {
                
                List<string> addresses = StorageService.GetAddress();
                cl = Abonent.LoadKeysFromFile(AppWindow.selectedUser + ".lynx");
                AppWindow.abonent.PrivateKey = cl.PrivateKey;
                AppWindow.abonent.SharedKey = cl.SharedKey;
                foreach (string address in addresses)
                {
                    var encrypt = AppWindow.abonent.Send(msg);
                    var encryptedMessage = Convert.ToBase64String(encrypt);
                    var baseIv = Convert.ToBase64String(AppWindow.abonent.IV);
                    try
                    {
                        Console.WriteLine(address);
                        SendedMessage message = new SendedMessage(AppWindow.CurrentUsername, encryptedMessage, chat, baseIv);
                        var json = JsonConvert.SerializeObject(message);
                        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "http://" + address + "/api/message");
                        request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                        var resx = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                        string str = await resx.Content.ReadAsStringAsync();
                        result = str;
                        if (result == "OK")
                        {
                            StorageService.SaveMessage(AppWindow.CurrentUsername, encryptedMessage, chat, baseIv, dateMessage, false, AppWindow.CurrentUsername);
                        }
                        else
                        {
                            MessageBox.Show("Ошибка отправки сообщения пользователю");
                        }
                        
                    }
                    catch
                    {
                        MessageBox.Show("Ошибка отправки сообщения пользователю");
                        result = "Fail";
                    }

                }
            }
            return result;
        }


        public static async Task<string> RequestMsg(string msg, string chat)
        {
            var proxy = new HttpToSocks5Proxy("127.0.0.1", 9050);
            string result = "Fail";
            var handler = new HttpClientHandler
            {
                Proxy = proxy
            };
            try
            {
                using (var httpClient = new HttpClient(handler))
                {



                    PublicKey message = new PublicKey(AppWindow.CurrentUsername, AppWindow.pbKeyString);
                    var json = JsonConvert.SerializeObject(message);
                    
                    List<string> addresses = StorageService.GetAddress();

                    foreach (string address in addresses)
                    {
                        string adr = "http://" + address + "/api/cert";
                        Console.WriteLine(adr);
                        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, adr);
                        request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                        var resx = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                        string str = await resx.Content.ReadAsStringAsync();
                        
                        PublicKey response = JsonConvert.DeserializeObject<PublicKey>(str);

                        AppWindow.abonent.GenerateSharedKey(Convert.FromBase64String(response.publicKey));
                        AppWindow.abonent.SaveKeysToFile(response.username + ".lynx");

                    }
                }
                result = await SendOnlyMsg(msg, chat);
            }
            catch
            {
                MessageBox.Show("Ошибка отправки сообщения запроса сертификата");
                result = "Fail";
            }

            return result;
        }







        public static async Task<string> SendFileMessage(string msg, string chat,string filename)
        {
            string response = "Fail";


            if (File.Exists("Resources/Keys/" + AppWindow.selectedUser + ".lynx"))
            {

                response = await SendOnlyFileMsg(msg, chat, filename);
            }
            else
            {
                response = await RequestMsgFile(msg, chat, filename);

            }
            return response;
        }

        public static async Task<string> SendOnlyFileMsg(string file, string chat, string filename)
        {
            var proxy = new HttpToSocks5Proxy("127.0.0.1", 9050);

            var handler = new HttpClientHandler
            {
                Proxy = proxy
            };
            Abonent cl = null;
            string dateMessage = DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss");
            string result = "";
            using (var httpClient = new HttpClient(handler))
            {

                List<string> addresses = StorageService.GetAddress();
                cl = Abonent.LoadKeysFromFile(AppWindow.selectedUser + ".lynx");
                AppWindow.abonent.PrivateKey = cl.PrivateKey;
                AppWindow.abonent.SharedKey = cl.SharedKey;
                foreach (string address in addresses)
                {
                    var encrypt = AppWindow.abonent.Send(file);
                    var encryptedFile = Convert.ToBase64String(encrypt);
                    var baseIv = Convert.ToBase64String(AppWindow.abonent.IV);
                    try
                    {
                        Console.WriteLine(address);
                        SendedFile message = new SendedFile(AppWindow.CurrentUsername, encryptedFile, chat, baseIv, filename);
                        var json = JsonConvert.SerializeObject(message);
                        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "http://" + address + "/api/file");
                        request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                        var resx = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                        string str = await resx.Content.ReadAsStringAsync();
                        result = str;
                        if (result == "OK")
                        {
                            encrypt = AppWindow.abonent.Send("Отправлен файл " + filename);
                            encryptedFile = Convert.ToBase64String(encrypt);
                            baseIv = Convert.ToBase64String(AppWindow.abonent.IV);
                            StorageService.SaveFileMessage(AppWindow.CurrentUsername, filename, chat, baseIv, dateMessage, false, AppWindow.CurrentUsername, encryptedFile);
                        }
                        else
                        {
                            MessageBox.Show("Ошибка отправки файла пользователю");
                        }

                    }
                    catch
                    {
                        MessageBox.Show("Ошибка отправки файла пользователю");
                        result = "Fail";
                    }

                }
            }
            return result;
        }


        public static async Task<string> RequestMsgFile(string msg, string chat, string filename)
        {
            var proxy = new HttpToSocks5Proxy("127.0.0.1", 9050);
            string result = "Fail";
            var handler = new HttpClientHandler
            {
                Proxy = proxy
            };
            try
            {
                using (var httpClient = new HttpClient(handler))
                {



                    PublicKey message = new PublicKey(AppWindow.CurrentUsername, AppWindow.pbKeyString);
                    var json = JsonConvert.SerializeObject(message);

                    List<string> addresses = StorageService.GetAddress();

                    foreach (string address in addresses)
                    {
                        string adr = "http://" + address + "/api/cert";
                        Console.WriteLine(adr);
                        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, adr);
                        request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                        var resx = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                        string str = await resx.Content.ReadAsStringAsync();

                        PublicKey response = JsonConvert.DeserializeObject<PublicKey>(str);

                        AppWindow.abonent.GenerateSharedKey(Convert.FromBase64String(response.publicKey));
                        AppWindow.abonent.SaveKeysToFile(response.username + ".lynx");

                    }
                }
                result = await SendOnlyFileMsg(msg, chat, filename);
            }
            catch
            {
                MessageBox.Show("Ошибка отправки сообщения запроса сертификата");
                result = "Fail";
            }

            return result;
        }


        public static void RegisterUser(string username, string password, string url)
        {

        }






    }
}
