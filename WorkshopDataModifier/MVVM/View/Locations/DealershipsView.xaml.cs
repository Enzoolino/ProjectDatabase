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
using WorkshopDataModifier.Core;
using WorkshopDataModifier.MVVM.Model;

namespace WorkshopDataModifier.MVVM.View
{
    /// <summary>
    /// Interaction logic for DealershipsView.xaml
    /// </summary>
    public partial class DealershipsView : UserControl
    {
        #region Counter of the current clients (dynamic)

        private int _rowCount;
        /// <summary>
        /// Counts number of items inside "dealership" table
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

            foreach (dealership row in DealershipsDataGrid.Items)
            {
                row.IsSelected = isChecked;
            }
        }

        private void RowCheckBox_Click(object sender, RoutedEventArgs e)
        {
            DataGridRow row = (DataGridRow)DealershipsDataGrid.ItemContainerGenerator.ContainerFromItem(((FrameworkElement)sender).DataContext);
            if (row != null)
            {
                dealership rowData = (dealership)row.Item;
                rowData.IsSelected = ((CheckBox)sender).IsChecked == true;
            }
        }
        #endregion

        #region Data Modification

        //List of all selected rows (Initialized with button click)
        static List<dealership> selectedRows = new List<dealership>();

        #region Edit Section

        //Button Click Handler - Sets up the rows for editing
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (dealership rowData in DealershipsDataGrid.Items)
            {
                if (rowData.IsSelected)
                {
                    selectedRows.Add(rowData);
                }
            }

            Button editButton = (Button)sender;
            dealership row = (dealership)editButton.DataContext;

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
                EditPhone.Text = row.Phone;

                EditPopup.IsOpen = true;
            }
            else
            {
                EditName.IsHitTestVisible = false;
                EditName.Foreground = Brushes.Gray;
                EditName.Text = "Can't Multi Edit !";

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
                using (var context = new DealershipsDbContext())
                {
                    if (selectedRows.Count == 0)
                    {
                        throw new Exception("Row was not selected or something went wrong");
                    }
                    else if (selectedRows.Count == 1)
                    {
                        dealership selectedRow = context.Dealership.Find(selectedRows[0].Name);

                        int txtBranch = int.Parse(EditBranch.Text);

                        selectedRow.Name  = EditName.Text;
                        selectedRow.Location = EditLocation.Text;
                        selectedRow.BranchID = txtBranch;
                        if (EditPhone.Text != "")
                            selectedRow.Phone = EditPhone.Text;
                        else
                            selectedRow.Phone = null; 
                    }
                    else
                    {
                        foreach (dealership dataRow in selectedRows)
                        {
                            dealership selectedRow = context.Dealership.Find(dataRow.Name);

                            int txtBranch = int.Parse(EditBranch.Text);

                            if (EditLocation.Text != "" && EditLocation.Text != null)
                                selectedRow.Location = EditLocation.Text;

                            if (EditBranch.Text != "" && EditBranch.Text != null)
                                selectedRow.BranchID = txtBranch;

                            if (EditPhone.Text != "" && EditPhone.Text != null)
                                selectedRow.Phone = EditPhone.Text;
                            else
                                selectedRow.Phone = null;
                        }
                    }

                    //Update DataGrid to show changes
                    context.SaveChanges();
                    DealershipsDataGrid.ItemsSource = context.Dealership.ToList();
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
                EditName.Text = "";
                EditLocation.Text = "";
                EditBranch.Text = "";
                EditPhone.Text = "";

                //Set the Inputs back to normal
                EditName.IsHitTestVisible = true;
                EditName.Foreground = Brushes.Black;
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
            EditName.Text = "";
            EditLocation.Text = "";
            EditBranch.Text = "";
            EditPhone.Text = "";

            //Set the Inputs back to normal
            EditName.IsHitTestVisible = true;
            EditName.Foreground = Brushes.Black;
        }
        #endregion

        #region Delete Section

        //Button Click Handler - Sets up the rows for deletion
        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (dealership rowData in DealershipsDataGrid.Items)
            {
                if (rowData.IsSelected)
                {
                    selectedRows.Add(rowData);
                }
            }

            Button removeButton = (Button)sender;
            dealership row = (dealership)removeButton.DataContext;

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
                using (var context = new DealershipsDbContext())
                {
                    if (selectedRows.Count == 0)
                    {
                        throw new Exception("Row was not selected or something went wrong");
                    }
                    else if (selectedRows.Count == 1)
                    {
                        dealership selectedRow = context.Dealership.Find(selectedRows[0].Name);
                        context.Dealership.Remove(selectedRow);
                    }
                    else
                    {
                        foreach (dealership dataRow in selectedRows)
                        {
                            dealership selectedRow = context.Dealership.Find(dataRow.Name);
                            context.Entry(selectedRow).State = EntityState.Deleted;
                        }
                    }

                    //Update DataGrid to show changes
                    context.SaveChanges();
                    DealershipsDataGrid.ItemsSource = context.Dealership.ToList();
                }

                //Clear Selection
                selectedRows.Clear();

                //Update Counter
                RowCount = DealershipsDataGrid.Items.Count;
                DealershipsCounter.Text = $"Current Saved Dealerships: {RowCount}";

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
                using (var context = new DealershipsDbContext())
                {
                    int txtBranch = int.Parse(AddBranch.Text);

                    string txtPhone;
                    if (AddPhone.Text != "")
                    {
                        txtPhone = AddPhone.Text;
                    }
                    else
                    {
                        txtPhone = null;
                    }

                    dealership newDealership = new dealership
                    {
                        Name = AddName.Text,
                        Location = AddLocation.Text,
                        BranchID = txtBranch,
                        Phone = txtPhone
                    };

                    context.Dealership.Add(newDealership);
                    context.SaveChanges();

                    DealershipsDataGrid.ItemsSource = context.Dealership.ToList();
                }

                //Update Counter
                RowCount = DealershipsDataGrid.Items.Count;
                DealershipsCounter.Text = $"Current Saved Dealerships: {RowCount}";

                //Disable scrimming
                MainContentWindow.Opacity = 1;
                MainContentWindow.Background = Brushes.Transparent;

                //Enable Controls
                EnableControls();

                //Close Popup
                AddPopup.IsOpen = false;

                //Set text back to empty
                AddName.Text = "";
                AddLocation.Text = "";
                AddBranch.Text = "";
                AddPhone.Text = "";
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
            AddName.Text = "";
            AddLocation.Text = "";
            AddBranch.Text = "";
            AddPhone.Text = "";
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

        //Dynamic search binded to TextChanged
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtSearchDealerships.Text != "Search in Dealerships...")
            {
                string searchText = txtSearchDealerships.Text.ToLower();

                DealershipsDataGrid.Items.Filter = item =>
                {
                    if (item is dealership dataItem)
                    {
                        //Nullable values handling
                        string txtPhone;
                        if (dataItem.Phone != null)
                        {
                            txtPhone = dataItem.Phone.ToString();
                        }
                        else
                        {
                            txtPhone = "pnull";
                        }

                        //Normal no string values handling
                        string txtBranch = dataItem.BranchID.ToString();

                        return dataItem.Name.ToLower().Contains(searchText) ||
                               dataItem.Location.ToLower().Contains(searchText) ||
                               txtBranch.Contains(searchText) ||
                               txtPhone.ToLower().Contains(searchText);
                    }

                    return false;
                };

                DealershipsDataGrid.Items.Refresh();
            }
        }

        #endregion

        #region Focus Settings

        private void DealershipsWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (txtSearchDealerships.Text == "")
            {
                Keyboard.ClearFocus();
                txtSearchDealerships.Text = "Search in Dealerships...";
            }
            else
            {
                Keyboard.ClearFocus();
            }

        }

        private void SearchDealerships_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (txtSearchDealerships.Text == "Search in Dealerships...")
            {
                txtSearchDealerships.Text = "";
            }
        }
        #endregion

        #region Controls Control

        //Disables all Controls
        private void DisableControls()
        {
            btnBranches.IsHitTestVisible = false;
            btnWarehouses.IsHitTestVisible = false;
            btnAdd.IsHitTestVisible = false;
            txtSearchDealerships.IsHitTestVisible = false;
            DealershipsDataGrid.IsHitTestVisible = false;
        }

        //Enables all Controls
        private void EnableControls()
        {
            btnBranches.IsHitTestVisible = true;
            btnWarehouses.IsHitTestVisible = true;
            btnAdd.IsHitTestVisible = true;
            txtSearchDealerships.IsHitTestVisible = true;
            DealershipsDataGrid.IsHitTestVisible = true;
        }
        #endregion

        #region Comboboxes

        //Set ItemSourcesof ComboBoxes
        private void BranchCombobox_Options()
        {
            using (var context = new DealershipsDbContext())
            {
                var options = context.Branch.ToList();
                AddBranch.ItemsSource = options;
                EditBranch.ItemsSource = options;
            }
        }
        #endregion


        byte accessLevel = UserAccessManager.AccessLevel;
        private DealershipsDbContext _dbContext;
        public DealershipsView()
        {
            InitializeComponent();

            //Database initializer
            _dbContext = new DealershipsDbContext();
            DealershipsDataGrid.ItemsSource = _dbContext.Dealership.ToList();

            //Counter initializer
            RowCount = DealershipsDataGrid.Items.Count;
            DealershipsCounter.Text = $"Current Saved Dealerships: {RowCount}";

            //Setup Comboboxes
            BranchCombobox_Options();

            //Access Level Adjustments
            if (accessLevel <= 2)
            {
                btnAdd.Visibility = Visibility.Collapsed;
                OperationsColumn.Visibility = Visibility.Collapsed;
            }
        }
    }


    /// <summary>
    /// Gets context of "dealership" tab from the connected DataBase
    /// </summary>
    public class DealershipsDbContext : DbContext
    {
        public DbSet<dealership> Dealership { get; set; }
        public DbSet<branch_office> Branch { get; set; }    

        public DealershipsDbContext() : base("DealershipCon")
        {
        }
    }
}
