using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CodeTest.Models;
using CodeTest.Service;
using OfficeOpenXml;

namespace CodeTest.Controllers;

public class HomeController(IProductService _productService) : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    [RequestSizeLimit(104857600)] // 100MB

    [HttpPost]
    public async Task<IActionResult> Import(IFormFile file)
    {
        if (file == null)
        {
            Console.WriteLine("sdsda");
            TempData["ErrorMessage"] = "No file selected.";
            return RedirectToAction("Index");
        }

        var success = await _productService.ImportData(file);
        if (success)
        {
            TempData["SuccessMessage"] = "Data imported successfully!";
        }
        else
        {
            TempData["ErrorMessage"] = "Data import failed!";
        }

        return RedirectToAction("Index");
    }

    public IActionResult ImportPage()
    {
        return View();
    }


    public async Task<IActionResult> Search(string searchTerm = "", int page = 1, int pageSize = 10)
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        var pagedResult = await _productService.GetPagedProductsAsync(searchTerm, page, pageSize);
        ViewData["SearchTerm"] = searchTerm;
        stopWatch.Stop();
        Console.WriteLine($"Reading data takes :  {stopWatch.Elapsed.TotalSeconds:F2} seconds.");

        return View(pagedResult);
    }

    [HttpPost]
    public async Task<IActionResult> ExportToExcel(string searchTerm = "")
    {
        var products = await _productService.GetProductsAsync(searchTerm);
        var stream = GenerateExcel(products);
        return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Products.xlsx");
    }

    private MemoryStream GenerateExcel(IEnumerable<Product> products)
    {
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Products");
        worksheet.Cells[1, 1].Value = "Band Number";
        worksheet.Cells[1, 2].Value = "Category Code";
        worksheet.Cells[1, 3].Value = "Manufacturer";
        worksheet.Cells[1, 4].Value = "Part SKU";
        worksheet.Cells[1, 5].Value = "Description";
        worksheet.Cells[1, 6].Value = "List Price";
        worksheet.Cells[1, 7].Value = "Min Discount";
        worksheet.Cells[1, 8].Value = "Discount Price";
        int row = 2;
        foreach (var product in products)
        {
            worksheet.Cells[row, 1].Value = product.BandNumber;
            worksheet.Cells[row, 2].Value = product.CategoryCode;
            worksheet.Cells[row, 3].Value = product.Manufacturer;
            worksheet.Cells[row, 4].Value = product.PartSku;
            worksheet.Cells[row, 5].Value = product.ItemDescription;
            worksheet.Cells[row, 6].Value = product.ListPrice;
            worksheet.Cells[row, 7].Value = product.MinDiscount;
            worksheet.Cells[row, 8].Value = product.DiscountPrice;
            row++;
        }

        var stream = new MemoryStream();
        package.SaveAs(stream);
        stream.Position = 0;

        return stream;
    }




}
