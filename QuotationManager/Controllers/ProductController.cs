using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuotationManager.Data;
using QuotationManager.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text.Json;
using QuotationManager.Models.ViewModels;
using QuotationManager.Utilities;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

[Authorize]
public class ProductController : Controller
{
    private readonly ApplicationDbContext _context;

    public ProductController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Product/Index
    public async Task<IActionResult> Index()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var products = await _context.Products.Include(p => p.User).ToListAsync();

        return View(products);
    }

    // POST: Product/Edit
    [HttpPost]
    public async Task<IActionResult> Edit(Product model, IFormFile? Image)
    {
        var product = await _context.Products.FindAsync(model.Id);
        if (product == null) return NotFound();

        product.Name = model.Name;
        product.UnitPrice = model.UnitPrice;
        product.Description = model.Description;

        if (Image != null)
        {
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            Directory.CreateDirectory(uploadsFolder);

            string imageFileName = Guid.NewGuid() + Path.GetExtension(Image.FileName);
            string filePath = Path.Combine(uploadsFolder, imageFileName);
            using var stream = new FileStream(filePath, FileMode.Create);
            await Image.CopyToAsync(stream);
            product.ImagePath = Path.Combine("uploads", imageFileName);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    // POST: Product/Delete
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("Index");
    }


    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(AddProductViewModel model)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        if (!ModelState.IsValid)
            return View(model);

        string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        Directory.CreateDirectory(uploadsFolder);

        string imagePath = null;
        if (model.Image != null)
        {
            string imageFileName = Guid.NewGuid() + Path.GetExtension(model.Image.FileName);
            string filePath = Path.Combine(uploadsFolder, imageFileName);
            using var stream = new FileStream(filePath, FileMode.Create);
            await model.Image.CopyToAsync(stream);
            imagePath = Path.Combine("uploads", imageFileName);
        }

        var product = new Product
        {
            Name = model.Name,
            UnitPrice = model.UnitPrice,
            Description = RefineProductDetails(model.ProductDetailsHtml),
            ImagePath = imagePath,
            UserId = userId
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Product saved successfully!";
        return RedirectToAction("Index","Dashboard");
    }

    [HttpPost]
    public async Task<IActionResult> Preview(AddProductViewModel model)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        // Basic checks
        if (model == null ||
            string.IsNullOrWhiteSpace(model.Name) ||
            model.UnitPrice <= 0 ||
            string.IsNullOrWhiteSpace(model.ProductDetailsHtml) ||
            model.Image == null || model.Image.Length == 0)
        {
            return BadRequest("Incomplete data");
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return Unauthorized();

        // ✅ Read image into memory directly (no file save)
        byte[] imageBytes;
        using (var ms = new MemoryStream())
        {
            await model.Image.CopyToAsync(ms);
            imageBytes = ms.ToArray();
        }

        var qModel = new QuotationViewModel
        {
            ProductName = model.Name,
            DetailsHtml = RefineProductDetails(model.ProductDetailsHtml),
            UnitPrice = model.UnitPrice,
            ImageBytes = imageBytes,
            LetterheadImagePath = user.LetterheadPdfPath ?? Path.Combine("wwwroot", "FixedAssets", "default-letterhead.jpg"),
            SignatureImagePath = user.SignatureImagePath,
            StampImagePath = user.StampImagePath,
            ReferenceNumber = "12345679316454"
        };
        var qvmList = new List<QuotationViewModel>() { qModel };
        var document = new QuotationDocument(qvmList);

        using var stream = new MemoryStream();
        document.GeneratePdf(stream);
        stream.Position = 0;

        return File(stream.ToArray(), "application/pdf");
    }

    string RefineProductDetails(string details) {

        return details;
    }
}