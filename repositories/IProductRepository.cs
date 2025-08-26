using System.Collections.Generic;
using System.Threading.Tasks;
using WpfApp1.model;

namespace WpfApp1.Repositories
{
    public interface IProductRepository
    {
        // Async
        Task<IEnumerable<ProductModel>> GetAllAsync();
        Task<ProductModel> AddAsync(ProductModel product);
        Task<bool> RemoveAsync(int id);
        Task<ProductModel> GetByBarcodeAsync(string barcode);
       


        // Sync
        IEnumerable<ProductModel> GetAll();
        ProductModel GetById(int id);
        ProductModel GetByBarcode(string barcode);
        ProductModel GetProductByBarcode(string barcode);

        // CRUD (product0 şemasına göre ileride implemente edilecek)
        void Add(ProductModel product);
        void Update(ProductModel product);
        void Delete(int id);
    }
}
