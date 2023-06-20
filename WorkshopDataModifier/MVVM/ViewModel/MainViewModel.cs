using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using WorkshopDataModifier.Core;

namespace WorkshopDataModifier.MVVM.ViewModel
{
    class MainViewModel : ObservableObject
    {

        //HOME SECTION --------------------------------------------------------------------------

        /// <summary>
        /// Command that is switching the View to HomeView using ViemModels
        /// </summary>
        public RelayCommand HomeViewCommand { get; set; }

        public HomeViewModel HomeVM { get; set; }

        //CUSTOMERS SECTION ---------------------------------------------------------------------

        /// <summary>
        /// Command that is switching the View to CustomersView using ViemModels
        /// </summary>
        public RelayCommand CustomersViewCommand { get; set; }

        public CustomersViewModel CustomersVM { get; set; }

        /// <summary>
        /// Command that is switching the View to PurchasesView using ViemModels
        /// </summary>
        public RelayCommand PurchasesViewCommand { get; set; }

        public PurchasesViewModel PurchasesVM { get; set; }

        //VEHICLES SECTION --------------------------------------------------------------------

        /// <summary>
        /// Command that is switching the View to VehiclesView using ViemModels
        /// </summary>
        public RelayCommand VehiclesViewCommand { get; set; }

        public VehiclesViewModel VehiclesVM { get; set; }

        /// <summary>
        /// Command that is switching the View to SoldVehiclesView using ViemModels
        /// </summary>
        public RelayCommand SoldVehiclesViewCommand { get; set; }

        public SoldVehiclesViewModel SoldVehiclesVM { get; set; }

        /// <summary>
        /// Command that is switching the View to WarehouseVehiclesView using ViemModels
        /// </summary>
        public RelayCommand WarehouseVehiclesViewCommand { get; set; }

        public WarehouseVehiclesViewModel WarehouseVehiclesVM { get; set; }

        
        //LOCATIONS SECTION ------------------------------------------------------------------

        /// <summary>
        /// Command that is switching the View to BranchesView using ViemModels
        /// </summary>
        public RelayCommand BranchesViewCommand { get; set; }

        public BranchesViewModel BranchesVM { get; set; }

        /// <summary>
        /// Command that is switching the View to DealershipsView using ViemModels
        /// </summary>
        public RelayCommand DealershipsViewCommand { get; set; }

        public DealershipsViewModel DealershipsVM { get; set; }

        /// <summary>
        /// Command that is switching the View to WarehousesView using ViemModels
        /// </summary>
        public RelayCommand WarehousesViewCommand { get; set; }

        public WarehousesViewModel WarehousesVM { get; set; }

        //EMPLOYEES SECTION ------------------------------------------------------------------

        /// <summary>
        /// Command that is switching the View to EmployeesView using ViemModels
        /// </summary>
        public RelayCommand EmployeesViewCommand { get; set; }

        public EmployeesViewModel EmployeesVM { get; set; }

        /// <summary>
        /// Command that is switching the View to SalesView using ViemModels
        /// </summary>
        public RelayCommand SalesViewCommand { get; set; }

        public SalesViewModel SalesVM { get; set; }

        //CODES SECTION ----------------------------------------------------------------------

        /// <summary>
        /// Command that is switching the View to BrandsView using ViemModels
        /// </summary>
        public RelayCommand BrandsViewCommand { get; set; }

        public BrandsViewModel BrandsVM { get; set; }

        /// <summary>
        /// Command that is switching the View to PositionsView using ViemModels
        /// </summary>
        public RelayCommand PositionsViewCommand { get; set; }

        public PositionsViewModel PositionsVM { get; set; }

        //USERS SECTION ----------------------------------------------------------------------

        /// <summary>
        /// Command that is switching the View to UsersView using ViemModels
        /// </summary>
        public RelayCommand UsersViewCommand { get; set; }

        public UsersViewModel UsersVM { get; set; }

        //-----------------------------------------------------------------------------------
        

        private object _currentView;
        /// <summary>
        /// Identifier of current selected view
        /// </summary>
        public object CurrentView
        {
            get { return _currentView; }
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }

        public MainViewModel()
        {
            //Home
            HomeVM = new HomeViewModel();

            //Customers
            CustomersVM = new CustomersViewModel();
            PurchasesVM = new PurchasesViewModel();

            //Vehicles
            VehiclesVM = new VehiclesViewModel();
            SoldVehiclesVM = new SoldVehiclesViewModel();
            WarehouseVehiclesVM = new WarehouseVehiclesViewModel();

            //Locations
            BranchesVM = new BranchesViewModel();
            DealershipsVM = new DealershipsViewModel();
            WarehousesVM = new WarehousesViewModel();

            //Employees
            EmployeesVM = new EmployeesViewModel();
            SalesVM = new SalesViewModel();

            //Codes
            BrandsVM = new BrandsViewModel();
            PositionsVM = new PositionsViewModel();

            //Users
            UsersVM = new UsersViewModel();

            
            // Set the current view to HomeView
            CurrentView = HomeVM;

            //Main bindings
            HomeViewCommand = new RelayCommand(o =>
            {
                CurrentView = HomeVM;
            });

            CustomersViewCommand = new RelayCommand(o =>
            {
                CurrentView = CustomersVM;
            });

            VehiclesViewCommand = new RelayCommand(o =>
            {
                CurrentView = VehiclesVM;
            });

            EmployeesViewCommand = new RelayCommand(o =>
            {
                CurrentView = EmployeesVM;
            });

            BranchesViewCommand = new RelayCommand(o =>
            {
                CurrentView = BranchesVM;
            });

            BrandsViewCommand = new RelayCommand(o =>
            {
                CurrentView = BrandsVM;
            });

            UsersViewCommand = new RelayCommand(o =>
            {
                CurrentView = UsersVM;
            });


            //Inside Bindings
            PurchasesViewCommand = new RelayCommand(o =>
            {
                CurrentView = PurchasesVM;
            });

            SoldVehiclesViewCommand = new RelayCommand(o =>
            {
                CurrentView = SoldVehiclesVM;
            });

            WarehouseVehiclesViewCommand = new RelayCommand(o =>
            {
                CurrentView = WarehouseVehiclesVM;
            });

            DealershipsViewCommand = new RelayCommand(o =>
            {
                CurrentView = DealershipsVM;
            });

            WarehousesViewCommand = new RelayCommand(o =>
            {
                CurrentView = WarehousesVM;
            });

            SalesViewCommand = new RelayCommand(o =>
            {
                CurrentView = SalesVM;
            });

            PositionsViewCommand = new RelayCommand(o =>
            {
                CurrentView = PositionsVM;
            });
        }
    }
}
