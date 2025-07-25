using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuotationManager.Data;
using System.Security.Claims;

namespace QuotationManager.Controllers
{

    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var totalQuotations = await _context.Quotations
                .CountAsync(q => q.UserId == userId);

            var totalValue = await _context.QuotationItems
                .Where(q => q.Quotation.UserId == userId)
                .SumAsync(q => (decimal?)q.TotalValue) ?? 0;

            var thisMonthQuotations = await _context.Quotations
                .Where(q => q.UserId == userId && q.CreatedAt.Month == DateTime.UtcNow.Month && q.CreatedAt.Year == DateTime.UtcNow.Year)
                .CountAsync();

            var totalProducts = await _context.Products
                .CountAsync(p => p.UserId == userId);

            var model = new
            {
                TotalQuotations = totalQuotations,
                TotalValue = totalValue,
                QuotationsThisMonth = thisMonthQuotations,
                TotalProducts = totalProducts
            };

            return View(model);
        }
    }
}
