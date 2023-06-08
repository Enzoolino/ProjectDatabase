using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Data.Entity;
using System.Globalization;
using System.ComponentModel;
using System.Windows.Input;
using WorkshopDataModifier.Core;
using System.Data.Entity.Validation;
using System.Text;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Windows.Controls.Primitives;

namespace WorkshopDataModifier.MVVM.View
{
    /// <summary>
    /// Interaction logic for ClientsView.xaml
    /// </summary>
    public partial class ClientsView : UserControl, INotifyPropertyChanged
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

            foreach (klienci row in ClientsDataGrid.Items)
            {
                row.IsSelected = isChecked;
            }
        }

        private void RowCheckBox_Click(object sender, RoutedEventArgs e)
        {
            DataGridRow row = (DataGridRow)ClientsDataGrid.ItemContainerGenerator.ContainerFromItem(((FrameworkElement)sender).DataContext);
            if (row != null)
            {
                klienci rowData = (klienci)row.Item;
                rowData.IsSelected = ((CheckBox)sender).IsChecked == true;
            }
        }
        #endregion

        #region Data Modification

        //List of all selected rows (Initialized with button click)
        static List<klienci> selectedRows = new List<klienci>(); 
        
        #region Edit Section
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (klienci rowData in ClientsDataGrid.Items)
            {
                if (rowData.IsSelected)
                {
                    selectedRows.Add(rowData);
                }
            }

            Button editButton = (Button)sender;
            klienci row = (klienci)editButton.DataContext;

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

                EditName.Text = row.imie;
                EditSurname.Text = row.nazwisko;
                EditAddress.Text = row.adres;
                EditPesel.Text = row.PESEL;

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
                using (var context = new ClientsDbContext())
                {
                    if (selectedRows.Count == 0)
                    {
                        throw new Exception("Row was not selected or something went wrong");
                    }
                    else if (selectedRows.Count == 1)
                    {
                        klienci selectedRow = context.klienci.Find(selectedRows[0].idKlienta);

                        selectedRow.imie = EditName.Text;
                        selectedRow.nazwisko = EditSurname.Text;
                        selectedRow.adres = EditAddress.Text;
                        selectedRow.PESEL = EditPesel.Text;
                    }
                    else
                    {
                        foreach (klienci dataRow in selectedRows)
                        {
                            klienci selectedRow = context.klienci.Find(dataRow.idKlienta);

                            if (EditName.Text != "" && EditName.Text != null)
                            selectedRow.imie = EditName.Text;

                            if (EditSurname.Text != "" && EditSurname.Text != null)
                            selectedRow.nazwisko = EditSurname.Text;

                            if (EditAddress.Text != "" && EditAddress.Text != null)
                                selectedRow.adres = EditAddress.Text;

                            if (EditPesel.Text != "" && EditPesel.Text != null)
                                selectedRow.PESEL = EditPesel.Text;
                        }
                    }

                    context.SaveChanges(); 
                    ClientsDataGrid.ItemsSource = context.klienci.ToList();
                }

                EditPopup.IsOpen = false;

                EditName.Text = "";
                EditSurname.Text = "";
                EditAddress.Text = "";
                EditPesel.Text = "";

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
            EditAddress.Text = "";
            EditPesel.Text = "";

            selectedRows.Clear();

            EditPopup.IsOpen = false;
        }
        #endregion

        #region Delete Section
        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (klienci rowData in ClientsDataGrid.Items)
            {
                if (rowData.IsSelected)
                {
                    selectedRows.Add(rowData);
                }
            }

            Button removeButton = (Button)sender;
            klienci row = (klienci)removeButton.DataContext;

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
                using (var context = new ClientsDbContext())
                {
                    if (selectedRows.Count == 0)
                    {
                        throw new Exception("Row was not selected or something went wrong");
                    }
                    else if (selectedRows.Count == 1)
                    {
                        klienci selectedRow = context.klienci.Find(selectedRows[0].idKlienta);
                        context.klienci.Remove(selectedRow);
                    }
                    else
                    {
                        foreach (klienci dataRow in selectedRows)
                        {
                            klienci selectedRow = context.klienci.Find(dataRow.idKlienta);
                            context.klienci.Remove(selectedRow);
                        }
                    }

                    //Update DataGrid to show changes
                    context.SaveChanges();
                    ClientsDataGrid.ItemsSource = context.klienci.ToList();
                }

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
                using (var context = new ClientsDbContext())
                {
                    klienci newClient = new klienci
                    {
                        imie = AddName.Text,
                        nazwisko = AddSurname.Text,
                        adres = AddAddress.Text,
                        PESEL = AddPesel.Text
                    };

                    context.klienci.Add(newClient);
                    context.SaveChanges();

                    ClientsDataGrid.ItemsSource = context.klienci.ToList();
                }

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
            AddAddress.Text = "";
            AddPesel.Text = "";

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


        private ClientsDbContext _dbContext;

        public ClientsView()
        {
            InitializeComponent();

            //Database initializer
            _dbContext = new ClientsDbContext();
            ClientsDataGrid.ItemsSource = _dbContext.klienci.ToList();

            //Counter initializer
            RowCount = ClientsDataGrid.Items.Count;
            ClientsCounter.Text = $"Current Saved Clients: {RowCount}";

        }
    }

    /// <summary>
    /// Gets context of clients tab from the connected DataBase
    /// </summary>
    public class ClientsDbContext : DbContext
    {
        public DbSet<klienci> klienci { get; set; } //DbSet dla tabeli "klienci"

        public ClientsDbContext() : base("ConString")
        {
        }
    }

    //Take initials from name and surname //UNUSED//
    #region
    public class InitialsConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 && values[0] is string imie && values[1] is string nazwisko && !string.IsNullOrEmpty(imie) && !string.IsNullOrEmpty(nazwisko))
            {
                string initials = string.Concat(imie[0], nazwisko[0]);
                return initials.ToUpper();
            }

            return string.Empty;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
    #endregion

    
    
       

    
}
