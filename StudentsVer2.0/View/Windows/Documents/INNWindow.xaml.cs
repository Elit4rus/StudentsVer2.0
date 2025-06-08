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
    /// Логика взаимодействия для INNWindow.xaml
    /// </summary>
    public partial class INNWindow : Window
    {
        public static INN newINN;
        public INNWindow()
        {
            InitializeComponent();

            // Проверяем, есть ли у студента ИНН
            if (SelectedStudentHelper.selectedStudent.INNID != null)
            {
                // Загружаем существующий паспорт
                newINN = App.context.INN.Find(SelectedStudentHelper.selectedStudent.INNID);
                if (newINN != null)
                {
                    // Заполняем поля данными
                    NumberTb.Text = newINN.Number;
                    DateRegistrationTb.Text = newINN.DateIssue.ToString("dd.MM.yyyy") ?? "";
                }
            }
            else
            {
                // Создаем новый объект для будущего сохранения
                newINN = new INN();
            }
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверка, все ли поля пустые
                if (string.IsNullOrEmpty(NumberTb.Text) && string.IsNullOrEmpty(DateRegistrationTb.Text))
                {
                    // Если все поля пустые и ИНН существовал, удаляем его
                    if (SelectedStudentHelper.selectedStudent.INNID != null)
                    {
                        var INNToDelete = App.context.INN.Find(SelectedStudentHelper.selectedStudent.INNID);
                        if (INNToDelete != null)
                        {
                            App.context.INN.Remove(INNToDelete);
                            SelectedStudentHelper.selectedStudent.INNID = null;
                            App.context.SaveChanges();
                            MessageBox.Show("ИНН удален!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                            DialogResult = true;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Нет данных для сохранения!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    return;
                }

                // Если ИНН уже существует, обновляем его
                if (SelectedStudentHelper.selectedStudent.INNID != null)
                {
                    var existingINN = App.context.INN.Find(SelectedStudentHelper.selectedStudent.INNID);
                    if (existingINN != null)
                    {
                        existingINN.Number = NumberTb.Text;
                        existingINN.DateIssue = (DateTime)ParseDate(DateRegistrationTb.Text);

                        App.context.SaveChanges();
                        MessageBox.Show("Данные ИНН обновлены!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                        DialogResult = true;
                    }
                }
                else
                {
                    //Если ИНН не было, добавляем новый
                    newINN = new INN()
                    {
                        Number = NumberTb.Text,
                        DateIssue = (DateTime)ParseDate(DateRegistrationTb.Text)
                    };

                    App.context.INN.Add(newINN);
                    App.context.SaveChanges();

                    SelectedStudentHelper.selectedStudent.INNID = newINN.ID;
                    App.context.SaveChanges();

                    MessageBox.Show("Данные ИНН добавлены!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
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

            // Удаляем все нецифровые символы (на случай, если пользователь вставил текст)
            string digitsOnly = new string(text.Where(char.IsDigit).ToArray());

            // Ограничиваем длину 12 цифрами
            if (digitsOnly.Length > 12)
            {
                textBox.Text = digitsOnly.Substring(0, 12);
                textBox.CaretIndex = textBox.Text.Length; // Перемещаем курсор в конец
            }
            else
            {
                textBox.Text = digitsOnly;
            }
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
