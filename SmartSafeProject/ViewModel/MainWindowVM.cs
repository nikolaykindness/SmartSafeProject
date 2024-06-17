using System;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using FirebirdSql.Data.FirebirdClient;

namespace SmartSafeProject.ViewModel
{
    internal class MainWindowVM : INotifyPropertyChanged
    {
        private ImageSource _imageSource;

        public ImageSource ImageStreamSource
        {
            get { return _imageSource; }
            set
            {
                _imageSource = value;
                OnProperyChanged(nameof(ImageStreamSource));
            }
        }

        public MainWindowVM()
        {
            string workingDirectory = Environment.CurrentDirectory;
            string projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.FullName;

            #region Работа с добавлением файла 
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

                BitmapImage image = LoadImage(imageBytes);

                ImageStreamSource = image;

                fileInfo = new FileInfo(fileFullPath);
                if (fileInfo.Exists)
                {
                    //TODO: Надо забрать данные с файла
                    MessageBox.Show($"Имя файла: {fileInfo.Name}");
                    MessageBox.Show($"Время создания: {fileInfo.CreationTime}");
                    MessageBox.Show($"Размер: {fileInfo.Length}");
                }
            }
            #endregion

            #region Подключени к БД и запись картинки
            string connectionString = $@"User=sysdba;Password=masterkey;Database={projectDirectory}\SMARTSAFE.FDB;DataSource=localhost;Port=3050;Dialect=3;Charset=NONE;Role=;Connection lifetime=15;Pooling=true;MinPoolSize=0;MaxPoolSize=50;Packet Size=8192;ServerType=0;";

            using (FbConnection connecting = new FbConnection(connectionString))
            {
                string insertQuery = string.Format(@"INSERT INTO Files (Id, Name, FileType, FileData) VALUES ({0}, '{1}', '{2}', @file_data)", fileInfo.Name, fileInfo.Extension);
                
                FbCommand fbcom = new FbCommand(insertQuery, connecting);
                
                FbParameter parBlob = new FbParameter("FileData", FbDbType.Binary);
                parBlob.Direction = ParameterDirection.Output; parBlob.Value = imageBytes;
                fbcom.Parameters.Add(parBlob);

                connecting.Open();
                fbcom.ExecuteNonQuery();

                FbDataAdapter executor = new FbDataAdapter();
                DataTable queryResult = new DataTable();

                string selectQuery = string.Format("SELECT * FROM Files");
                FbCommand selectCommand = new FbCommand(selectQuery, connecting);

                executor.SelectCommand = selectCommand;
                executor.Fill(queryResult);

                if(queryResult.Rows.Count > 0)
                {

                }
            }

            #endregion
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
