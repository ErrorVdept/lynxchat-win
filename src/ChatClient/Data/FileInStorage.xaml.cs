using Microsoft.Win32;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;

using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ChatClient.Data
{
    /// <summary>
    /// Логика взаимодействия для FileInStorage.xaml
    /// </summary>
    public partial class FileInStorage : UserControl
    {
        public string Username { get; set; }
        public string FileData { get; set; }
        public string Filename { get; set; }
        public string DateMessage { get; set; }
        public FileInStorage( string username, string file, string filename, string dateMessage)
        {
            InitializeComponent();
            Username = username;
            FileData = file;
            Filename = filename;
            DateMessage = dateMessage;
            LabelName.Content = Username;
            LabelFile.Content = Filename;
            LabelDate.Content = DateMessage;
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            byte[] filedata = Convert.FromBase64String(FileData);

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = Filename; // Default file name
            string extention = System.IO.Path.GetExtension(Filename);
            saveFileDialog.DefaultExt = extention; // Default file extension
            saveFileDialog.Filter = "File (" + extention + ")|*" + extention;
            if (saveFileDialog.ShowDialog() == true)
                File.WriteAllBytes(saveFileDialog.FileName, filedata);
        }
    }
}
