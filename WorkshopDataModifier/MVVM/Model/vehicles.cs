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

    public partial class vehicles : ObservableObject
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

        public int Vin { get; set; }
        public string Brand { get; set; }
        public string Color { get; set; }
        public Nullable<int> Year { get; set; }
        public string Model { get; set; }
        public Nullable<byte> Door { get; set; }
        public Nullable<decimal> Price { get; set; }
        public string Dealership { get; set; }
        public Nullable<System.DateTime> DeliveryTime { get; set; }
    
        public virtual brands brands { get; set; }
        public virtual dealership dealership1 { get; set; }
    }
}