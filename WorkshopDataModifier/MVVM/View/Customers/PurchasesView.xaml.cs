using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WorkshopDataModifier.MVVM.Model;

namespace WorkshopDataModifier.MVVM.View
{
    /// <summary>
    /// Interaction logic for SalesView.xaml
    /// </summary>
    public partial class PurchasesView : UserControl
    {
        #region Counter of the current purchases (dynamic)

        private int _rowCount;
        /// <summary>
        /// Counts number of items inside "purchase" table
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

            foreach (purchase row in PurchasesDataGrid.Items)
            {
                row.IsSelected = isChecked;
            }
        }

        private void RowCheckBox_Click(object sender, RoutedEventArgs e)
        {
            DataGridRow row = (DataGridRow)PurchasesDataGrid.ItemContainerGenerator.ContainerFromItem(((FrameworkElement)sender).DataContext);
            if (row != null)
            {
                purchase rowData = (purchase)row.Item;
                rowData.IsSelected = ((CheckBox)sender).IsChecked == true;
            }
        }
        #endregion

        #region Data Modification

        //List of all selected rows (Initialized with button click)
        static List<purchase> selectedRows = new List<purchase>();

        #region Edit Section

        //Button Click Handler - Sets up the rows for editing
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (purchase rowData in PurchasesDataGrid.Items)
            {
                if (rowData.IsSelected)
                {
                    selectedRows.Add(rowData);
                }
            }

            Button editButton = (Button)sender;
            purchase row = (purchase)editButton.DataContext;

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
                soldVehicleOptionsEdit = allSoldVehicles.Where(sv => !purchasedVins.Contains(sv.Vin) || row.Vin == sv.Vin).ToList();
                EditVin.ItemsSource = soldVehicleOptionsEdit;

                EditVin.Text = row.Vin.ToString();
                EditDealership.Text = row.Dealership.ToString();
                
                EditPopup.IsOpen = true;
            }
            else
            {
                MessageBox.Show("This DataGrid can't be multi edited !", "Multi Edition Warning", MessageBoxButton.OK, MessageBoxImage.Information);
                selectedRows.Clear();
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
                using (var context = new PurchasesDbContext())
                {
                    if (selectedRows.Count == 0)
                    {
                        throw new Exception("Row was not selected or something went wrong");
                    }
                    else if (selectedRows.Count == 1)
                    {
                        purchase selectedRow = context.Purchase.Find(selectedRows[0].Sin, selectedRows[0].Vin);

                        int txtVin = int.Parse(EditVin.Text);
                        DateTime txtPurchaseTime = DateTime.Parse(EditPurchaseTime.Text);

                        selectedRow.Vin = txtVin;
                        selectedRow.Dealership = EditDealership.Text;
                        selectedRow.PurchaseTime = txtPurchaseTime;
                    }

                    //Update DataGrid to show changes
                    context.SaveChanges();
                    PurchasesDataGrid.ItemsSource = context.Purchase.ToList();
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
                EditVin.Text = "";
                EditDealership.Text = "";
                EditPurchaseTime.Text = "";
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
            EditVin.Text = "";
            EditDealership.Text = "";
            EditPurchaseTime.Text = "";
        }
        #endregion

        #region Delete Section

        //Button Click Handler - Sets up the rows for deletion
        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (purchase rowData in PurchasesDataGrid.Items)
            {
                if (rowData.IsSelected)
                {
                    selectedRows.Add(rowData);
                }
            }

            Button removeButton = (Button)sender;
            purchase row = (purchase)removeButton.DataContext;

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
                using (var context = new PurchasesDbContext())
                {
                    if (selectedRows.Count == 0)
                    {
                        throw new Exception("Row was not selected or something went wrong");
                    }
                    else if (selectedRows.Count == 1)
                    {
                        purchase selectedRow = context.Purchase.Find(selectedRows[0].Sin, selectedRows[0].Vin);
                        context.Purchase.Remove(selectedRow);
                    }
                    else
                    {
                        foreach (purchase dataRow in selectedRows)
                        {
                            purchase selectedRow = context.Purchase.Find(dataRow.Sin, dataRow.Vin);
                            context.Entry(selectedRow).State = EntityState.Deleted;
                        }
                    }

                    //Update DataGrid to show changes
                    context.SaveChanges();
                    PurchasesDataGrid.ItemsSource = context.Purchase.ToList();
                }

                //Clear Selection
                selectedRows.Clear();

                //Update Counter
                RowCount = PurchasesDataGrid.Items.Count;
                CustomersCounter.Text = $"Current Saved Clients: {RowCount}";

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
                using (var context = new PurchasesDbContext())
                {
                    int txtVin = int.Parse(AddVin.Text);
                    DateTime txtPurchaseTime = DateTime.Parse(AddPurchaseTime.Text);

                    purchase newPurchase = new purchase
                    {
                        Vin = txtVin,
                        Dealership = AddDealership.Text,
                        PurchaseTime = txtPurchaseTime 
                    };

                    context.Purchase.Add(newPurchase);
                    context.SaveChanges();

                    PurchasesDataGrid.ItemsSource = context.Purchase.ToList();
                }

                //Update Counter
                RowCount = PurchasesDataGrid.Items.Count;
                CustomersCounter.Text = $"Current Saved Purchases: {RowCount}";

                //Disable scrimming
                MainContentWindow.Opacity = 1;
                MainContentWindow.Background = Brushes.Transparent;

                //Enable Controls
                EnableControls();

                //Close Popup
                AddPopup.IsOpen = false;

                //Set text back to empty
                AddVin.Text = "";
                AddDealership.Text = "";
                AddPurchaseTime.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while adding the client: {ex.Message}", "Add Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
            }
        }

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
            AddVin.Text = "";
            AddDealership.Text = "";
            AddPurchaseTime.Text = "";
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
            if (txtSearchPurchases.Text != "Search in Purchases...")
            {

                string searchText = txtSearchPurchases.Text.ToLower();


                PurchasesDataGrid.Items.Filter = item =>
                {
                    if (item is purchase dataItem)
                    {
                        string txtSin = dataItem.Sin.ToString();
                        string txtVin = dataItem.Vin.ToString();
                        string timeStamp = dataItem.PurchaseTime.ToString();

                        return txtSin.Contains(searchText) ||
                               txtVin.Contains(searchText) ||
                               dataItem.Dealership.ToLower().Contains(searchText) ||
                               timeStamp.Contains(searchText);
                    }

                    return false;
                };

                PurchasesDataGrid.Items.Refresh();
            }
        }

        #endregion

        #region Focus Settings

        private void PurchasesWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (txtSearchPurchases.Text == "")
            {
                Keyboard.ClearFocus();
                txtSearchPurchases.Text = "Search in Purchases...";
            }
            else
            {
                Keyboard.ClearFocus();
            }

        }

        private void SearchPurchases_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (txtSearchPurchases.Text == "Search in Purchases...")
            {
                txtSearchPurchases.Text = "";
            }
        }
        #endregion

        #region Comboboxes

        //Lists for external use
        List<sold_vehicles> allSoldVehicles;
        List<int> purchasedVins;
        List<sold_vehicles> soldVehicleOptionsEdit;

        //Set ItemSources of ComboBoxes
        private void Combobox_Options()
        {
            using (var context = new PurchasesDbContext())
            {
                //List of all sold vehicles
                allSoldVehicles = context.SoldVehicle.ToList();

                // Retrieve the list of already purchased VINs
                purchasedVins = context.Purchase.Select(p => p.Vin).ToList();

                // Exclude the purchased VINs from the sold vehicle options
                var soldVehicleOptions = allSoldVehicles.Where(sv => !purchasedVins.Contains(sv.Vin)).ToList();
                soldVehicleOptionsEdit = allSoldVehicles.Where(sv => !purchasedVins.Contains(sv.Vin)).ToList();

                AddVin.ItemsSource = soldVehicleOptions;

                EditVin.ItemsSource = soldVehicleOptions;
            }
        }

        //Automatically update the rows connected to each other
        private void VinComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender == AddVin)
            {
                if (AddVin.SelectedItem is sold_vehicles selectedVehicle)
                {
                    AddDealership.Text = selectedVehicle.Dealership.ToString();

                    DateTime? dataTime = selectedVehicle.SellTime;
                    string date = dataTime?.ToString("dd/MM/yyyy h:mm tt") ?? "No date";

                    AddPurchaseTime.Text = date;
                }
            }

            if (sender == EditVin)
            {
                if (EditVin.SelectedItem is sold_vehicles selectedVehicle)
                {
                    EditVin.Text = selectedVehicle.Dealership.ToString();

                    DateTime? dataTime = selectedVehicle.SellTime;
                    string date = dataTime?.ToString("dd/MM/yyyy h:mm tt") ?? "No date";

                    EditPurchaseTime.Text = date;
                }
            }
        }
        #endregion

        #region Controls Control
        private void DisableControls()
        {
            btnCustomer.IsHitTestVisible = false;
            btnAdd.IsHitTestVisible = false;
            txtSearchPurchases.IsHitTestVisible = false;
            PurchasesDataGrid.IsHitTestVisible = false;
        }
        private void EnableControls()
        {
            btnCustomer.IsHitTestVisible = true;
            btnAdd.IsHitTestVisible = true;
            txtSearchPurchases.IsHitTestVisible = true;
            PurchasesDataGrid.IsHitTestVisible = true;
        }

        #endregion


        private PurchasesDbContext _dbContext;
        public PurchasesView()
        {
            InitializeComponent();

            //Database initializer
            _dbContext = new PurchasesDbContext();
            PurchasesDataGrid.ItemsSource = _dbContext.Purchase.ToList();

            //Counter initializer
            RowCount = PurchasesDataGrid.Items.Count;
            CustomersCounter.Text = $"Current Saved Purchases: {RowCount}";

            //Setup Comboboxes
            Combobox_Options();
        }
    }


    /// <summary>
    /// Gets context of purchase tab from the connected DataBase
    /// </summary>
    public class PurchasesDbContext : DbContext
    {
        public DbSet<purchase> Purchase { get; set; }
        public DbSet<sold_vehicles> SoldVehicle { get; set; }

        public PurchasesDbContext() : base("DealershipCon")
        {
        }
    }
}
