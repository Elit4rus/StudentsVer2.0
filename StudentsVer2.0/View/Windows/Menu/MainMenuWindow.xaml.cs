using ClosedXML.Excel;
using Microsoft.Win32;
using StudentsVer2._0.AppData;
using StudentsVer2._0.Model;
using StudentsVer2._0.View.Windows.Login;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace StudentsVer2._0.View.Windows.Menu
{
    /// <summary>
    /// Логика взаимодействия для MainMenuWindow.xaml
    /// </summary>
    public partial class MainMenuWindow : System.Windows.Window
    {
        // Создаем список пользователей
        List<Student> students = App.context.Student.ToList();
        List<Group> groups = App.context.Group.ToList();
        Student studentInLv = new Student();
        Group selectedGroup = new Group();
        public MainMenuWindow()
        {
            InitializeComponent();

            FrameHelper.mainFrame = MainFrame;

            // Авторизация пользователя
            RoleNameTbl.Text = AuthorizationHelper.currentUser.Role.Title;
            AccountNameTbl.Text = AuthorizationHelper.currentUser.Surname + " " + AuthorizationHelper.currentUser.Name;

            // Передаем данные в ComboBox
            GroupCmb.SelectedValuePath = "ID";
            GroupCmb.DisplayMemberPath = "Title";

            // Загружаем группы, связанные с текущим пользователем
            LoadGroups(AuthorizationHelper.currentUser.ID);

            if (AuthorizationHelper.currentUser.RoleID != 1)
            {
                ImportBtn.Visibility = Visibility.Collapsed;
            }
            if (AuthorizationHelper.currentUser.RoleID != 1)
            {
                AddStudentBtn.Visibility = Visibility.Collapsed;
            }
            if (AuthorizationHelper.currentUser.RoleID != 1)
            {
                DeleteStudentBtn.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadGroups(int userID)
        {
            // Загружаем группы, связанные с текущим пользователем, через таблицу UserGroup
            var userGroup = (from ug in App.context.UserGroup
                             join g in App.context.Group on ug.GroupID equals g.ID
                             where ug.UserID == userID
                             select g).ToList();
            GroupCmb.ItemsSource = userGroup;
        }

        private void StudentsLv_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Получаем выбранного студента
            SelectedStudentHelper.selectedStudent = StudentsLv.SelectedItem as Student;

            if (SelectedStudentHelper.selectedStudent != null)
            {
                // Загружаем название группы из текущего контекста
                string groupTitle = App.context.Group.FirstOrDefault(g => g.ID == SelectedStudentHelper.selectedStudent.GroupID)?.Title ?? "Группа не найдена";

                // Открываем окно с личным делом студента
                MainFrame.Navigate(new View.Pages.Menu.StudentDetailPage(SelectedStudentHelper.selectedStudent, groupTitle));
            }
        }

        private void GroupCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedGroup = GroupCmb.SelectedItem as Group;
            if (selectedGroup != null)
            {
                // Загружаем студентов для выбранной группы
                LoadStudents(selectedGroup.ID, AuthorizationHelper.currentUser.ID);
            }
        }
        private void LoadStudents(int groupID, int userRoleID)
        {
            List<Student> students;
            if (userRoleID == 1) // Администратор
            {
                // Администратор видит всех студентов в выбранной группе
                students = (from student in App.context.Student
                            where student.GroupID == groupID
                            select student).ToList();
            }
            else if (userRoleID == 2) // Преподаватель
            {
                // Преподаватель видит студентов, связанных с его группами
                students = (from student in App.context.Student
                            join userGroup in App.context.UserGroup on student.GroupID equals userGroup.GroupID
                            where userGroup.UserID == AuthorizationHelper.currentUser.RoleID && student.GroupID == groupID
                            select student).ToList();
            }
            else
            {
                students = new List<Student>();
            }
            // Загружаем данные в ListView
            StudentsLv.ItemsSource = students;
        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            // Создаем и показываем кастомное окно
            LogoutWindow logoutWindow = new LogoutWindow();

            logoutWindow.Left = 1650;
            logoutWindow.Top = 70;

            bool? result = logoutWindow.ShowDialog();

            // Если пользователь нажал "Выйти"
            if (result == true)
            {
                // Логика выхода из аккаунта
                PerformLogout();
            }
        }
        private void PerformLogout()
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }

        private void ImportBtn_Click(object sender, RoutedEventArgs e)
        {
            ChoiseCuratorWindow choiseCuratorWindow = new ChoiseCuratorWindow();
            choiseCuratorWindow.ShowDialog();

            var dialog = new OpenFileDialog { Filter = "Excel Files|*.xls;*.xlsx;*.xlsm" };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    using (var workbook = new XLWorkbook(dialog.FileName))
                    {
                        var worksheet = workbook.Worksheet(1);
                        var rows = worksheet.RowsUsed().Skip(1); // Пропускаем заголовок

                        int added = 0, skipped = 0;
                        var transaction = App.context.Database.BeginTransaction();

                        foreach (var row in rows)
                        {
                            try
                            {
                                var groupTitle = row.Cell(5).GetString().Trim();
                                if (string.IsNullOrEmpty(groupTitle))
                                {
                                    skipped++;
                                    continue;
                                }

                                // Получаем или создаем группу
                                var group = App.context.Group.FirstOrDefault(g => g.Title == groupTitle)
                                    ?? new Group { Title = groupTitle };

                                if (group.ID == 0)
                                {
                                    App.context.Group.Add(group);
                                    App.context.SaveChanges(); // Сохраняем для получения ID
                                }

                                // Проверяем существование студента
                                var surname = row.Cell(2).GetString().Trim();
                                var name = row.Cell(3).GetString().Trim();
                                var patronymic = row.Cell(4).GetString().Trim();

                                if (App.context.Student.Any(s =>
                                    s.Surname == surname &&
                                    s.Name == name &&
                                    s.Patronymic == patronymic &&
                                    s.GroupID == group.ID))
                                {
                                    skipped++;
                                    continue;
                                }

                                // Парсим дату рождения
                                if (!DateTime.TryParse(row.Cell(6).GetString(), out var birthDate))
                                {
                                    skipped++;
                                    continue;
                                }

                                // Определяем пол
                                var genderInput = row.Cell(7).GetString().Trim().ToLower();
                                var gender = App.context.Gender.FirstOrDefault(g => g.Title == genderInput)
                                    ?? App.context.Gender.First(); // По умолчанию

                                // Создаем студента
                                var student = new Student
                                {
                                    Surname = surname,
                                    Name = name,
                                    Patronymic = patronymic,
                                    BirthDay = birthDate,
                                    GenderID = gender.ID,
                                    GroupID = group.ID
                                };

                                App.context.Student.Add(student);
                                added++;
                            }
                            catch { skipped++; }
                        }

                        try
                        {
                            App.context.SaveChanges();
                            transaction.Commit();
                            MessageBox.Show($"Импорт завершен!\nДобавлено: {added}\nПропущено: {skipped}");
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка импорта: {ex.Message}");
                }
            }

            LoadGroups(AuthorizationHelper.currentUser.ID);
        }
        private string GetCellValue(IXLRow row, int column)
        {
            return row.Cell(column).GetString().Trim();
        }
        private DateTime? ParseDate(string dateString)
        {
            if (DateTime.TryParse(dateString, out DateTime result))
                return result;
            return null;
        }
        private int ParseGender(StudentEntities context, string gender)
        {
            if (string.IsNullOrWhiteSpace(gender))
                return 1;

            Gender genderEntity = context.Gender
                .FirstOrDefault(g => g.Title == gender.Trim().ToLower());

            return (int)(genderEntity?.ID); // Вернет ID или null, если пол не найден
        }
        private Group GetOrCreateGroup(StudentEntities context, string groupTitle)
        {
            var group = context.Group.FirstOrDefault(g => g.Title == groupTitle);

            if (group == null)
            {
                group = new Group { Title = groupTitle };
                context.Group.Add(group);
            }

            return group;
        }

        private bool IsStudentExists(StudentEntities context, string surname, string name, string patronymic, int groupId)
        {
            return context.Student.Any(s =>
                s.Surname.Equals(surname) &&
                s.Name.Equals(name) &&
                (s.Patronymic == patronymic || (s.Patronymic == null && patronymic == "")) &&
                s.GroupID == groupId);
        }

        private void AddStudentBtn_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddStudentWindow();
            if (addWindow.ShowDialog() == true)
            {
                // Обновляем список студентов текущей группы
                LoadStudents(selectedGroup.ID, AuthorizationHelper.currentUser.RoleID);
                MessageBox.Show("Студент успешно добавлен!");
            }
        }

        private void DeleteStudentBtn_Click(object sender, RoutedEventArgs e)
        {
            var selectedStudents = StudentsLv.Items.Cast<Student>()
        .Where(s => s.IsSelected == true)
        .ToList();

            if (selectedStudents.Count == 0)
            {
                MessageBox.Show("Выберите студентов для удаления!");
                return;
            }

            try
            {
                // Получаем ID для удаления
                var idsToDelete = selectedStudents.Select(s => s.ID).ToList();

                // Находим студентов в основном контексте
                var studentsToDelete = App.context.Student
                    .Where(s => idsToDelete.Contains(s.ID))
                    .ToList();

                // Удаление
                App.context.Student.RemoveRange(studentsToDelete);
                App.context.SaveChanges();

                // Обновление интерфейса
                LoadStudents(selectedGroup.ID, AuthorizationHelper.currentUser.RoleID);
                MessageBox.Show($"Удалено {studentsToDelete.Count} студентов");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}");
            }
        }

        private void HeaderCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var student in StudentsLv.Items.OfType<Student>())
            {
                student.IsSelected = true;
            }
        }

        private void HeaderCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (var student in StudentsLv.Items.OfType<Student>())
            {
                student.IsSelected = false;
            }
        }
    }
}
