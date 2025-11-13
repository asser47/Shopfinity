namespace ECommerce.PL.ViewModels
{
    public class CategoryProductsViewModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string CategoryDescription { get; set; }
        public IEnumerable<ProductViewModel> Products { get; set; }
    }
}