﻿using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Globalization;
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
    /// Interaction logic for WorkersView.xaml
    /// </summary>
    public partial class EmployeesView : UserControl
    {
        #region Counter of the current clients (dynamic)

        private int _rowCount;
        /// <summary>
        /// Counts number of items inside "employee" table
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

            foreach (employee row in EmployeesDataGrid.Items)
            {
                row.IsSelected = isChecked;
            }
        }

        private void RowCheckBox_Click(object sender, RoutedEventArgs e)
        {
            DataGridRow row = (DataGridRow)EmployeesDataGrid.ItemContainerGenerator.ContainerFromItem(((FrameworkElement)sender).DataContext);
            if (row != null)
            {
                employee rowData = (employee)row.Item;
                rowData.IsSelected = ((CheckBox)sender).IsChecked == true;
            }
        }
        #endregion

        #region Data Modification

        //List of all selected rows (Initialized with button click)
        static List<employee> selectedRows = new List<employee>();

        #region Edit Section

        //Button Click Handler - Sets up the rows for editing
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (employee rowData in EmployeesDataGrid.Items)
            {
                if (rowData.IsSelected)
                {
                    selectedRows.Add(rowData);
                }
            }

            Button editButton = (Button)sender;
            employee row = (employee)editButton.DataContext;

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

                DateTime? dataTime = row.EmployedDate;

                string date = dataTime?.ToString("dd/MM/yyyy") ?? "No date";
                string time = dataTime?.ToString("h:mm tt") ?? "No time";

                EditName.Text = row.Name;
                EditSurname.Text = row.Surname;
                EditSuperior.Text = row.SuperiorID.ToString();
                EditBranch.Text = row.BranchID.ToString();
                EditLocation.Text = row.WorkLocation;
                EditPosition.Text = row.Position;
                EditEmployDate.Text = date;
                EditEmployTime.Text = time;

                EditPopup.IsOpen = true;
            }
            else
            {
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
                using (var context = new EmployeesDbContext())
                {
                    if (selectedRows.Count == 0)
                    {
                        throw new Exception("Row was not selected or something went wrong");
                    }
                    else if (selectedRows.Count == 1)
                    {
                        employee selectedRow = context.Employee.Find(selectedRows[0].EmpID);

                        long? txtSuperior;
                        if (EditSuperior.Text != "")
                        {
                            txtSuperior = long.Parse(EditSuperior.Text);
                        }
                        else
                        {
                            txtSuperior = null;
                        }

                        int txtBranch = int.Parse(EditBranch.Text);
                        DateTime txtEmployDate = DateTime.Parse(EditEmployDate.Text + " " + EditEmployTime.Text);

                        selectedRow.Name = EditName.Text;
                        selectedRow.Surname = EditSurname.Text;
                        selectedRow.SuperiorID = txtSuperior;
                        selectedRow.BranchID = txtBranch;

                        if (EditLocation.Text != "")
                            selectedRow.WorkLocation = EditLocation.Text;
                        else
                            selectedRow.WorkLocation = null;

                        selectedRow.Position = EditPosition.Text;
                        selectedRow.EmployedDate = txtEmployDate;
                    }
                    else
                    {
                        foreach (employee dataRow in selectedRows)
                        {
                            employee selectedRow = context.Employee.Find(dataRow.EmpID);

                            if (EditName.Text != "" && EditName.Text != null)
                                selectedRow.Name = EditName.Text;

                            if (EditSurname.Text != "" && EditSurname.Text != null)
                                selectedRow.Surname = EditSurname.Text;

                            if (EditSuperior.Text != "" && EditSuperior.Text != null && long.TryParse(EditSuperior.Text, out long txtSuperior))
                                selectedRow.SuperiorID = txtSuperior;
                            else
                                selectedRow.SuperiorID = null;

                            if (EditBranch.Text != "" && EditBranch.Text != null && int.TryParse(EditBranch.Text, out int txtBranch))
                                selectedRow.BranchID = txtBranch;

                            if (EditLocation.Text != "" && EditLocation.Text != null)
                                selectedRow.WorkLocation = EditLocation.Text;
                            else
                                selectedRow.WorkLocation = null;

                            if (EditPosition.Text != "" && EditPosition.Text != null)
                                selectedRow.Position = EditPosition.Text;

                            if (EditEmployDate.Text != "" && EditEmployDate.Text != null)
                            {
                                if (EditEmployTime.Text != "" && EditEmployTime.Text != null)
                                {
                                    DateTime txtEmployDate = DateTime.Parse(EditEmployDate.Text + " " + EditEmployTime.Text);
                                    selectedRow.EmployedDate = txtEmployDate;
                                }
                                else
                                {
                                    DateTime txtEmployDate = DateTime.Parse(EditEmployDate.Text);
                                    selectedRow.EmployedDate = txtEmployDate;
                                }
                            }    
                        }
                    }

                    //Update DataGrid to show changes
                    context.SaveChanges();
                    EmployeesDataGrid.ItemsSource = context.Employee.ToList();
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
                EditSurname.Text = "";
                EditSuperior.Text = "";
                EditBranch.Text = "";
                EditLocation.Text = "";
                EditPosition.Text = "";
                EditEmployDate.Text = "";
                EditEmployTime.Text = "";
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
            EditSurname.Text = "";
            EditSuperior.Text = "";
            EditBranch.Text = "";
            EditLocation.Text = "";
            EditPosition.Text = "";
            EditEmployDate.Text = "";
            EditEmployTime.Text = "";
        }
        #endregion

        #region Delete Section

        //Button Click Handler - Sets up the rows for deletion
        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (employee rowData in EmployeesDataGrid.Items)
            {
                if (rowData.IsSelected)
                {
                    selectedRows.Add(rowData);
                }
            }

            Button removeButton = (Button)sender;
            employee row = (employee)removeButton.DataContext;

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
                using (var context = new EmployeesDbContext())
                {
                    if (selectedRows.Count == 0)
                    {
                        throw new Exception("Row was not selected or something went wrong");
                    }
                    else if (selectedRows.Count == 1)
                    {
                        employee selectedRow = context.Employee.Find(selectedRows[0].EmpID);
                        context.Employee.Remove(selectedRow);
                    }
                    else
                    {
                        foreach (employee dataRow in selectedRows)
                        {
                            employee selectedRow = context.Employee.Find(dataRow.EmpID);
                            context.Entry(selectedRow).State = EntityState.Deleted;
                        }
                    }

                    //Update DataGrid to show changes
                    context.SaveChanges();
                    EmployeesDataGrid.ItemsSource = context.Employee.ToList();

                    //Update Comboboxes
                    Combobox_Options();
                }

                //Clear Selection
                selectedRows.Clear();

                //Update Counter
                RowCount = EmployeesDataGrid.Items.Count;
                EmployeesCounter.Text = $"Current Saved Employees: {RowCount}";

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
                using (var context = new EmployeesDbContext())
                {
                    long? txtSuperior;
                    if (AddSuperior.Text != "")
                    {
                        txtSuperior = long.Parse(AddSuperior.Text);
                    }
                    else
                    {
                        txtSuperior = null;
                    }

                    string txtLocation;
                    if (AddLocation.Text != "")
                    {
                        txtLocation = AddLocation.Text;
                    }
                    else
                    {
                        txtLocation = null;
                    }

                    int txtBranch = int.Parse(AddBranch.Text);
                    DateTime txtEmployDate =  DateTime.Parse(AddEmployDate.Text + " " + AddEmployTime.Text);

                    employee newEmployee = new employee
                    {
                        Name = AddName.Text,
                        Surname = AddSurname.Text,
                        SuperiorID = txtSuperior,
                        BranchID = txtBranch,
                        WorkLocation = txtLocation,
                        Position = AddPosition.Text,
                        EmployedDate = txtEmployDate
                    };

                    context.Employee.Add(newEmployee);
                    context.SaveChanges();

                    EmployeesDataGrid.ItemsSource = context.Employee.ToList();

                    //Update Comboboxes
                    Combobox_Options();
                }

                //Update Counter
                RowCount = EmployeesDataGrid.Items.Count;
                EmployeesCounter.Text = $"Current Saved Employees: {RowCount}";

                //Disable scrimming
                MainContentWindow.Opacity = 1;
                MainContentWindow.Background = Brushes.Transparent;

                //Enable Controls
                EnableControls();

                //Close Popup
                AddPopup.IsOpen = false;

                //Set text back to empty
                AddName.Text = "";
                AddSurname.Text = "";
                AddSuperior.Text = "";
                AddBranch.Text = "";
                AddLocation.Text = "";
                AddPosition.Text = "";
                AddEmployDate.Text = "";
                AddEmployTime.Text = "";
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
            AddSurname.Text = "";
            AddSuperior.Text = "";
            AddBranch.Text = "";
            AddLocation.Text = "";
            AddPosition.Text = "";
            AddEmployDate.Text = "";
            AddEmployTime.Text = "";
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
            if (txtSearchEmployees.Text != "Search in Employees...")
            {
                string searchText = txtSearchEmployees.Text.ToLower();

                EmployeesDataGrid.Items.Filter = item =>
                {
                    if (item is employee dataItem)
                    {
                        //Nullable values handling
                        string txtSuperior;
                        if (dataItem.SuperiorID != null)
                        {
                            txtSuperior = dataItem.SuperiorID.ToString();
                        }
                        else
                        {
                            txtSuperior = "snull";
                        }

                        string txtWorkLocation;
                        if (dataItem.WorkLocation != null)
                        {
                            txtWorkLocation = dataItem.WorkLocation.ToString();
                        }
                        else
                        {
                            txtWorkLocation= "lnull";
                        }
                        
                        //Normal no string values handling
                        string txtBranch = dataItem.BranchID.ToString();
                        string txtEmployedDate = dataItem.EmployedDate.ToString();

                        return dataItem.Name.ToLower().Contains(searchText) ||
                               dataItem.Surname.ToLower().Contains(searchText) ||
                               txtSuperior.ToLower().Contains(searchText) ||
                               txtBranch.ToLower().Contains(searchText) ||
                               txtWorkLocation.ToLower().Contains(searchText) ||
                               dataItem.Position.ToLower().Contains(searchText) ||
                               txtEmployedDate.ToLower().Contains(searchText);
                    }

                    return false;
                };

                EmployeesDataGrid.Items.Refresh();
            }
        }

        #endregion

        #region Focus Settings

        private void EmployeesWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (txtSearchEmployees.Text == "")
            {
                Keyboard.ClearFocus();
                txtSearchEmployees.Text = "Search in Employees...";
            }
            else
            {
                Keyboard.ClearFocus();
            }

        }

        private void SearchEmployees_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (txtSearchEmployees.Text == "Search in Employees...")
            {
                txtSearchEmployees.Text = "";
            }
        }
        #endregion

        #region Controls Control

        //Disables all Controls
        private void DisableControls()
        {
            btnSales.IsHitTestVisible = false;
            btnAdd.IsHitTestVisible = false;
            txtSearchEmployees.IsHitTestVisible = false;
            EmployeesDataGrid.IsHitTestVisible = false;
        }

        //Enables all Controls
        private void EnableControls()
        {
            btnSales.IsHitTestVisible = true;
            btnAdd.IsHitTestVisible = true;
            txtSearchEmployees.IsHitTestVisible = true;
            EmployeesDataGrid.IsHitTestVisible = true;
        }
        #endregion

        #region Comboboxes

        //Set ItemSourcesof ComboBoxes
        private void Combobox_Options()
        {
            using (var context = new EmployeesDbContext())
            {
                var superiorOptions = context.Employee.ToList();
                var branchOptions = context.Branch.ToList();
                var dealershipOptions = context.Dealership.ToList();
                var positionOptions = context.Position.ToList();

                AddSuperior.ItemsSource = superiorOptions;
                AddBranch.ItemsSource = branchOptions;
                AddLocation.ItemsSource = dealershipOptions;
                AddPosition.ItemsSource = positionOptions;

                EditSuperior.ItemsSource = superiorOptions;
                EditBranch.ItemsSource = branchOptions;
                EditLocation.ItemsSource = dealershipOptions;
                EditPosition.ItemsSource = positionOptions;
            }
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

                if (sender == EditEmployTime)
                {
                    if (EditEmployDate.SelectedDate == currentDateTime.Date && selectedTime.Value.TimeOfDay > currentDateTime.TimeOfDay)
                    {
                        EditEmployTime.SelectedTime = currentDateTime;
                    }
                }

                if (sender == AddEmployTime)
                {
                    if (AddEmployDate.SelectedDate == currentDateTime.Date && selectedTime.Value.TimeOfDay > currentDateTime.TimeOfDay)
                    {
                        AddEmployTime.SelectedTime = currentDateTime;
                    }
                }
            }
        }
        #endregion

        private EmployeesDbContext _dbContext;
        /// <summary>
        /// Main constructor of EmployeesView UserControl.
        /// </summary>
        public EmployeesView()
        {
            InitializeComponent();

            //Database initializer
            _dbContext = new EmployeesDbContext();
            EmployeesDataGrid.ItemsSource = _dbContext.Employee.ToList();

            //Counter initializer
            RowCount = EmployeesDataGrid.Items.Count;
            EmployeesCounter.Text = $"Current Saved Employees: {RowCount}";

            //Setup Comboboxes
            Combobox_Options();
        }
    }

    /// <summary>
    /// Represents the context of the employee tab from the connected database.
    /// </summary>
    public class EmployeesDbContext : DbContext
    {
        /// <summary>
        /// Gets or sets the DbSet of employee.
        /// </summary>
        public DbSet<employee> Employee { get; set; }

        /// <summary>
        /// Gets or sets the DbSet of branch_office.
        /// </summary>
        public DbSet<branch_office> Branch { get; set; }

        /// <summary>
        /// Gets or sets the DbSet of dealership.
        /// </summary>
        public DbSet<dealership> Dealership { get; set; }

        /// <summary>
        /// Gets or sets the DbSet of position.
        /// </summary>
        public DbSet<position> Position { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmployeesDbContext"/> class using the specified connection string.
        /// </summary>
        public EmployeesDbContext() : base("DealershipCon")
        {
        }
    }
}
