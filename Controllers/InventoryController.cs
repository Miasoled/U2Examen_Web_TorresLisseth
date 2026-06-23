using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NorthwindApp.Data;
using NorthwindApp.Models;

namespace NorthwindApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class InventoryController : Controller
    {
        private readonly NorthwindContext _context;

        public InventoryController(NorthwindContext context)
        {
            _context = context;
        }

        // GET: Inventory - Ver todos los productos con stock
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .OrderBy(p => p.ProductName)
                .ToListAsync();
            return View(products);
        }

        // GET: Inventory/LowStock - Productos con bajo stock
        public async Task<IActionResult> LowStock()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.UnitsInStock <= p.ReorderLevel)
                .OrderBy(p => p.UnitsInStock)
                .ToListAsync();
            return View(products);
        }

        // POST: Inventory/IncreaseStock
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IncreaseStock(short productId, short amount)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product != null)
            {
                product.UnitsInStock += amount;
                _context.Products.Update(product);
                await _context.SaveChangesAsync();
                TempData["Success"] = $" Stock de '{product.ProductName}' incrementado en {amount} unidades.";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Inventory/DecreaseStock
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DecreaseStock(short productId, short amount)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product != null)
            {
                if (product.UnitsInStock >= amount)
                {
                    product.UnitsInStock -= amount;
                    _context.Products.Update(product);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"✅ Stock de '{product.ProductName}' reducido en {amount} unidades.";
                }
                else
                {
                    TempData["Error"] = $"❌ Stock insuficiente. Stock actual: {product.UnitsInStock}";
                }
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Inventory/Orders - Ver todas las órdenes
        public async Task<IActionResult> Orders()
        {
            var orders = await _context.Orders
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
            return View(orders);
        }

        // GET: Inventory/OrderDetails/5
        public async Task<IActionResult> OrderDetails(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
                return NotFound();

            return View(order);
        }
    }
}
