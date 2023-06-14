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
using WorkshopDataModifier.Data;

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


        private void ExitApp(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            DragMove();
        }

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

                    // Open Main Window
                    MainWindow mainWindow = new MainWindow(initials, fullName);
                    mainWindow.Show();

                    // Close login tab
                    Close();
                }
                else
                {
                    MessageBox.Show("Invalid username or password.");
                }
            }

        }

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
