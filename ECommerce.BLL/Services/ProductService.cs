using ECommerce.BLL.Interfaces;
using ECommerce.DAL.Models;
using ECommerce.DAL.Repositories.Interfaces;

namespace ECommerce.BLL.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<Product> GetAllProducts()
        {
            return _unitOfWork.Products.GetAll();
        }

        public Product GetProductById(int id)
        {
            return _unitOfWork.Products.GetById(id);
        }

        public void AddProduct(Product product)
        {
            _unitOfWork.Products.Add(product);
            _unitOfWork.Complete();
        }

        public void UpdateProduct(Product product)
        {
            _unitOfWork.Products.Update(product);
            _unitOfWork.Complete();
        }

        public void DeleteProduct(int id)
        {
            _unitOfWork.Products.Delete(id);
            _unitOfWork.Complete();
        }

        public IEnumerable<Product> GetProductsByCategory(int categoryId)
        {
            return _unitOfWork.Products.Find(p => p.CategoryId == categoryId);
        }

        // ✅ NEW: Search and Filter Implementation
        public IEnumerable<Product> SearchProducts(
            string searchTerm = null,
            int? categoryId = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            bool? inStock = null,
            string sortBy = null)
        {
            var query = _unitOfWork.Products.GetAll().AsQueryable();

            // Search by name or description
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(searchTerm) ||
                    (p.Description != null && p.Description.ToLower().Contains(searchTerm)));
            }

            // Filter by category
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            // Filter by price range
            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            // Filter by stock availability
            if (inStock.HasValue)
            {
                if (inStock.Value)
                    query = query.Where(p => p.Stock > 0);
                else
                    query = query.Where(p => p.Stock == 0);
            }

            // Sorting
            query = sortBy switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "name_asc" => query.OrderBy(p => p.Name),
                "name_desc" => query.OrderByDescending(p => p.Name),
                "newest" => query.OrderByDescending(p => p.Id),
                _ => query.OrderBy(p => p.Name) // Default sort
            };

            return query.ToList();
        }
    }
}
