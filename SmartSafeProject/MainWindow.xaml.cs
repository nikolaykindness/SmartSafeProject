using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using FBFormAppExample;
using SmartSafeProject.ViewModel;

namespace SmartSafeProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowVM();

            Microsoft.Office.Interop.Word.Application wordObject = new Microsoft.Office.Interop.Word.Application();

            string fileName = "C:\\Users\\nikol\\Desktop\\Вареников Практика Отчет.docx";

            //var stream = new StreamReader(fileName).BaseStream;

            //byte[] readBytes = System.IO.File.ReadAllBytes(fileName);

            //File.WriteAllBytes($@"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\New File.docx", readBytes);

            //object File = stream;
            /*object nullobject = Missing.Value;

            Microsoft.Office.Interop.Word.Application wordobject = new Microsoft.Office.Interop.Word.Application();
            wordobject.DisplayAlerts = Microsoft.Office.Interop.Word.WdAlertLevel.wdAlertsNone;
            Microsoft.Office.Interop.Word._Document docs = wordObject.Documents.Open(ref File, ref nullobject, ref nullobject, ref nullobject, ref nullobject, ref nullobject, ref nullobject, ref nullobject, ref nullobject, ref nullobject, ref nullobject, ref nullobject, ref nullobject, ref nullobject, ref nullobject, ref nullobject);
            docs.ActiveWindow.Selection.WholeStory();
            docs.ActiveWindow.Selection.Copy();

            richTextBox.Paste();
            richTextBox.Document = new System.Windows.Documents.FlowDocument();

            docs.Close(ref nullobject, ref nullobject, ref nullobject);*/

            ////Подключени к БД
            //string workingDirectory = Environment.CurrentDirectory;
            //string projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.FullName;

            //string connectionString = $@"User=sysdba;Password=masterkey;Database={projectDirectory}\SMARTSAFE.FDB;DataSource=localhost;Port=3050;Dialect=3;Charset=NONE;Role=;Connection lifetime=15;Pooling=true;MinPoolSize=0;MaxPoolSize=50;Packet Size=8192;ServerType=0;";

            //FbConnection connecting = new FbConnection(connectionString);
            //connecting.Open();


            //#region Работа с добавлением файла 
            ////открытие формы для поиска файла
            //var dialogSearchImages = new Microsoft.Win32.OpenFileDialog();
            //dialogSearchImages.InitialDirectory = $@"{projectDirectory}\files\";
            //dialogSearchImages.DefaultExt = ".JPEG"; 
            //dialogSearchImages.Filter = "Image Files|*.BMP;*.JPG;*.GIF;*.JPEG";

            //bool? result = dialogSearchImages.ShowDialog();

            //if (result == true)
            //{
            //    //Чтение данных с файла
            //    string fileFullPath = dialogSearchImages.FileName;
            //    byte[] imageBytes = File.ReadAllBytes(fileFullPath);

            //    BitmapImage image = LoadImage(imageBytes);

            //    ImageStreamSource = image;

            //    FileInfo fileInfo = new FileInfo(fileFullPath);
            //    if (fileInfo.Exists)
            //    {
            //        //TODO: Надо забрать данные с файла
            //        MessageBox.Show($"Имя файла: {fileInfo.Name}");
            //        MessageBox.Show($"Время создания: {fileInfo.CreationTime}");
            //        MessageBox.Show($"Размер: {fileInfo.Length}");
            //    }
            //}
            //#endregion

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

        private void DocListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            FILES file = (FILES)((ListBox)sender).SelectedItem;

            string path = string.Empty;

            if(file != null)
            {
                if (file.FILETYPE.Contains(".docx"))
                {
                    path = $@"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\Temp_Doc.docx";

                    System.IO.File.WriteAllBytes(path, file.FILEDATA);

                    object File = path;
                    object nullobject = Missing.Value;

                    Microsoft.Office.Interop.Word.Application wordObject = new Microsoft.Office.Interop.Word.Application();
                    wordObject.DisplayAlerts = Microsoft.Office.Interop.Word.WdAlertLevel.wdAlertsNone;
                    Microsoft.Office.Interop.Word._Document docs = wordObject.Documents.Open(ref File, ref nullobject, ref nullobject, ref nullobject, ref nullobject, ref nullobject, ref nullobject, ref nullobject, ref nullobject, ref nullobject, ref nullobject, ref nullobject, ref nullobject, ref nullobject, ref nullobject, ref nullobject);

                    docs.ActiveWindow.Selection.WholeStory();
                    docs.ActiveWindow.Selection.Copy();

                    docRichTextBox.Paste();

                    docs.Close();
                    wordObject.Quit();
                }
                else if (file.FILETYPE.Contains(".txt"))
                {
                    path = $@"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\Temp_Doc.txt";

                    File.WriteAllBytes(path, file.FILEDATA);

                    FlowDocument flowDocument = new FlowDocument();

                    using (var fs = new FileStream(path, FileMode.Open))
                    {
                        var range = new TextRange(flowDocument.ContentStart, flowDocument.ContentEnd);
                        range.Load(fs, DataFormats.Text);
                    }

                    docRichTextBox.Document = flowDocument;
                }

                File.Delete(path);
            }
        }
    }
}
