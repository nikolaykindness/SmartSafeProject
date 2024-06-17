using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using FBFormAppExample;
using FirebirdSql.Data.FirebirdClient;
using System.Windows.Documents;

namespace SmartSafeProject.ViewModel
{
    internal class MainWindowVM : INotifyPropertyChanged
    {
        #region Свойства картинок
        private ObservableCollection<ImageFile> _dataImageSources;
        private ImageFile _selectedImageFile;

        public ObservableCollection<ImageFile> DataImageSources
        {
            get { return _dataImageSources; }
            set
            {
                _dataImageSources = value;
                OnProperyChanged(nameof(DataImageSources));
            }
        }

        // События при выборе картинки
        private delegate void SelectedImageFileHandler();
        private event SelectedImageFileHandler SelectedImageFileChanged;

        public ImageFile SelectedImageFile
        {
            get { return _selectedImageFile; }
            set
            {
                _selectedImageFile = value;
                OnProperyChanged(nameof(SelectedImageFile));
                SelectedImageFileChanged?.Invoke();
            }
        }
        #endregion

        #region Свойства документов
        private FlowDocument _flowDocument;

        public FlowDocument FlowDoc
        {
            get { return _flowDocument; }
            set
            {
                _flowDocument = value;
                OnProperyChanged(nameof(FlowDoc));
            }
        }
        
        private ObservableCollection<FILES> _docFiles;
        private FILES _selectedDocFile;

        public ObservableCollection<FILES> DocFiles
        {
            get { return _docFiles; }
            set
            {
                _docFiles = value;
                OnProperyChanged(nameof(DocFiles));
            }
        }

        public FILES SelectedDocFile 
        { 
            get { return _selectedDocFile; }
            set
            {
                _selectedDocFile = value;
                OnProperyChanged(nameof(SelectedDocFile));
            }
        }
        #endregion

        public ICommand LoadFileDialog => new RelayCommand(LoadFileDialogOpen);
        public ICommand DeleteAllFiles => new RelayCommand(DeleteAllFilesExecute);

        private void LoadFileDialogOpen(object obj)
        {
            #region Работа с добавлением файла 
            string workingDirectory = Environment.CurrentDirectory;
            string projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.FullName;

            //открытие формы для поиска файла
            var dialogSearchImages = new Microsoft.Win32.OpenFileDialog();
            dialogSearchImages.InitialDirectory = $@"{projectDirectory}\files\";
            dialogSearchImages.Filter = "Image Files|*.bmp;*.jpg;*.gif;*.jpeg;*.png|Document Files|*.doc;*.docx;*.txt;*.rtf";

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
            else
            {
                return;
            }
            #endregion

            string connectionString = $@"User=sysdba;Password=masterkey;Database={projectDirectory}\SMARTSAFE.FDB;DataSource=localhost;Port=3050;Dialect=3;Charset=UTF8;Role=;Connection lifetime=15;Pooling=true;MinPoolSize=0;MaxPoolSize=50;Packet Size=8192;ServerType=0;";

            using (FbConnection connection = new FbConnection(connectionString))
            {
                #region Добавление картинок в БД
                connection.Open();
                FbTransaction transaction = connection.BeginTransaction();
                string insertQuery = string.Format(@"INSERT INTO Files (Id, Name, FileType, FileData) VALUES ({0}, '{1}', '{2}', @FILE_DATA)", DataImageSources.Count, fileInfo.Name, fileInfo.Extension);

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

                #region Преобразование данных из БД
                DocFiles.Clear();
                DataImageSources.Clear();
                FILES model = new FILES();

                if (queryResult.Rows.Count > 0)
                {
                    for (int i = 0; i < queryResult.Rows.Count; i++)
                    {
                        model = new FILES();

                        model.ID = (int)queryResult.Rows[i].ItemArray[0];
                        model.NAME = (string)queryResult.Rows[i].ItemArray[1];
                        model.FILETYPE = (string)queryResult.Rows[i].ItemArray[2];
                        model.FILEDATA = (byte[])queryResult.Rows[i].ItemArray[3];

                        if(model.FILETYPE.Contains(".bmp")
                            || model.FILETYPE.Contains(".jpg")
                            || model.FILETYPE.Contains(".jpeg")
                            || model.FILETYPE.Contains(".gif")
                            || model.FILETYPE.Contains(".png"))
                        {
                            ImageFile imageFile = new ImageFile(model);

                            DataImageSources.Add(imageFile);
                        }
                        else
                        {
                            DocFiles.Add(model);
                        }
                    }
                }
                #endregion

                connection.Close();
            }

        }

        private void DeleteAllFilesExecute(object obj)
        {
            string workingDirectory = Environment.CurrentDirectory;
            string projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.FullName;

            string connectionString = $@"User=sysdba;Password=masterkey;Database={projectDirectory}\SMARTSAFE.FDB;DataSource=localhost;Port=3050;Dialect=3;Charset=UTF8;Role=;Connection lifetime=15;Pooling=true;MinPoolSize=0;MaxPoolSize=50;Packet Size=8192;ServerType=0;";

            using (FbConnection connection = new FbConnection(connectionString))
            {
                connection.Open();

                FbTransaction transaction = connection.BeginTransaction();
                string deleteAllQuery = "DELETE FROM Files";

                FbCommand deleteAllCommand = new FbCommand();
                deleteAllCommand.CommandText = deleteAllQuery;
                deleteAllCommand.Transaction = transaction;
                deleteAllCommand.Connection = connection;

                // Execute query
                deleteAllCommand.ExecuteNonQuery();

                // Commit changes
                transaction.Commit();

                // Free command resources in Firebird Server
                deleteAllCommand.Dispose();

                connection.Close();
            }

            using (FbConnection connection = new FbConnection(connectionString))
            {
                connection.Open();

                #region Получение картинок из БД
                FbDataAdapter executor = new FbDataAdapter();
                DataTable queryResult = new DataTable();

                string selectQuery = string.Format("SELECT * FROM Files");
                FbCommand selectCommand = new FbCommand(selectQuery, connection);

                executor.SelectCommand = selectCommand;
                executor.Fill(queryResult);
                #endregion

                #region Преобразование данных из БД в картинку
                DocFiles.Clear();
                DataImageSources.Clear();
                FILES model = new FILES();

                if (queryResult.Rows.Count > 0)
                {
                    for (int i = 0; i < queryResult.Rows.Count; i++)
                    {
                        model = new FILES();

                        model.ID = (int)queryResult.Rows[i].ItemArray[0];
                        model.NAME = (string)queryResult.Rows[i].ItemArray[1];
                        model.FILETYPE = (string)queryResult.Rows[i].ItemArray[2];
                        model.FILEDATA = (byte[])queryResult.Rows[i].ItemArray[3];

                        if (model.FILETYPE.Contains(".bmp")
                            || model.FILETYPE.Contains(".jpg")
                            || model.FILETYPE.Contains(".jpeg")
                            || model.FILETYPE.Contains(".gif")
                            || model.FILETYPE.Contains(".png"))
                        {
                            ImageFile imageFile = new ImageFile(model);

                            DataImageSources.Add(imageFile);
                        }
                        else
                        {
                            DocFiles.Add(model);
                        }
                    }
                }
                #endregion
            }
        }

        public MainWindowVM()
        {
            DataImageSources = new ObservableCollection<ImageFile>();
            DocFiles = new ObservableCollection<FILES>();

            string workingDirectory = Environment.CurrentDirectory;
            string projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.FullName;

            string connectionString = $@"User=sysdba;Password=masterkey;Database={projectDirectory}\SMARTSAFE.FDB;DataSource=localhost;Port=3050;Dialect=3;Charset=UTF8;Role=;Connection lifetime=15;Pooling=true;MinPoolSize=0;MaxPoolSize=50;Packet Size=8192;ServerType=0;";

            using (FbConnection connection = new FbConnection(connectionString))
            {
                connection.Open();

                #region Получение картинок из БД
                FbDataAdapter executor = new FbDataAdapter();
                DataTable queryResult = new DataTable();

                string selectQuery = string.Format("SELECT * FROM Files");
                FbCommand selectCommand = new FbCommand(selectQuery, connection);

                executor.SelectCommand = selectCommand;
                executor.Fill(queryResult);
                #endregion

                #region Преобразование данных из БД
                DocFiles.Clear();
                DataImageSources.Clear();
                FILES model = new FILES();

                if (queryResult.Rows.Count > 0)
                {
                    for (int i = 0; i < queryResult.Rows.Count; i++)
                    {
                        model = new FILES();

                        model.ID = (int)queryResult.Rows[i].ItemArray[0];
                        model.NAME = (string)queryResult.Rows[i].ItemArray[1];
                        model.FILETYPE = (string)queryResult.Rows[i].ItemArray[2];
                        model.FILEDATA = (byte[])queryResult.Rows[i].ItemArray[3];

                        if (model.FILETYPE.Contains(".bmp")
                            || model.FILETYPE.Contains(".jpg")
                            || model.FILETYPE.Contains(".jpeg")
                            || model.FILETYPE.Contains(".gif")
                            || model.FILETYPE.Contains(".png"))
                        {
                            ImageFile imageFile = new ImageFile(model);

                            DataImageSources.Add(imageFile);
                        }
                        else
                        {
                            DocFiles.Add(model);
                        }
                    }
                }
                #endregion
            }
        }

        public ICommand DeleteSelectedImage => new RelayCommand(DeleteSelectedImageExecute);
        public ICommand DeleteSelectedDoc => new RelayCommand(DeleteSelectedDocExecute);

        private void DeleteSelectedImageExecute(object obj)
        {
            string workingDirectory = Environment.CurrentDirectory;
            string projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.FullName;

            string connectionString = $@"User=sysdba;Password=masterkey;Database={projectDirectory}\SMARTSAFE.FDB;DataSource=localhost;Port=3050;Dialect=3;Charset=UTF8;Role=;Connection lifetime=15;Pooling=true;MinPoolSize=0;MaxPoolSize=50;Packet Size=8192;ServerType=0;";

            using (FbConnection connection = new FbConnection(connectionString))
            {
                connection.Open();

                string deleteQuery = $"DELETE FROM Files WHERE ID = {SelectedImageFile.FileInfo.ID}";

                FbTransaction transaction = connection.BeginTransaction();
                FbCommand deleteAllCommand = new FbCommand();
                deleteAllCommand.CommandText = deleteQuery;
                deleteAllCommand.Transaction = transaction;
                deleteAllCommand.Connection = connection;

                // Execute query
                deleteAllCommand.ExecuteNonQuery();

                // Commit changes
                transaction.Commit();

                // Free command resources in Firebird Server
                deleteAllCommand.Dispose();

                #region Получение картинок из БД
                FbDataAdapter executor = new FbDataAdapter();
                DataTable queryResult = new DataTable();

                string selectQuery = string.Format("SELECT * FROM Files");
                FbCommand selectCommand = new FbCommand(selectQuery, connection);

                executor.SelectCommand = selectCommand;
                executor.Fill(queryResult);
                #endregion

                #region Преобразование данных из БД
                DocFiles.Clear();
                DataImageSources.Clear();
                FILES model = new FILES();

                if (queryResult.Rows.Count > 0)
                {
                    for (int i = 0; i < queryResult.Rows.Count; i++)
                    {
                        model = new FILES();

                        model.ID = (int)queryResult.Rows[i].ItemArray[0];
                        model.NAME = (string)queryResult.Rows[i].ItemArray[1];
                        model.FILETYPE = (string)queryResult.Rows[i].ItemArray[2];
                        model.FILEDATA = (byte[])queryResult.Rows[i].ItemArray[3];

                        if (model.FILETYPE.Contains(".bmp")
                            || model.FILETYPE.Contains(".jpg")
                            || model.FILETYPE.Contains(".jpeg")
                            || model.FILETYPE.Contains(".gif")
                            || model.FILETYPE.Contains(".png"))
                        {
                            ImageFile imageFile = new ImageFile(model);

                            DataImageSources.Add(imageFile);
                        }
                        else
                        {
                            DocFiles.Add(model);
                        }
                    }
                }
                #endregion

                connection.Close();
            }
        }
        private void DeleteSelectedDocExecute(object obj)
        {
            string workingDirectory = Environment.CurrentDirectory;
            string projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.FullName;

            string connectionString = $@"User=sysdba;Password=masterkey;Database={projectDirectory}\SMARTSAFE.FDB;DataSource=localhost;Port=3050;Dialect=3;Charset=UTF8;Role=;Connection lifetime=15;Pooling=true;MinPoolSize=0;MaxPoolSize=50;Packet Size=8192;ServerType=0;";

            using (FbConnection connection = new FbConnection(connectionString))
            {
                connection.Open();

                string deleteQuery = $"DELETE FROM Files WHERE ID = {SelectedDocFile.ID}";

                FbTransaction transaction = connection.BeginTransaction();
                FbCommand deleteAllCommand = new FbCommand();
                deleteAllCommand.CommandText = deleteQuery;
                deleteAllCommand.Transaction = transaction;
                deleteAllCommand.Connection = connection;

                // Execute query
                deleteAllCommand.ExecuteNonQuery();

                // Commit changes
                transaction.Commit();

                // Free command resources in Firebird Server
                deleteAllCommand.Dispose();

                #region Получение картинок из БД
                FbDataAdapter executor = new FbDataAdapter();
                DataTable queryResult = new DataTable();

                string selectQuery = string.Format("SELECT * FROM Files");
                FbCommand selectCommand = new FbCommand(selectQuery, connection);

                executor.SelectCommand = selectCommand;
                executor.Fill(queryResult);
                #endregion

                #region Преобразование данных из БД
                DocFiles.Clear();
                DataImageSources.Clear();
                FILES model = new FILES();

                if (queryResult.Rows.Count > 0)
                {
                    for (int i = 0; i < queryResult.Rows.Count; i++)
                    {
                        model = new FILES();

                        model.ID = (int)queryResult.Rows[i].ItemArray[0];
                        model.NAME = (string)queryResult.Rows[i].ItemArray[1];
                        model.FILETYPE = (string)queryResult.Rows[i].ItemArray[2];
                        model.FILEDATA = (byte[])queryResult.Rows[i].ItemArray[3];

                        if (model.FILETYPE.Contains(".bmp")
                            || model.FILETYPE.Contains(".jpg")
                            || model.FILETYPE.Contains(".jpeg")
                            || model.FILETYPE.Contains(".gif")
                            || model.FILETYPE.Contains(".png"))
                        {
                            ImageFile imageFile = new ImageFile(model);

                            DataImageSources.Add(imageFile);
                        }
                        else
                        {
                            DocFiles.Add(model);
                        }
                    }
                }
                #endregion

                connection.Close();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnProperyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
