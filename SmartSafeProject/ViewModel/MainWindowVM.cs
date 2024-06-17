using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FBFormAppExample;
using FirebirdSql.Data.FirebirdClient;

namespace SmartSafeProject.ViewModel
{
    internal class MainWindowVM : INotifyPropertyChanged
    {
        private ImageSource _imageSource;

        public ObservableCollection<ImageSource> DataImageStreamSource;
        /*
        public ImageSource ImageStreamSource
        {
            get { return _imageSource; }
            set
            {
                _imageSource = value;
                OnProperyChanged(nameof(ImageStreamSource));
            }
        }*/
        public ICommand LoadImageDialog => new RelayCommand(LoadImageDialogOpen);
        string connectionString;
        private void LoadImageDialogOpen(object obj)
        {
            #region Работа с добавлением файла 
            string workingDirectory = Environment.CurrentDirectory;
            string projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.FullName;

            //открытие формы для поиска файла
            var dialogSearchImages = new Microsoft.Win32.OpenFileDialog();
            dialogSearchImages.InitialDirectory = $@"{projectDirectory}\files\";
            dialogSearchImages.DefaultExt = ".JPEG";
            dialogSearchImages.Filter = "Image Files|*.BMP;*.JPG;*.GIF;*.JPEG";

            bool? result = dialogSearchImages.ShowDialog();

            byte[] imageBytes = null;
            FileInfo fileInfo = null;

            if (result == true)
            {
                //Чтение данных с файла
                string fileFullPath = dialogSearchImages.FileName;
                imageBytes = File.ReadAllBytes(fileFullPath);

                fileInfo = new FileInfo(fileFullPath);
                if (fileInfo.Exists)
                {
                    //MessageBox.Show($"Имя файла: {fileInfo.Name}");
                    //MessageBox.Show($"Время создания: {fileInfo.CreationTime}");
                    //MessageBox.Show($"Размер: {fileInfo.Length}");
                }
            }
            #endregion


            string connectionString = $@"User=sysdba;Password=masterkey;Database={projectDirectory}\SMARTSAFE.FDB;DataSource=localhost;Port=3050;Dialect=3;Charset=NONE;Role=;Connection lifetime=15;Pooling=true;MinPoolSize=0;MaxPoolSize=50;Packet Size=8192;ServerType=0;";

            using (FbConnection connection = new FbConnection(connectionString))
            {
                #region Добавление картинок в БД
                connection.Open();
                FbTransaction transaction = connection.BeginTransaction();
                string insertQuery = string.Format(@"INSERT INTO Files (Id, Name, FileType, FileData) VALUES ({0}, '{1}', '{2}', @FILE_DATA)", DataImageStreamSource.Count + 6, fileInfo.Name, fileInfo.Extension);

                FbCommand command = new FbCommand();
                command.CommandText = insertQuery;
                command.Connection = connection;
                command.Transaction = transaction;

                command.Parameters.Add("@FILE_DATA", FbDbType.Binary, imageBytes.Length, "FILE_DATA");
                command.Parameters[0].Value = imageBytes;

                // Execute query
                command.ExecuteNonQuery();


                // Commit changes
                transaction.Commit();

                // Free command resources in Firebird Server
                command.Dispose();
                #endregion

                #region Получение картинок из БД
                FbDataAdapter executor = new FbDataAdapter();
                DataTable queryResult = new DataTable();

                string selectQuery = string.Format("SELECT * FROM Files");
                FbCommand selectCommand = new FbCommand(selectQuery, connection);

                executor.SelectCommand = selectCommand;
                executor.Fill(queryResult);
                #endregion 

                #region Преобразование данных из БД в картинку
                FILES model = new FILES();

                if (queryResult.Rows.Count == 1)
                {
                    model.ID = (int)queryResult.Rows[0].ItemArray[0];
                    model.NAME = (string)queryResult.Rows[0].ItemArray[1];
                    model.FILETYPE = (string)queryResult.Rows[0].ItemArray[2];
                    model.FILEDATA = (byte[])queryResult.Rows[0].ItemArray[3];
                }

                BitmapImage image = LoadImage(model.FILEDATA);
                DataImageStreamSource.Add(image);
                #endregion
            }

        }

        public MainWindowVM()
        {
            DataImageStreamSource = new ObservableCollection<ImageSource>();
        }

        private BitmapImage LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;
            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnProperyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
