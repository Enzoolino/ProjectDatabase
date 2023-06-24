using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WorkshopDataModifier.MVVM.Model;
using MaterialDesignThemes.Wpf;

namespace WorkshopDataModifier.MVVM.View
{
    /// <summary>
    /// Interaction logic for CarsView.xaml
    /// </summary>
    public partial class VehiclesView : UserControl
    {

        #region Counter of the current clients (dynamic)
        private int _rowCount;
        /// <summary>
        /// Counts number of items inside "vehicles" table
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

            foreach (vehicles row in VehiclesDataGrid.Items)
            {
                row.IsSelected = isChecked;
            }
        }

        private void RowCheckBox_Click(object sender, RoutedEventArgs e)
        {
            DataGridRow row = (DataGridRow)VehiclesDataGrid.ItemContainerGenerator.ContainerFromItem(((FrameworkElement)sender).DataContext);
            if (row != null)
            {
                vehicles rowData = (vehicles)row.Item;
                rowData.IsSelected = ((CheckBox)sender).IsChecked == true;
            }
        }
        #endregion

        #region Data Modification

        //List of all selected rows (Initialized with button click)
        static List<vehicles> selectedRows = new List<vehicles>();

        #region Edit Section

        //Button Click Handler - Sets up the rows for editing
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (vehicles rowData in VehiclesDataGrid.Items)
            {
                if (rowData.IsSelected)
                {
                    selectedRows.Add(rowData);
                }
            }

            Button editButton = (Button)sender;
            vehicles row = (vehicles)editButton.DataContext;

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
                EditPrice.Text = row.Price.ToString();
                EditDealership.Text = row.Dealership;
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
                using (var context = new VehiclesDbContext())
                {
                    if (selectedRows.Count == 0)
                    {
                        throw new Exception("Row was not selected or something went wrong");
                    }
                    else if (selectedRows.Count == 1)
                    {
                        vehicles selectedRow = context.Vehicle.Find(selectedRows[0].Vin);

                        int txtVin = int.Parse(EditVin.Text);
                        int txtYear = int.Parse(EditYear.Text);
                        byte txtDoor = byte.Parse(EditDoor.Text);
                        decimal txtPrice = decimal.Parse(EditPrice.Text);
                        DateTime txtDelivery = DateTime.Parse(EditDeliveryDate.Text + " " + EditDeliveryTime.Text);

                        selectedRow.Vin = txtVin;
                        selectedRow.Brand = EditBrand.Text;
                        selectedRow.Color = EditColor.Text;
                        selectedRow.Year = txtYear;
                        selectedRow.Model = EditModel.Text;
                        selectedRow.Door = txtDoor;
                        selectedRow.Price = txtPrice;
                        selectedRow.Dealership = EditDealership.Text;
                        selectedRow.DeliveryTime = txtDelivery;
                    }
                    else
                    {
                        foreach (vehicles dataRow in selectedRows)
                        {
                            vehicles selectedRow = context.Vehicle.Find(dataRow.Vin);

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
                    VehiclesDataGrid.ItemsSource = context.Vehicle.ToList();
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
            EditPrice.Text = "";
            EditDealership.Text = "";
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
            foreach (vehicles rowData in VehiclesDataGrid.Items)
            {
                if (rowData.IsSelected)
                {
                    selectedRows.Add(rowData);
                }
            }

            Button removeButton = (Button)sender;
            vehicles row = (vehicles)removeButton.DataContext;

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
                using (var context = new VehiclesDbContext())
                {
                    if (selectedRows.Count == 0)
                    {
                        throw new Exception("Row was not selected or something went wrong");
                    }
                    else if (selectedRows.Count == 1)
                    {
                        vehicles selectedRow = context.Vehicle.Find(selectedRows[0].Vin);
                        context.Vehicle.Remove(selectedRow);
                    }
                    else
                    {
                        foreach (vehicles dataRow in selectedRows)
                        {
                            vehicles selectedRow = context.Vehicle.Find(dataRow.Vin);
                            context.Entry(selectedRow).State = EntityState.Deleted;
                        }
                    }

                    //Update DataGrid to show changes
                    context.SaveChanges();
                    VehiclesDataGrid.ItemsSource = context.Vehicle.ToList();
                }

                //Clear Selection
                selectedRows.Clear();

                //Update Counter
                RowCount = VehiclesDataGrid.Items.Count;
                VehiclesCounter.Text = $"Current Saved Vehicles: {RowCount}";

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
                using (var context = new VehiclesDbContext())
                {
                    int txtVin = int.Parse(AddVin.Text);
                    int txtYear = int.Parse(AddYear.Text);
                    byte txtDoor = byte.Parse(AddDoor.Text);
                    decimal txtPrice = decimal.Parse(AddPrice.Text);
                    DateTime txtDelivery = DateTime.Parse(AddDeliveryDate.Text + " " + AddDeliveryTime.Text);

                    vehicles newVehicle = new vehicles
                    {
                        Vin = txtVin,
                        Brand = AddBrand.Text,
                        Color = AddColor.Text,
                        Year = txtYear,
                        Model = AddModel.Text,
                        Door = txtDoor,
                        Price = txtPrice,
                        Dealership = AddDealership.Text,
                        DeliveryTime = txtDelivery
                    };

                    context.Vehicle.Add(newVehicle);
                    context.SaveChanges();

                    VehiclesDataGrid.ItemsSource = context.Vehicle.ToList();
                }

                //Update Counter
                RowCount = VehiclesDataGrid.Items.Count;
                VehiclesCounter.Text = $"Current Saved Vehicles: {RowCount}";

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
                AddPrice.Text = "";
                AddDealership.Text = "";
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
            AddPrice.Text = "";
            AddDealership.Text = "";
            AddDeliveryDate.Text = "";
            AddDeliveryTime.Text = "";
        }
        #endregion

        #region MoveToSold Section

        //Button Click Handler - Opens Row Moving Popup
        private void MoveToSoldButton_Click(object sender, RoutedEventArgs e)
        {
            //Enable Scrimming
            MainContentWindow.Opacity = 0.5;
            MainContentWindow.Background = new SolidColorBrush(Color.FromArgb(0xAA, 0x00, 0x00, 0x00));

            //Disable Controls
            DisableControls();

            //Identify the Selected Row
            Button moveToSoldButton = (Button)sender;
            vehicles row = (vehicles)moveToSoldButton.DataContext;
            selectedRows.Add(row);

            //Set the employees context to only allow users from the same dealership as the car
            using (var context = new VehiclesDbContext())
            {
                var employeesFromDealer = context.Employee.Where(em => em.WorkLocation == row.Dealership || em.WorkLocation == null).ToList();

                MoveToSoldEmployee.ItemsSource = employeesFromDealer;
            }

            //Open Popup
            MoveToSoldPopup.IsOpen = true;
        }

        private void ConfirmMoveToSoldButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var context = new VehiclesDbContext())
                {
                    vehicles selectedRow = context.Vehicle.Find(selectedRows[0].Vin);

                    //Handle the addition to "sold_vehicles" tab
                    DateTime txtSellTime = DateTime.Parse(MoveToSoldDate.Text + " " + MoveToSoldTime.Text);
                    sold_vehicles newSoldVehicle = new sold_vehicles
                    {
                        Vin = selectedRow.Vin,
                        Brand = selectedRow.Brand,
                        Color = selectedRow.Color,
                        Year = selectedRow.Year,
                        Model = selectedRow.Model,
                        Door = selectedRow.Door,
                        Price = selectedRow.Price,
                        Dealership = selectedRow.Dealership,
                        SellTime = txtSellTime
                    };

                    context.SoldVehicle.Add(newSoldVehicle);

                    //Handle the addition to "purchase" tab
                    purchase newPurchase = new purchase
                    {
                        Vin = newSoldVehicle.Vin,
                        Dealership = newSoldVehicle.Dealership,
                        PurchaseTime = newSoldVehicle.SellTime
                    };

                    context.Purchase.Add(newPurchase);

                    //Handle the addition to "customer" tab
                    customer newCustomer = new customer
                    {
                        Sin = newPurchase.Sin,
                        Vin = newPurchase.Vin,
                        Name = MoveToSoldCustomerName.Text,
                        Surname = MoveToSoldCustomerSurname.Text,
                        Phone = MoveToSoldCustomerPhone.Text,
                        AddTime = newPurchase.PurchaseTime
                    };

                    context.Customer.Add(newCustomer);

                    //Handle the addition to "sell" tab
                    long txtEmployee = long.Parse(MoveToSoldEmployee.Text);
                    sell newSale = new sell
                    {
                        Sin = newPurchase.Sin,
                        Vin = newPurchase.Vin,
                        EmpID = txtEmployee,
                        SellTime = newPurchase.PurchaseTime
                    };

                    context.Sale.Add(newSale);

                    
                    //Handle the deletion from "vehicles" tab
                    context.Vehicle.Remove(selectedRow);

                    //Save the changes and Update Datagrid
                    context.SaveChanges();
                    VehiclesDataGrid.ItemsSource = context.Vehicle.ToList();
                }

                //Clear Selection
                selectedRows.Clear();

                //Update Counter
                RowCount = VehiclesDataGrid.Items.Count;
                VehiclesCounter.Text = $"Current Saved Vehicles: {RowCount}";

                //Disable Scrimming
                MainContentWindow.Opacity = 1;
                MainContentWindow.Background = Brushes.Transparent;

                //Enable Controls
                EnableControls();

                //Close Popup
                MoveToSoldPopup.IsOpen = false;

                //Set text back to empty
                MoveToSoldEmployee.Text = "";
                MoveToSoldDate.Text = "";
                MoveToSoldTime.Text = "";
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

        private void CancelMoveToSoldButton_Click(object sender, RoutedEventArgs e)
        {
            //Clear Selection
            selectedRows.Clear();

            //Enable Controls
            EnableControls();

            //Disable scrimming
            MainContentWindow.Opacity = 1;
            MainContentWindow.Background = Brushes.Transparent;

            //Close Popup
            MoveToSoldPopup.IsOpen = false;

            //Set text back to empty
            MoveToSoldEmployee.Text = "";
            MoveToSoldDate.Text = "";
            MoveToSoldTime.Text = "";
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

        #region MoveToSold Popup
        private void MoveToSoldPopup_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                isDraggingPopup = true;
                startPoint = e.GetPosition(MoveToSoldPopup);
            }
        }

        private void MoveToSoldPopup_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDraggingPopup)
            {
                Point currentPoint = e.GetPosition(MoveToSoldPopup);
                double offsetX = currentPoint.X - startPoint.X;
                double offsetY = currentPoint.Y - startPoint.Y;

                AddPopup.HorizontalOffset += offsetX;
                AddPopup.VerticalOffset += offsetY;

                startPoint = currentPoint;
            }
        }

        private void MoveToSoldPopup_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
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
            if (txtSearchVehicles.Text != "Search in Vehicles...")
            {
                string searchText = txtSearchVehicles.Text.ToLower();

                VehiclesDataGrid.Items.Filter = item =>
                {
                    if (item is vehicles dataItem)
                    {
                        string txtVin = dataItem.Vin.ToString();
                        string txtYear = dataItem.Year.ToString(); 
                        string txtDoor = dataItem.Door.ToString();
                        string txtPrice = dataItem.Price.ToString();
                        string txtDelivery = dataItem.DeliveryTime.ToString();

                        return txtVin.Contains(searchText) ||
                               dataItem.Brand.ToLower().Contains(searchText) ||
                               dataItem.Color.ToLower().Contains(searchText) ||
                               txtYear.Contains(searchText) ||
                               dataItem.Model.ToLower().Contains(searchText) ||
                               txtDoor.Contains(searchText) ||
                               txtPrice.Contains(searchText) ||
                               dataItem.Dealership.ToLower().Contains(searchText) ||
                               txtDelivery.Contains(searchText);
                    }

                    return false;
                };

                VehiclesDataGrid.Items.Refresh();
            }
        }

        #endregion

        #region Focus Settings

        private void VehiclesWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (txtSearchVehicles.Text == "")
            {
                Keyboard.ClearFocus();
                txtSearchVehicles.Text = "Search in Vehicles...";
            }
            else
            {
                Keyboard.ClearFocus();
            }

        }

        private void SearchVehicles_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (txtSearchVehicles.Text == "Search in Vehicles...")
            {
                txtSearchVehicles.Text = "";
            }
        }
        #endregion

        #region Controls Control

        //Disables all Controls
        private void DisableControls()
        {
            btnWarehouseVehicles.IsHitTestVisible = false;
            btnSoldVehicles.IsHitTestVisible = false;
            btnAdd.IsHitTestVisible = false;
            txtSearchVehicles.IsHitTestVisible = false;
            VehiclesDataGrid.IsHitTestVisible = false;
        }

        //Enables all Controls
        private void EnableControls()
        {
            btnWarehouseVehicles.IsHitTestVisible = true;
            btnSoldVehicles.IsHitTestVisible = true;
            btnAdd.IsHitTestVisible = true;
            txtSearchVehicles.IsHitTestVisible = true;
            VehiclesDataGrid.IsHitTestVisible = true;
        }
        #endregion

        #region Comboboxes & DateTime Pickers

        //Set ItemSourcesof ComboBoxes
        private void Combobox_Options()
        {
            using (var context = new VehiclesDbContext())
            {
                var brandOptions = context.Brand.ToList();
                var dealershipOptions = context.Dealership.ToList();
                
                AddBrand.ItemsSource = brandOptions;
                AddDealership.ItemsSource = dealershipOptions;

                EditBrand.ItemsSource = brandOptions;
                EditDealership.ItemsSource = dealershipOptions;
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

                if (sender == MoveToSoldTime)
                {
                    if (MoveToSoldDate.SelectedDate == currentDateTime.Date && selectedTime.Value.TimeOfDay > currentDateTime.TimeOfDay)
                    {
                        MoveToSoldTime.SelectedTime = currentDateTime;
                    }
                }
            }
        }
        #endregion

        private VehiclesDbContext _dbContext;
        /// <summary>
        /// Main constructor of VehiclesView UserControl.
        /// </summary>
        public VehiclesView()
        {
            InitializeComponent();

            //Database initializer
            _dbContext = new VehiclesDbContext();
            VehiclesDataGrid.ItemsSource = _dbContext.Vehicle.ToList();

            //Counter initializer
            RowCount = VehiclesDataGrid.Items.Count;
            VehiclesCounter.Text = $"Current Saved Vehicles: {RowCount}";

            //Setup Comboboxes
            Combobox_Options();
        }
    }


    /// <summary>
    /// Represents the context of the vehicles tab from the connected database.
    /// </summary>
    public class VehiclesDbContext : DbContext
    {
        /// <summary>
        /// Gets or sets the DbSet of vehicles.
        /// </summary>
        public DbSet<vehicles> Vehicle { get; set; }

        /// <summary>
        /// Gets or sets the DbSet of sold_vehicles.
        /// </summary>
        public DbSet<sold_vehicles> SoldVehicle { get; set; }

        /// <summary>
        /// Gets or sets the DbSet of brands.
        /// </summary>
        public DbSet<brands> Brand { get; set; }

        /// <summary>
        /// Gets or sets the DbSet of dealership.
        /// </summary>
        public DbSet<dealership> Dealership { get; set; }

        /// <summary>
        /// Gets or sets the DbSet of purchase.
        /// </summary>
        public DbSet<purchase> Purchase { get; set; }

        /// <summary>
        /// Gets or sets the DbSet of employee.
        /// </summary>
        public DbSet<employee> Employee { get; set; }

        /// <summary>
        /// Gets or sets the DbSet of sell.
        /// </summary>
        public DbSet<sell> Sale { get; set; }

        /// <summary>
        /// Gets or sets the DbSet of customer.
        /// </summary>
        public DbSet<customer> Customer { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VehiclesDbContext"/> class using the specified connection string.
        /// </summary>
        public VehiclesDbContext() : base("DealershipCon")
        {
        }
    }


}
