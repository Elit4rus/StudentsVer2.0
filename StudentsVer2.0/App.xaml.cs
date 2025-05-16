using StudentsVer2._0.Model;
using System.Windows;

namespace StudentsVer2._0
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static StudentEntities context = new StudentEntities();
    }
}
