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
    
    public partial class dealership
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public dealership()
        {
            this.vehicles = new ObservableCollection<vehicles>();
            this.purchase = new ObservableCollection<purchase>();
            this.employee = new ObservableCollection<employee>();
        }
    
        public string Name { get; set; }
        public string Location { get; set; }
        public Nullable<int> BranchID { get; set; }
    
        public virtual branch_office branch_office { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ObservableCollection<vehicles> vehicles { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ObservableCollection<purchase> purchase { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ObservableCollection<employee> employee { get; set; }
    }
}