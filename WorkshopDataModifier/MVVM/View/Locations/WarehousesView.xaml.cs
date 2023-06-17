﻿using System;
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
    /// Interaction logic for WarehousesView.xaml
    /// </summary>
    public partial class WarehousesView : UserControl
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

            foreach (warehouse row in WarehousesDataGrid.Items)
            {
                row.IsSelected = isChecked;
            }
        }

        private void RowCheckBox_Click(object sender, RoutedEventArgs e)
        {
            DataGridRow row = (DataGridRow)WarehousesDataGrid.ItemContainerGenerator.ContainerFromItem(((FrameworkElement)sender).DataContext);
            if (row != null)
            {
                warehouse rowData = (warehouse)row.Item;
                rowData.IsSelected = ((CheckBox)sender).IsChecked == true;
            }
        }
        #endregion

        #region Data Modification

        //List of all selected rows (Initialized with button click)
        static List<warehouse> selectedRows = new List<warehouse>();

        #region Edit Section
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (warehouse rowData in WarehousesDataGrid.Items)
            {
                if (rowData.IsSelected)
                {
                    selectedRows.Add(rowData);
                }
            }

            Button editButton = (Button)sender;
            warehouse row = (warehouse)editButton.DataContext;

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

                EditName.Text = row.Name;
                EditLocation.Text = row.Location;
                EditBranch.Text = row.BranchID.ToString();

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
                using (var context = new WarehousesDbContext())
                {
                    if (selectedRows.Count == 0)
                    {
                        throw new Exception("Row was not selected or something went wrong");
                    }
                    else if (selectedRows.Count == 1)
                    {
                        warehouse selectedRow = context.Warehouse.Find(selectedRows[0].Name);

                        int txtBranch = int.Parse(EditBranch.Text);

                        selectedRow.Name = EditName.Text;
                        selectedRow.Location = EditLocation.Text;
                        selectedRow.BranchID = txtBranch;
                    }
                    else
                    {
                        foreach (warehouse dataRow in selectedRows)
                        {
                            warehouse selectedRow = context.Warehouse.Find(dataRow.Name);

                            int txtBranch = int.Parse(EditBranch.Text);

                            if (EditLocation.Text != "" && EditLocation.Text != null)
                                selectedRow.Location = EditLocation.Text;

                            if (EditBranch.Text != "" && EditBranch.Text != null)
                                selectedRow.BranchID = txtBranch;
                        }
                    }

                    context.SaveChanges();
                    WarehousesDataGrid.ItemsSource = context.Warehouse.ToList();
                }
                EditPopup.IsOpen = false;

                EditName.Text = "";
                EditLocation.Text = "";
                EditBranch.Text = "";

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
            EditName.Text = "";
            EditLocation.Text = "";
            EditBranch.Text = "";

            selectedRows.Clear();

            EditPopup.IsOpen = false;
        }
        #endregion

        #region Delete Section
        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (warehouse rowData in WarehousesDataGrid.Items)
            {
                if (rowData.IsSelected)
                {
                    selectedRows.Add(rowData);
                }
            }

            Button removeButton = (Button)sender;
            warehouse row = (warehouse)removeButton.DataContext;

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
                using (var context = new WarehousesDbContext())
                {
                    if (selectedRows.Count == 0)
                    {
                        throw new Exception("Row was not selected or something went wrong");
                    }
                    else if (selectedRows.Count == 1)
                    {
                        warehouse selectedRow = context.Warehouse.Find(selectedRows[0].Name);
                        context.Warehouse.Remove(selectedRow);
                    }
                    else
                    {
                        foreach (warehouse dataRow in selectedRows)
                        {
                            warehouse selectedRow = context.Warehouse.Find(dataRow.Name);
                            context.Entry(selectedRow).State = EntityState.Deleted;
                        }
                    }

                    //Update DataGrid to show changes
                    context.SaveChanges();
                    WarehousesDataGrid.ItemsSource = context.Warehouse.ToList();
                }

                RowCount = WarehousesDataGrid.Items.Count;
                WarehousesCounter.Text = $"Current Saved Warehouses: {RowCount}";

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
                using (var context = new WarehousesDbContext())
                {
                    int txtBranch = int.Parse(AddBranch.Text);

                    warehouse newBranch = new warehouse
                    {
                        Name = AddName.Text,
                        Location = AddLocation.Text,
                        BranchID = txtBranch
                    };

                    context.Warehouse.Add(newBranch);
                    context.SaveChanges();

                    WarehousesDataGrid.ItemsSource = context.Warehouse.ToList();
                }

                RowCount = WarehousesDataGrid.Items.Count;
                WarehousesCounter.Text = $"Current Saved Warehouses: {RowCount}";
                AddPopup.IsOpen = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while adding the client: {ex.Message}", "Add Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
            }
        }

        private void CancelAddButton_Click(object sender, RoutedEventArgs e)
        {
            AddName.Text = "";
            AddLocation.Text = "";
            AddBranch.Text = "";

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
            if (txtSearchWarehouses.Text != "Search in Warehouses...")
            {
                string searchText = txtSearchWarehouses.Text.ToLower();

                WarehousesDataGrid.Items.Filter = item =>
                {
                    if (item is warehouse dataItem)
                    {
                        string txtBranch = dataItem.BranchID.ToString();

                        return dataItem.Name.ToLower().Contains(searchText) ||
                               dataItem.Location.ToLower().Contains(searchText) ||
                               txtBranch.Contains(searchText);
                    }

                    return false;
                };

                WarehousesDataGrid.Items.Refresh();
            }
        }

        #endregion

        #region Focus Settings

        private void WarehousesWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (txtSearchWarehouses.Text == "")
            {
                Keyboard.ClearFocus();
                txtSearchWarehouses.Text = "Search in Warehouses...";
            }
            else
            {
                Keyboard.ClearFocus();
            }

        }

        private void txtSearchWarehouses_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (txtSearchWarehouses.Text == "Search in Warehouses...")
            {
                txtSearchWarehouses.Text = "";
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


        private WarehousesDbContext _dbContext;
        public WarehousesView()
        {
            InitializeComponent();

            //Database initializer
            _dbContext = new WarehousesDbContext();
            WarehousesDataGrid.ItemsSource = _dbContext.Warehouse.ToList();

            //Counter initializer
            RowCount = WarehousesDataGrid.Items.Count;
            WarehousesCounter.Text = $"Current Saved Warehouses: {RowCount}";
        }
    }

    /// <summary>
    /// Gets context of warehouse tab from the connected DataBase
    /// </summary>
    public class WarehousesDbContext : DbContext
    {
        public DbSet<warehouse> Warehouse { get; set; } //DbSet dla tabeli "warehouse"

        public WarehousesDbContext() : base("DealershipCon")
        {
        }
    }
}