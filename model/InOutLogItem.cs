using System;

public class InOutLogItem
{
    public string OperationType { get; set; }       // Örn: "Zimmet Verildi"
    public string ActionDescription { get; set; }   // Örn: "Laptop XPS 13 - 1234567890123 | Kimden: Ali ➜ Kime: Ayşe"
    public DateTime Time { get; set; }

    // ComboBox veya liste için görsel metin
    public string DisplayText { get; set; }
}
