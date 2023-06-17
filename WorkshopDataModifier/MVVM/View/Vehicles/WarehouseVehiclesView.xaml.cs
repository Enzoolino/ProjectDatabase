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
using WorkshopDataModifier.Data;

namespace WorkshopDataModifier.MVVM.View
{
    /// <summary>
    /// Interaction logic for WarehouseVehiclesView.xaml
    /// </summary>
    public partial class WarehouseVehiclesView : UserControl
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

            foreach (warehouse_vehicles row in WarehouseVehiclesDataGrid.Items)
            {
                row.IsSelected = isChecked;
            }
        }

        private void RowCheckBox_Click(object sender, RoutedEventArgs e)
        {
            DataGridRow row = (DataGridRow)WarehouseVehiclesDataGrid.ItemContainerGenerator.ContainerFromItem(((FrameworkElement)sender).DataContext);
            if (row != null)
            {
                warehouse_vehicles rowData = (warehouse_vehicles)row.Item;
                rowData.IsSelected = ((CheckBox)sender).IsChecked == true;
            }
        }
        #endregion

        #region Data Modification

        //List of all selected rows (Initialized with button click)
        static List<warehouse_vehicles> selectedRows = new List<warehouse_vehicles>();

        #region Edit Section
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (warehouse_vehicles rowData in WarehouseVehiclesDataGrid.Items)
            {
                if (rowData.IsSelected)
                {
                    selectedRows.Add(rowData);
                }
            }

            Button editButton = (Button)sender;
            warehouse_vehicles row = (warehouse_vehicles)editButton.DataContext;

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
                EditWarehouse.Text = row.Warehouse;
                EditDeliveryTime.Text = row.DeliveryTime.ToString();

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
                using (var context = new WarehouseVehiclesDbContext())
                {
                    if (selectedRows.Count == 0)
                    {
                        throw new Exception("Row was not selected or something went wrong");
                    }
                    else if (selectedRows.Count == 1)
                    {
                        warehouse_vehicles selectedRow = context.WarehouseVehicle.Find(selectedRows[0].Vin);

                        int txtVin = int.Parse(EditVin.Text);
                        int txtYear = int.Parse(EditYear.Text);
                        byte txtDoor = byte.Parse(EditDoor.Text);
                        DateTime txtDelivery = DateTime.Parse(EditDeliveryTime.Text);

                        selectedRow.Vin = txtVin;
                        selectedRow.Brand = EditBrand.Text;
                        selectedRow.Color = EditColor.Text;
                        selectedRow.Year = txtYear;
                        selectedRow.Model = EditModel.Text;
                        selectedRow.Door = txtDoor;
                        selectedRow.Warehouse = EditWarehouse.Text;
                        selectedRow.DeliveryTime = txtDelivery;
                    }
                    else
                    {
                        foreach (warehouse_vehicles dataRow in selectedRows)
                        {
                            warehouse_vehicles selectedRow = context.WarehouseVehicle.Find(dataRow.Vin);

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

                            if (EditWarehouse.Text != "" && EditWarehouse.Text != null)
                                selectedRow.Warehouse = EditWarehouse.Text;

                            if (EditDeliveryTime.Text != "" && EditDeliveryTime.Text != null && DateTime.TryParse(EditDeliveryTime.Text, out DateTime txtDelivery))
                                selectedRow.DeliveryTime = txtDelivery;
                        }
                    }

                    context.SaveChanges();
                    WarehouseVehiclesDataGrid.ItemsSource = context.WarehouseVehicle.ToList();
                }

                EditPopup.IsOpen = false;

                EditVin.Text = "";
                EditBrand.Text = "";
                EditColor.Text = "";
                EditYear.Text = "";
                EditModel.Text = "";
                EditDoor.Text = "";
                EditWarehouse.Text = "";
                EditDeliveryTime.Text = "";

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
            EditWarehouse.Text = "";
            EditDeliveryTime.Text = "";

            selectedRows.Clear();

            EditPopup.IsOpen = false;
        }
        #endregion

        #region Delete Section
        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (warehouse_vehicles rowData in WarehouseVehiclesDataGrid.Items)
            {
                if (rowData.IsSelected)
                {
                    selectedRows.Add(rowData);
                }
            }

            Button removeButton = (Button)sender;
            warehouse_vehicles row = (warehouse_vehicles)removeButton.DataContext;

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
                using (var context = new WarehouseVehiclesDbContext())
                {
                    if (selectedRows.Count == 0)
                    {
                        throw new Exception("Row was not selected or something went wrong");
                    }
                    else if (selectedRows.Count == 1)
                    {
                        warehouse_vehicles selectedRow = context.WarehouseVehicle.Find(selectedRows[0].Vin);
                        context.WarehouseVehicle.Remove(selectedRow);
                    }
                    else
                    {
                        foreach (warehouse_vehicles dataRow in selectedRows)
                        {
                            warehouse_vehicles selectedRow = context.WarehouseVehicle.Find(dataRow.Vin);
                            context.Entry(selectedRow).State = EntityState.Deleted;
                        }
                    }

                    //Update DataGrid to show changes
                    context.SaveChanges();
                    WarehouseVehiclesDataGrid.ItemsSource = context.WarehouseVehicle.ToList();
                }

                RowCount = WarehouseVehiclesDataGrid.Items.Count;
                WarehouseVehiclesCounter.Text = $"Current Saved Vehicles: {RowCount}";

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
                using (var context = new WarehouseVehiclesDbContext())
                {
                    int txtVin = int.Parse(AddVin.Text);
                    int txtYear = int.Parse(AddYear.Text);
                    byte txtDoor = byte.Parse(AddDoor.Text);
                    DateTime txtDeliveryTime = DateTime.Parse(AddDeliveryTime.Text);

                    warehouse_vehicles newVehicle = new warehouse_vehicles
                    {
                        Vin = txtVin,
                        Brand = AddBrand.Text,
                        Color = AddColor.Text,
                        Year = txtYear,
                        Model = AddModel.Text,
                        Door = txtDoor,
                        Warehouse = AddWarehouse.Text,
                        DeliveryTime = txtDeliveryTime
                    };

                    context.WarehouseVehicle.Add(newVehicle);
                    context.SaveChanges();

                    WarehouseVehiclesDataGrid.ItemsSource = context.WarehouseVehicle.ToList();
                }

                RowCount = WarehouseVehiclesDataGrid.Items.Count;
                WarehouseVehiclesCounter.Text = $"Current Saved Warehouse Vehicles: {RowCount}";
                AddPopup.IsOpen = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while adding the client: {ex.Message}", "Add Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
            }
        }

        private void CancelAddButton_Click(object sender, RoutedEventArgs e)
        {
            AddVin.Text = "";
            AddBrand.Text = "";
            AddColor.Text = "";
            AddYear.Text = "";
            AddModel.Text = "";
            AddDoor.Text = "";
            AddWarehouse.Text = "";
            AddDeliveryTime.Text = "";

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
            if (txtSearchWarehouseVehicles.Text != "Search in Warehouse...")
            {
                string searchText = txtSearchWarehouseVehicles.Text.ToLower();

                WarehouseVehiclesDataGrid.Items.Filter = item =>
                {
                    if (item is warehouse_vehicles dataItem)
                    {
                        string txtVin = dataItem.Vin.ToString();
                        string txtYear = dataItem.Year.ToString();
                        string txtDoor = dataItem.Door.ToString();
                        
                        string txtDeliveryTime = dataItem.DeliveryTime.ToString();

                        return txtVin.Contains(searchText) ||
                               dataItem.Brand.ToLower().Contains(searchText) ||
                               dataItem.Color.ToLower().Contains(searchText) ||
                               txtYear.Contains(searchText) ||
                               dataItem.Model.ToLower().Contains(searchText) ||
                               txtDoor.Contains(searchText) ||
                               dataItem.Warehouse.ToLower().Contains(searchText) ||
                               txtDeliveryTime.Contains(searchText);
                    }

                    return false;
                };

                WarehouseVehiclesDataGrid.Items.Refresh();
            }
        }

        #endregion

        #region Focus Settings

        private void WarehouseVehiclesWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (txtSearchWarehouseVehicles.Text == "")
            {
                Keyboard.ClearFocus();
                txtSearchWarehouseVehicles.Text = "Search in Warehouse...";
            }
            else
            {
                Keyboard.ClearFocus();
            }

        }

        private void txtSearchWarehouseVehicles_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (txtSearchWarehouseVehicles.Text == "Search in Warehouse...")
            {
                txtSearchWarehouseVehicles.Text = "";
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


        private WarehouseVehiclesDbContext _dbContext;

        public WarehouseVehiclesView()
        {
            InitializeComponent();


            //Database initializer
            _dbContext = new WarehouseVehiclesDbContext();
            WarehouseVehiclesDataGrid.ItemsSource = _dbContext.WarehouseVehicle.ToList();

            //Counter initializer
            RowCount = WarehouseVehiclesDataGrid.Items.Count;
            WarehouseVehiclesCounter.Text = $"Current Saved Warehouse Vehicles: {RowCount}";
        }
    }



    /// <summary>
    /// Gets context of warehouse_vahicles tab from the connected DataBase
    /// </summary>
    public class WarehouseVehiclesDbContext : DbContext
    {
        public DbSet<warehouse_vehicles> WarehouseVehicle { get; set; } //DbSet dla tabeli "customer"

        public WarehouseVehiclesDbContext() : base("DealershipCon")
        {
        }
    }

}
