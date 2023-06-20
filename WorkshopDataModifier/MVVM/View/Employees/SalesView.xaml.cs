using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
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
using WorkshopDataModifier.MVVM.Model;

namespace WorkshopDataModifier.MVVM.View
{
    /// <summary>
    /// Interaction logic for SalesView.xaml
    /// </summary>
    public partial class SalesView : UserControl
    {

        #region Counter of the current clients (dynamic)
        private int _rowCount;
        /// <summary>
        /// Counts number of items inside "sales" table
        /// </summary>
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

            foreach (sell row in SalesDataGrid.Items)
            {
                row.IsSelected = isChecked;
            }
        }

        private void RowCheckBox_Click(object sender, RoutedEventArgs e)
        {
            DataGridRow row = (DataGridRow)SalesDataGrid.ItemContainerGenerator.ContainerFromItem(((FrameworkElement)sender).DataContext);
            if (row != null)
            {
                sell rowData = (sell)row.Item;
                rowData.IsSelected = ((CheckBox)sender).IsChecked == true;
            }
        }
        #endregion

        #region Data Modification

        //List of all selected rows (Initialized with button click)
        static List<sell> selectedRows = new List<sell>();

        #region Edit Section

        //Button Click Handler - Sets up the rows for editing
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (sell rowData in SalesDataGrid.Items)
            {
                if (rowData.IsSelected)
                {
                    selectedRows.Add(rowData);
                }
            }

            Button editButton = (Button)sender;
            sell row = (sell)editButton.DataContext;

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
                EditEmployee.Text = row.EmpID.ToString();
                
                EditPopup.IsOpen = true;
            }
            else
            {
                EditSin.IsHitTestVisible = false;
                EditSin.Foreground = Brushes.Gray;
                EditSin.Text = "Can't Multi Edit !";

                EditPopup.DataContext = selectedRows;
                MultiEditionWarning.Visibility = Visibility.Visible;

                EditPopup.IsOpen = true;
            }

            //Enable Scrimming and Disable Controls if Popup is open
            if (EditPopup.IsOpen == true)
            {
                DisableControls();

                MainContentWindow.Opacity = 0.5;
                MainContentWindow.Background = new SolidColorBrush(Color.FromArgb(0xAA, 0x00, 0x00, 0x00));
            }
        }

        //Button Click Handler - Updates database if everything correct
        private void ConfirmEditButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var context = new SalesDbContext())
                {
                    if (selectedRows.Count == 0)
                    {
                        throw new Exception("Row was not selected or something went wrong");
                    }
                    else if (selectedRows.Count == 1)
                    {
                        sell selectedRow = context.Sale.Find(selectedRows[0].Sin, selectedRows[0].Vin);

                        long txtSin = long.Parse(EditSin.Text);
                        int txtVin = int.Parse(EditVin.Text);
                        long txtEmployee = long.Parse(EditEmployee.Text);
                        DateTime txtSellTime = DateTime.Parse(EditSellTime.Text);

                        selectedRow.Sin = txtSin;
                        selectedRow.Vin = txtVin;
                        selectedRow.EmpID = txtEmployee;
                        selectedRow.SellTime = txtSellTime;
                    }
                    else
                    {
                        foreach (sell dataRow in selectedRows)
                        {
                            sell selectedRow = context.Sale.Find(dataRow.Sin, dataRow.Vin);

                            if (EditEmployee.Text != "" && EditEmployee.Text != null && long.TryParse(EditEmployee.Text, out long txtEmployee))
                                selectedRow.EmpID = txtEmployee;
                        }
                    }

                    //Update DataGrid to show changes
                    context.SaveChanges();
                    SalesDataGrid.ItemsSource = context.Sale.ToList();
                }

                //Clear Selection
                selectedRows.Clear();

                //Disable Scrimming
                MainContentWindow.Opacity = 1;
                MainContentWindow.Background = Brushes.Transparent;

                //Enable Controls
                EnableControls();

                //Close Popup
                EditPopup.IsOpen = false;

                //Set text back to empty
                EditSin.Text = "";
                EditVin.Text = "";
                EditEmployee.Text = "";
                EditSellTime.Text = "";

                //Set the Inputs back to normal
                EditSin.IsHitTestVisible = true;
                EditSin.Foreground = Brushes.Black;
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
            //Clear Selection
            selectedRows.Clear();

            //Disable Scrimming
            MainContentWindow.Opacity = 1;
            MainContentWindow.Background = Brushes.Transparent;

            //Enable Controls
            EnableControls();

            //Close Popup
            EditPopup.IsOpen = false;

            //Set text back to empty
            EditSin.Text = "";
            EditVin.Text = "";
            EditEmployee.Text = "";
            EditSellTime.Text = "";

            //Set the Inputs back to normal
            EditSin.IsHitTestVisible = true;
            EditSin.Foreground = Brushes.Black;
        }
        #endregion

        #region Delete Section

        //Button Click Handler - Sets up the rows for deletion
        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (sell rowData in SalesDataGrid.Items)
            {
                if (rowData.IsSelected)
                {
                    selectedRows.Add(rowData);
                }
            }

            Button removeButton = (Button)sender;
            sell row = (sell)removeButton.DataContext;

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

            //Enable Scrimming and Disable Controls if Popup is open
            if (DeletePopup.IsOpen == true)
            {
                DisableControls();

                MainContentWindow.Opacity = 0.5;
                MainContentWindow.Background = new SolidColorBrush(Color.FromArgb(0xAA, 0x00, 0x00, 0x00));
            }
        }

        //Button Click Handler - Updates database if everything correct
        private void ConfirmRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var context = new SalesDbContext())
                {
                    if (selectedRows.Count == 0)
                    {
                        throw new Exception("Row was not selected or something went wrong");
                    }
                    else if (selectedRows.Count == 1)
                    {
                        sell selectedRow = context.Sale.Find(selectedRows[0].Sin, selectedRows[0].Vin);
                        context.Sale.Remove(selectedRow);
                    }
                    else
                    {
                        foreach (sell dataRow in selectedRows)
                        {
                            sell selectedRow = context.Sale.Find(dataRow.Sin, dataRow.Vin);
                            context.Entry(selectedRow).State = EntityState.Deleted;
                        }
                    }

                    //Update DataGrid to show changes
                    context.SaveChanges();
                    SalesDataGrid.ItemsSource = context.Sale.ToList();
                }

                //Clear Selection
                selectedRows.Clear();

                //Update Counter
                RowCount = SalesDataGrid.Items.Count;
                SalesCounter.Text = $"Current Saved Clients: {RowCount}";

                //Disable Scrimming
                MainContentWindow.Opacity = 1;
                MainContentWindow.Background = Brushes.Transparent;

                //Enable Controls
                EnableControls();

                //Close Popup
                DeletePopup.IsOpen = false;
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
            //Clear Selection
            selectedRows.Clear();

            //Enable Controls
            EnableControls();

            //Disable Scrimming
            MainContentWindow.Opacity = 1;
            MainContentWindow.Background = Brushes.Transparent;

            //Close Popup
            DeletePopup.IsOpen = false;
        }
        #endregion

        #region Add Section

        //Button Click Handler - Opens Row Adding Popup
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            //Enable Scrimming
            MainContentWindow.Opacity = 0.5;
            MainContentWindow.Background = new SolidColorBrush(Color.FromArgb(0xAA, 0x00, 0x00, 0x00));

            //Disable Controls
            DisableControls();

            //Open Popup
            AddPopup.IsOpen = true;
        }

        //Button Click Handler - Adds row to the table if everything correct
        private void ConfirmAddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var context = new SalesDbContext())
                {
                    long txtSin = long.Parse(AddSin.Text);
                    int txtVin = int.Parse(AddVin.Text);
                    long txtEmployee = long.Parse(AddEmployee.Text);
                    DateTime txtSellTime = DateTime.Parse(AddSellTime.Text);

                    sell newSale = new sell
                    {
                        Sin = txtSin,
                        Vin = txtVin,
                        EmpID = txtEmployee,
                        SellTime = txtSellTime
                    };

                    context.Sale.Add(newSale);
                    context.SaveChanges();

                    SalesDataGrid.ItemsSource = context.Sale.ToList();
                }

                //Update Counter
                RowCount = SalesDataGrid.Items.Count;
                SalesCounter.Text = $"Current Saved Sales: {RowCount}";

                //Disable scrimming
                MainContentWindow.Opacity = 1;
                MainContentWindow.Background = Brushes.Transparent;

                //Enable Controls
                EnableControls();

                //Close Popup
                AddPopup.IsOpen = false;

                //Set text back to empty
                AddSin.Text = "";
                AddVin.Text = "";
                AddEmployee.Text = "";
                AddSellTime.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while adding the client: {ex.Message}", "Add Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
            }
        }

        //Button Click Handler - Closes the Adding Popup
        private void CancelAddButton_Click(object sender, RoutedEventArgs e)
        {
            //Disable scrimming
            MainContentWindow.Opacity = 1;
            MainContentWindow.Background = Brushes.Transparent;

            //Enable Controls
            EnableControls();

            //Close Popup
            AddPopup.IsOpen = false;

            //Set text back to empty
            AddSin.Text = "";
            AddVin.Text = "";
            AddEmployee.Text = "";
            AddSellTime.Text = "";
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
            if (txtSearchSales.Text != "Search in Sales...")
            {

                string searchText = txtSearchSales.Text.ToLower();


                SalesDataGrid.Items.Filter = item =>
                {
                    if (item is sell dataItem)
                    {
                        string txtSin = dataItem.Sin.ToString();
                        string txtVin = dataItem.Vin.ToString();
                        string txtEmployee = dataItem.EmpID.ToString();
                        string timeStamp = dataItem.SellTime.ToString();

                        return txtSin.Contains(searchText) ||
                               txtVin.Contains(searchText) ||
                               txtEmployee.Contains(searchText) ||
                               timeStamp.Contains(searchText);

                    }

                    return false;
                };

                SalesDataGrid.Items.Refresh();
            }
        }

        #endregion

        #region Focus Settings

        private void SalesWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (txtSearchSales.Text == "")
            {
                Keyboard.ClearFocus();
                txtSearchSales.Text = "Search in Sales...";
            }
            else
            {
                Keyboard.ClearFocus();
            }

        }

        private void SearchSales_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (txtSearchSales.Text == "Search in Sales...")
            {
                txtSearchSales.Text = "";
            }
        }
        #endregion

        #region Controls Control

        //Disables all Controls
        private void DisableControls()
        {
            btnEmployees.IsHitTestVisible = false;
            btnAdd.IsHitTestVisible = false;
            txtSearchSales.IsHitTestVisible = false;
            SalesDataGrid.IsHitTestVisible = false;
        }

        //Enables all Controls
        private void EnableControls()
        {
            btnEmployees.IsHitTestVisible = true;
            btnAdd.IsHitTestVisible = true;
            txtSearchSales.IsHitTestVisible = true;
            SalesDataGrid.IsHitTestVisible = true;
        }
        #endregion

        #region Comboboxes

        //Set ItemSourcesof ComboBoxes
        private void Combobox_Options()
        {
            using (var context = new SalesDbContext())
            {
                var purchaseOptions = context.Purchase.ToList();
                var employeeOptions = context.Employee.ToList();

                AddSin.ItemsSource = purchaseOptions;
                AddEmployee.ItemsSource = employeeOptions;

                EditSin.ItemsSource = purchaseOptions;
                EditEmployee.ItemsSource = employeeOptions;
            }
        }

        //Automatically update the rows connected to each other
        private void SinComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender == AddSin)
            {
                if (AddSin.SelectedItem is purchase selectedPurchase)
                {
                    AddVin.Text = selectedPurchase.Vin.ToString();

                    DateTime? dataTime = selectedPurchase.PurchaseTime;
                    string date = dataTime?.ToString("dd/MM/yyyy h:mm tt") ?? "No date";

                    AddSellTime.Text = date;
                }
            }

            if (sender == EditSin)
            {
                if (EditSin.SelectedItem is purchase selectedPurchase)
                {
                    EditVin.Text = selectedPurchase.Vin.ToString();

                    DateTime? dataTime = selectedPurchase.PurchaseTime;
                    string date = dataTime?.ToString("dd/MM/yyyy h:mm tt") ?? "No date";

                    EditSellTime.Text = date;
                }
            }
        }
        #endregion


        private SalesDbContext _dbContext;
        public SalesView()
        {
            InitializeComponent();

            //Database initializer
            _dbContext = new SalesDbContext();
            SalesDataGrid.ItemsSource = _dbContext.Sale.ToList();

            //Counter initializer
            RowCount = SalesDataGrid.Items.Count;
            SalesCounter.Text = $"Current Saved Sales: {RowCount}";

            //Setup Comboboxes
            Combobox_Options();
        }
    }

    /// <summary>
    /// Gets context of sell tab from the connected DataBase
    /// </summary>
    public class SalesDbContext : DbContext
    {
        public DbSet<sell> Sale { get; set; } 
        public DbSet<purchase> Purchase { get; set; }
        public DbSet<employee> Employee { get; set; }

        public SalesDbContext() : base("DealershipCon")
        {
        }
    }

}
