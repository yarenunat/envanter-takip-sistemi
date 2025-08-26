using System.Collections.ObjectModel;
using System.Linq;
using WpfApp1.model;
using WpfApp1.Repositories;

namespace WpfApp1.viewModel
{
    public class ProductViewModel : ViewModelBase
    {
        private readonly IProductRepository _productRepository;

        private ObservableCollection<ProductModel> _products;
        public ObservableCollection<ProductModel> Products
        {
            get => _products;
            set { _products = value; OnPropertyChanged(nameof(Products)); }
        }

        private ProductModel _selectedProduct;
        public ProductModel SelectedProduct
        {
            get => _selectedProduct;
            set { _selectedProduct = value; OnPropertyChanged(nameof(SelectedProduct)); }
        }

        public ProductViewModel()
        {
            _productRepository = new ProductRepository();
            LoadProducts();
        }

        public void LoadProducts()
        {
            var productList = _productRepository.GetAll();
            Products = new ObservableCollection<ProductModel>(productList);
            SelectedProduct = Products.FirstOrDefault();
        }

        public void SearchByBarcode(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                LoadProducts();
                return;
            }

            query = query.Trim().ToLower();

            var filteredList = _productRepository
                .GetAll()
                .Where(p =>
                    !string.IsNullOrWhiteSpace(p.Name) &&
                    p.Name.ToLower().Contains(query))
                .ToList();

            Products = new ObservableCollection<ProductModel>(filteredList);
            SelectedProduct = Products.FirstOrDefault();
        }
    }
}
