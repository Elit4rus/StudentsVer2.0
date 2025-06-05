using StudentsVer2._0.AppData;
using StudentsVer2._0.Model;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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

            // Проверяем, есть ли у студента паспорт
            if (SelectedStudentHelper.selectedStudent.PassportID != null)
            {
                // Загружаем существующий паспорт
                newPassport = App.context.Passport.Find(SelectedStudentHelper.selectedStudent.PassportID);
                if (newPassport != null)
                {
                    // Заполняем поля данными
                    PassportIssuedTb.Text = newPassport.PassportIssued;
                    DateOfIssueTb.Text = newPassport.DateOfIssue?.ToString("dd.MM.yyyy") ?? "";
                    DepartmentCodeTb.Text = newPassport.DepartmentCode;
                    SeriesAndNumberTb.Text = newPassport.SeriesAndNumber;
                    RegistrationDateTb.Text = newPassport.RegistrationDate?.ToString("dd.MM.yyyy") ?? "";
                    PlaceOfResidenceTb.Text = newPassport.PlaceOfResidence;
                }
            }
            else
            {
                // Создаем новый объект для будущего сохранения
                newPassport = new Passport();
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
                // Проверка, все ли поля пустые
                if (string.IsNullOrWhiteSpace(PassportIssuedTb.Text) &&
                    string.IsNullOrWhiteSpace(DateOfIssueTb.Text) &&
                    string.IsNullOrWhiteSpace(DepartmentCodeTb.Text) &&
                    string.IsNullOrWhiteSpace(SeriesAndNumberTb.Text) &&
                    string.IsNullOrWhiteSpace(RegistrationDateTb.Text) &&
                    string.IsNullOrWhiteSpace(PlaceOfResidenceTb.Text))
                {
                    // Если все поля пустые и паспорт существовал, удаляем его
                    if (SelectedStudentHelper.selectedStudent.PassportID != null)
                    {
                        var passportToDelete = App.context.Passport.Find(SelectedStudentHelper.selectedStudent.PassportID);
                        if (passportToDelete != null)
                        {
                            App.context.Passport.Remove(passportToDelete);
                            SelectedStudentHelper.selectedStudent.PassportID = null;
                            App.context.SaveChanges();
                            MessageBox.Show("Паспорт удален!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Нет данных для сохранения!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    return;
                }

                // Если паспорт уже существует, обновляем его
                if (SelectedStudentHelper.selectedStudent.PassportID != null)
                {
                    var existingPassport = App.context.Passport.Find(SelectedStudentHelper.selectedStudent.PassportID);
                    if (existingPassport != null)
                    {
                        existingPassport.PassportIssued = PassportIssuedTb.Text;
                        existingPassport.DateOfIssue = ParseDate(DateOfIssueTb.Text);
                        existingPassport.DepartmentCode = DepartmentCodeTb.Text;
                        existingPassport.SeriesAndNumber = SeriesAndNumberTb.Text;
                        existingPassport.RegistrationDate = ParseDate(RegistrationDateTb.Text);
                        existingPassport.PlaceOfResidence = PlaceOfResidenceTb.Text;

                        App.context.SaveChanges();
                        MessageBox.Show("Данные паспорта обновлены!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    // Если паспорта не было, добавляем новый
                    newPassport = new Passport()
                    {
                        PassportIssued = PassportIssuedTb.Text,
                        DateOfIssue = ParseDate(DateOfIssueTb.Text),
                        DepartmentCode = DepartmentCodeTb.Text,
                        SeriesAndNumber = SeriesAndNumberTb.Text,
                        RegistrationDate = ParseDate(RegistrationDateTb.Text),
                        PlaceOfResidence = PlaceOfResidenceTb.Text
                    };

                    App.context.Passport.Add(newPassport);
                    App.context.SaveChanges();

                    SelectedStudentHelper.selectedStudent.PassportID = newPassport.ID;
                    App.context.SaveChanges();

                    MessageBox.Show("Данные паспорта добавлены!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // Метод для безопасного преобразования даты
        private DateTime? ParseDate(string dateString)
        {
            if (DateTime.TryParse(dateString, out DateTime result))
                return result;
            return null;
        }

        private void SeriesAndNumberTb_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;

            string text = textBox.Text;

            // Удаляем все нецифровые символы
            string digitsOnly = new string(text.Where(char.IsDigit).ToArray());

            // Форматируем текст
            if (digitsOnly.Length > 4)
            {
                textBox.Text = $"{digitsOnly.Substring(0, 2)} {digitsOnly.Substring(2, 2)} {digitsOnly.Substring(4)}";
            }
            else if (digitsOnly.Length > 2)
            {
                textBox.Text = $"{digitsOnly.Substring(0, 2)} {digitsOnly.Substring(2)}";
            }
            else
            {
                textBox.Text = digitsOnly;
            }

            // Перемещаем курсор в конец текста
            textBox.CaretIndex = textBox.Text.Length;
        }

        private void DepartmentCodeTb_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;

            string text = textBox.Text;

            // Удаляем все нецифровые символы
            string digitsOnly = new string(text.Where(char.IsDigit).ToArray());

            // Добавляем дефис после третьего символа
            if (digitsOnly.Length > 3)
            {
                textBox.Text = $"{digitsOnly.Substring(0, 3)}-{digitsOnly.Substring(3)}";
            }
            else
            {
                textBox.Text = digitsOnly;
            }

            // Перемещаем курсор в конец текста
            textBox.CaretIndex = textBox.Text.Length;
        }
    }
}
