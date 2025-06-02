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
                AddStudentBtn.Visibility = Visibility.Collapsed;
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
            else
            {
                // Преподаватель видит студентов, связанных с его группами
                students = App.context.Student.Where(s => s.GroupID == groupID && App.context.UserGroup.Any(ug =>
                   ug.GroupID == groupID &&
                   ug.UserID == AuthorizationHelper.currentUser.ID)).ToList();
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

            var choiseWindow = new ChoiseCuratorWindow();
            if (choiseWindow.ShowDialog() != true) return;

            var openFileDialog = new OpenFileDialog { Filter = "Excel Files|*.xls;*.xlsx;*.xlsm" };
            if (openFileDialog.ShowDialog() != true) return;

            try
            {
                using (var workbook = new XLWorkbook(openFileDialog.FileName))
                using (var context = new StudentEntities())
                {
                    var worksheet = workbook.Worksheet(1);
                    var rows = worksheet.RowsUsed().Skip(1); // Пропускаем заголовок

                    int addedStudents = 0;
                    int currentGroupId = 0;
                    Group currentGroup = null;

                    foreach (var row in rows)
                    {
                        var groupTitle = row.Cell(5).GetString().Trim();
                        if (string.IsNullOrEmpty(groupTitle)) continue;

                        // Создание/получение группы
                        currentGroup = context.Group.FirstOrDefault(g => g.Title == groupTitle)
                            ?? new Group { Title = groupTitle };

                        if (currentGroup.ID == 0)
                        {
                            context.Group.Add(currentGroup);
                            context.SaveChanges();
                        }
                        currentGroupId = currentGroup.ID;

                        // Парсинг данных студента
                        var surname = row.Cell(2).GetString().Trim();
                        var name = row.Cell(3).GetString().Trim();
                        var patronymic = row.Cell(4).GetString().Trim();
                        var birthDate = row.Cell(6).GetDateTime();
                        var gender = row.Cell(7).GetString().Trim().ToLower();

                        // Проверка дубликатов
                        if (context.Student.Any(s =>
                            s.Surname == surname &&
                            s.Name == name &&
                            s.Patronymic == patronymic &&
                            s.GroupID == currentGroupId))
                        {
                            continue;
                        }

                        // Создание студента
                        var student = new Student
                        {
                            Surname = surname,
                            Name = name,
                            Patronymic = string.IsNullOrEmpty(patronymic) ? null : patronymic,
                            BirthDay = birthDate,
                            GenderID = gender == "ж" ? 2 : 1,
                            GroupID = currentGroupId
                        };

                        context.Student.Add(student);
                        addedStudents++;
                    }

                    context.SaveChanges();

                    // Обработка связей UserGroup
                    var selectedCurator = CuratorHelper.selectedCurator;
                    if (!context.UserGroup.Any(ug =>
                        ug.GroupID == currentGroupId && ug.UserID == selectedCurator.ID))
                    {
                        context.UserGroup.Add(new UserGroup
                        {
                            GroupID = currentGroupId,
                            UserID = selectedCurator.ID
                        });

                        // Добавление администратора если куратор не админ
                        if (selectedCurator.RoleID != 1 &&
                            !context.UserGroup.Any(ug =>
                                ug.GroupID == currentGroupId && ug.UserID == 1))
                        {
                            context.UserGroup.Add(new UserGroup
                            {
                                GroupID = currentGroupId,
                                UserID = 1
                            });
                        }

                        context.SaveChanges();
                    }

                    MessageBox.Show($"Импорт завершен! Добавлено студентов: {addedStudents}");
                    LoadGroups(AuthorizationHelper.currentUser.ID);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка импорта: {ex.Message}");
            }
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
