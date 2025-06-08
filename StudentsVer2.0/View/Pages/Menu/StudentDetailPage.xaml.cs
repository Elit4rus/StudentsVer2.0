using StudentsVer2._0.AppData;
using StudentsVer2._0.Model;
using StudentsVer2._0.View.Windows.Documents;
using System;
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
        public StudentDetailPage(Student student, string groupTitle)
        {
            InitializeComponent();

            PassportBorder.MouseLeftButtonDown += PassportClick;
            MilitaryCertificateBorder.MouseLeftButtonDown += MilitaryCertificateClick;
            INNBorder.MouseLeftButtonDown += INNClick;

            SurnameTbl.Text = student.Surname;
            NameTbl.Text = student.Name;
            PatronymicTbl.Text = student.Patronymic ?? "Нет данных"; // Если отчество отсутствует
            GroupTbl.Text = groupTitle;

            if (SelectedStudentHelper.selectedStudent.PassportID != null) UpdatePassportIcon();
            if (SelectedStudentHelper.selectedStudent.MilitaryCertificateID != null) UpdateMilitaryCertificateIcon();
            if (SelectedStudentHelper.selectedStudent.INNID != null) UpdateINNIcon();

            if (SelectedStudentHelper.selectedStudent.GenderID == 2)
            {
                MilitaryCertificateBorder.Visibility = Visibility.Collapsed;
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
                // Меняем на серую иконку, если паспорт есть
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
                // Меняем на серую иконку, если паспорт есть
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

        private void PassportClick(object sender, MouseButtonEventArgs e)
        {

            PassportWindow passportWindow = new PassportWindow();
            if (passportWindow.ShowDialog() == true)
            {
                // Обновляем иконку после закрытия окна, если данные были сохранены
                UpdatePassportIcon();
            }

        }

        private void MilitaryCertificateClick(object sender, MouseButtonEventArgs e)
        {
            // Здесь откройте окно для заполнения данных военного билета
            MilitaryCertificateWindow militaryCertificateWindow = new MilitaryCertificateWindow();
            if (militaryCertificateWindow.ShowDialog() == true)
            {
                // Обновляем иконку после закрытия окна, если данные были сохранены
                UpdateMilitaryCertificateIcon();
            }
        }

        private void INNClick(object sender, MouseButtonEventArgs e)
        {
            // Здесь откройте окно для заполнения данных ИНН
            INNWindow INNWindow = new INNWindow();
            if (INNWindow.ShowDialog() == true)
            {
                // Обновляем иконку после закрытия окна, если данные были сохранены
                UpdateINNIcon();
            }
        }

        private void BackBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.NavigationService.Content = null;
        }

    }
}
