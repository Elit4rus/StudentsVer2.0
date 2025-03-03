using System.Windows;

namespace StudentsVer2._0.View.Windows.Menu
{
    /// <summary>
    /// Логика взаимодействия для LogoutWindow.xaml
    /// </summary>
    public partial class LogoutWindow : Window
    {
        public LogoutWindow()
        {
            InitializeComponent();

        }

        private void LogoutBtn_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
