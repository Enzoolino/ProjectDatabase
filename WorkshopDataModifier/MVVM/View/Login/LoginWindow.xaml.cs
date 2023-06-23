using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WorkshopDataModifier.MVVM.Model;

namespace WorkshopDataModifier.MVVM.View.Login
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private UsersDbContext _dbContext;

        public LoginWindow()
        {
            InitializeComponent();

            //Database initializer
            _dbContext = new UsersDbContext();
        }

        //Exit the application
        private void ExitApp(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        //Make the window movable
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            DragMove();
        }

        //Login into the application
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Password;

            using (var context = new UsersDbContext())
            {
                var user = context.users.FirstOrDefault(u => u.Username == username && u.Password == password);
                
                if (user != null)
                {
                    // Get the employee associated with the user
                    var employee = user.employee;

                    //Get initials and fullname of user
                    string initials = GetInitials(employee.Name, employee.Surname);
                    string fullName = employee.Name + " " + employee.Surname;

                    // Retrieve the user's access level
                    byte userAccessLevel = user.AccessLevel;

                    // Initialize MainWindow and pass the access level as a parameter
                    MainWindow mainWindow = new MainWindow(initials, fullName, userAccessLevel);

                    //Open MainWindow
                    mainWindow.Show();

                    // Close login tab
                    Close();
                }
                else
                {
                    MessageBox.Show("Invalid username or password.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        //Transfer name and surname into initials
        private string GetInitials(string name, string surname)
        {
            string initials = "";
            if (!string.IsNullOrWhiteSpace(name))
            {
                initials += name[0];
            }
            if (!string.IsNullOrWhiteSpace(surname))
            {
                initials += surname[0];
            }
            return initials.ToUpper();
        }
    }

    /// <summary>
    /// Gets context of users tab from the connected DataBase
    /// </summary>
    public class UsersDbContext : DbContext
    {
        public DbSet<users> users { get; set; } //DbSet dla tabeli "klienci"

        public UsersDbContext() : base("DealershipCon")
        {
        }
    }
}
