using StudentsVer2._0.AppData;
using StudentsVer2._0.Model;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StudentsVer2._0.View.Windows.Documents
{
    /// <summary>
    /// Логика взаимодействия для InsuranceWindow.xaml
    /// </summary>
    public partial class InsuranceWindow : Window
    {
        public static InsuranceNumber newInsurance;
        public InsuranceWindow()
        {
            InitializeComponent();

            // Проверяем, есть ли у студента СНИЛС
            if (SelectedStudentHelper.selectedStudent.InsuranceNumberID != null)
            {
                // Загружаем существующий СНИЛС
                newInsurance = App.context.InsuranceNumber.Find(SelectedStudentHelper.selectedStudent.InsuranceNumberID);
                if (newInsurance != null)
                {
                    // Заполняем поля данными
                    NumberTb.Text = newInsurance.Number;
                    DateRegistrationTb.Text = newInsurance.DateRegistration.ToString("dd.MM.yyyy") ?? "";
                }
            }
            else
            {
                // Создаем новый объект для будущего сохранения
                newInsurance = new InsuranceNumber();
            }
        }
        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверка, все ли поля пустые
                if (string.IsNullOrEmpty(NumberTb.Text) && string.IsNullOrEmpty(DateRegistrationTb.Text))
                {
                    // Если все поля пустые и СНИЛС существовал, удаляем его
                    if (SelectedStudentHelper.selectedStudent.InsuranceNumberID != null)
                    {
                        var InsuranceToDelete = App.context.InsuranceNumber.Find(SelectedStudentHelper.selectedStudent.InsuranceNumberID);
                        if (InsuranceToDelete != null)
                        {
                            DialogResult = true;
                            App.context.InsuranceNumber.Remove(InsuranceToDelete);
                            SelectedStudentHelper.selectedStudent.InsuranceNumberID = null;
                            App.context.SaveChanges();
                            MessageBox.Show("СНИЛС удален!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Нет данных для сохранения!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    return;
                }

                // Если СНИЛС уже существует, обновляем его
                if (SelectedStudentHelper.selectedStudent.InsuranceNumberID != null)
                {
                    var existingInsurance = App.context.InsuranceNumber.Find(SelectedStudentHelper.selectedStudent.InsuranceNumberID);
                    if (existingInsurance != null)
                    {
                        existingInsurance.Number = NumberTb.Text;
                        existingInsurance.DateRegistration = (DateTime)ParseDate(DateRegistrationTb.Text);

                        App.context.SaveChanges();
                        MessageBox.Show("Данные СНИЛС обновлены!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                        DialogResult = true;
                    }
                }
                else
                {
                    //Если СНИЛС не было, добавляем новый
                    newInsurance = new InsuranceNumber()
                    {
                        Number = NumberTb.Text,
                        DateRegistration = (DateTime)ParseDate(DateRegistrationTb.Text)
                    };

                    App.context.InsuranceNumber.Add(newInsurance);
                    App.context.SaveChanges();

                    SelectedStudentHelper.selectedStudent.InsuranceNumberID = newInsurance.ID;
                    App.context.SaveChanges();

                    MessageBox.Show("Данные СНИЛС добавлены!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void NumberTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;

            string text = textBox.Text;

            // Удаляем все нецифровые символы
            string digitsOnly = new string(text.Where(char.IsDigit).ToArray());

            // Форматируем текст
            if (digitsOnly.Length > 9)
            {
                textBox.Text = $"{digitsOnly.Substring(0, 3)}-{digitsOnly.Substring(3, 3)}-{digitsOnly.Substring(6, 3)} {digitsOnly.Substring(9)}";
            }
            else if (digitsOnly.Length > 6)
            {
                textBox.Text = $"{digitsOnly.Substring(0, 3)}-{digitsOnly.Substring(3, 3)}-{digitsOnly.Substring(6)}";
            }
            else if (digitsOnly.Length > 3)
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
        private void NumberTb_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;

            // Разрешаем ввод только цифр
            if (!char.IsDigit(e.Text, e.Text.Length - 1))
            {
                e.Handled = true; // Отменяем ввод
            }

            // Проверяем, чтобы длина не превышала 14 символов (например, "123-456-789 10")
            if (textBox.Text.Length >= 14)
            {
                e.Handled = true; // Отменяем ввод
            }
        }
        private void DateRegistrationTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;

            string text = textBox.Text;

            // Удаляем все нецифровые символы
            string digitsOnly = new string(text.Where(char.IsDigit).ToArray());

            // Форматируем текст
            if (digitsOnly.Length > 4)
            {
                string day = digitsOnly.Substring(0, 2);
                string month = digitsOnly.Substring(2, 2);
                string year = digitsOnly.Substring(4);

                // Проверка корректности дня (1-31) и месяца (1-12)
                if (int.Parse(day) > 31 || int.Parse(month) > 12)
                {
                    textBox.Text = $"{digitsOnly.Substring(0, 2)}.{digitsOnly.Substring(2, 2)}.{digitsOnly.Substring(4)}";
                }
                else
                {
                    textBox.Text = $"{day}.{month}.{year}";
                }
            }
            else if (digitsOnly.Length > 2)
            {
                textBox.Text = $"{digitsOnly.Substring(0, 2)}.{digitsOnly.Substring(2)}";
            }
            else
            {
                textBox.Text = digitsOnly;
            }

            // Перемещаем курсор в конец текста
            textBox.CaretIndex = textBox.Text.Length;
        }
        private void DateRegistrationTb_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;

            // Разрешаем ввод только цифр
            if (!char.IsDigit(e.Text, e.Text.Length - 1))
            {
                e.Handled = true; // Отменяем ввод
            }

            // Проверяем, чтобы длина не превышала 10 символов (например, "23.03.2024")
            if (textBox.Text.Length >= 10)
            {
                e.Handled = true; // Отменяем ввод
            }
        }
        // Метод для безопасного преобразования даты
        private DateTime? ParseDate(string dateString)
        {
            if (DateTime.TryParse(dateString, out DateTime result))
                return result;
            return null;
        }
        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
