using CodeTest.Models;
using ClosedXML.Excel;
using CodeTest.Repository.Interface;
using System.Diagnostics;
using CodeTest.Data;
using Microsoft.EntityFrameworkCore;
using System.Data;
using ExcelDataReader;

namespace CodeTest.Service
{
    public class ProductService(IProductRepository _productRepository) : IProductService
    {
        public async Task<bool> ImportData(IFormFile file)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            var tasks = new List<Task>();
            var stop = new Stopwatch();
            stop.Start();
            Console.WriteLine("Opening the file");

            using var stream = file.OpenReadStream();
            using var reader = ExcelReaderFactory.CreateReader(stream);
            var dataSet = CreateDataSet(reader);
            int sheetCount = dataSet.Tables.Count;
            Console.WriteLine($"Found {sheetCount} sheets in the file.");
            stop.Stop();
            Console.WriteLine($"File opened in {stop.Elapsed.TotalSeconds:F2} seconds.");
            Console.WriteLine("Starting to save ...");

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var semaphore = new SemaphoreSlim(4);
            int batchSize = 10000;

            for (int sheetIndex = 0; sheetIndex < sheetCount; sheetIndex++)
            {
                var table = dataSet.Tables[sheetIndex];
                Console.WriteLine($"Processing sheet: {table.TableName}");

                int totalRows = table.Rows.Count;

                for (int i = 2; i < totalRows; i += batchSize)
                {
                    int endRow = Math.Min(i + batchSize, totalRows);
                    var batch = table.AsEnumerable().Skip(i).Take(batchSize).ToList();

                    tasks.Add(ProcessBatchAsync(batch, semaphore));
                }
            }

            await Task.WhenAll(tasks);
            stopWatch.Stop();
            Console.WriteLine($"Import completed in {stopWatch.Elapsed.TotalSeconds:F2} seconds.");

            return true;
        }

        private DataSet CreateDataSet(IExcelDataReader reader)
        {
            var config = new ExcelDataSetConfiguration
            {
                ConfigureDataTable = _ => new ExcelDataTableConfiguration
                {
                    UseHeaderRow = true
                }
            };
            return reader.AsDataSet(config);
        }

        private async Task ProcessBatchAsync(List<DataRow> batch, SemaphoreSlim semaphore)
        {
            await semaphore.WaitAsync();
            try
            {
                using (var dbContext = new ProductDBContext())
                {
                    var batchProducts = ParseProducts(batch);
                    var idsToUpdate = batchProducts.Select(p => p.PartSku).ToHashSet();

                    if (batchProducts.Any())
                    {
                        var existingProducts = await dbContext.Products
                            .Where(p => idsToUpdate.Contains(p.PartSku))
                            .ToListAsync();

                        UpdateOrAddProducts(dbContext, batchProducts, existingProducts);
                        await dbContext.SaveChangesAsync();
                    }
                }
            }
            finally
            {
                semaphore.Release();
            }
        }

        private List<Product> ParseProducts(List<DataRow> batch)
        {
            var products = new List<Product>();

            foreach (var row in batch)
            {
                try
                {
                    var bandNumber = row[1]?.ToString();
                    var categoryCode = row[2]?.ToString();
                    var manufacturer = row[3]?.ToString();
                    var partSku = row[4]?.ToString();
                    var itemDescription = row[5]?.ToString();
                    var listPrice = Convert.ToDecimal(row[6]?.ToString().Replace("$", "").Replace(",", ""));
                    var minDiscount = Convert.ToDecimal(row[7]?.ToString().Replace("%", ""));
                    var discountPrice = Convert.ToDecimal(row[8]?.ToString().Replace("$", "").Replace(",", ""));

                    if (string.IsNullOrEmpty(bandNumber) || string.IsNullOrEmpty(categoryCode) || string.IsNullOrEmpty(partSku))
                    {
                        continue;
                    }

                    var product = new Product
                    {
                        BandNumber = bandNumber,
                        CategoryCode = categoryCode,
                        Manufacturer = manufacturer,
                        PartSku = partSku,
                        ItemDescription = itemDescription,
                        ListPrice = listPrice,
                        MinDiscount = minDiscount,
                        DiscountPrice = discountPrice
                    };

                    products.Add(product);
                }
                catch
                {
                    continue;
                }
            }

            return products;
        }

        private void UpdateOrAddProducts(ProductDBContext dbContext, List<Product> batchProducts, List<Product> existingProducts)
        {
            foreach (var product in batchProducts)
            {
                var existingProduct = existingProducts.FirstOrDefault(p => p.PartSku == product.PartSku);
                if (existingProduct != null)
                {
                    existingProduct.BandNumber = product.BandNumber;
                    existingProduct.CategoryCode = product.CategoryCode;
                    existingProduct.Manufacturer = product.Manufacturer;
                    existingProduct.ItemDescription = product.ItemDescription;
                    existingProduct.ListPrice = product.ListPrice;
                    existingProduct.MinDiscount = product.MinDiscount;
                    existingProduct.DiscountPrice = product.DiscountPrice;
                }
                else
                {
                    dbContext.Products.Add(product);
                }
            }
        }


        public async Task<PagedResult<Product>> GetPagedProductsAsync(string searchTerm, int page, int pageSize)
        {
            var pagedResult = await _productRepository.GetPagedProductsAsync(searchTerm, page, pageSize);
            var productViewModels = pagedResult.Items?.Select(p => new Product
            {
                BandNumber = p.BandNumber,
                CategoryCode = p.CategoryCode,
                Manufacturer = p.Manufacturer,
                PartSku = p.PartSku,
                ItemDescription = p.ItemDescription,
                ListPrice = p.ListPrice,
                MinDiscount = p.MinDiscount,
                DiscountPrice = p.DiscountPrice
            });

            return new PagedResult<Product>
            {
                Items = productViewModels,
                TotalItems = pagedResult.TotalItems,
                PageSize = pagedResult.PageSize,
                CurrentPage = pagedResult.CurrentPage
            };
        }

        public async Task<IEnumerable<Product>> GetProductsAsync(string searchTerm)
        {
            var products = await _productRepository.GetProductsAsync(searchTerm);
            return products.Select(p => new Product
            {
                BandNumber = p.BandNumber,
                CategoryCode = p.CategoryCode,
                Manufacturer = p.Manufacturer,
                PartSku = p.PartSku,
                ItemDescription = p.ItemDescription,
                ListPrice = p.ListPrice,
                MinDiscount = p.MinDiscount,
                DiscountPrice = p.DiscountPrice
            });
        }

    }
}