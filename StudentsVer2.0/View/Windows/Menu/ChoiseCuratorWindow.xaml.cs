using StudentsVer2._0.AppData;
using StudentsVer2._0.Model;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace StudentsVer2._0.View.Windows.Menu
{
    /// <summary>
    /// Логика взаимодействия для ChoiseCuratorWindow.xaml
    /// </summary>
    public partial class ChoiseCuratorWindow : Window
    {
        List<User> curators = App.context.User.ToList();
        public ChoiseCuratorWindow()
        {
            InitializeComponent();
            CuratorCmb.SelectedValuePath = "ID";
            CuratorCmb.ItemsSource = curators;
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ChoiseBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CuratorCmb.SelectedValue == null)
            {
                MessageBox.Show("Выберите куратора группы!", "Ошибка");
            }
            else
            {
                CuratorHelper.selectedCurator = CuratorCmb.SelectedItem as User;
                DialogResult = true;
                Close();
            }
        }
    }
}
