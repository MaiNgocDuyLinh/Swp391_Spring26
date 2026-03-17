using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.ViewModels.Retail;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayOS;
using PayOS.Models.V2.PaymentRequests; // THÊM THƯ VIỆN NÀY ĐỂ TẠO LINK PAYOS
using System.Security.Claims;

namespace Group3_SWP391_PetMedical.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CheckoutController : Controller
    {
        private readonly PetClinicContext _context;

        private const string BankCode = "BIDV";
        private const string AccountNumber = "4880689237";
        private readonly PayOSClient _payOSClient;
        private readonly string _baseUrl;

        public CheckoutController(PetClinicContext context, IConfiguration configuration)
        {
            _context = context;
            //_baseUrl = configuration["BaseUrl"] ?? "https://localhost:7000";
            _baseUrl = "https://tamia-pinkish-denzel.ngrok-free.dev";

            string clientId = configuration["PayOS:ClientId"] ?? "";
            string apiKey = configuration["PayOS:ApiKey"] ?? "";
            string checksumKey = configuration["PayOS:ChecksumKey"] ?? "";

            _payOSClient = new PayOSClient(new PayOSOptions
            {
                ClientId = clientId,
                ApiKey = apiKey,
                ChecksumKey = checksumKey
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index([FromForm] int[] medicineIds)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Forbid();

            if (medicineIds == null || medicineIds.Length == 0)
                return RedirectToAction("Index", "Cart");

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.medicine)
                .FirstOrDefaultAsync(c => c.user_id == userId.Value && c.status == "ACTIVE");

            if (cart == null || !cart.CartItems.Any())
                return RedirectToAction("Index", "Cart");

            var selectedSet = new HashSet<int>(medicineIds);
            var items = cart.CartItems
                .Where(ci => selectedSet.Contains(ci.medicine_id))
                .Select(ci => new CheckoutItemVm
                {
                    medicine_id = ci.medicine_id,
                    medicine_name = ci.medicine.name,
                    unit_price = ci.medicine.unit_price,
                    quantity = ci.quantity,
                    stock_quantity = ci.medicine.stock_quantity ?? 0
                })
                .ToList();

            if (!items.Any())
                return RedirectToAction("Index", "Cart");

            var vm = new CheckoutVm
            {
                Items = items,
                TotalAmount = items.Sum(i => i.line_total),
                SelectedMedicineIds = items.Select(i => i.medicine_id).ToArray()
            };

            foreach (var item in items)
            {
                if (item.quantity > item.stock_quantity)
                {
                    vm.StockErrors.Add(
                        $"Thuốc {item.medicine_name} chỉ còn {item.stock_quantity} đơn vị, nhưng bạn chọn {item.quantity}.");
                }
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(CheckoutVm form)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Forbid();

            if (form.SelectedMedicineIds == null || form.SelectedMedicineIds.Length == 0)
                return RedirectToAction("Index", "Cart");

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.medicine)
                .FirstOrDefaultAsync(c => c.user_id == userId.Value && c.status == "ACTIVE");

            if (cart == null || !cart.CartItems.Any())
                return RedirectToAction("Index", "Cart");

            var selectedSet = new HashSet<int>(form.SelectedMedicineIds);
            var cartItems = cart.CartItems
                .Where(ci => selectedSet.Contains(ci.medicine_id))
                .ToList();

            if (!cartItems.Any())
                return RedirectToAction("Index", "Cart");

            var items = cartItems.Select(ci => new CheckoutItemVm
            {
                medicine_id = ci.medicine_id,
                medicine_name = ci.medicine.name,
                unit_price = ci.medicine.unit_price,
                quantity = ci.quantity,
                stock_quantity = ci.medicine.stock_quantity ?? 0
            }).ToList();

            var stockErrors = new List<string>();
            foreach (var item in items)
            {
                if (item.quantity > item.stock_quantity)
                {
                    stockErrors.Add(
                        $"Thuốc {item.medicine_name} chỉ còn {item.stock_quantity} đơn vị, nhưng bạn chọn {item.quantity}.");
                }
            }

            if (stockErrors.Any())
            {
                var vm = new CheckoutVm
                {
                    Items = items,
                    TotalAmount = items.Sum(i => i.line_total),
                    PickupSlot = form.PickupSlot,
                    Note = form.Note,
                    SelectedMedicineIds = items.Select(i => i.medicine_id).ToArray(),
                    StockErrors = stockErrors
                };
                return View("Index", vm);
            }

            using var tx = await _context.Database.BeginTransactionAsync();

            var total = items.Sum(i => i.line_total);

            var order = new RetailOrder
            {
                user_id = userId.Value,
                total_amount = total,
                status = "PENDING",
                created_at = DateTime.UtcNow,
                pickup_slot = form.PickupSlot,
                note = form.Note
            };

            _context.RetailOrders.Add(order);
            await _context.SaveChangesAsync();

            foreach (var ci in cartItems)
            {
                var detail = new OrderDetail
                {
                    order_id = order.id,
                    medicine_id = ci.medicine_id,
                    quantity = ci.quantity
                };
                _context.OrderDetails.Add(detail);
            }

            _context.CartItems.RemoveRange(cartItems);

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            // --------------------------------------------------------
            // GỌI PAYOS ĐỂ TẠO CỔNG THANH TOÁN
            // --------------------------------------------------------
            try
            {
                var request = new CreatePaymentLinkRequest
                {
                    OrderCode = order.id,
                    Amount = (int)total,
                    Description = $"PET{order.id}", // Mã này giúp Webhook nhận diện đúng đơn hàng
                    CancelUrl = $"{_baseUrl}/api/payment/cancel",
                    ReturnUrl = $"{_baseUrl}/api/payment/success"
                };

                CreatePaymentLinkResponse result = await _payOSClient.PaymentRequests.CreateAsync(request);

                // Bay thẳng sang giao diện thanh toán xịn xò của PayOS
                return Redirect(result.CheckoutUrl);
            }
            catch (Exception ex)
            {
                return Content("Lỗi kết nối PayOS: " + ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> CheckOrderStatus(int id)
        {
            var order = await _context.RetailOrders
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.id == id);

            if (order == null) return NotFound();

            return Json(new { status = order.status });
        }

        private int? GetCurrentUserId()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? User.FindFirstValue("user_id");
            return int.TryParse(raw, out var id) ? id : null;
        }
    }
}