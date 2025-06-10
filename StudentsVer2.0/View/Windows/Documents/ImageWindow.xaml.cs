using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace StudentsVer2._0.View.Windows.Documents
{
    /// <summary>
    /// Логика взаимодействия для ImageWindow.xaml
    /// </summary>
    public partial class ImageWindow : Window
    {
        private int imageId;
        public ImageWindow(byte[] imageData, int imageId)
        {
            InitializeComponent();

            ImageDisplay.Source = LoadImage(imageData);
            this.imageId = imageId;
        }

        // Преобразование байтов в изображение
        private static BitmapImage LoadImage(byte[] imageData)
        {
            var bitmap = new BitmapImage();
            using (var stream = new MemoryStream(imageData))
            {
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
            }
            return bitmap;
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Удалить изображение?",
                                    "Подтверждение",
                                    MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Поиск и удаление записи StudentImage
                    var studentImage = App.context.StudentImage
                        .FirstOrDefault(si => si.ImageID == imageId);

                    if (studentImage != null)
                    {
                        App.context.StudentImage.Remove(studentImage);
                        App.context.SaveChanges();
                    }

                    // Удаление записи ImageDocument
                    var image = App.context.ImageDocument.Find(imageId);
                    if (image != null)
                    {
                        App.context.ImageDocument.Remove(image);
                        App.context.SaveChanges();
                    }

                    DialogResult = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}");
                }
            }
        }

        private void DownloadBtn_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "JPEG Image|*.jpg|PNG Image|*.png",
                FileName = "document_image"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    var image = App.context.ImageDocument.Find(imageId);
                    if (image != null)
                    {
                        File.WriteAllBytes(saveFileDialog.FileName, image.ImageDoc);
                        MessageBox.Show("Изображение успешно сохранено");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка сохранения: {ex.Message}");
                }
            }
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
