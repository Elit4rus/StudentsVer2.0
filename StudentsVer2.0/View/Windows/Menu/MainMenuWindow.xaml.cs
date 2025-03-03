using StudentsVer2._0.AppData;
using StudentsVer2._0.Model;
using StudentsVer2._0.View.Windows.Login;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace StudentsVer2._0.View.Windows.Menu
{
    /// <summary>
    /// Логика взаимодействия для MainMenuWindow.xaml
    /// </summary>
    public partial class MainMenuWindow : Window
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
            var selectedStudent = StudentsLv.SelectedItem as Student;

            if (selectedStudent != null)
            {
                // Загружаем название группы из текущего контекста
                string groupTitle = App.context.Group.FirstOrDefault(g => g.ID == selectedStudent.GroupID)?.Title ?? "Группа не найдена";

                // Открываем окно с личным делом студента
                //StudentDetailsWindow studentDetailsWindow = new StudentDetailsWindow(selectedStudent, groupTitle);
                //studentDetailsWindow.Show();

                MainFrame.Navigate(new View.Pages.Menu.StudentDetailPage(selectedStudent, groupTitle));
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
    }
}
