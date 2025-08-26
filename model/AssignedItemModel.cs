using System;

namespace WpfApp1.model
{
    public class AssignedItemModel
    {
        // Genel kimlik (in-memory testlerde kullanılıyor)
        public int Id { get; set; }

        // Asıl alanlar
        public int AssignmentId { get; set; }
        public int ProductId { get; set; }
        public int EmployeeId { get; set; }

        // İlişkili isimler
        public string EmployeeName { get; set; }
        public string ProductName { get; set; }
        public string Status { get; set; }

        // İlgili ek alanlar
        public string CategoryName { get; set; }
        public string Name { get; set; }   // Bazı view'lerde Name olarak geçiyor
        public string Brand { get; set; }
        public string Model { get; set; }
        public string Barcode { get; set; }
        public string SerialNumber { get; set; }

        public DateTime? AssignedAt { get; set; }
        public DateTime? UnassignedAt { get; set; }
        public bool IsCanceled { get; set; }

        public int StatusId { get; set; }
        public string StatusName { get; set; }

        public int LocationId { get; set; }
        public string LocationName { get; set; }

        public bool IsActive { get; set; }
        public decimal? Price { get; set; }
    }
}
