using ChatClient.Data;
using ChatClient.Services;
using MihaZupan;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ChatClient
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public static UIElement button = null;
        public static UIElement loading = null;
        public LoginWindow()
        {
            InitializeComponent();
            button = MyStack.Children[6];
            loading = MyStack.Children[7];
            MyStack.Children.RemoveAt(7);
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            MyStack.Children.RemoveAt(6);
            MyStack.Children.Add(loading);
            string token = await Login();
            if (String.IsNullOrEmpty(token))
            {
                MessageBox.Show("Ошибка авторизации. Неправильный логин или пароль или ошибка связи с сервером");
                MyStack.Children.RemoveAt(6);
                MyStack.Children.Add(button);
            }
            else
            {

                AppWindow appWindow = new AppWindow(TextBoxUsername.Text, token, TextBoxServerUrl.Text);
                appWindow.Show();
                this.Owner = appWindow;
                this.Close();
            }

        }
        public async Task<string> Login()
        {
            string token = "";

            var proxy = new HttpToSocks5Proxy("127.0.0.1", 9050);

            var handler = new HttpClientHandler
            {
                Proxy = proxy
            };
            try
            {
                using (var httpClient = new HttpClient(handler))//new SocksPortHandler("127.0.0.1", socksPort: 9050)))
                {

                    Auth auth = new Auth();

                    auth.username = TextBoxUsername.Text;
                    auth.password = TextBoxPassword.Password.ToString();
                    var json = JsonConvert.SerializeObject(auth);
                    //http://r32273fkio3tx6mdbofjtvq6i7us6lskp43lvuip4c5li5ohwxvdpeqd.onion/token/generate-token

                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "http://" + TextBoxServerUrl.Text + ":8080/token/generate-token");
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                    var resx = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                    string str = await resx.Content.ReadAsStringAsync();
                    AuthResponse authResponse = JsonConvert.DeserializeObject<AuthResponse>(str);
                    token = authResponse.token;
                    await OnlineService.UpdateOnlineServerFromStart("http://" + TextBoxServerUrl.Text + ":8080/user", token);
                }
            }
            catch
            {

            }
            return token;
        }
        private async void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                MyStack.Children.RemoveAt(6);
                MyStack.Children.Add(loading);
                string token = await Login();
                if (String.IsNullOrEmpty(token))
                {
                    MessageBox.Show("Ошибка авторизации. Неправильный логин или пароль или ошибка связи с сервером");
                    MyStack.Children.RemoveAt(6);
                    MyStack.Children.Add(button);
                }
                else
                {
                    await OnlineService.UpdateOnlineServer();
                    AppWindow appWindow = new AppWindow(TextBoxUsername.Text, token, TextBoxServerUrl.Text);
                    appWindow.Show();
                    this.Owner = appWindow;
                    this.Close();
                }
            }

        }



    }
}
