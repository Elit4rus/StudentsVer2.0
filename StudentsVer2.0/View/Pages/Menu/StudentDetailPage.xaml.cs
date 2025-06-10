using StudentsVer2._0.AppData;
using StudentsVer2._0.Model;
using StudentsVer2._0.View.Windows.Documents;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace StudentsVer2._0.View.Pages.Menu
{
    /// <summary>
    /// Логика взаимодействия для StudentDetailPage.xaml
    /// </summary>
    public partial class StudentDetailPage : Page
    {
        private Student currentStudent;
        private List<ImageDocument> studentImages;
        public StudentDetailPage(Student student, string groupTitle)
        {
            InitializeComponent();
            currentStudent = student;

            // Загрузка изображений при инициализации
            LoadStudentImages();

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
        }

        private void LoadStudentImages()
        {
            studentImages = App.context.StudentImage
            .Where(si => si.StudentID == currentStudent.ID)
            .Select(si => si.ImageDocument)
            .ToList();
            ImagesPanel.Children.Clear();

            foreach (var image in studentImages)
            {
                AddImageToPanel(image);
            }
        }

        private void AddImageToPanel(ImageDocument image)
        {
            // Создание элемента изображения
            var imageControl = new Image
            {
                Source = LoadImage(image.ImageDoc),
                Width = 100,
                Height = 100,
                Margin = new Thickness(5),
                Cursor = Cursors.Hand,
                Tag = image.ID
            };

            imageControl.MouseLeftButtonDown += (s, e) =>
            {
                var window = new ImageWindow(image.ImageDoc, image.ID);
                if (window.ShowDialog() == true)
                {
                    LoadStudentImages(); // Обновляем список после удаления
                }
            };

            ImagesPanel.Children.Add(imageControl);
        }

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

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Проверка, что для студента уже есть не более 10 изображений (если нужно)
                    if (studentImages.Count >= 10)
                    {
                        MessageBox.Show("Достигнут лимит изображений (10 шт.)");
                        return;
                    }

                    byte[] imageData = File.ReadAllBytes(openFileDialog.FileName);
                    App.context.ImageDocument.Add(new ImageDocument { ImageDoc = imageData });
                    App.context.SaveChanges();

                    var newStudentImage = new StudentImage
                    {
                        StudentID = currentStudent.ID,
                        ImageID = App.context.ImageDocument.Max(i => i.ID)
                    };
                    App.context.StudentImage.Add(newStudentImage);
                    App.context.SaveChanges();

                    LoadStudentImages(); // Обновляем панель
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}");
                }
            }
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
