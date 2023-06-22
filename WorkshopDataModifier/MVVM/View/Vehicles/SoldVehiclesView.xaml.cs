using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.Entity;
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
using MaterialDesignThemes.Wpf;

namespace WorkshopDataModifier.MVVM.View
{
    /// <summary>
    /// Interaction logic for SoldVehiclesView.xaml
    /// </summary>
    public partial class SoldVehiclesView : UserControl
    {
        #region Counter of the current clients (dynamic)

        private int _rowCount;
        /// <summary>
        /// Counts number of items inside "sold_vehicles" table
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

            foreach (sold_vehicles row in SoldVehiclesDataGrid.Items)
            {
                row.IsSelected = isChecked;
            }
        }

        private void RowCheckBox_Click(object sender, RoutedEventArgs e)
        {
            DataGridRow row = (DataGridRow)SoldVehiclesDataGrid.ItemContainerGenerator.ContainerFromItem(((FrameworkElement)sender).DataContext);
            if (row != null)
            {
                sold_vehicles rowData = (sold_vehicles)row.Item;
                rowData.IsSelected = ((CheckBox)sender).IsChecked == true;
            }
        }
        #endregion

        #region Data Modification

        //List of all selected rows (Initialized with button click)
        static List<sold_vehicles> selectedRows = new List<sold_vehicles>();

        #region Edit Section

        //Button Click Handler - Sets up the rows for editing
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (sold_vehicles rowData in SoldVehiclesDataGrid.Items)
            {
                if (rowData.IsSelected)
                {
                    selectedRows.Add(rowData);
                }
            }

            Button editButton = (Button)sender;
            sold_vehicles row = (sold_vehicles)editButton.DataContext;

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

                DateTime? dataTime = row.SellTime;

                string date = dataTime?.ToString("dd/MM/yyyy") ?? "No date";
                string time = dataTime?.ToString("h:mm tt") ?? "No time";

                EditVin.Text = row.Vin.ToString();
                EditBrand.Text = row.Brand;
                EditColor.Text = row.Color;
                EditYear.Text = row.Year.ToString();
                EditModel.Text = row.Model;
                EditDoor.Text = row.Door.ToString();
                EditPrice.Text = row.Price.ToString();
                EditDealership.Text = row.Dealership;
                EditSellDate.Text = date;
                EditSellTime.Text = time;

                EditPopup.IsOpen = true;
            }
            else
            {
                EditVin.IsHitTestVisible = false;
                EditVin.Foreground = Brushes.Gray;
                EditVin.Text = "Can't Multi Edit !";

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
                using (var context = new SoldVehiclesDbContext())
                {
                    if (selectedRows.Count == 0)
                    {
                        throw new Exception("Row was not selected or something went wrong");
                    }
                    else if (selectedRows.Count == 1)
                    {
                        sold_vehicles selectedRow = context.SoldVehicle.Find(selectedRows[0].Vin);

                        int txtVin = int.Parse(EditVin.Text);
                        int txtYear = int.Parse(EditYear.Text);
                        byte txtDoor = byte.Parse(EditDoor.Text);
                        decimal txtPrice = decimal.Parse(EditPrice.Text);
                        DateTime txtSellTime = DateTime.Parse(EditSellDate.Text + " " + EditSellTime.Text);

                        selectedRow.Vin = txtVin;
                        selectedRow.Brand = EditBrand.Text;
                        selectedRow.Color = EditColor.Text;
                        selectedRow.Year = txtYear;
                        selectedRow.Model = EditModel.Text;
                        selectedRow.Door = txtDoor;
                        selectedRow.Price = txtPrice;
                        selectedRow.Dealership = EditDealership.Text;
                        selectedRow.SellTime = txtSellTime;
                    }
                    else
                    {
                        foreach (sold_vehicles dataRow in selectedRows)
                        {
                            sold_vehicles selectedRow = context.SoldVehicle.Find(dataRow.Vin);

                            if (EditBrand.Text != "" && EditBrand.Text != null)
                                selectedRow.Brand = EditBrand.Text;

                            if (EditColor.Text != "" && EditColor.Text != null)
                                selectedRow.Color = EditColor.Text;

                            if (EditYear.Text != "" && EditYear.Text != null && int.TryParse(EditYear.Text, out int txtYear))
                                selectedRow.Year = txtYear;

                            if (EditModel.Text != "" && EditModel.Text != null)
                                selectedRow.Model = EditModel.Text;

                            if (EditDoor.Text != "" && EditDoor.Text != null && byte.TryParse(EditDoor.Text, out byte txtDoor))
                                selectedRow.Door = txtDoor;

                            if (EditPrice.Text != "" && EditPrice.Text != null && decimal.TryParse(EditPrice.Text, out decimal txtPrice))
                                selectedRow.Price = txtPrice;

                            if (EditDealership.Text != "" && EditDealership.Text != null)
                                selectedRow.Dealership = EditDealership.Text;

                            if (EditSellDate.Text != "" && EditSellDate.Text != null)
                            {
                                if (EditSellTime.Text != "" && EditSellTime.Text != null)
                                {
                                    DateTime txtDelivery = DateTime.Parse(EditSellDate.Text + " " + EditSellTime.Text);
                                    selectedRow.SellTime = txtDelivery;
                                }
                                else
                                {
                                    DateTime txtDelivery = DateTime.Parse(EditSellDate.Text);
                                    selectedRow.SellTime = txtDelivery;
                                }
                            }
                        }
                    }

                    //Update DataGrid to show changes
                    context.SaveChanges();
                    SoldVehiclesDataGrid.ItemsSource = context.SoldVehicle.ToList();
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
                EditBrand.Text = "";
                EditColor.Text = "";
                EditYear.Text = "";
                EditModel.Text = "";
                EditDoor.Text = "";
                EditPrice.Text = "";
                EditDealership.Text = "";
                EditSellDate.Text = "";
                EditSellTime.Text = "";

                //Set the Inputs back to normal
                EditVin.IsHitTestVisible = true;
                EditVin.Foreground = Brushes.Black;
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
            EditBrand.Text = "";
            EditColor.Text = "";
            EditYear.Text = "";
            EditModel.Text = "";
            EditDoor.Text = "";
            EditPrice.Text = "";
            EditDealership.Text = "";
            EditSellDate.Text = "";
            EditSellTime.Text = "";

            //Set the Inputs back to normal
            EditVin.IsHitTestVisible = true;
            EditVin.Foreground = Brushes.Black;
        }
        #endregion

        #region Delete Section

        //Button Click Handler - Sets up the rows for deletion
        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (sold_vehicles rowData in SoldVehiclesDataGrid.Items)
            {
                if (rowData.IsSelected)
                {
                    selectedRows.Add(rowData);
                }
            }

            Button removeButton = (Button)sender;
            sold_vehicles row = (sold_vehicles)removeButton.DataContext;

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
                using (var context = new SoldVehiclesDbContext())
                {
                    if (selectedRows.Count == 0)
                    {
                        throw new Exception("Row was not selected or something went wrong");
                    }
                    else if (selectedRows.Count == 1)
                    {
                        sold_vehicles selectedRow = context.SoldVehicle.Find(selectedRows[0].Vin);
                        context.SoldVehicle.Remove(selectedRow);
                    }
                    else
                    {
                        foreach (sold_vehicles dataRow in selectedRows)
                        {
                            sold_vehicles selectedRow = context.SoldVehicle.Find(dataRow.Vin);
                            context.Entry(selectedRow).State = EntityState.Deleted;
                        }
                    }

                    //Update DataGrid to show changes
                    context.SaveChanges();
                    SoldVehiclesDataGrid.ItemsSource = context.SoldVehicle.ToList();
                }

                //Clear Selection
                selectedRows.Clear();

                //Update Counter
                RowCount = SoldVehiclesDataGrid.Items.Count;
                SoldVehiclesCounter.Text = $"Current Saved Sold Vehicles: {RowCount}";

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

        #endregion

        #region Draggable Popups

        private bool isDraggingPopup = false;
        private Point startPoint;

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
            if (txtSearchSoldVehicles.Text != "Search in Sold Vehicles...")
            {
                string searchText = txtSearchSoldVehicles.Text.ToLower();

                SoldVehiclesDataGrid.Items.Filter = item =>
                {
                    if (item is sold_vehicles dataItem)
                    {
                        string txtVin = dataItem.Vin.ToString();
                        string txtYear = dataItem.Year.ToString();
                        string txtDoor = dataItem.Door.ToString();
                        string txtPrice = dataItem.Price.ToString();
                        string txtSellTime = dataItem.SellTime.ToString();

                        return txtVin.Contains(searchText) ||
                               dataItem.Brand.ToLower().Contains(searchText) ||
                               dataItem.Color.ToLower().Contains(searchText) ||
                               txtYear.Contains(searchText) ||
                               dataItem.Model.ToLower().Contains(searchText) ||
                               txtDoor.Contains(searchText) ||
                               txtPrice.Contains(searchText) ||
                               dataItem.Dealership.ToLower().Contains(searchText) ||
                               txtSellTime.Contains(searchText);
                    }

                    return false;
                };

                SoldVehiclesDataGrid.Items.Refresh();
            }
        }

        #endregion

        #region Focus Settings

        private void SoldVehiclesWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (txtSearchSoldVehicles.Text == "")
            {
                Keyboard.ClearFocus();
                txtSearchSoldVehicles.Text = "Search in Sold Vehicles...";
            }
            else
            {
                Keyboard.ClearFocus();
            }

        }

        private void SearchSoldVehicles_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (txtSearchSoldVehicles.Text == "Search in Sold Vehicles...")
            {
                txtSearchSoldVehicles.Text = "";
            }
        }
        #endregion

        #region Controls Control

        //Disables all Controls
        private void DisableControls()
        {
            btnWarehouseVehicles.IsHitTestVisible = false;
            btnVehicles.IsHitTestVisible = false;
            txtSearchSoldVehicles.IsHitTestVisible = false;
            SoldVehiclesDataGrid.IsHitTestVisible = false;
        }

        //Enables all Controls
        private void EnableControls()
        {
            btnWarehouseVehicles.IsHitTestVisible = true;
            btnVehicles.IsHitTestVisible = true;
            txtSearchSoldVehicles.IsHitTestVisible = true;
            SoldVehiclesDataGrid.IsHitTestVisible = true;
        }
        #endregion

        #region Comboboxes & DateTime Pickers

        //Set ItemSourcesof ComboBoxes
        private void Combobox_Options()
        {
            using (var context = new WarehouseVehiclesDbContext())
            {
                var brandOptions = context.Brand.ToList();
                var dealershipOptions = context.Dealership.ToList();

                EditBrand.ItemsSource = brandOptions;
                EditDealership.ItemsSource = dealershipOptions;
            }

            int currentYear = DateTime.Now.Year;
            List<int> years = new List<int>();

            for (int year = currentYear; year >= 1900; year--)
            {
                years.Add(year);
            }

            EditYear.ItemsSource = years;
        }

        //Do not allow future dates
        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            DatePicker datePicker = (DatePicker)sender;

            DateTime? selectedDate = datePicker.SelectedDate;

            // Check if the selected date is in the future
            if (selectedDate.HasValue && selectedDate.Value > DateTime.Now.Date)
            {
                // Set the selected date to the current date
                datePicker.SelectedDate = DateTime.Now.Date;
            }
        }

        //Do not allow future time
        private void TimePicker_SelectedTimeChanged(object sender, RoutedPropertyChangedEventArgs<DateTime?> e)
        {
            TimePicker timePicker = (TimePicker)sender;

            DateTime? selectedTime = timePicker.SelectedTime;

            if (selectedTime.HasValue)
            {
                // Get the current date and time
                DateTime currentDateTime = DateTime.Now;

                if (sender == EditSellTime)
                {
                    if (EditSellDate.SelectedDate == currentDateTime.Date && selectedTime.Value.TimeOfDay > currentDateTime.TimeOfDay)
                    {
                        EditSellTime.SelectedTime = currentDateTime;
                    }
                }
            }
        }
        #endregion


        private SoldVehiclesDbContext _dbContext;

        public SoldVehiclesView()
        {
            InitializeComponent();

            //Database initializer
            _dbContext = new SoldVehiclesDbContext();
            SoldVehiclesDataGrid.ItemsSource = _dbContext.SoldVehicle.ToList();

            //Counter initializer
            RowCount = SoldVehiclesDataGrid.Items.Count;
            SoldVehiclesCounter.Text = $"Current Saved Sold Vehicles: {RowCount}";

            //Setup Comboboxes
            Combobox_Options();
        }
    }


    /// <summary>
    /// Gets context of sold vahicles tab from the connected DataBase
    /// </summary>
    public class SoldVehiclesDbContext : DbContext
    {
        public DbSet<sold_vehicles> SoldVehicle { get; set; }
        public DbSet<brands> Brand { get; set; }
        public DbSet<dealership> Dealership { get; set; }

        public SoldVehiclesDbContext() : base("DealershipCon")
        {
        }
    }


}
