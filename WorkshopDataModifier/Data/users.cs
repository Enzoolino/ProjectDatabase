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
    using System.Collections.Generic;
    
    public partial class users
    {
        public long ID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public byte AccessLevel { get; set; }
        public Nullable<long> Owner { get; set; }
        public Nullable<System.DateTime> AddTime { get; set; }
    
        public virtual employee employee { get; set; }
    }
}
