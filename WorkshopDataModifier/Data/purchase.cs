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

    public partial class purchase : ObservableObject
    {
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
        public long Sin { get; set; }
        public int Vin { get; set; }
        public string Dealership { get; set; }
        public Nullable<System.DateTime> PurchaseTime { get; set; }
    
        public virtual customer customer { get; set; }
        public virtual dealership dealership1 { get; set; }
        public virtual sold_vehicles sold_vehicles { get; set; }
        public virtual sell sell { get; set; }
    }
}
