using System.IO;
using System;
using System.Windows;
using FirebirdSql.Data.FirebirdClient;
using System.Data.Entity.Infrastructure;
using EntityFramework.Firebird;
using EntityFramework;
using Microsoft.Win32;
using System.Windows.Controls;
using System.Data.Entity.Core.Objects;
using System.Data.Entity;

namespace SmartSafeProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

  
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //Подключени к БД
            string workingDirectory = Environment.CurrentDirectory;
            string projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.FullName;

            string connectionString = $@"User=sysdba;Password=masterkey;Database={projectDirectory}\SMARTSAFE.FDB;DataSource=localhost;Port=3050;Dialect=3;Charset=NONE;Role=;Connection lifetime=15;Pooling=true;MinPoolSize=0;MaxPoolSize=50;Packet Size=8192;ServerType=0;";
            
            FbConnection connecting = new FbConnection(connectionString);
            connecting.Open();

            //Получаем контекст
            var dbContext = AppVariables.CreateDbContext();



            #region Работа с добавлением файла 
            //открытие формы для поиска файла
            var dialogSearchImages = new Microsoft.Win32.OpenFileDialog();
            dialogSearchImages.InitialDirectory = $@"{projectDirectory}\files\";
            dialogSearchImages.DefaultExt = ".JPEG"; 
            dialogSearchImages.Filter = "Image Files|*.BMP;*.JPG;*.GIF;*.JPEG";

            bool? result = dialogSearchImages.ShowDialog();

            if (result == true)
            {
                //Чтение данных с файла
                string fileFullPath = dialogSearchImages.FileName;
                FileInfo fileInfo = new FileInfo(fileFullPath);
                if (fileInfo.Exists)
                {
                    //TODO: Надо забрать данные с файла
                    MessageBox.Show($"Имя файла: {fileInfo.Name}");
                    MessageBox.Show($"Время создания: {fileInfo.CreationTime}");
                    MessageBox.Show($"Размер: {fileInfo.Length}");
                }
            }
            #endregion

        }
    }
}
