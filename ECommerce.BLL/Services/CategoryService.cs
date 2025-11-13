using ECommerce.BLL.Interfaces;
using ECommerce.DAL.Models;
using ECommerce.DAL.Repositories.Interfaces;

namespace ECommerce.BLL.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<Category> GetAllCategories()
        {
            // ✅ Just get categories - NO eager loading needed!
            return _unitOfWork.Categories.GetAll();
        }

        public Category GetCategoryById(int id)
        {
            // ✅ Just get category - NO eager loading needed!
            return _unitOfWork.Categories.GetById(id);
        }

        public IEnumerable<Product> GetProductsByCategory(int categoryId)
        {
            // ✅ Get products separately by filtering
            return _unitOfWork.Products.Find(p => p.CategoryId == categoryId);
        }

        public void AddCategory(Category category)
        {
            _unitOfWork.Categories.Add(category);
            _unitOfWork.Complete();
        }

        public void UpdateCategory(Category category)
        {
            _unitOfWork.Categories.Update(category);
            _unitOfWork.Complete();
        }

        public void DeleteCategory(int id)
        {
            _unitOfWork.Categories.Delete(id);
            _unitOfWork.Complete();
        }
    }
}