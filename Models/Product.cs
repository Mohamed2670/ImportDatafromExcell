using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace CodeTest.Models
{
    public class Product
    {
        [Key]
        public required string PartSku { get; set; }
        public required string BandNumber { get; set; }
        public required string CategoryCode { get; set; }
        public required string Manufacturer { get; set; }
        public required string ItemDescription { get; set; }
        public required decimal ListPrice { get; set; }
        public required decimal MinDiscount { get; set; }
        public required decimal DiscountPrice { get; set; }
    }
    public class PagedResult<T>
    {
        public IEnumerable<T>? Items { get; set; }
        public int TotalItems { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }

        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
    }
}

