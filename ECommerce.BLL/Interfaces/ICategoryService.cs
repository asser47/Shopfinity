using ECommerce.DAL.Models;
using System.Collections.Generic;

namespace ECommerce.BLL.Interfaces
{
    public interface ICategoryService
    {
        IEnumerable<Category> GetAllCategories();
        Category GetCategoryById(int id);
        void AddCategory(Category category);
        void UpdateCategory(Category category);
        void DeleteCategory(int id);
        IEnumerable<Product> GetProductsByCategory(int categoryId);
    }
}