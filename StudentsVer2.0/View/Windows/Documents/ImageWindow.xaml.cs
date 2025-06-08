using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace StudentsVer2._0.View.Windows.Documents
{
    /// <summary>
    /// Логика взаимодействия для ImageWindow.xaml
    /// </summary>
    public partial class ImageWindow : Window
    {
        public byte[] ImageData { get; private set; } // Свойство для передачи данных об изображении
        public ImageWindow(byte[] imageData)
        {
            InitializeComponent();
            ImageData = imageData;
            LoadImage();
        }
        private void LoadImage()
        {
            ImageViewer.Source = LoadBitmapImage(ImageData);
        }
        private BitmapImage LoadBitmapImage(byte[] imageData)
        {
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = new MemoryStream(imageData);
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            return bitmapImage;
        }
        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            // Устанавливаем DialogResult в true, чтобы сообщить, что изображение нужно удалить
            DialogResult = true;
            Close();
        }
    }
}
