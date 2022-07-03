using Microsoft.Win32;
using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ChatClient.Data
{
    /// <summary>
    /// Логика взаимодействия для MessageInList.xaml
    /// </summary>
    public partial class MessageInList : UserControl
    {
        public string Username { get; set; }
        public string Message { get; set; }
        public string DateMessage { get; set; }
        public string Iv { get; set; }
        public string Type { get; set; }
        public string Filename { get; set; }
        public string FileData { get; set; }
        private readonly DoubleAnimation myDoubleAnimation;
        public MessageInList(string username, string message, string datemessage, string iv, string type="message", string filename="", string fileData = "")
        {
            InitializeComponent();
            myDoubleAnimation = new DoubleAnimation();
            myDoubleAnimation.From = Opacity;
            myDoubleAnimation.To = 1;
            myDoubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.5));
            BeginAnimation(OpacityProperty, myDoubleAnimation);


            
            Username = username;
            Message = message;
            DateMessage = datemessage;
            Type = type;
            Filename = filename;
            FileData = fileData;
            Iv = iv;
            if (Username == AppWindow.CurrentUsername)
            {
                this.HorizontalAlignment = HorizontalAlignment.Right;
            }
            else
            {
                this.HorizontalAlignment = HorizontalAlignment.Left;
            }
            

            LabelUsername.Content = Username;
            if (type == "message")
            {
                LabelMessageText.Text = Message;
            } else
            {
                LabelMessageText.Text = filename;
                var bc = new BrushConverter();
                LabelMessageText.Foreground = (System.Windows.Media.Brush)bc.ConvertFrom("#FF7F37ED"); // "#FF7F37ED";
            }
            
            LabelDatetime.Content = DateMessage;
            
        }

        

        private void LabelMessageText_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Type == "file")
            {
                byte[] filedata = Convert.FromBase64String(FileData);

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.FileName = Filename; // Default file name
                string extention = Path.GetExtension(Filename);
                saveFileDialog.DefaultExt = extention; // Default file extension
                saveFileDialog.Filter = "File ("+ extention + ")|*" + extention;
                if (saveFileDialog.ShowDialog() == true)
                    File.WriteAllBytes(saveFileDialog.FileName, filedata);
                
                
            }
        }
    }
}
