//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Umamido.DL
{
    using System;
    using System.Collections.Generic;
    
    public partial class RestaurantTitle
    {
        public int RestaurantTitleId { get; set; }
        public int RestaurantId { get; set; }
        public int LangId { get; set; }
        public string Text { get; set; }
    
        public virtual Lang Lang { get; set; }
        public virtual Restaurant Restaurant { get; set; }
    }
}
