using ChatClient.Data;
using ChatClient.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;

namespace ChatClient
{
    /// <summary>
    /// Логика взаимодействия для AllFileWindow.xaml
    /// </summary>
    public partial class AllFileWindow : Window
    {
        public StorageService storageService = new StorageService();

        
        public static List<FileInStorage> files;
        public AllFileWindow()
        {
            InitializeComponent();
            LoadFiles();
        }

        public void LoadFiles()
        {

            FilePanel.Children.Clear();
            files = storageService.LoadFiles();
            
            foreach (var file in files)
            {
                FilePanel.Children.Add(file);
            }
            
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadFiles();

        }
    }
}
