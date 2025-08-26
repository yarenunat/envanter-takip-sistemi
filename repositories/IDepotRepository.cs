// WpfApp1/Repositories/IDepotRepository.cs
using System.Collections.Generic;
using WpfApp1.model;

namespace WpfApp1.Repositories
{
    public interface IDepotRepository
    {
        IEnumerable<DepotItem> GetDepotItems(string category = null, string status = null);
        IEnumerable<string> GetAllCategories();
        IEnumerable<string> GetStatusNames(); // Yeni eklenen metod
    }
}