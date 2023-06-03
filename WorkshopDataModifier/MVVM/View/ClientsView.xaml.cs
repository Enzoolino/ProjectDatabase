using System;
using System.Collections.Generic;
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
using System.Data;
using System.Data.SqlClient;
using System.Data.Entity;
using WorkshopDataModifier.MVVM.ViewModel;
using WorkshopDataModifier;
using System.Globalization;

namespace WorkshopDataModifier.MVVM.View
{
    /// <summary>
    /// Interaction logic for ClientsView.xaml
    /// </summary>
    public partial class ClientsView : UserControl
    {

        private ClientsDbContext dbContext;

        public ClientsView()
        {
            InitializeComponent();
            dbContext = new ClientsDbContext();
            ClientsDataGrid.ItemsSource = dbContext.klienci.ToList();
            ClientsCounter.Text = $"Number of Saved Clients: {ClientsDataGrid.Items.Count}";
        }
    }

    public class ClientsDbContext : DbContext
    {
        public DbSet<klienci> klienci { get; set; } //DbSet dla tabeli "klienci"

        public ClientsDbContext() : base("ConString")
        {
            
        }
    }

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

   
}
