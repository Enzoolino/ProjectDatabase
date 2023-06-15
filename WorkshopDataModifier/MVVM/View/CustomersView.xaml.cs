using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Data.Entity;
using System.Globalization;
using System.ComponentModel;
using System.Windows.Input;
using WorkshopDataModifier.Core;
using WorkshopDataModifier.Data;
using System.Data.Entity.Validation;
using System.Text;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Windows.Controls.Primitives;
using System.Collections.Specialized;

namespace WorkshopDataModifier.MVVM.View
{
    /// <summary>
    /// Interaction logic for ClientsView.xaml
    /// </summary>
    public partial class CustomersView : UserControl, INotifyPropertyChanged
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

            foreach (customer row in CustomersDataGrid.Items)
            {
                row.IsSelected = isChecked;
            }
        }

        private void RowCheckBox_Click(object sender, RoutedEventArgs e)
        {
            DataGridRow row = (DataGridRow)CustomersDataGrid.ItemContainerGenerator.ContainerFromItem(((FrameworkElement)sender).DataContext);
            if (row != null)
            {
                customer rowData = (customer)row.Item;
                rowData.IsSelected = ((CheckBox)sender).IsChecked == true;
            }
        }
        #endregion

        #region Data Modification

        //List of all selected rows (Initialized with button click)
        static List<customer> selectedRows = new List<customer>(); 
        
        #region Edit Section
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (customer rowData in CustomersDataGrid.Items)
            {
                if (rowData.IsSelected)
                {
                    selectedRows.Add(rowData);
                }
            }

            Button editButton = (Button)sender;
            customer row = (customer)editButton.DataContext;

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

                EditSin.Text = row.Sin.ToString();
                EditVin.Text = row.Vin.ToString();
                EditName.Text = row.Name;
                EditSurname.Text = row.Surname;
                EditPhone.Text = row.Phone;
                
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
                using (var context = new CustomersDbContext())
                {
                    if (selectedRows.Count == 0)
                    {
                        throw new Exception("Row was not selected or something went wrong");
                    }
                    else if (selectedRows.Count == 1)
                    {
                        customer selectedRow = context.customer.Find(selectedRows[0].Sin, selectedRows[0].Vin);

                        long txtSin = long.Parse(EditSin.Text);
                        int txtVin = int.Parse(EditVin.Text);

                        selectedRow.Sin = txtSin;
                        selectedRow.Vin = txtVin;
                        selectedRow.Name = EditName.Text;
                        selectedRow.Surname = EditSurname.Text;
                        selectedRow.Phone = EditPhone.Text;
                    }
                    else
                    {
                        foreach (customer dataRow in selectedRows)
                        {
                            customer selectedRow = context.customer.Find(dataRow.Sin, dataRow.Vin);

                            long txtSin;
                            int txtVin;

                            if (EditSin.Text != "" && EditSin.Text != null && long.TryParse(EditSin.Text, out txtSin))
                                selectedRow.Sin = txtSin;

                            if (EditVin.Text != "" && EditVin.Text != null && int.TryParse(EditSin.Text, out txtVin))
                                selectedRow.Vin = txtVin;

                            if (EditName.Text != "" && EditName.Text != null)
                                selectedRow.Name = EditName.Text;

                            if (EditSurname.Text != "" && EditSurname.Text != null)
                                selectedRow.Surname = EditSurname.Text;

                            if (EditPhone.Text != "" && EditPhone.Text != null)
                                selectedRow.Phone = EditPhone.Text;
                        }
                    }

                    context.SaveChanges(); 
                    CustomersDataGrid.ItemsSource = context.customer.ToList();
                }

                EditPopup.IsOpen = false;

                EditSin.Text = "";
                EditVin.Text = "";
                EditName.Text = "";
                EditSurname.Text = "";
                EditPhone.Text = "";
                
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
            EditVin.Text = "";
            EditSin.Text = "";
            EditName.Text = "";
            EditSurname.Text = "";
            EditPhone.Text = "";

            selectedRows.Clear();

            EditPopup.IsOpen = false;
        }
        #endregion

        #region Delete Section
        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (customer rowData in CustomersDataGrid.Items)
            {
                if (rowData.IsSelected)
                {
                    selectedRows.Add(rowData);
                }
            }

            Button removeButton = (Button)sender;
            customer row = (customer)removeButton.DataContext;

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
                using (var context = new CustomersDbContext())
                {
                    if (selectedRows.Count == 0)
                    {
                        throw new Exception("Row was not selected or something went wrong");
                    }
                    else if (selectedRows.Count == 1)
                    {
                        customer selectedRow = context.customer.Find(selectedRows[0].Sin, selectedRows[0].Vin);
                        context.customer.Remove(selectedRow);
                    }
                    else
                    {
                        foreach (customer dataRow in selectedRows)
                        {
                            customer selectedRow = context.customer.Find(dataRow.Sin, dataRow.Vin);
                            context.Entry(selectedRow).State = EntityState.Deleted;
                        }
                    }

                    //Update DataGrid to show changes
                    context.SaveChanges();
                    CustomersDataGrid.ItemsSource = context.customer.ToList();
                }

                RowCount = CustomersDataGrid.Items.Count;
                CustomersCounter.Text = $"Current Saved Clients: {RowCount}";

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
                using (var context = new CustomersDbContext())
                {
                    long txtSin = long.Parse(AddSin.Text);
                    int txtVin = int.Parse(AddVin.Text);

                    customer newCustomer = new customer
                    {
                        Sin = txtSin,
                        Vin = txtVin,
                        Name = AddName.Text,
                        Surname = AddSurname.Text,
                        Phone = AddPhone.Text
                    };

                    context.customer.Add(newCustomer);
                    context.SaveChanges();

                    CustomersDataGrid.ItemsSource = context.customer.ToList();
                }

                RowCount = CustomersDataGrid.Items.Count;
                CustomersCounter.Text = $"Current Saved Clients: {RowCount}";
                AddPopup.IsOpen = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while adding the client: {ex.Message}", "Add Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
            }
        }

        private void CancelAddButton_Click(object sender, RoutedEventArgs e)
        {
            AddSin.Text = "";
            AddVin.Text = "";
            AddName.Text = "";
            AddSurname.Text = "";
            AddPhone.Text = "";
           
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
            if (txtSearchCustomers.Text != "Search in Customers...")
            {

                string searchText = txtSearchCustomers.Text.ToLower();


                CustomersDataGrid.Items.Filter = item =>
                {
                    if (item is customer dataItem)
                    {
                        string txtSin = dataItem.Sin.ToString();
                        string txtVin = dataItem.Vin.ToString();
                        string timeStamp = dataItem.AddTime.ToString();

                        return txtSin.Contains(searchText) ||
                               txtVin.Contains(searchText) ||
                               dataItem.Name.ToLower().Contains(searchText) ||
                               dataItem.Surname.ToLower().Contains(searchText) ||
                               dataItem.Phone.Contains(searchText) ||
                               timeStamp.Contains(searchText);
                               
                    }

                    return false;
                };

                CustomersDataGrid.Items.Refresh();
            }
        }

        #endregion

        #region Focus Settings

        private void CustomersWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (txtSearchCustomers.Text == "")
            {
                Keyboard.ClearFocus();
                txtSearchCustomers.Text = "Search in Customers...";
            }
            else
            {
                Keyboard.ClearFocus();
            }

        }

        private void txtSearchCustomers_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (txtSearchCustomers.Text == "Search in Customers...")
            {
                txtSearchCustomers.Text = "";
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

        private CustomersDbContext _dbContext;

        public CustomersView()
        {
            InitializeComponent();

            //Database initializer
            _dbContext = new CustomersDbContext();
            CustomersDataGrid.ItemsSource = _dbContext.customer.ToList();

            //Counter initializer
            RowCount = CustomersDataGrid.Items.Count;
            CustomersCounter.Text = $"Current Saved Clients: {RowCount}";

        }
    }

    /// <summary>
    /// Gets context of clients tab from the connected DataBase
    /// </summary>
    public class CustomersDbContext : DbContext
    {
        public DbSet<customer> customer { get; set; } //DbSet dla tabeli "customer"

        public CustomersDbContext() : base("DealershipCon")
        {
        }
    }

 
}
