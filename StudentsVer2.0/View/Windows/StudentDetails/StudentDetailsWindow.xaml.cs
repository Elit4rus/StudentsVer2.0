using StudentsVer2._0.Model;
using System.Windows;

namespace StudentsVer2._0.View.Windows.StudentDetails
{
    /// <summary>
    /// Логика взаимодействия для StudentDetailsWindow.xaml
    /// </summary>
    public partial class StudentDetailsWindow : Window
    {
        public StudentDetailsWindow(Student student, string groupTitle)
        {
            InitializeComponent();
            SurnameTbl.Text = student.Surname;
            NameTbl.Text = student.Name;
            PatronymicTbl.Text = student.Patronymic ?? "Нет данных"; // Если отчество отсутствует

            GroupTbl.Text = groupTitle;
        }
    }
}
