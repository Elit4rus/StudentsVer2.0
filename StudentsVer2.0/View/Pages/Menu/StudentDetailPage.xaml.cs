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

            PassportBorder.MouseLeftButtonDown += OnDesignElementClicked;

            SurnameTbl.Text = student.Surname;
            NameTbl.Text = student.Name;
            PatronymicTbl.Text = student.Patronymic ?? "Нет данных"; // Если отчество отсутствует
            GroupTbl.Text = groupTitle;

            UpdatePassportIcon();

            if (SelectedStudentHelper.selectedStudent.GenderID == 2)
            {
                MilitaryCertificateBorder.Visibility = Visibility.Collapsed;
            }
        }

        private void UpdatePassportIcon()
        {
            if (SelectedStudentHelper.selectedStudent.PassportID != null)
            {
                // Меняем на серую иконку, если паспорт есть
                PassportIconImg.Source = new BitmapImage(new Uri("/Resource/Image/check.png", UriKind.Relative));
                PassportTextTbl.Text = "Паспорт заполнен";
                PassportTextTbl.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6C6C6C"));
            }
            else
            {
                // Или оставляем стандартную, если нет
                PassportIconImg.Source = new BitmapImage(new Uri("/Resource/Icons/Group 17.png", UriKind.Relative));
                PassportTextTbl.Text = "Заполнить паспорт";
            }
        }

        private void OnDesignElementClicked(object sender, MouseButtonEventArgs e)
        {

            PassportWindow passportWindow = new PassportWindow();
            if (passportWindow.ShowDialog() == true)
            {
                // Обновляем иконку после закрытия окна, если данные были сохранены
                UpdatePassportIcon();
            }

        }

        private void BackBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.NavigationService.Content = null;
        }

    }
}
