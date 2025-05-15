using StudentsVer2._0.AppData;
using StudentsVer2._0.Model;
using System;
using System.Windows;

namespace StudentsVer2._0.View.Windows.Documents
{
    /// <summary>
    /// Логика взаимодействия для PassportWindow.xaml
    /// </summary>
    public partial class PassportWindow : Window
    {
        public static Passport newPassport;
        public PassportWindow()
        {
            InitializeComponent();
            // Проверка на наличие заполнения паспорта (сделать метод)
            if (SelectedStudentHelper.selectedStudent.PassportID != null)
            {
                PassportIssuedTb.Text = SelectedStudentHelper.selectedStudent.Passport.PassportIssued;
                DateOfIssueTb.Text = string.Format("{0:dd.MM.yyyy}", SelectedStudentHelper.selectedStudent.Passport.DateOfIssue);
                DepartmentCodeTb.Text = SelectedStudentHelper.selectedStudent.Passport.DepartmentCode;
                SeriesAndNumberTb.Text = SelectedStudentHelper.selectedStudent.Passport.SeriesAndNumber;
                RegistrationDateTb.Text = string.Format("{0:dd.MM.yyyy}", SelectedStudentHelper.selectedStudent.Passport.RegistrationDate);
                PlaceOfResidenceTb.Text = SelectedStudentHelper.selectedStudent.Passport.PlaceOfResidence;
            }
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Добавление паспорта
                newPassport = new Passport()
                {
                    PassportIssued = PassportIssuedTb.Text,
                    DateOfIssue = Convert.ToDateTime(DateOfIssueTb.Text),
                    DepartmentCode = DepartmentCodeTb.Text,
                    SeriesAndNumber = SeriesAndNumberTb.Text,
                    RegistrationDate = Convert.ToDateTime(RegistrationDateTb.Text),
                    PlaceOfResidence = PlaceOfResidenceTb.Text
                };
                // Добавление паспорта конкретному студенту
                SelectedStudentHelper.selectedStudent.PassportID = newPassport.ID;
                App.context.Passport.Add(newPassport);
                App.context.SaveChanges();
                MessageBox.Show("Данные добавлены!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch
            {
                MessageBox.Show("Данные введены некорректно!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
