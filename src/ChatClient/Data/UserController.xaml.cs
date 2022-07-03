using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ChatClient.Data
{
    /// <summary>
    /// Логика взаимодействия для UserControl.xaml
    /// </summary>
    public partial class UserController : UserControl
    {
        public string Username { get { return username; } set { username = value; } }
        public bool Online { get { return online; } set { online = value; OnlineChange(online); } }
        public bool HasMessage { get { return hasMessage; } set { hasMessage = value; MessageChange(hasMessage); } }



        private string username;
        private bool online;
        private bool hasMessage;
        public UserController(string u, bool o, bool h)
        {
            InitializeComponent();
            username = u;
            online = o;
            hasMessage = h;
            LabelUsername.Content = username;
            if (online)
            {
                onlineImage.Source = new BitmapImage(new Uri(@"/Resources/Online.png", UriKind.Relative));
            }
            else
            {
                onlineImage.Source = new BitmapImage(new Uri(@"/Resources/Ofline.png", UriKind.Relative));
            }

            if (hasMessage)
            {
                messageImage.Source = new BitmapImage(new Uri(@"/Resources/Message.png", UriKind.Relative));
            }
            else
            {
                messageImage.Source = new BitmapImage(new Uri(@"/Resources/NonMessage.png", UriKind.Relative));
            }
        }

        public void OnlineChange(bool online)
        {
            if (online)
            {
                onlineImage.Source = new BitmapImage(new Uri(@"/Resources/Online.png", UriKind.Relative));
            }
            else
            {
                onlineImage.Source = new BitmapImage(new Uri(@"/Resources/Ofline.png", UriKind.Relative));
            }
        }

        public void MessageChange(bool message)
        {
            if (message)
            {
                messageImage.Source = new BitmapImage(new Uri(@"/Resources/Message.png", UriKind.Relative));
            }
            else
            {
                messageImage.Source = new BitmapImage(new Uri(@"/Resources/NonMessage.png", UriKind.Relative));
            }
        }
    }
}
