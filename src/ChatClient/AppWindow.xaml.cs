using ChatClient.Data;
using ChatClient.Services;
using EmbedIO.Net;
using HeyRed.MarkdownSharp;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Media;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using System.Windows.Input;
using System.Windows.Threading;

namespace ChatClient
{
    /// <summary>
    /// Логика взаимодействия для AppWindow.xaml
    /// </summary>
    public partial class AppWindow : Window
    {
        public StorageService storageService = new StorageService();

        public static string CurrentUsername;
        public static ObservableCollection<MessageInList> messages; //Сообщения





        public static ObservableCollection<UserController> users; // список пользователей
        public static string selectedUser = ""; // выбраный пользователь


        public static SQLiteConnection con = new SQLiteConnection(@"Data Source=database.db;Cache=Shared");//SQL коннект
        public static SQLiteCommand command = new SQLiteCommand("Select * from Message");// SQL команда


        public static SoundPlayer player = new SoundPlayer();// Плеер звука сообщения

        public static Abonent abonent;// Абонент для дешифровки
        public static byte[] pbKey;
        public static string pbKeyString;
        public MessageService messageService = new MessageService();

        public static string Token = ""; // Токен для обращения к серверу
        public static string Server = "";
        System.Timers.Timer aTimer;


        public static Markdown mark;
        
        public AppWindow(string usrNm, string token, string server)
        {
            this.DataContext = this;
            InitializeComponent();
            
            Token = token;
            Server = "http://" + server + ":8080"; // Настройка порта сервера
            CurrentUsername = usrNm;
            CurrentUserLabel.Content = CurrentUsername;
            CheckCerst(); // Проверка наличия  своего сертификата
            pbKey = abonent.PublicKey;
            pbKeyString = Convert.ToBase64String(pbKey);
            player.SoundLocation = "message.wav"; //
            player.Load();                        // Звук сообщения 
            con.Open();
            command.Connection = con;
            
            var options = new MarkdownOptions
            {
                AutoHyperlink = true,
                AutoNewLines = true,
                LinkEmails = false,
                QuoteSingleLine = true,
                StrictBoldItalic = true,
                DisableImages = true
            };
            mark = new Markdown(options);

            messages = new ObservableCollection<MessageInList>();
            users = new ObservableCollection<UserController>();
            UserList.ItemsSource = users;
            UpdateUserList();
            
            messages.CollectionChanged += LoadLast;
            //users = storageService.LoadUsers();

            // СЕРВИС ПО ОБНОВЛЕНИЮ ПОЛЬЗОВАТЕЛЕЙ
            StartAllServices();
            aTimer = new System.Timers.Timer();
            aTimer.Interval = 10000;
            aTimer.Elapsed += CheckOnline;
            aTimer.AutoReset = true;

            // Start the timer
            aTimer.Enabled = true;




        }

        public async void StartAllServices()
        {
            var url = "http://*:8888"; // the same arises if I use "+" instead of "*"

            EndPointManager.UseIpv6 = false;
            using (var ctSource = new CancellationTokenSource())
            {
                await messageService.RunWebServerAsync(url, ctSource.Token);
            }

        }
        // ОБРАБОТЧИК СОБЫТИЯ СЛУЖБЫ ОБНОВЛЕНИЯ ОНЛАЙНА
        public async void CheckOnline(Object source, System.Timers.ElapsedEventArgs e)
        {
            await LoadOnline();
        }

        public async Task LoadOnline()
        {
            await OnlineService.UpdateOnlineServer();
            await Application.Current.Dispatcher.BeginInvoke(
                    DispatcherPriority.Background,
                    new Action(() => UpdateUserList()));

        }
        // ОБНОВЛЕНИЕ ИНТЕРФЕЙСА ДЛЯ ОТОБРАЖЕНИЯ ПОЛЬЗОВАТЕЛЕЙ
        public void UpdateUserList()
        {
            //Console.WriteLine(selectedUser);
            ObservableCollection<UserController> tmp = storageService.LoadUsers();

            bool contains = false;
            foreach (UserController user in tmp)
            {
                contains = false;
                foreach (UserController user2 in users)
                {
                    if (user.Username == user2.Username)
                    {
                        contains = true;
                    }
                }
                if (contains == false)
                {
                    users.Add(user);
                }

            }
            foreach (UserController user in users)
            {
                foreach (UserController user2 in tmp)
                {

                    if (user.Username == user2.Username)
                    {
                        user.HasMessage = user2.HasMessage;
                        user.Online = user2.Online;
                        if (user.Online == false)
                        {
                            SendButton.IsEnabled = false;
                        }
                        else
                        {
                            SendButton.IsEnabled = true;
                        }
                    }
                }

            }
        }

        
        // ПРОВЕРКА НАЛИЧИЯ СВОЕГО СЕРТИФИКАТА И ЕГО ГЕНЕРАЦИЯ
        public void CheckCerst()
        {
            if (File.Exists("Resources/Keys/" + CurrentUsername + "mykey.lynx"))
            {
                abonent = Abonent.LoadKeysFromFile(CurrentUsername + "mykey.lynx");
            }
            else
            {
                abonent = new Abonent();
                abonent.SaveKeysToFile(CurrentUsername + "mykey.lynx");
            }
        }
        // ОТПРАВКА СООБЩЕНИЯ
        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            //string dateMessage = DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss");
            //dateMessage = dateMessage.Replace("T", " ");
            //string sended = selectedUser;
            //string msg = MessageTextBox.Text;
            //string a = mark.Transform(msg);

            //messages.Insert(0, new MessageInList(CurrentUsername, a, dateMessage, "random"));
            if (!String.IsNullOrEmpty(selectedUser) && !String.IsNullOrEmpty(MessageTextBox.Text))
            {
                string sended = selectedUser;
                string msg = MessageTextBox.Text;
                MessageTextBox.Clear();
                string response = await MessageService.SendMessage(msg, sended);
                string dateMessage = DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss");
                dateMessage = dateMessage.Replace("T", " ");
                if (selectedUser == sended && response == "OK")
                {
                    messages.Insert(0, new MessageInList(CurrentUsername, msg, dateMessage, "random"));
                }
            }
        }
        // ОБРАБОТЧИК ДОБАВЛЕНИЯ ПОСЛЕДНЕГО СООБЩЕНИЯ
        public void LoadLast(object sender, NotifyCollectionChangedEventArgs e)
        {

            MessagePanel.Children.Add(messages[0]);
            ScrolMessages.ScrollToEnd();
        }
        // ЗАГРУЗКА ВСЕХ СООБЩЕНИЙ
        public void LoadAllMessages()
        {
            for (int i = messages.Count - 1; i >= 0; i--)
            {
                MessagePanel.Children.Add(messages[i]);
                ScrolMessages.ScrollToEnd();
            }
        }




        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.Show();
        }



        private void UserList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var user in users)
            {
                if (UserList.SelectedItem == user)
                {

                    selectedUser = user.Username;
                    messages = storageService.LoadMessages(selectedUser);
                    MessagePanel.Children.Clear();
                    LoadAllMessages();
                    messages.CollectionChanged += LoadLast;
                    if (user.Online == false)
                    {
                        SendButton.IsEnabled = false;
                    }
                    else
                    {
                        SendButton.IsEnabled = true;
                    }
                }
            }
        }



        private void UserList_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            UserList.Items.Refresh();
        }

        // ОБРАБОТЧИК НАЖАТИЯ ENTER ДЛЯ ОТПРАВКИ СООБЩЕНИЯ
        private async void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                if (!String.IsNullOrEmpty(selectedUser) && !String.IsNullOrEmpty(MessageTextBox.Text))
                {
                    string sended = selectedUser;
                    string msg = MessageTextBox.Text;
                    MessageTextBox.Clear();
                    await MessageService.SendMessage(msg, sended);
                    string dateMessage = DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss");
                    dateMessage = dateMessage.Replace("T", " ");
                    if (selectedUser == sended)
                    {
                        messages.Insert(0, new MessageInList(CurrentUsername, msg, dateMessage, "random"));
                    }
                }
            }

        }

        private async void SendFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            string filename = "";
            string safeFilename = "";
            byte[] file = null;
            if (openFileDialog.ShowDialog() == true)
            {
                filename = openFileDialog.FileName;
                safeFilename = openFileDialog.SafeFileName;
                file = File.ReadAllBytes(filename);
                string base64File = Convert.ToBase64String(file);
                if (!String.IsNullOrEmpty(selectedUser))
                {
                    string sended = selectedUser;
                    
                    MessageTextBox.Clear();
                    string response = await MessageService.SendFileMessage(base64File, sended, safeFilename);
                    string dateMessage = DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss");
                    dateMessage = dateMessage.Replace("T", " ");
                    if (selectedUser == sended && response == "OK")
                    {
                        messages.Insert(0, new MessageInList(CurrentUsername, "файл", dateMessage, "random","file", safeFilename, base64File));
                    }
                }
            }


        }

        private void FileStorageButton_Click(object sender, RoutedEventArgs e)
        {
            AllFileWindow allFileWindow = new AllFileWindow();
            allFileWindow.Show();
        }
    }
}
