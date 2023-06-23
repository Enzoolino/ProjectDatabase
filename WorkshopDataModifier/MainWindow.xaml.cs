using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using WorkshopDataModifier.MVVM.View;
using WorkshopDataModifier.MVVM.View.Login;
using WorkshopDataModifier.Core;

namespace WorkshopDataModifier
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _userInitials;
        private string _userFullName;
        
        public MainWindow(string initials, string fullname, byte userAccessLevel)
        {
            InitializeComponent();

            //Initialize the user
            _userInitials = initials;
            _userFullName = fullname;

            // Set up the UI with the user's initials
            Username_Initials.Text = _userInitials;
            Username_Full.Text = _userFullName;

            // Adjust the visibility of MainWindow elements based on the access level
            AdjustByAccessLevel(userAccessLevel);

            //Save the access level
            UserAccessManager.SetAccessLevel(userAccessLevel);
        }

        //Set visibility of MainWindow based on AcessLevel
        public void AdjustByAccessLevel(byte userAccessLevel)
        {
            if (userAccessLevel > 2)
            {
                AdminSection.Visibility = Visibility.Visible;
                
            }
            else
            {
                AdminSection.Visibility = Visibility.Collapsed;
            }
        }

        //Make the Window movable by border
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        //Logout and Open the login window
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            //Open Login Window
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();

            // Close Main window
            Close();
        }
    }
}
