using System;
using System.IO;
using System.Windows;

namespace ChatClient
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        App()
        {
            StreamReader sr = new StreamReader("Settings.conf");
            string line = sr.ReadLine();
            if (line.Contains("ru-RU"))
            {
                System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("ru-RU");
            }
            else
            {
                System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en");
            }
            //System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en"); 
            this.Dispatcher.UnhandledException += OnDispatcherUnhandledException;
        }
        void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            
            using (StreamWriter writer = new StreamWriter("log.txt", true))
            {
                
                writer.WriteLine(e.Exception.Message);
                
            }
        }
    }
}
