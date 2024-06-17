using FBFormAppExample;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SmartSafeProject
{
    internal class ImageFile : INotifyPropertyChanged
    {
        FILES _fileInfo;
        ImageSource _dataImageSource;

        public ImageSource DataImageSource
        {
            get { return _dataImageSource; }
            set
            {
                _dataImageSource = value;
                OnPropertyChanged(nameof(DataImageSource));
            }
        }

        public FILES FileInfo
        {
            get { return _fileInfo; }
            set
            {
                _fileInfo = value;
                OnPropertyChanged(nameof(FileInfo));
            }
        }

        public ImageFile() { }

        public ImageFile(FILES fileInfo)
        {
            FileInfo = new FILES()
            {
                ID = fileInfo.ID,
                NAME = fileInfo.NAME,
                FILETYPE = fileInfo.FILETYPE,
                FILEDATA = fileInfo.FILEDATA
            };

            BitmapImage imageSource = LoadImage(fileInfo.FILEDATA);
            
            DataImageSource = imageSource;
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
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
