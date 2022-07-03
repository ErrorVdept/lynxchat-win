using System.IO;
using System.Windows;

namespace ChatClient
{
    /// <summary>
    /// Логика взаимодействия для SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            if (this.Title == "Settings")
            {
                RbEnglish.IsChecked = true;
                RbRussian.IsChecked = false;
            }
            else
            {
                RbEnglish.IsChecked = false;
                RbRussian.IsChecked = true;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string text = "en";
            if (RbRussian.IsChecked == true)
            {
                text = "ru-RU";
            }
            using (StreamWriter writer = new StreamWriter("Settings.conf", false))
            {
                writer.WriteLine(text);
            }
            if (this.Title == "Settings")
            {
                MessageBox.Show("Restart programm to change language");
            }
            else
            {
                MessageBox.Show("Перезагрузите программу для смены языка");
            }

            this.Close();
        }

        private void RbEnglish_Checked(object sender, RoutedEventArgs e)
        {
            RbRussian.IsChecked = false;
        }

        private void RbRussian_Checked(object sender, RoutedEventArgs e)
        {
            RbEnglish.IsChecked = false;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
