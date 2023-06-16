using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using WorkshopDataModifier.Data;

namespace WorkshopDataModifier.MVVM.View
{
    /// <summary>
    /// Interaction logic for UsersView.xaml
    /// </summary>
    public partial class UsersView : UserControl
    {

        #region Counter of the current clients (dynamic)
        private int _rowCount;
        public int RowCount
        {
            get { return _rowCount; }
            set
            {
                _rowCount = value;
                OnPropertyChanged(nameof(RowCount));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Multi Select
        private void SelectAllCheckBox_Click(object sender, RoutedEventArgs e)
        {
            bool isChecked = ((CheckBox)sender).IsChecked == true;

            foreach (users row in UsersDataGrid.Items)
            {
                row.IsSelected = isChecked;
            }
        }

        private void RowCheckBox_Click(object sender, RoutedEventArgs e)
        {
            DataGridRow row = (DataGridRow)UsersDataGrid.ItemContainerGenerator.ContainerFromItem(((FrameworkElement)sender).DataContext);
            if (row != null)
            {
                users rowData = (users)row.Item;
                rowData.IsSelected = ((CheckBox)sender).IsChecked == true;
            }
        }
        #endregion

        #region Data Modification

        //List of all selected rows (Initialized with button click)
        static List<users> selectedRows = new List<users>();

        #region Edit Section
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (users rowData in UsersDataGrid.Items)
            {
                if (rowData.IsSelected)
                {
                    selectedRows.Add(rowData);
                }
            }

            Button editButton = (Button)sender;
            users row = (users)editButton.DataContext;

            if (selectedRows.Count == 0)
                selectedRows.Add(row);

            if (!selectedRows.Contains(row))
            {
                selectedRows.Clear();
                return;
            }

            if (selectedRows.Count == 0)
            {
                throw new Exception("Row was not selected or something went wrong");
            }
            else if (selectedRows.Count == 1)
            {
                MultiEditionWarning.Visibility = Visibility.Collapsed;

                EditUsername.Text = row.Username;
                EditPassword.Text = row.Password;
                EditLevel.Text = row.AccessLevel.ToString();
                EditOwner.Text = row.Owner.ToString();

                EditPopup.IsOpen = true;
            }
            else
            {
                EditPopup.DataContext = selectedRows;
                MultiEditionWarning.Visibility = Visibility.Visible;

                EditPopup.IsOpen = true;
            }

        }

        private void ConfirmEditButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var context = new UsersDbContext())
                {
                    if (selectedRows.Count == 0)
                    {
                        throw new Exception("Row was not selected or something went wrong");
                    }
                    else if (selectedRows.Count == 1)
                    {
                        users selectedRow = context.User.Find(selectedRows[0].ID);

                        byte txtLevel = byte.Parse(EditLevel.Text);
                        long txtOwner = long.Parse(EditOwner.Text);
                        
                        selectedRow.Username = EditUsername.Text;
                        selectedRow.Password = EditPassword.Text;
                        selectedRow.AccessLevel = txtLevel;
                        selectedRow.Owner = txtOwner;
                    }
                    else
                    {
                        foreach (users dataRow in selectedRows)
                        {
                            users selectedRow = context.User.Find(dataRow.ID);

                            if (EditUsername.Text != "" && EditUsername.Text != null)
                                selectedRow.Username = EditUsername.Text;

                            if (EditPassword.Text != "" && EditPassword.Text != null)
                                selectedRow.Password = EditPassword.Text;

                            if (EditLevel.Text != "" && EditLevel.Text != null && byte.TryParse(EditLevel.Text, out byte txtLevel))
                                selectedRow.AccessLevel = txtLevel;

                            if (EditOwner.Text != "" && EditOwner.Text != null && long.TryParse(EditOwner.Text, out long txtOwner))
                                selectedRow.Owner = txtOwner;
                        }
                    }

                    context.SaveChanges();
                    UsersDataGrid.ItemsSource = context.User.ToList();
                }
                EditPopup.IsOpen = false;

                EditUsername.Text = "";
                EditPassword.Text = "";
                EditLevel.Text = "";
                EditOwner.Text = "";

                selectedRows.Clear();
            }
            catch (DbEntityValidationException ex)
            {
                // Handle validation errors
                StringBuilder errorMessage = new StringBuilder();
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        errorMessage.AppendLine($"Error: {validationError.ErrorMessage}");
                    }
                }

                MessageBox.Show(errorMessage.ToString(), "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                // Handle other database errors
                MessageBox.Show($"An error occurred while saving changes: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelEditButton_Click(object sender, RoutedEventArgs e)
        {
            EditUsername.Text = "";
            EditPassword.Text = "";
            EditLevel.Text = "";
            EditOwner.Text = "";

            selectedRows.Clear();

            EditPopup.IsOpen = false;
        }
        #endregion

        #region Delete Section
        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (users rowData in UsersDataGrid.Items)
            {
                if (rowData.IsSelected)
                {
                    selectedRows.Add(rowData);
                }
            }

            Button removeButton = (Button)sender;
            users row = (users)removeButton.DataContext;

            if (selectedRows.Count == 0)
                selectedRows.Add(row);

            if (!selectedRows.Contains(row))
            {
                selectedRows.Clear();
                return;
            }

            if (selectedRows.Count == 0)
            {
                throw new Exception("Row was not selected or something went wrong");
            }
            else if (selectedRows.Count == 1)
            {
                DeletePopup.IsOpen = true;
            }
            else
            {
                DeletePopup.DataContext = selectedRows;
                DeletePopup.IsOpen = true;
            }
        }

        private void ConfirmRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var context = new UsersDbContext())
                {
                    if (selectedRows.Count == 0)
                    {
                        throw new Exception("Row was not selected or something went wrong");
                    }
                    else if (selectedRows.Count == 1)
                    {
                        users selectedRow = context.User.Find(selectedRows[0].ID);
                        context.User.Remove(selectedRow);
                    }
                    else
                    {
                        foreach (users dataRow in selectedRows)
                        {
                            users selectedRow = context.User.Find(dataRow.ID);
                            context.Entry(selectedRow).State = EntityState.Deleted;
                        }
                    }

                    //Update DataGrid to show changes
                    context.SaveChanges();
                    UsersDataGrid.ItemsSource = context.User.ToList();
                }

                RowCount = UsersDataGrid.Items.Count;
                UsersCounter.Text = $"Current Saved Users: {RowCount}";

                DeletePopup.IsOpen = false;
                selectedRows.Clear();
            }
            catch (DbUpdateException ex)
            {
                // Handle specific database exceptions
                if (ex.InnerException is SqlException sqlException)
                {
                    // Handle referential integrity constraint violation
                    if (sqlException.Number == 547)
                    {
                        MessageBox.Show("Cannot delete the item because it is referenced by other entities.", "Delete Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    }
                    else
                    {
                        MessageBox.Show($"An error occurred while deleting the item: {sqlException.Message}", "Delete Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    }
                }
                else
                {
                    MessageBox.Show($"An error occurred while deleting the item: {ex.Message}", "Delete Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                }
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                MessageBox.Show($"An error occurred while deleting the item: {ex.Message}", "Delete Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
            }
        }

        private void CancelRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            selectedRows.Clear();
            DeletePopup.IsOpen = false;
        }
        #endregion

        #region Add Section

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            AddPopup.IsOpen = true;
        }

        private void ConfirmAddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var context = new UsersDbContext())
                {
                    byte txtLevel = byte.Parse(AddLevel.Text);
                    long txtOwner = long.Parse(AddOwner.Text);

                    users newUser = new users
                    {
                        Username = AddUsername.Text,
                        Password = AddPassword.Text,
                        AccessLevel = txtLevel,
                        Owner = txtOwner
                    };

                    context.User.Add(newUser);
                    context.SaveChanges();

                    UsersDataGrid.ItemsSource = context.User.ToList();
                }

                RowCount = UsersDataGrid.Items.Count;
                UsersCounter.Text = $"Current Saved Users: {RowCount}";
                AddPopup.IsOpen = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while adding the client: {ex.Message}", "Add Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
            }
        }

        private void CancelAddButton_Click(object sender, RoutedEventArgs e)
        {
            AddUsername.Text = "";
            AddPassword.Text = "";
            AddLevel.Text = "";
            AddOwner.Text = "";

            AddPopup.IsOpen = false;
        }

        #endregion

        #endregion 

        #region Draggable Popups

        private bool isDraggingPopup = false;
        private Point startPoint;

        #region Add Popup
        private void AddPopup_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                isDraggingPopup = true;
                startPoint = e.GetPosition(AddPopup);
            }
        }

        private void AddPopup_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDraggingPopup)
            {
                Point currentPoint = e.GetPosition(AddPopup);
                double offsetX = currentPoint.X - startPoint.X;
                double offsetY = currentPoint.Y - startPoint.Y;

                AddPopup.HorizontalOffset += offsetX;
                AddPopup.VerticalOffset += offsetY;

                startPoint = currentPoint;
            }
        }

        private void AddPopup_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                isDraggingPopup = false;
            }
        }
        #endregion

        #region Edit Popup
        private void EditPopup_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                isDraggingPopup = true;
                startPoint = e.GetPosition(EditPopup);
            }
        }

        private void EditPopup_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDraggingPopup)
            {
                Point currentPoint = e.GetPosition(EditPopup);
                double offsetX = currentPoint.X - startPoint.X;
                double offsetY = currentPoint.Y - startPoint.Y;

                EditPopup.HorizontalOffset += offsetX;
                EditPopup.VerticalOffset += offsetY;

                startPoint = currentPoint;
            }
        }

        private void EditPopup_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                isDraggingPopup = false;
            }
        }
        #endregion

        #region Delete Popup
        private void DeletePopup_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                isDraggingPopup = true;
                startPoint = e.GetPosition(DeletePopup);
            }
        }

        private void DeletePopup_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDraggingPopup)
            {
                Point currentPoint = e.GetPosition(DeletePopup);
                double offsetX = currentPoint.X - startPoint.X;
                double offsetY = currentPoint.Y - startPoint.Y;

                DeletePopup.HorizontalOffset += offsetX;
                DeletePopup.VerticalOffset += offsetY;

                startPoint = currentPoint;
            }
        }

        private void DeletePopup_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                isDraggingPopup = false;
            }
        }
        #endregion

        #endregion

        #region Search

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtSearchUsers.Text != "Search in Users...")
            {
                string searchText = txtSearchUsers.Text.ToLower();

                UsersDataGrid.Items.Filter = item =>
                {
                    if (item is users dataItem)
                    {
                        string txtID = dataItem.ID.ToString();
                        string txtLevel = dataItem.AccessLevel.ToString();
                        string txtOwner = dataItem.Owner.ToString();
                        string txtAddTime = dataItem.AddTime.ToString();

                        return txtID.Contains(searchText) ||
                               dataItem.Username.ToLower().Contains(searchText) ||
                               dataItem.Password.ToLower().Contains(searchText) ||
                               txtLevel.Contains(searchText) ||
                               txtOwner.Contains(searchText) ||
                               txtAddTime.Contains(searchText);
                    }

                    return false;
                };

                UsersDataGrid.Items.Refresh();
            }
        }

        #endregion

        #region Focus Settings

        private void UsersWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (txtSearchUsers.Text == "")
            {
                Keyboard.ClearFocus();
                txtSearchUsers.Text = "Search in Users...";
            }
            else
            {
                Keyboard.ClearFocus();
            }

        }

        private void txtSearchUsers_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (txtSearchUsers.Text == "Search in Users...")
            {
                txtSearchUsers.Text = "";
            }
        }
        #endregion

        #region Date Filtering

        private void DataFilterButton_Checked(object sender, RoutedEventArgs e)
        {
            if (DayButton.IsChecked == true)
            {

            }
            else if (WeekButton.IsChecked == true)
            {

            }
            else if (MonthButton.IsChecked == true)
            {

            }
            else if (YearButton.IsChecked == true)
            {

            }
            else if (InfButton.IsChecked == true)
            {

            }

        }
        #endregion

        private UsersDbContext _dbContext;
        public UsersView()
        {
            InitializeComponent();

            //Database initializer
            _dbContext = new UsersDbContext();
            UsersDataGrid.ItemsSource = _dbContext.User.ToList();

            //Counter initializer
            RowCount = UsersDataGrid.Items.Count;
            UsersCounter.Text = $"Current Saved Users: {RowCount}";
        }
    }



    /// <summary>
    /// Gets context of users tab from the connected DataBase
    /// </summary>
    public class UsersDbContext : DbContext
    {
        public DbSet<users> User { get; set; } //DbSet dla tabeli "customer"

        public UsersDbContext() : base("DealershipCon")
        {
        }
    }
}
