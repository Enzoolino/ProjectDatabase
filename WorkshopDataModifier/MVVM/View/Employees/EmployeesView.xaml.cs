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
    /// Interaction logic for WorkersView.xaml
    /// </summary>
    public partial class EmployeesView : UserControl
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

                EditName.Text = row.Name;
                EditSurname.Text = row.Surname;
                EditSuperior.Text = row.SuperiorID.ToString();
                EditBranch.Text = row.BranchID.ToString();
                EditLocation.Text = row.WorkLocation;
                EditPosition.Text = row.Position;
                EditEmployDate.Text = row.EmployedDate.ToString();

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
                using (var context = new EmployeesDbContext())
                {
                    if (selectedRows.Count == 0)
                    {
                        throw new Exception("Row was not selected or something went wrong");
                    }
                    else if (selectedRows.Count == 1)
                    {
                        employee selectedRow = context.Employee.Find(selectedRows[0].EmpID);

                        long txtSuperior = long.Parse(EditSuperior.Text);
                        int txtBranch = int.Parse(EditBranch.Text);
                        DateTime txtEmployDate = DateTime.Parse(EditEmployDate.Text);

                        selectedRow.Name = EditName.Text;
                        selectedRow.Surname = EditSurname.Text;
                        selectedRow.SuperiorID = txtSuperior;
                        selectedRow.BranchID = txtBranch;
                        selectedRow.WorkLocation = EditLocation.Text;
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

                            if (EditBranch.Text != "" && EditBranch.Text != null && int.TryParse(EditBranch.Text, out int txtBranch))
                                selectedRow.BranchID = txtBranch;

                            if (EditLocation.Text != "" && EditLocation.Text != null)
                                selectedRow.WorkLocation = EditLocation.Text;

                            if (EditPosition.Text != "" && EditPosition.Text != null)
                                selectedRow.Position = EditPosition.Text;

                            if (EditEmployDate.Text != "" && EditEmployDate.Text != null && DateTime.TryParse(EditEmployDate.Text, out DateTime txtEmployDate))
                                selectedRow.EmployedDate = txtEmployDate;

                        }
                    }

                    context.SaveChanges();
                    EmployeesDataGrid.ItemsSource = context.Employee.ToList();
                }

                EditPopup.IsOpen = false;

                EditName.Text = "";
                EditSurname.Text = "";
                EditSuperior.Text = "";
                EditBranch.Text = "";
                EditLocation.Text = "";
                EditPosition.Text = "";
                EditEmployDate.Text = "";

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
            EditSurname.Text = "";
            EditSuperior.Text = "";
            EditBranch.Text = "";
            EditLocation.Text = "";
            EditPosition.Text = "";
            EditEmployDate.Text = "";

            selectedRows.Clear();

            EditPopup.IsOpen = false;
        }
        #endregion

        #region Delete Section
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
        }

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
                }

                RowCount = EmployeesDataGrid.Items.Count;
                EmployeesCounter.Text = $"Current Saved Employees: {RowCount}";

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
                using (var context = new EmployeesDbContext())
                {
                    long txtSuperior = long.Parse(AddSuperior.Text);
                    int txtBranch = int.Parse(AddBranch.Text);
                    DateTime txtEmployDate =  DateTime.Parse(AddEmployDate.Text);

                    employee newEmployee = new employee
                    {
                        Name = AddName.Text,
                        Surname = AddSurname.Text,
                        SuperiorID = txtSuperior,
                        BranchID = txtBranch,
                        WorkLocation = AddLocation.Text,
                        Position = AddPosition.Text,
                        EmployedDate = txtEmployDate
                    };

                    context.Employee.Add(newEmployee);
                    context.SaveChanges();

                    EmployeesDataGrid.ItemsSource = context.Employee.ToList();
                }

                RowCount = EmployeesDataGrid.Items.Count;
                EmployeesCounter.Text = $"Current Saved Clients: {RowCount}";
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
            AddSurname.Text = "";
            AddSuperior.Text = "";
            AddBranch.Text = "";
            AddLocation.Text = "";
            AddPosition.Text = "";
            AddEmployDate.Text = "";

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
            if (txtSearchEmployees.Text != "Search in Employees...")
            {

                string searchText = txtSearchEmployees.Text.ToLower();


                EmployeesDataGrid.Items.Filter = item =>
                {
                    if (item is employee dataItem)
                    {
                        string txtSuperior = dataItem.SuperiorID.ToString();
                        string txtBranch = dataItem.BranchID.ToString();
                        string txtEmployedDate = dataItem.EmployedDate.ToString();

                        return dataItem.Name.ToLower().Contains(searchText) ||
                               dataItem.Surname.ToLower().Contains(searchText) ||
                               txtSuperior.Contains(searchText) ||
                               txtBranch.Contains(searchText) ||
                               dataItem.WorkLocation.Contains(searchText) ||
                               dataItem.Position.Contains(searchText) ||
                               txtEmployedDate.Contains(searchText);

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

        private void txtSearchEmployees_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (txtSearchEmployees.Text == "Search in Employees...")
            {
                txtSearchEmployees.Text = "";
            }
        }
        #endregion


        private EmployeesDbContext _dbContext;
        public EmployeesView()
        {
            InitializeComponent();

            //Database initializer
            _dbContext = new EmployeesDbContext();
            EmployeesDataGrid.ItemsSource = _dbContext.Employee.ToList();

            //Counter initializer
            RowCount = EmployeesDataGrid.Items.Count;
            EmployeesCounter.Text = $"Current Saved Employees: {RowCount}";
        }
    }

    /// <summary>
    /// Gets context of employee tab from the connected DataBase
    /// </summary>
    public class EmployeesDbContext : DbContext
    {
        public DbSet<employee> Employee { get; set; } //DbSet dla tabeli "employee"

        public EmployeesDbContext() : base("DealershipCon")
        {
        }
    }
}
