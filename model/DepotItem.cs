using System;

namespace WpfApp1.model
{
    public class DepotItem
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string Barcode { get; set; }
        public string Category { get; set; }
        public string LocationName { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public string StatusName { get; set; }
    }
}