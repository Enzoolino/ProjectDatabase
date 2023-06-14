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
        public RelayCommand CarsViewCommand { get; set; }
        public RelayCommand TasksViewCommand { get; set; }
        public RelayCommand WorkersViewCommand { get; set; }
        public RelayCommand EngineCodesViewCommand { get; set; }
        public RelayCommand TaskNamesViewCommand { get; set; }
        public RelayCommand UsersViewCommand { get; set; }


        public HomeViewModel HomeVM { get; set; }
        public CustomersViewModel CustomersVM { get; set; }
        public CarsViewModel CarsVM { get; set; }
        public TasksViewModel TasksVM { get; set; }
        public WorkersViewModel WorkersVM { get; set; }
        public EngineCodesViewModel EngineCodesVM { get; set; }
        public TaskNamesViewModel TaskNamesVM { get; set; }
        public UsersViewModel UsersVM { get; set; }

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
            CarsVM = new CarsViewModel();
            TasksVM = new TasksViewModel();
            WorkersVM = new WorkersViewModel();
            EngineCodesVM = new EngineCodesViewModel();
            TaskNamesVM = new TaskNamesViewModel();
            UsersVM = new UsersViewModel();
            

            CurrentView = HomeVM;

            HomeViewCommand = new RelayCommand(o =>
            {
                CurrentView = HomeVM;
            });

            CustomersViewCommand = new RelayCommand(o =>
            {
                CurrentView = CustomersVM;
            });

            CarsViewCommand = new RelayCommand(o =>
            {
                CurrentView = CarsVM;
            });

            TasksViewCommand = new RelayCommand(o =>
            {
                CurrentView = TasksVM;
            });

            WorkersViewCommand = new RelayCommand(o =>
            {
                CurrentView = WorkersVM;
            });

            EngineCodesViewCommand = new RelayCommand(o =>
            {
                CurrentView = EngineCodesVM;
            });

            TaskNamesViewCommand = new RelayCommand(o =>
            {
                CurrentView = TaskNamesVM;
            });

            UsersViewCommand = new RelayCommand(o =>
            {
                CurrentView = UsersVM;
            });

        }
    }
}
