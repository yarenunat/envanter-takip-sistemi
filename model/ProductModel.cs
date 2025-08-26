using System;

namespace WpfApp1.model
{
    public class ProductModel
    {
        public int Id { get; set; }
        public string Barcode { get; set; }             // Barkod
        public string Name { get; set; }                // Ürün adı
        public string Brand { get; set; }               // Marka
        public string Status { get; set; }              // Ürün Durumu (Mevcut, Zimmetli, Hurda vb.)

        // Opsiyonel/eski alanlar
        public string Description { get; set; }         // Açıklama (opsiyonel)
        public decimal? Price { get; set; }             // Fiyat (opsiyonel)
        public int Stock { get; set; }                  // Stok (opsiyonel)
        public DateTime? PurchaseDate { get; set; }
        public DateTime CreatedDate { get; set; }           // Eklenme/PurchaseDate

        // Yeni alanlar (v_product0_list ile geliyor)
        public string CategoryName { get; set; }        // Kategori
        public string AssignedTo { get; set; }          // Zimmetli kişi (varsa)
        public string AssignedDepartment { get; set; }  // Zimmetli kişinin departmanı (varsa)
        public DateTime? AssignedAt { get; set; }       // Zimmet tarihi (varsa)

        public string DisplayName => $"{Name} - {Barcode}";
    }
}