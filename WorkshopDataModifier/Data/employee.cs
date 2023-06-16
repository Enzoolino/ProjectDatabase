//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WorkshopDataModifier.Data
{
    using System;
    using System.Collections.ObjectModel;
    using WorkshopDataModifier.Core;

    public partial class employee : ObservableObject
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public employee()
        {
            this.users = new ObservableCollection<users>();
            this.sell = new ObservableCollection<sell>();
            this.employee1 = new ObservableCollection<employee>();
        }

        private bool _isSelected;

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }

        public long EmpID { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public Nullable<long> SuperiorID { get; set; }
        public Nullable<int> BranchID { get; set; }
        public string WorkLocation { get; set; }
        public string Position { get; set; }
        public Nullable<System.DateTime> EmployedDate { get; set; }
    
        public virtual branch_office branch_office { get; set; }
        public virtual dealership dealership { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ObservableCollection<users> users { get; set; }
        public virtual position position1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ObservableCollection<sell> sell { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ObservableCollection<employee> employee1 { get; set; }
        public virtual employee employee2 { get; set; }
    }
}
