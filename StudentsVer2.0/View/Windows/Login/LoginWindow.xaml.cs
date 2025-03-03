using StudentsVer2._0.AppData;
using StudentsVer2._0.View.Windows.Menu;
using System.Windows;

namespace StudentsVer2._0.View.Windows.Login
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            if (AuthorizationHelper.CheckData(LoginTb.Text, PasswordTb.Text) == true)
            {
                MainMenuWindow mainMenuWindow = new MainMenuWindow();
                mainMenuWindow.Show();
                Close();
            }
            else
            {
                MessageBox.Show("Данные неверны");
            }
        }

    }
}
