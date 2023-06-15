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
using WorkshopDataModifier.Data;

namespace WorkshopDataModifier.MVVM.View
{
    /// <summary>
    /// Interaction logic for SoldVehiclesView.xaml
    /// </summary>
    public partial class SoldVehiclesView : UserControl
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

                EditVin.Text = row.Vin.ToString();
                EditBrand.Text = row.Brand;
                EditColor.Text = row.Color;
                EditYear.Text = row.Year.ToString();
                EditModel.Text = row.Model;
                EditDoor.Text = row.Door.ToString();
                EditPrice.Text = row.Price.ToString();
                EditDealership.Text = row.Dealership;
                EditSellTime.Text = row.SellTime.ToString();

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
                        DateTime txtSellTime = DateTime.Parse(EditSellTime.Text);

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

                            if (EditVin.Text != "" && EditVin.Text != null && int.TryParse(EditVin.Text, out int txtVin))
                                selectedRow.Vin = txtVin;

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

                            if (EditSellTime.Text != "" && EditSellTime.Text != null && DateTime.TryParse(EditSellTime.Text, out DateTime txtSellTime))
                                selectedRow.SellTime = txtSellTime;
                        }
                    }

                    context.SaveChanges();
                    SoldVehiclesDataGrid.ItemsSource = context.SoldVehicle.ToList();
                }

                EditPopup.IsOpen = false;

                EditVin.Text = "";
                EditBrand.Text = "";
                EditColor.Text = "";
                EditYear.Text = "";
                EditModel.Text = "";
                EditDoor.Text = "";
                EditPrice.Text = "";
                EditDealership.Text = "";
                EditSellTime.Text = "";

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
            EditBrand.Text = "";
            EditColor.Text = "";
            EditYear.Text = "";
            EditModel.Text = "";
            EditDoor.Text = "";
            EditPrice.Text = "";
            EditDealership.Text = "";
            EditSellTime.Text = "";

            selectedRows.Clear();

            EditPopup.IsOpen = false;
        }
        #endregion

        #region Delete Section
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
        }

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

                RowCount = SoldVehiclesDataGrid.Items.Count;
                SoldVehiclesCounter.Text = $"Current Saved Sold Vehicles: {RowCount}";

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

        #endregion

        #region Move To Vehicles

        private void MoveToVehiclesButton_Click(object sender, RoutedEventArgs e)
        {

        }
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

        private void txtSearchSoldVehicles_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (txtSearchSoldVehicles.Text == "Search in Sold Vehicles...")
            {
                txtSearchSoldVehicles.Text = "";
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

        }
    }


    /// <summary>
    /// Gets context of sold vahicles tab from the connected DataBase
    /// </summary>
    public class SoldVehiclesDbContext : DbContext
    {
        public DbSet<sold_vehicles> SoldVehicle { get; set; } //DbSet dla tabeli "sold_vehicles"

        public SoldVehiclesDbContext() : base("DealershipCon")
        {
        }
    }


}
