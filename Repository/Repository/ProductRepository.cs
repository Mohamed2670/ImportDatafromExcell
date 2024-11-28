using CodeTest.Data;
using CodeTest.Models;
using CodeTest.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace CodeTest.Repository.Repository
{
    public class ProductRepository(ProductDBContext _context) : IProductRepository
    {
        public async Task AddProduct(Product product)
        {
            await _context.Products.AddAsync(product);
        }
        public async Task<Product?> GetProductById(string productId)
        {
            return await _context.Products.FirstOrDefaultAsync(x => x.PartSku == productId);
        }
        public async Task UpdateProduct(Product product)
        {
            var item = await _context.Products.FirstOrDefaultAsync(p => p.PartSku == product.PartSku);
            if (item == null)
            {
                throw new ArgumentException(nameof(product));
            }
            _context.Entry(item).CurrentValues.SetValues(product);
        }
        public async Task<bool> SaveChanges()
        {
            return await _context.SaveChangesAsync() > 0;
        }
        public async Task<IEnumerable<Product>> GetProductsAsync(string searchTerm)
        {
            return await _context.Products
                .Where(p => string.IsNullOrEmpty(searchTerm) || p.PartSku.Contains(searchTerm))
                .ToListAsync();
        }

        public async Task<PagedResult<Product>> GetPagedProductsAsync(string searchTerm, int page, int pageSize)
        {
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.PartSku.Contains(searchTerm));
            }

            var totalItems = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResult<Product>
            {
                Items = items,
                TotalItems = totalItems,
                PageSize = pageSize,
                CurrentPage = page
            };
        }

        public async Task<int> GetProductCountAsync(string searchTerm)
        {
            return await _context.Products
                .Where(p => string.IsNullOrEmpty(searchTerm) || p.PartSku.Contains(searchTerm))
                .CountAsync();
        }

    }
}