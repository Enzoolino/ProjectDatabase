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

        public RelayCommand HomeViewCommand { get; set; }
        public RelayCommand CustomersViewCommand { get; set; }
        public RelayCommand VehiclesViewCommand { get; set; }
        public RelayCommand TasksViewCommand { get; set; }
        public RelayCommand LocationsViewCommand { get; set; }
        public RelayCommand EmployeesViewCommand { get; set; }
        public RelayCommand CodesViewCommand { get; set; }
        public RelayCommand UsersViewCommand { get; set; }

        //Inside Commands
        public RelayCommand PurchasesViewCommand { get; set; }
        public RelayCommand SoldVehiclesViewCommand { get; set; }
        public RelayCommand WarehouseVehiclesViewCommand { get; set; }
        public RelayCommand DealershipsViewCommand { get; set; }
        public RelayCommand WarehousesViewCommand { get; set; }
        public RelayCommand SalesViewCommand { get; set; }
        public RelayCommand PositionsViewCommand { get; set; }




        public HomeViewModel HomeVM { get; set; }
        public CustomersViewModel CustomersVM { get; set; }
        public VehiclesViewModel VehiclesVM { get; set; }
        public TasksViewModel TasksVM { get; set; }
        public EmployeesViewModel EmployeesVM { get; set; }
        public LocationsViewModel LocationsVM { get; set; }
        public CodesViewModel CodesVM { get; set; }
        public UsersViewModel UsersVM { get; set; }

        //Inside ViewModels
        public PurchasesViewModel PurchasesVM { get; set; }
        public SoldVehiclesViewModel SoldVehiclesVM { get; set; }
        public WarehouseVehiclesViewModel WarehouseVehiclesVM { get; set; }
        public DealershipsViewModel DealershipsVM { get; set; }
        public WarehousesViewModel WarehousesVM { get; set; }
        public SalesViewModel SalesVM { get; set; }
        public PositionsViewModel PositionsVM { get; set; }


        private object _currentView;

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
            HomeVM = new HomeViewModel();
            CustomersVM = new CustomersViewModel();
            VehiclesVM = new VehiclesViewModel();
            TasksVM = new TasksViewModel();
            EmployeesVM = new EmployeesViewModel();
            LocationsVM = new LocationsViewModel();
            CodesVM = new CodesViewModel();
            UsersVM = new UsersViewModel();

            //Inside Declarations
            PurchasesVM = new PurchasesViewModel();
            SoldVehiclesVM = new SoldVehiclesViewModel();
            WarehouseVehiclesVM = new WarehouseVehiclesViewModel();
            DealershipsVM = new DealershipsViewModel();
            WarehousesVM = new WarehousesViewModel();   
            SalesVM = new SalesViewModel();
            PositionsVM = new PositionsViewModel();
            

            CurrentView = HomeVM;

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

            TasksViewCommand = new RelayCommand(o =>
            {
                CurrentView = TasksVM;
            });

            EmployeesViewCommand = new RelayCommand(o =>
            {
                CurrentView = EmployeesVM;
            });

            LocationsViewCommand = new RelayCommand(o =>
            {
                CurrentView = LocationsVM;
            });

            CodesViewCommand = new RelayCommand(o =>
            {
                CurrentView = CodesVM;
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
