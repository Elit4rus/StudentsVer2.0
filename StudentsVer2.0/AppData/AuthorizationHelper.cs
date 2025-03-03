using StudentsVer2._0.Model;
using StudentsVer2._0.View.Windows.Login;
using StudentsVer2._0.View.Windows.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace StudentsVer2._0.AppData
{
    internal class AuthorizationHelper
    {
        public static User currentUser;
        public static bool CheckData(string login, string password)
        {
            currentUser = App.context.User.FirstOrDefault(cu => cu.Login == login && cu.Password == password);
            if (currentUser != null) 
            { 
                if (currentUser.Login == login && currentUser.Password == password) 
                {
                    return true;
                }
            }
            return false;
        }
    }
}
