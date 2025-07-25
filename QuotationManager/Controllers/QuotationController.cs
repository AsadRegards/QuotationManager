using Microsoft.AspNetCore.Mvc;
using QuotationManager.Data;
using QuotationManager.Models.ViewModels;
using QuotationManager.Models;
using QuotationManager.Utilities;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;

namespace QuotationManager.Controllers
{
    public class QuotationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public QuotationController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string referenceFilter, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Quotations.AsQueryable();

            if (!string.IsNullOrWhiteSpace(referenceFilter))
                query = query.Where(q => q.ReferenceNumber.Contains(referenceFilter));

            if (startDate.HasValue)
                query = query.Where(q => q.QuotationDate >= startDate.Value.Date);

            if (endDate.HasValue)
                query = query.Where(q => q.QuotationDate <= endDate.Value.Date);

            var quotations = await query
                .OrderByDescending(q => q.QuotationDate)
                .ToListAsync();

            return View(quotations);
        }


        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Products = await _context.Products.ToListAsync();
            ViewBag.ReferenceNumber = $"SC/Hytera/{DateTime.Now.Day}/{DateTime.Now.Month}/{DateTime.Now.Hour+DateTime.Now.Minute+DateTime.Now.Second}";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Preview([FromForm] CreateQuotationViewModel model)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            if (!ModelState.IsValid || model.Products == null || !model.Products.Any())
                return BadRequest("Invalid data.");

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return Unauthorized();

            var productIds = model.Products.Select(p => p.ProductId).ToList();
            var dbProducts = await _context.Products.Where(p => productIds.Contains(p.Id)).ToListAsync();

            var viewModels = new List<QuotationViewModel>();

            foreach (var p in model.Products)
            {
                var dbProduct = dbProducts.FirstOrDefault(x => x.Id == p.ProductId);
                if (dbProduct == null)
                    continue;

                var imagePath = Path.Combine("wwwroot", dbProduct.ImagePath ?? "FixedAssets/placeholder.jpeg");
                byte[] imageBytes = string.IsNullOrWhiteSpace(dbProduct.ImagePath) 
                    ? Array.Empty<byte>() 
                    : System.IO.File.Exists(imagePath) 
                        ? await System.IO.File.ReadAllBytesAsync(imagePath) 
                        :  Array.Empty<byte>();

                var gstAmount = dbProduct.UnitPrice * p.Quantity * (model.GSTPercentage / 100);
                var total = dbProduct.UnitPrice * p.Quantity + gstAmount;

                viewModels.Add(new QuotationViewModel
                {
                    CustomerName = model.CustomerName,
                    AddressLine1 = model.AddressLine1,
                    AddressLine2 = model.AddressLine2,
                    AddressLine3 = model.AddressLine3,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    ReferenceNumber = model.ReferenceNumber ?? "PREVIEW-" + DateTime.Now.Ticks,
                    Date = model.QuotationDate,
                    ProductName = dbProduct.Name,
                    DetailsHtml = dbProduct.Description,
                    Quantity = p.Quantity,
                    UnitPrice = dbProduct.UnitPrice,
                    GSTPercentage = model.GSTPercentage,
                    GSTValue = gstAmount,
                    Total = total,
                    ImageBytes = imageBytes,
                    SignatureImagePath = user.SignatureImagePath,
                    StampImagePath = user.StampImagePath,
                    Designation = user.Designation,
                    UserName = user.Name,
                    LetterheadImagePath = user.LetterheadPdfPath
                });
            }

            var document = new QuotationDocument(viewModels);

            using var stream = new MemoryStream();
            document.GeneratePdf(stream);
            stream.Position = 0;
            return File(stream.ToArray(), "application/pdf");
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateQuotationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Products = await _context.Products.ToListAsync();
                return View(model);
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            // Step 1: Fetch selected products from DB
            var productIds = model.Products.Select(p => p.ProductId).ToList();
            var dbProducts = await _context.Products.Where(p => productIds.Contains(p.Id)).ToListAsync();

            // Step 2: Prepare Quotation Items
            var items = new List<QuotationItem>();
            decimal gstSubTotal = 0;
            decimal subtotal = 0;

            foreach (var p in model.Products)
            {
                var dbProduct = dbProducts.First(x => x.Id == p.ProductId);
                var gstTotal = (dbProduct.UnitPrice * p.Quantity) * (model.GSTPercentage / 100);
                var lineTotal = (dbProduct.UnitPrice * p.Quantity) + gstTotal;

                items.Add(new QuotationItem
                {
                    ProductId = dbProduct.Id,
                    Quantity = p.Quantity,
                    UnitPrice = dbProduct.UnitPrice,
                    TotalValue = lineTotal
                });

                subtotal += lineTotal;
                gstSubTotal += gstTotal;
            }

            // Step 3: Save Quotation to DB (initially without file path)
            var gstAmount = gstSubTotal;
            var total = subtotal;

            var quotation = new Quotation
            {
                UserId = userId,
                ReferenceNumber = model.ReferenceNumber,
                QuotationDate = model.QuotationDate,
                GSTPercentage = model.GSTPercentage,
                GSTAmount = gstAmount,
                SubTotal = total - gstAmount,
                Total = total,
                QuotationItems = items
            };

            _context.Quotations.Add(quotation);
            await _context.SaveChangesAsync();

            // Step 4: Fetch saved quotation with related data
            var savedQuotation = await _context.Quotations
                .Include(q => q.QuotationItems).ThenInclude(qi => qi.Product)
                .Include(q => q.User)
                .FirstOrDefaultAsync(q => q.Id == quotation.Id);

            // Step 5: Build list of QuotationViewModel for PDF
            var viewModels = new List<QuotationViewModel>();

            foreach (var item in savedQuotation.QuotationItems)
            {
                var imagePath = Path.Combine("wwwroot", item.Product.ImagePath ?? "");
                byte[]? imageBytes = System.IO.File.Exists(imagePath)
                    ? await System.IO.File.ReadAllBytesAsync(imagePath)
                    : null;

                var itemTotal = item.UnitPrice * item.Quantity;
                var gstValue = itemTotal * (savedQuotation.GSTPercentage / 100);
                var totalWithGst = itemTotal + gstValue;

                viewModels.Add(new QuotationViewModel
                {
                    CustomerName = model.CustomerName,
                    AddressLine1 = model.AddressLine1,
                    AddressLine2 = model.AddressLine2,
                    AddressLine3 = model.AddressLine3,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    ReferenceNumber = savedQuotation.ReferenceNumber,
                    Date = savedQuotation.QuotationDate,
                    ProductName = item.Product.Name,
                    DetailsHtml = item.Product.Description,
                    UnitPrice = item.UnitPrice,
                    Quantity = item.Quantity,
                    GSTPercentage = savedQuotation.GSTPercentage,
                    GSTValue = gstValue,
                    Total = totalWithGst,
                    SignatureImagePath = savedQuotation.User.SignatureImagePath,
                    StampImagePath = savedQuotation.User.StampImagePath,
                    LetterheadImagePath = savedQuotation.User.LetterheadPdfPath,
                    UserName = savedQuotation.User.Name,
                    Designation = savedQuotation.User.Designation,
                    ImageBytes = imageBytes
                });
            }

            // Step 6: Generate and Save PDF
            var pdfDoc = new QuotationDocument(viewModels);
            var pdfBytes = pdfDoc.GeneratePdf();

            var fileName = $"quotation-{quotation.Id}-{DateTime.Now:yyyyMMddHHmmss}.pdf";
            var pdfPath = Path.Combine("wwwroot", "quotations", fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(pdfPath));
            await System.IO.File.WriteAllBytesAsync(pdfPath, pdfBytes);

            // Step 7: Update Quotation record with file path
            quotation.QuotationFIlePath = Path.Combine("quotations", fileName);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Quotation created and PDF saved successfully!";
            return RedirectToAction("Index", "Dashboard");
        }

    }
}
