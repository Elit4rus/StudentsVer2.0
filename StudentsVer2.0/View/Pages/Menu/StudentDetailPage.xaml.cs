using StudentsVer2._0.Model;
using System.Windows.Controls;

namespace StudentsVer2._0.View.Pages.Menu
{
    /// <summary>
    /// Логика взаимодействия для StudentDetailPage.xaml
    /// </summary>
    public partial class StudentDetailPage : Page
    {
        public StudentDetailPage(Student student, string groupTitle)
        {
            InitializeComponent();
            SurnameTbl.Text = student.Surname;
            NameTbl.Text = student.Name;
            PatronymicTbl.Text = student.Patronymic ?? "Нет данных"; // Если отчество отсутствует

            GroupTbl.Text = groupTitle;
        }

        private void BackBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.NavigationService.Content = null;
        }
    }
}
