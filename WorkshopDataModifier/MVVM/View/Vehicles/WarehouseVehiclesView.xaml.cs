using MaterialDesignThemes.Wpf;
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
    /// Interaction logic for WarehouseVehiclesView.xaml
    /// </summary>
    public partial class WarehouseVehiclesView : UserControl
    {

        #region Counter of the current clients (dynamic)

        private int _rowCount;
        /// <summary>
        /// Counts number of items inside "warehouse_vehicles" table
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

        //Button Click Handler - Sets up the rows for editing
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

                DateTime? dataTime = row.DeliveryTime;

                string date = dataTime?.ToString("dd/MM/yyyy") ?? "No date";
                string time = dataTime?.ToString("h:mm tt") ?? "No time";

                EditVin.Text = row.Vin.ToString();
                EditBrand.Text = row.Brand;
                EditColor.Text = row.Color;
                EditYear.Text = row.Year.ToString();
                EditModel.Text = row.Model;
                EditDoor.Text = row.Door.ToString();
                EditWarehouse.Text = row.Warehouse;
                EditDeliveryDate.Text = date;
                EditDeliveryTime.Text = time;

                EditPopup.IsOpen = true;
            }
            else
            {
                EditVin.IsHitTestVisible = false;
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
                        DateTime txtDelivery = DateTime.Parse(EditDeliveryDate.Text + " " + EditDeliveryTime.Text);

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

                            if (EditDeliveryDate.Text != "" && EditDeliveryDate.Text != null)
                            {
                                if (EditDeliveryTime.Text != "" && EditDeliveryTime.Text != null)
                                {
                                    DateTime txtDelivery = DateTime.Parse(EditDeliveryDate.Text + " " + EditDeliveryTime.Text);
                                    selectedRow.DeliveryTime = txtDelivery;
                                }
                                else
                                {
                                    DateTime txtDelivery = DateTime.Parse(EditDeliveryDate.Text);
                                    selectedRow.DeliveryTime = txtDelivery;
                                }
                            }
                        }
                    }

                    //Update DataGrid to show changes
                    context.SaveChanges();
                    WarehouseVehiclesDataGrid.ItemsSource = context.WarehouseVehicle.ToList();
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
                EditWarehouse.Text = "";
                EditDeliveryDate.Text = "";
                EditDeliveryTime.Text = "";

                //Set the Inputs back to normal
                EditVin.IsHitTestVisible = true;
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
            EditWarehouse.Text = "";
            EditDeliveryDate.Text = "";
            EditDeliveryTime.Text = "";

            //Set the Inputs back to normal
            EditVin.IsHitTestVisible = true;
            EditVin.Foreground = Brushes.Black;
        }
        #endregion

        #region Delete Section

        //Button Click Handler - Sets up the rows for deletion
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

                //Clear Selection
                selectedRows.Clear();

                //Update Counter
                RowCount = WarehouseVehiclesDataGrid.Items.Count;
                WarehouseVehiclesCounter.Text = $"Current Saved Warehouse Vehicles: {RowCount}";

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
                using (var context = new WarehouseVehiclesDbContext())
                {
                    int txtVin = int.Parse(AddVin.Text);
                    int txtYear = int.Parse(AddYear.Text);
                    byte txtDoor = byte.Parse(AddDoor.Text);
                    DateTime txtDelivery = DateTime.Parse(AddDeliveryDate.Text + " " + AddDeliveryTime.Text);

                    warehouse_vehicles newVehicle = new warehouse_vehicles
                    {
                        Vin = txtVin,
                        Brand = AddBrand.Text,
                        Color = AddColor.Text,
                        Year = txtYear,
                        Model = AddModel.Text,
                        Door = txtDoor,
                        Warehouse = AddWarehouse.Text,
                        DeliveryTime = txtDelivery
                    };

                    context.WarehouseVehicle.Add(newVehicle);
                    context.SaveChanges();

                    WarehouseVehiclesDataGrid.ItemsSource = context.WarehouseVehicle.ToList();
                }

                //Update Counter
                RowCount = WarehouseVehiclesDataGrid.Items.Count;
                WarehouseVehiclesCounter.Text = $"Current Saved Warehouse Vehicles: {RowCount}";

                //Disable scrimming
                MainContentWindow.Opacity = 1;
                MainContentWindow.Background = Brushes.Transparent;

                //Enable Controls
                EnableControls();

                //Close Popup
                AddPopup.IsOpen = false;

                //Set text back to empty
                AddVin.Text = "";
                AddBrand.Text = "";
                AddColor.Text = "";
                AddYear.Text = "";
                AddModel.Text = "";
                AddDoor.Text = "";
                AddWarehouse.Text = "";
                AddDeliveryDate.Text = "";
                AddDeliveryTime.Text = "";
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
            AddVin.Text = "";
            AddBrand.Text = "";
            AddColor.Text = "";
            AddYear.Text = "";
            AddModel.Text = "";
            AddDoor.Text = "";
            AddWarehouse.Text = "";
            AddDeliveryDate.Text = "";
            AddDeliveryTime.Text = "";
        }

        #endregion

        #region MoveToDealer Section

        //Button Click Handler - Opens Row Moving Popup
        private void MoveToDealerButton_Click (object sender, RoutedEventArgs e)
        {
            //Enable Scrimming
            MainContentWindow.Opacity = 0.5;
            MainContentWindow.Background = new SolidColorBrush(Color.FromArgb(0xAA, 0x00, 0x00, 0x00));

            //Disable Controls
            DisableControls();

            //Identify the Selected Row
            Button moveToDealerButton = (Button)sender;
            warehouse_vehicles row = (warehouse_vehicles)moveToDealerButton.DataContext;
            selectedRows.Add(row);

            //Open Popup
            MoveToDealerPopup.IsOpen = true;
        }

        private void ConfirmMoveToDealerButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var context = new WarehouseVehiclesDbContext())
                {
                    warehouse_vehicles selectedRow = context.WarehouseVehicle.Find(selectedRows[0].Vin);

                    //Handle the addition to "vehicles" tab
                    decimal txtPrice = decimal.Parse(MoveToDealerPrice.Text);
                    DateTime txtDelivery = DateTime.Parse(MoveToDealerDeliveryDate.Text + " " + MoveToDealerDeliveryTime.Text);

                    vehicles newVehicle = new vehicles
                    {
                        Vin = selectedRow.Vin,
                        Brand = selectedRow.Brand,
                        Color = selectedRow.Color,
                        Year = selectedRow.Year,
                        Model = selectedRow.Model,
                        Door = selectedRow.Door,
                        Price = txtPrice,
                        Dealership = MoveToDealerDealership.Text,
                        DeliveryTime = txtDelivery
                    };

                    context.Vehicles.Add(newVehicle);
                    
                    //Handle the deletion from "warehouse_vehicles" tab
                    context.WarehouseVehicle.Remove(selectedRow);

                    //Save the changes and Update Datagrid
                    context.SaveChanges();
                    WarehouseVehiclesDataGrid.ItemsSource = context.WarehouseVehicle.ToList();
                }

                //Clear Selection
                selectedRows.Clear();

                //Update Counter
                RowCount = WarehouseVehiclesDataGrid.Items.Count;
                WarehouseVehiclesCounter.Text = $"Current Saved Warehouse Vehicles: {RowCount}";

                //Disable Scrimming
                MainContentWindow.Opacity = 1;
                MainContentWindow.Background = Brushes.Transparent;

                //Enable Controls
                EnableControls();

                //Close Popup
                MoveToDealerPopup.IsOpen = false;

                //Set text back to empty
                MoveToDealerPrice.Text = "";
                MoveToDealerDealership.Text = "";
                MoveToDealerDeliveryDate.Text = "";
                MoveToDealerDeliveryTime.Text = "";
            }
            catch (DbUpdateException ex)
            {
                // Handle specific database exceptions
                if (ex.InnerException is SqlException sqlException)
                {
                    // Handle referential integrity constraint violation
                    if (sqlException.Number == 547)
                    {
                        MessageBox.Show("Cannot move the item because it is referenced by other entities.", "Move Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    }
                    else
                    {
                        MessageBox.Show($"An error occurred while moving the item: {sqlException.Message}", "Move Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    }
                }
                else
                {
                    MessageBox.Show($"An error occurred while moving the item: {ex.Message}", "Move Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                }
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                MessageBox.Show($"An error occurred while moving the item: {ex.Message}", "Move Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
            }
        }

        private void CancelMoveToDealerButton_Click(object sender, RoutedEventArgs e)
        {
            //Clear Selection
            selectedRows.Clear();

            //Enable Controls
            EnableControls();

            //Disable scrimming
            MainContentWindow.Opacity = 1;
            MainContentWindow.Background = Brushes.Transparent;

            //Close Popup
            MoveToDealerPopup.IsOpen = false;

            //Set text back to empty
            MoveToDealerPrice.Text = "";
            MoveToDealerDealership.Text = "";
            MoveToDealerDeliveryDate.Text = "";
            MoveToDealerDeliveryTime.Text = "";
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

        #region MoveToDealer Popup

        private void MoveToDealerPopup_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                isDraggingPopup = true;
                startPoint = e.GetPosition(MoveToDealerPopup);
            }
        }

        private void MoveToDealerPopup_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDraggingPopup)
            {
                Point currentPoint = e.GetPosition(MoveToDealerPopup);
                double offsetX = currentPoint.X - startPoint.X;
                double offsetY = currentPoint.Y - startPoint.Y;

                AddPopup.HorizontalOffset += offsetX;
                AddPopup.VerticalOffset += offsetY;

                startPoint = currentPoint;
            }
        }

        private void MoveToDealerPopup_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
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

        private void SearchWarehouseVehicles_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (txtSearchWarehouseVehicles.Text == "Search in Warehouse...")
            {
                txtSearchWarehouseVehicles.Text = "";
            }
        }
        #endregion

        #region Controls Control

        //Disables all Controls
        private void DisableControls()
        {
            btnVehicles.IsHitTestVisible = false;
            btnSoldVehicles.IsHitTestVisible = false;
            btnAdd.IsHitTestVisible = false;
            txtSearchWarehouseVehicles.IsHitTestVisible = false;
            WarehouseVehiclesDataGrid.IsHitTestVisible = false;
        }

        //Enables all Controls
        private void EnableControls()
        {
            btnVehicles.IsHitTestVisible = true;
            btnSoldVehicles.IsHitTestVisible = true;
            btnAdd.IsHitTestVisible = true;
            txtSearchWarehouseVehicles.IsHitTestVisible = true;
            WarehouseVehiclesDataGrid.IsHitTestVisible = true;
        }
        #endregion

        #region Comboboxes & DateTime Pickers

        //Set ItemSourcesof ComboBoxes
        private void Combobox_Options()
        {
            using (var context = new WarehouseVehiclesDbContext())
            {
                var brandOptions = context.Brand.ToList();
                var warehouseOptions = context.Warehouse.ToList();
                var dealershipOptions = context.Dealership.ToList();

                AddBrand.ItemsSource = brandOptions;
                AddWarehouse.ItemsSource = warehouseOptions;

                EditBrand.ItemsSource = brandOptions;
                EditWarehouse.ItemsSource = warehouseOptions;

                MoveToDealerDealership.ItemsSource = dealershipOptions;
            }

            int currentYear = DateTime.Now.Year;
            List<int> years = new List<int>();

            for (int year = currentYear; year >= 1900; year--)
            {
                years.Add(year);
            }

            AddYear.ItemsSource = years;
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

                if (sender == EditDeliveryTime)
                {
                    if (EditDeliveryDate.SelectedDate == currentDateTime.Date && selectedTime.Value.TimeOfDay > currentDateTime.TimeOfDay)
                    {
                        EditDeliveryTime.SelectedTime = currentDateTime;
                    }
                }

                if (sender == AddDeliveryTime)
                {
                    if (AddDeliveryDate.SelectedDate == currentDateTime.Date && selectedTime.Value.TimeOfDay > currentDateTime.TimeOfDay)
                    {
                        AddDeliveryTime.SelectedTime = currentDateTime;
                    }
                }
            }
        }
        #endregion


        private WarehouseVehiclesDbContext _dbContext;
        /// <summary>
        /// Main constructor of WarehouseVehiclesView UserControl.
        /// </summary>
        public WarehouseVehiclesView()
        {
            InitializeComponent();


            //Database initializer
            _dbContext = new WarehouseVehiclesDbContext();
            WarehouseVehiclesDataGrid.ItemsSource = _dbContext.WarehouseVehicle.ToList();

            //Counter initializer
            RowCount = WarehouseVehiclesDataGrid.Items.Count;
            WarehouseVehiclesCounter.Text = $"Current Saved Warehouse Vehicles: {RowCount}";

            //Setup Comboboxes
            Combobox_Options();
        }
    }


    /// <summary>
    /// Represents the context of the warehouse_vehicles tab from the connected database.
    /// </summary>
    public class WarehouseVehiclesDbContext : DbContext
    {
        /// <summary>
        /// Gets or sets the DbSet of warehouse_vehicles.
        /// </summary>
        public DbSet<warehouse_vehicles> WarehouseVehicle { get; set; }

        /// <summary>
        /// Gets or sets the DbSet of brands.
        /// </summary>
        public DbSet<brands> Brand { get; set; }

        /// <summary>
        /// Gets or sets the DbSet of warehouse.
        /// </summary>
        public DbSet<warehouse> Warehouse { get; set; }

        /// <summary>
        /// Gets or sets the DbSet of vehicles.
        /// </summary>
        public DbSet<vehicles> Vehicles { get; set; }

        /// <summary>
        /// Gets or sets the DbSet of dealership.
        /// </summary>
        public DbSet<dealership> Dealership { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WarehouseVehiclesDbContext"/> class using the specified connection string.
        /// </summary>
        public WarehouseVehiclesDbContext() : base("DealershipCon")
        {
        }
    }

}
