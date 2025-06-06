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
    /// Логика взаимодействия для MilitaryCertificateWindow.xaml
    /// </summary>
    public partial class MilitaryCertificateWindow : Window
    {
        public static MilitaryCertificate newMilitaryCertificate;
        public MilitaryCertificateWindow()
        {
            InitializeComponent();

            // Проверяем, есть ли у студента паспорт
            if (SelectedStudentHelper.selectedStudent.MilitaryCertificateID != null)
            {
                // Загружаем существующий паспорт
                newMilitaryCertificate = App.context.MilitaryCertificate.Find(SelectedStudentHelper.selectedStudent.MilitaryCertificateID);
                if (newMilitaryCertificate != null)
                {
                    // Заполняем поля данными
                    SeriesAndNumberTb.Text = newMilitaryCertificate.SeriesAndNumber;
                    DateRegistrationTb.Text = newMilitaryCertificate.DateRegistration.ToString("dd.MM.yyyy") ?? "";
                }
            }
            else
            {
                // Создаем новый объект для будущего сохранения
                newMilitaryCertificate = new MilitaryCertificate();
            }
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверка, все ли поля пустые
                if (string.IsNullOrEmpty(SeriesAndNumberTb.Text) && string.IsNullOrEmpty(DateRegistrationTb.Text))
                {
                    // Если все поля пустые и приписное св-во существовало, удаляем его
                    if (SelectedStudentHelper.selectedStudent.MilitaryCertificateID != null)
                    {
                        var militaryCertificaterToDelete = App.context.MilitaryCertificate.Find(SelectedStudentHelper.selectedStudent.MilitaryCertificateID);
                        if (militaryCertificaterToDelete != null)
                        {
                            App.context.MilitaryCertificate.Remove(militaryCertificaterToDelete);
                            SelectedStudentHelper.selectedStudent.MilitaryCertificateID = null;
                            App.context.SaveChanges();
                            MessageBox.Show("Приписное св-во удалено!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                            DialogResult = true;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Нет данных для сохранения!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    return;
                }

                // Если паспорт уже существует, обновляем его
                if (SelectedStudentHelper.selectedStudent.MilitaryCertificateID != null)
                {
                    var existingMilitaryCertificate = App.context.MilitaryCertificate.Find(SelectedStudentHelper.selectedStudent.MilitaryCertificateID);
                    if (existingMilitaryCertificate != null)
                    {
                        existingMilitaryCertificate.SeriesAndNumber = SeriesAndNumberTb.Text;
                        existingMilitaryCertificate.DateRegistration = (DateTime)ParseDate(DateRegistrationTb.Text);

                        App.context.SaveChanges();
                        MessageBox.Show("Данные приписного св-ва обновлены!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                        DialogResult = true;
                    }
                }
                else
                {
                    //Если паспорта не было, добавляем новый
                    newMilitaryCertificate = new MilitaryCertificate()
                    {
                        SeriesAndNumber = SeriesAndNumberTb.Text,
                        DateRegistration = (DateTime)ParseDate(DateRegistrationTb.Text)
                    };

                    App.context.MilitaryCertificate.Add(newMilitaryCertificate);
                    App.context.SaveChanges();

                    SelectedStudentHelper.selectedStudent.MilitaryCertificateID = newMilitaryCertificate.ID;
                    App.context.SaveChanges();

                    MessageBox.Show("Данные приписного св-во добавлены!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
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
    }
}
