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
            Group selectedGroup = GroupCmb.SelectedItem as Group;
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

            UserGroup newUserGroup = new UserGroup();

            if (choiseCuratorWindow.DialogResult == true)
            {
                // Открытие диалогового окна для выбора Excel-файла
                OpenFileDialog dialog = new OpenFileDialog
                {
                    Filter = "Excel Files|*.xls;*.xlsx;*.xlsm"
                };

                if (dialog.ShowDialog() == true)
                {
                    try
                    {
                        using (var workbook = new XLWorkbook(dialog.FileName))
                        using (var context = new StudentEntities()) // Один контекст для всей операции
                        {
                            var worksheet = workbook.Worksheet(1);
                            var rows = worksheet.RowsUsed().Skip(1);

                            foreach (var row in rows)
                            {
                                try
                                {
                                    // 1. Обработка группы
                                    var groupTitle = GetCellValue(row, 5); // Колонка E
                                    if (string.IsNullOrWhiteSpace(groupTitle))
                                    {
                                        MessageBox.Show($"Пропуск строки {row.RowNumber()}: отсутствует группа");
                                        continue;
                                    }

                                    var group = GetOrCreateGroup(context, groupTitle);

                                    // 2. Сохранение группы ПЕРЕД добавлением студента
                                    if (context.Entry(group).State == EntityState.Added)
                                    {
                                        context.SaveChanges(); // Явное сохранение новой группы
                                    }

                                    // 3. Создание студента
                                    var student = new Student
                                    {
                                        Surname = GetCellValue(row, 2),
                                        Name = GetCellValue(row, 3),
                                        Patronymic = GetCellValue(row, 4),
                                        BirthDay = ParseDate(GetCellValue(row, 6)),
                                        Gender = ParseGender(GetCellValue(row, 7)),
                                        GroupID = group.ID // Связь через ID
                                    };

                                    newUserGroup.UserID = CuratorHelper.selectedCurator.ID;
                                    newUserGroup.GroupID = student.GroupID;
                                    context.Student.Add(student);
                                    context.UserGroup.Add(newUserGroup);
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show($"Ошибка в строке {row.RowNumber()}: {ex.Message}");
                                }
                            }

                            // Фиксация всех изменений
                            context.SaveChanges();
                            MessageBox.Show("Импорт завершён успешно!");

                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Общая ошибка: {ex.Message}");
                    }
                }

            }


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

        private string ParseGender(string gender)
        {
            if (string.IsNullOrEmpty(gender))
                return null;

            switch (gender.ToLower())
            {
                case "м":
                    return "м";
                case "ж":
                    return "ж";
                default:
                    return null;
            }
        }

        private Group GetOrCreateGroup(StudentEntities context, string groupTitle)
        {
            var group = context.Group
                .FirstOrDefault(g => g.Title == groupTitle);

            if (group == null)
            {
                group = new Group { Title = groupTitle };
                context.Group.Add(group);
            }

            return group;
        }

        private void AddStudentBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DeleteStudentBtn_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
