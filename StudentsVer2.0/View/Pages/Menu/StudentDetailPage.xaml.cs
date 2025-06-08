using Microsoft.Win32;
using StudentsVer2._0.AppData;
using StudentsVer2._0.Model;
using StudentsVer2._0.View.Windows.Documents;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Image = StudentsVer2._0.Model.Image;

namespace StudentsVer2._0.View.Pages.Menu
{
    /// <summary>
    /// Логика взаимодействия для StudentDetailPage.xaml
    /// </summary>
    public partial class StudentDetailPage : Page
    {
        public StudentDetailPage(Student student, string groupTitle)
        {
            InitializeComponent();

            PassportBorder.MouseLeftButtonDown += PassportClick;
            MilitaryCertificateBorder.MouseLeftButtonDown += MilitaryCertificateClick;
            INNBorder.MouseLeftButtonDown += INNClick;
            InsuranceBorder.MouseLeftButtonDown += InsuranceClick;

            SurnameTbl.Text = student.Surname;
            NameTbl.Text = student.Name;
            PatronymicTbl.Text = student.Patronymic ?? "Нет данных"; // Если отчество отсутствует
            GroupTbl.Text = groupTitle;

            if (SelectedStudentHelper.selectedStudent.PassportID != null) UpdatePassportIcon();
            if (SelectedStudentHelper.selectedStudent.MilitaryCertificateID != null) UpdateMilitaryCertificateIcon();
            if (SelectedStudentHelper.selectedStudent.INNID != null) UpdateINNIcon();
            if (SelectedStudentHelper.selectedStudent.InsuranceNumberID != null) UpdateInsuranceIcon();

            if (SelectedStudentHelper.selectedStudent.GenderID == 2)
            {
                MilitaryCertificateBorder.Visibility = Visibility.Collapsed;
            }
            LoadImages(); // Загрузка изображений при инициализации страницы
        }

        private void LoadImages()
        {
            var student = SelectedStudentHelper.selectedStudent;
            ImagesPanel.Children.Clear(); // Очищаем панель перед загрузкой изображений

            // Получаем все изображения для текущего студента
            var studentImages = App.context.StudentImage
                .Where(si => si.StudentID == student.ID)
                .Select(si => si.Image)
                .ToList();

            foreach (var image in studentImages)
            {
                AddImageToPanel(image); // Добавляем изображение в панель
            }

            // Меняем иконку загрузки, если изображения есть
            if (studentImages.Any())
            {
                // Здесь можно изменить иконку, например, поменять цвет или добавить галочку
            }
        }

        private void AddImageToPanel(Image imageEntity)
        {
            // Получаем данные изображения из сущности Image
            var imageBytes = imageEntity.Image1;

            // Используем WPF-контрол Image
            var imageControl = new System.Windows.Controls.Image
            {
                Source = LoadBitmapImage(imageBytes), // Устанавливаем источник изображения
                Width = 100, // Ширина изображения
                Height = 100, // Высота изображения
                Margin = new Thickness(5), // Отступы
                Cursor = System.Windows.Input.Cursors.Hand // Изменяем курсор при наведении
            };

            // Обработчик клика на изображение для открытия ImageWindow
            imageControl.MouseDown += (s, e) =>
            {
                var imageWindow = new ImageWindow(imageBytes);
                imageWindow.ShowDialog();
            };

            var deleteButton = new System.Windows.Controls.Button
            {
                Content = "Удалить", // Текст на кнопке
                Tag = imageEntity.ID, // Сохраняем ID изображения для удаления
                Margin = new Thickness(5) // Отступы
            };

            // Обработчик нажатия на кнопку "Удалить"
            deleteButton.Click += (s, e) =>
            {
                // Диалог с подтверждением удаления
                var result = MessageBox.Show(
                    "Вы действительно хотите удалить изображение?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    DeleteImage(imageEntity.ID); // Удаляем изображение
                }
            };

            var stackPanel = new System.Windows.Controls.StackPanel
            {
                Orientation = System.Windows.Controls.Orientation.Vertical // Ориентация панели
            };
            stackPanel.Children.Add(imageControl); // Добавляем изображение в панель
            stackPanel.Children.Add(deleteButton); // Добавляем кнопку в панель

            ImagesPanel.Children.Add(stackPanel); // Добавляем панель в ImagesPanel
        }

        private void DeleteImage(int imageId)
        {
            // Находим изображение в таблице Image
            var imageToDelete = App.context.Image.FirstOrDefault(img => img.ID == imageId);
            if (imageToDelete != null)
            {
                // Находим связь в таблице StudentImage
                var studentImageToDelete = App.context.StudentImage.FirstOrDefault(si => si.ImageID == imageToDelete.ID);
                if (studentImageToDelete != null)
                {
                    // Удаляем связь из таблицы StudentImage
                    App.context.StudentImage.Remove(studentImageToDelete);
                    // Удаляем изображение из таблицы Image
                    App.context.Image.Remove(imageToDelete);
                    // Сохраняем изменения в базе данных
                    App.context.SaveChanges();
                }
            }

            // Перезагружаем изображения
            LoadImages();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                var imageId = (int)button.Tag; // Получаем ID изображения из Tag кнопки

                // Находим изображение в таблице Image
                var imageToDelete = App.context.Image.FirstOrDefault(img => img.ID == imageId);
                if (imageToDelete != null)
                {
                    // Находим связь в таблице StudentImage
                    var studentImageToDelete = App.context.StudentImage.FirstOrDefault(si => si.ImageID == imageToDelete.ID);
                    if (studentImageToDelete != null)
                    {
                        // Удаляем связь из таблицы StudentImage
                        App.context.StudentImage.Remove(studentImageToDelete);
                        // Удаляем изображение из таблицы Image
                        App.context.Image.Remove(imageToDelete);
                        // Сохраняем изменения в базе данных
                        App.context.SaveChanges();
                    }
                }

                // Перезагружаем изображения
                LoadImages();
            }
        }

        private BitmapImage LoadBitmapImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = new MemoryStream(imageData);
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            return bitmapImage;
        }
        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var filePath = openFileDialog.FileName;
                var imageData = File.ReadAllBytes(filePath);

                // Открываем окно ImageWindow с выбранным изображением
                var imageWindow = new ImageWindow(imageData);
                if (imageWindow.ShowDialog() == true)
                {
                    // Пользователь нажал "Удалить", удаляем изображение из базы данных
                    DeleteImage(imageWindow.ImageData);
                }
                else
                {
                    // Пользователь закрыл окно, сохраняем изображение в базу данных
                    SaveImage(imageData);
                }

                LoadImages(); // Перезагрузка изображений
            }
        }

        private void SaveImage(byte[] imageData)
        {
            var student = SelectedStudentHelper.selectedStudent;

            // Создаем новую запись в таблице Image
            var newImage = new Image { Image1 = imageData };
            App.context.Image.Add(newImage);
            App.context.SaveChanges();

            // Связываем изображение со студентом в таблице StudentImage
            var studentImage = new StudentImage
            {
                ImageID = newImage.ID,
                StudentID = student.ID
            };
            App.context.StudentImage.Add(studentImage);
            App.context.SaveChanges();
        }

        private void DeleteImage(byte[] imageData)
        {
            var student = SelectedStudentHelper.selectedStudent;

            // Находим изображение в таблице Image
            var imageToDelete = App.context.Image.FirstOrDefault(img => img.Image1 == imageData);
            if (imageToDelete != null)
            {
                // Находим связь в таблице StudentImage
                var studentImageToDelete = App.context.StudentImage.FirstOrDefault(si => si.ImageID == imageToDelete.ID);
                if (studentImageToDelete != null)
                {
                    App.context.StudentImage.Remove(studentImageToDelete);
                    App.context.Image.Remove(imageToDelete);
                    App.context.SaveChanges();
                }
            }
        }

        private void ViewImage_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var imageData = button.Tag as byte[];

            // Открываем окно ImageWindow для просмотра изображения
            var imageWindow = new ImageWindow(imageData);
            imageWindow.ShowDialog();
        }

        private void UpdatePassportIcon()
        {
            var border = PassportBorder;
            var image = PassportIconImg;
            var textBlock = PassportTextTbl;

            if (SelectedStudentHelper.selectedStudent.PassportID != null)
            {
                // Меняем на серую иконку, если паспорт есть
                border.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6C6C6C"));
                image.Source = new BitmapImage(new Uri("/Resource/Image/check.png", UriKind.Relative));
                textBlock.Text = "Паспорт заполнен";
                textBlock.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6C6C6C"));
            }
            else
            {
                // Или оставляем стандартную, если нет
                border.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5367D3"));
                image.Source = new BitmapImage(new Uri("/Resource/Icons/Group 17.png", UriKind.Relative));
                textBlock.Text = "Заполнить паспорт";
                textBlock.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5367D3"));
            }
        }
        private void UpdateMilitaryCertificateIcon()
        {
            var border = MilitaryCertificateBorder;
            var image = MilitaryCertificateIconImg;
            var textBlock = MilitaryCertificateTbl;

            if (SelectedStudentHelper.selectedStudent.MilitaryCertificateID != null)
            {
                // Меняем на серую иконку, если приписное св-во есть
                border.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6C6C6C"));
                image.Source = new BitmapImage(new Uri("/Resource/Image/check.png", UriKind.Relative));
                textBlock.Text = "Приписное св-во заполнено";
                textBlock.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6C6C6C"));
            }
            else
            {
                // Или оставляем стандартную, если нет
                border.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5367D3"));
                image.Source = new BitmapImage(new Uri("/Resource/Icons/Group 17.png", UriKind.Relative));
                textBlock.Text = "Заполнить приписное св-во";
                textBlock.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5367D3"));
            }
        }
        private void UpdateINNIcon()
        {
            var border = INNBorder;
            var image = INNIconImg;
            var textBlock = INNTbl;

            if (SelectedStudentHelper.selectedStudent.INNID != null)
            {
                // Меняем на серую иконку, если ИНН есть
                border.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6C6C6C"));
                image.Source = new BitmapImage(new Uri("/Resource/Image/check.png", UriKind.Relative));
                textBlock.Text = "ИНН заполнен";
                textBlock.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6C6C6C"));
            }
            else
            {
                // Или оставляем стандартную, если нет
                border.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5367D3"));
                image.Source = new BitmapImage(new Uri("/Resource/Icons/Group 17.png", UriKind.Relative));
                textBlock.Text = "Заполнить ИНН";
                textBlock.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5367D3"));
            }
        }
        private void UpdateInsuranceIcon()
        {
            var border = InsuranceBorder;
            var image = InsuranceIconImg;
            var textBlock = InsuranceTbl;

            if (SelectedStudentHelper.selectedStudent.InsuranceNumberID != null)
            {
                // Меняем на серую иконку, если СНИЛС есть
                border.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6C6C6C"));
                image.Source = new BitmapImage(new Uri("/Resource/Image/check.png", UriKind.Relative));
                textBlock.Text = "СНИЛС заполнен";
                textBlock.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6C6C6C"));
            }
            else
            {
                // Или оставляем стандартную, если нет
                border.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5367D3"));
                image.Source = new BitmapImage(new Uri("/Resource/Icons/Group 17.png", UriKind.Relative));
                textBlock.Text = "Заполнить СНИЛС";
                textBlock.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5367D3"));
            }
        }
        private void PassportClick(object sender, MouseButtonEventArgs e)
        {

            PassportWindow passportWindow = new PassportWindow();
            if (passportWindow.ShowDialog() == true)
            {
                UpdatePassportIcon();
            }

        }
        private void MilitaryCertificateClick(object sender, MouseButtonEventArgs e)
        {
            MilitaryCertificateWindow militaryCertificateWindow = new MilitaryCertificateWindow();
            if (militaryCertificateWindow.ShowDialog() == true)
            {
                UpdateMilitaryCertificateIcon();
            }
        }
        private void INNClick(object sender, MouseButtonEventArgs e)
        {
            INNWindow INNWindow = new INNWindow();
            if (INNWindow.ShowDialog() == true)
            {
                UpdateINNIcon();
            }
        }
        private void InsuranceClick(object sender, MouseButtonEventArgs e)
        {
            InsuranceWindow insuranceWindow = new InsuranceWindow();
            if (insuranceWindow.ShowDialog() == true)
            {
                UpdateInsuranceIcon();
            }
        }
        private void BackBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.NavigationService.Content = null;
        }

    }
}
