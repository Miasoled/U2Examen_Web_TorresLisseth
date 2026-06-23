using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NorthwindApp.Data;
using NorthwindApp.Models;
using System.Text.Json;

namespace NorthwindApp.Controllers
{
    [Authorize(Roles = "Client")]
    public class ShopController : Controller
    {
        private readonly NorthwindContext _context;
        private const string CartKey = "ShoppingCart";

        public ShopController(NorthwindContext context)
        {
            _context = context;
        }

        // GET: Shop
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.UnitsInStock > 0 && p.Discontinued == 0)
                .OrderBy(p => p.ProductName)
                .ToListAsync();

            return View(products);
        }

        // POST: Shop/AddToCart
        [HttpPost]
        public async Task<IActionResult> AddToCart(
            List<short> productIds,
            Dictionary<short, int> quantities)
        {
            var cartJson = HttpContext.Session.GetString(CartKey);
            var cart = cartJson != null
                ? JsonSerializer.Deserialize<List<CartItem>>(cartJson)!
                : new List<CartItem>();

            foreach (var productId in productIds)
            {
                if (!quantities.ContainsKey(productId) || quantities[productId] <= 0)
                    continue;

                var product = await _context.Products.FindAsync(productId);
                if (product == null) continue;

                var existing = cart.FirstOrDefault(c => c.ProductId == productId);
                if (existing != null)
                {
                    existing.Quantity += quantities[productId];
                }
                else
                {
                    cart.Add(new CartItem
                    {
                        ProductId   = productId,
                        ProductName = product.ProductName,
                        UnitPrice   = product.UnitPrice ?? 0,
                        Quantity    = quantities[productId]
                    });
                }
            }

            HttpContext.Session.SetString(CartKey, JsonSerializer.Serialize(cart));
            TempData["Success"] = $" {cart.Count} producto(s) en el carrito.";
            return RedirectToAction(nameof(Cart));
        }

        // GET: Shop/Cart
        public IActionResult Cart()
        {
            var cartJson = HttpContext.Session.GetString(CartKey);
            var cart = cartJson != null
                ? JsonSerializer.Deserialize<List<CartItem>>(cartJson)!
                : new List<CartItem>();

            return View(cart);
        }

        // POST: Shop/RemoveFromCart
        [HttpPost]
        public IActionResult RemoveFromCart(short productId)
        {
            var cartJson = HttpContext.Session.GetString(CartKey);
            var cart = cartJson != null
                ? JsonSerializer.Deserialize<List<CartItem>>(cartJson)!
                : new List<CartItem>();

            cart.RemoveAll(c => c.ProductId == productId);
            HttpContext.Session.SetString(CartKey, JsonSerializer.Serialize(cart));
            return RedirectToAction(nameof(Cart));
        }

        // GET: Shop/Checkout
        [HttpGet]
        [ActionName("Checkout")]
        public IActionResult CheckoutGet()
        {
            var cartJson = HttpContext.Session.GetString(CartKey);
            var cart = cartJson != null
                ? JsonSerializer.Deserialize<List<CartItem>>(cartJson)!
                : new List<CartItem>();

            if (!cart.Any())
                return RedirectToAction(nameof(Index));

            return View("Checkout", cart);
        }

        // POST: Shop/PlaceOrder — Genera Order + OrderDetails + descuenta stock
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder()
        {
            var cartJson = HttpContext.Session.GetString(CartKey);
            var cart = cartJson != null
                ? JsonSerializer.Deserialize<List<CartItem>>(cartJson)!
                : new List<CartItem>();

            if (!cart.Any())
                return RedirectToAction(nameof(Index));

            // 1. Crear la Order
            short nextId = (short)((_context.Orders.Max(o => (int?)o.OrderId) ?? 0) + 1);

            var order = new Order
            {
                OrderId      = nextId,
                OrderDate    = DateOnly.FromDateTime(DateTime.Now),
                RequiredDate = DateOnly.FromDateTime(DateTime.Now.AddDays(7)),
                ShipName     = User.Identity!.Name,
                ShipAddress  = "Dirección del cliente",
                ShipCity     = "Quito",
                ShipCountry  = "Ecuador",
                Freight      = 0
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // 2. Crear OrderDetails y descontar stock
            foreach (var item in cart)
            {
                _context.OrderDetails.Add(new OrderDetail
                {
                    OrderId   = order.OrderId,
                    ProductId = item.ProductId,
                    UnitPrice = item.UnitPrice,
                    Quantity  = (short)item.Quantity,
                    Discount  = 0
                });

                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                    product.UnitsInStock = (short)((product.UnitsInStock ?? 0) - item.Quantity);
            }

            await _context.SaveChangesAsync();

            // 3. Limpiar carrito
            HttpContext.Session.Remove(CartKey);

            TempData["Success"] = $"✅ Orden #{order.OrderId} registrada exitosamente.";
            return RedirectToAction(nameof(OrderConfirmation), new { id = order.OrderId });
        }

        // GET: Shop/OrderConfirmation
        public async Task<IActionResult> OrderConfirmation(int id)
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
