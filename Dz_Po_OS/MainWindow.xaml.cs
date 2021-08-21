using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using Microsoft.WindowsAPICodePack.Dialogs;
namespace Dz_Po_OS
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Thread tr0;
        Thread tr1;
        Thread tr2;
        Thread tr3;
        VKParsing parse;
        string svpath = "";
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Op(object sender, RoutedEventArgs e)//открыть ВК
        {
            parse = new VKParsing();
            Thread tr = new Thread(() => parse.OpenWeb());
            tr.Start();
            tr.Join();
            but.IsEnabled = true; Fres.IsEnabled = true;
        }

        private void Auth(object sender, RoutedEventArgs e)//Авторизация
        {
            Thread.Sleep(1000);
            parse.login = login.Text;
            parse.password = password.Text;
            parse.Authorize();
            pars.IsEnabled = true;
        }

        private void Parsering(object sender, RoutedEventArgs e)//Парсинг

        {
            pars.IsEnabled = false;
            int cnt = Int32.Parse(textbox1.Text);
            parse.ReadPost(cnt);
            
            //Запись в три файла тремя разными потоками
            tr1 = new Thread(() => parse.Serialization(svpath + "TextMediaVK.json", VKParsing.SerType.Text));
            tr1.Start();
            tr2 = new Thread(() => parse.Serialization(svpath + "ImageMediaVK.json", VKParsing.SerType.Image));
            tr2.Start();
            tr3 = new Thread(() => parse.Serialization(svpath + "ReferOnPostVK.json", VKParsing.SerType.Refer));
            tr3.Start();
            tr1.Join(); tr2.Join(); tr3.Join();
            pars.IsEnabled = true;
        }



        private void Refreshing(object sender, RoutedEventArgs e)//Обновить браузер
        {
            parse.Fresh();
        }

        private void DirectoryPath_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new CommonOpenFileDialog();
            dlg.Title = "Место сохранения файлов";
            dlg.IsFolderPicker = true;
            dlg.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            dlg.DefaultDirectory = AppDomain.CurrentDomain.BaseDirectory;
            dlg.EnsureFileExists = true;
            dlg.EnsurePathExists = true;
            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                Directory.Text = dlg.FileName;
                svpath = Directory.Text+"/";
            }
        }

        //private void Button_Click(object sender, RoutedEventArgs e)//Выход из приложения
        //{

        //    tr0.Join(); tr1.Join(); tr2.Join(); tr3.Join();
        //    Application.Current.Shutdown();
        //}

        //private void RadioButton_Checked(object sender, RoutedEventArgs e)
        //{
        //    Thread ILoveYou = new Thread(parse.Enigma);
        //    ILoveYou.Start();
        //}
    }
}

    

