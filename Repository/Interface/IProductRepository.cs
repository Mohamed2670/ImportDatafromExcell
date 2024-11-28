using CodeTest.Models;

namespace CodeTest.Repository.Interface
{
    public interface IProductRepository
    {
        Task AddProduct(Product product);
        Task UpdateProduct(Product product);
        Task<Product?> GetProductById(string productId);
        Task<bool> SaveChanges();
        Task<IEnumerable<Product>> GetProductsAsync(string searchTerm);
        Task<PagedResult<Product>> GetPagedProductsAsync(string searchTerm, int page, int pageSize);
        Task<int> GetProductCountAsync(string searchTerm);



    }
}