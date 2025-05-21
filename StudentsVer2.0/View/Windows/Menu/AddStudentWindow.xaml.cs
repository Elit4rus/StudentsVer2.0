using StudentsVer2._0.Model;
using System;
using System.Linq;
using System.Windows;

namespace StudentsVer2._0.View.Windows.Menu
{
    /// <summary>
    /// Логика взаимодействия для AddStudentWindow.xaml
    /// </summary>
    public partial class AddStudentWindow : Window
    {
        public Student newStudent { get; private set; }
        public AddStudentWindow()
        {
            InitializeComponent();
            LoadGroupsAndGender();
        }

        private void LoadGroupsAndGender()
        {
            GroupCmb.SelectedValuePath = "ID";
            GroupCmb.DisplayMemberPath = "Title";

            GenderCmb.SelectedValuePath = "ID";
            GenderCmb.DisplayMemberPath = "Title";

            GroupCmb.ItemsSource = App.context.Group.ToList();
            GenderCmb.ItemsSource = App.context.Gender.ToList();
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void AddStudentBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                try
                {
                    // Создаем нового студента с привязкой ID вместо навигационных свойств
                    newStudent = new Student
                    {
                        Surname = SurnameTb.Text.Trim(),
                        Name = NameTb.Text.Trim(),
                        Patronymic = string.IsNullOrWhiteSpace(PatronymicTb.Text) ? null : PatronymicTb.Text.Trim(),
                        GroupID = ((Group)GroupCmb.SelectedItem).ID,
                        BirthDay = BirthdayDp.SelectedDate ?? DateTime.Today,
                        GenderID = ((Gender)GenderCmb.SelectedItem).ID
                    };

                    // Добавляем в контекст и сохраняем
                    App.context.Student.Add(newStudent);
                    App.context.SaveChanges();

                    DialogResult = true;
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении: {ex.Message}");
                }
            }
        }
        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(SurnameTb.Text) ||
                string.IsNullOrWhiteSpace(NameTb.Text) ||
                GroupCmb.SelectedItem == null)
            {
                MessageBox.Show("Заполните обязательные поля (Фамилия, Имя, Группа)!");
                return false;
            }
            return true;
        }


    }
}
