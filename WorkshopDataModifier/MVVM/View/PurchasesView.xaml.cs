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
    /// Interaction logic for SalesView.xaml
    /// </summary>
    public partial class PurchasesView : UserControl
    {
        #region Counter of the current purchases (dynamic)
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
                MultiEditionWarning.Visibility = Visibility.Collapsed;

                EditSin.Text = row.Sin.ToString();
                EditVin.Text = row.Vin.ToString();
                EditDealership.Text = row.Dealership;
                
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
                using (var context = new PurchasesDbContext())
                {
                    if (selectedRows.Count == 0)
                    {
                        throw new Exception("Row was not selected or something went wrong");
                    }
                    else if (selectedRows.Count == 1)
                    {
                        purchase selectedRow = context.purchase.Find(selectedRows[0].Sin, selectedRows[0].Vin);

                        long txtSin = long.Parse(EditSin.Text);
                        int txtVin = int.Parse(EditVin.Text);

                        selectedRow.Sin = txtSin;
                        selectedRow.Vin = txtVin;
                        selectedRow.Dealership = EditDealership.Text;
                    }
                    else
                    {
                        foreach (purchase dataRow in selectedRows)
                        {
                            purchase selectedRow = context.purchase.Find(dataRow.Sin, dataRow.Vin);

                            if (EditSin.Text != "" && EditSin.Text != null && long.TryParse(EditSin.Text, out long txtSin))
                                selectedRow.Sin = txtSin;

                            if (EditVin.Text != "" && EditVin.Text != null && int.TryParse(EditVin.Text, out int txtVin))
                                selectedRow.Vin = txtVin;

                            if (EditDealership.Text != "" && EditDealership.Text != null)
                                selectedRow.Dealership = EditDealership.Text;
                        }
                    }

                    context.SaveChanges();
                    PurchasesDataGrid.ItemsSource = context.purchase.ToList();
                }

                EditPopup.IsOpen = false;

                EditSin.Text = "";
                EditVin.Text = "";
                EditDealership.Text = "";

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
            EditSin.Text = "";
            EditDealership.Text = "";
     
            selectedRows.Clear();

            EditPopup.IsOpen = false;
        }
        #endregion

        #region Delete Section
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
        }

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
                        purchase selectedRow = context.purchase.Find(selectedRows[0].Sin, selectedRows[0].Vin);
                        context.purchase.Remove(selectedRow);
                    }
                    else
                    {
                        foreach (purchase dataRow in selectedRows)
                        {
                            purchase selectedRow = context.purchase.Find(dataRow.Sin, dataRow.Vin);
                            context.Entry(selectedRow).State = EntityState.Deleted;
                        }
                    }

                    //Update DataGrid to show changes
                    context.SaveChanges();
                    PurchasesDataGrid.ItemsSource = context.purchase.ToList();
                }

                RowCount = PurchasesDataGrid.Items.Count;
                CustomersCounter.Text = $"Current Saved Clients: {RowCount}";

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
                using (var context = new PurchasesDbContext())
                {
                    long txtSin = long.Parse(AddSin.Text);
                    int txtVin = int.Parse(AddVin.Text);

                    purchase newPurchase = new purchase
                    {
                        Sin = txtSin,
                        Vin = txtVin,
                        Dealership = AddDealership.Text,
                        
                    };

                    context.purchase.Add(newPurchase);
                    context.SaveChanges();

                    PurchasesDataGrid.ItemsSource = context.purchase.ToList();
                }

                RowCount = PurchasesDataGrid.Items.Count;
                CustomersCounter.Text = $"Current Saved Purchases: {RowCount}";
                AddPopup.IsOpen = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while adding the client: {ex.Message}", "Add Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
            }
        }

        private void CancelAddButton_Click(object sender, RoutedEventArgs e)
        {
            AddSin.Text = "";
            AddVin.Text = "";
            AddDealership.Text = "";
            
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

        private void txtSearchPurchases_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (txtSearchPurchases.Text == "Search in Purchases...")
            {
                txtSearchPurchases.Text = "";
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

        private PurchasesDbContext _dbContext;
        public PurchasesView()
        {
            InitializeComponent();

            //Database initializer
            _dbContext = new PurchasesDbContext();
            PurchasesDataGrid.ItemsSource = _dbContext.purchase.ToList();

            //Counter initializer
            RowCount = PurchasesDataGrid.Items.Count;
            CustomersCounter.Text = $"Current Saved Purchases: {RowCount}";
        }
    }

    /// <summary>
    /// Gets context of clients tab from the connected DataBase
    /// </summary>
    public class PurchasesDbContext : DbContext
    {
        public DbSet<purchase> purchase { get; set; } //DbSet dla tabeli "customer"

        public PurchasesDbContext() : base("DealershipCon")
        {
        }
    }
}
