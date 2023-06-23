using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WorkshopDataModifier.MVVM.Model;


namespace WorkshopDataModifier.MVVM.View
{
    /// <summary>
    /// Interaction logic for HomeView.xaml
    /// </summary>
    public partial class HomeView : UserControl
    {
        public HomeView()
        {
            InitializeComponent();

            HomeDate.Text = $"Today is {DateTime.Now.ToString("dd/MM/yyyy")}";

            // Retrieve the number of sold vehicles from the database
            using (var context = new HomeDbContext())
            {
                int numberOfSoldVehicles = context.SoldVehicle.Count();
                int numberOfOpenedDealerships = context.Dealership.Count();

                // Update the Number property of the InfoCard controls
                SoldVehiclesCard.Number = numberOfSoldVehicles.ToString();
                OpenedDealershipsCard.Number = numberOfOpenedDealerships.ToString();
            }

        }

        /// <summary>
        /// Gets context of sold vahicles tab from the connected DataBase
        /// </summary>
        public class HomeDbContext : DbContext
        {
            public DbSet<sold_vehicles> SoldVehicle { get; set; }
            public DbSet<dealership> Dealership { get; set; }
            
            public HomeDbContext() : base("DealershipCon")
            {
            }
        }

    }
}
