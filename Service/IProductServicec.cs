using CodeTest.Models;

namespace CodeTest.Service
{
    public interface IProductService
    {
        Task<bool> ImportData(IFormFile file);
        Task<PagedResult<Product>> GetPagedProductsAsync(string searchTerm, int page, int pageSize);
        Task<IEnumerable<Product>> GetProductsAsync(string searchTerm);


    }
}