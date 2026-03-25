using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.ViewModels.Retail;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayOS;
using PayOS.Models.V2.PaymentRequests;
using System.Security.Claims;

namespace Group3_SWP391_PetMedical.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CheckoutController : Controller
    {
        private readonly PetClinicContext _context;
        private readonly PayOSClient _payOSClient;
        private readonly string _baseUrl;

        public CheckoutController(PetClinicContext context, IConfiguration configuration)
        {
            _context = context;
            // Dùng link Ngrok để không bị văng đăng nhập
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

        // 1. HÀM MỚI: Đón dữ liệu POST từ Giỏ hàng và Redirect sang GET
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index([FromForm] int[] medicineIds)
        {
            if (medicineIds == null || medicineIds.Length == 0)
                return RedirectToAction("Index", "Cart");

            // Lưu danh sách ID vào TempData (chỉ sống được qua 1 lần chuyển trang)
            TempData["SelectedMedicineIds"] = medicineIds;

            // Chuyển hướng sang hàm GET bên dưới
            return RedirectToAction(nameof(ConfirmOrder));
        }

        
        [HttpGet]
        public async Task<IActionResult> ConfirmOrder() // dùng get để không bị lỗi resubmit
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Forbid();

            // Lấy lại danh sách ID từ TempData
            var medicineIds = TempData["SelectedMedicineIds"] as int[];

            if (medicineIds == null || medicineIds.Length == 0)
                return RedirectToAction("Index", "Cart");

            // Giữ lại TempData để nếu khách bấm F5 trang này thì vẫn còn dữ liệu
            TempData.Keep("SelectedMedicineIds");

            var cart = await _context.CartsMedicin
                .Include(c => c.CartItemsMedicin).ThenInclude(ci => ci.medicine)
                .FirstOrDefaultAsync(c => c.user_id == userId.Value && c.status == "ACTIVE");

            if (cart == null || !cart.CartItemsMedicin.Any())
                return RedirectToAction("Index", "Cart");

            var selectedSet = new HashSet<int>(medicineIds);
            var items = cart.CartItemsMedicin
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

            var vm = new CheckoutVm
            {
                Items = items,
                TotalAmount = items.Sum(i => i.line_total),
                SelectedMedicineIds = medicineIds
            };

            // Check tồn kho
            foreach (var item in items)
            {
                if (item.quantity > item.stock_quantity)
                {
                    vm.StockErrors.Add($"Thuốc {item.medicine_name} chỉ còn {item.stock_quantity} đơn vị, nhưng bạn chọn {item.quantity}.");
                }
            }

            // Trả về View Index.cshtml 
            return View("Index", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(CheckoutVm form)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Forbid();

            if (form.SelectedMedicineIds == null || form.SelectedMedicineIds.Length == 0)
                return RedirectToAction("Index", "Cart");

            var cart = await _context.CartsMedicin
                .Include(c => c.CartItemsMedicin).ThenInclude(ci => ci.medicine)
                .FirstOrDefaultAsync(c => c.user_id == userId.Value && c.status == "ACTIVE");

            var selectedSet = new HashSet<int>(form.SelectedMedicineIds);
            var cartItems = cart.CartItemsMedicin.Where(ci => selectedSet.Contains(ci.medicine_id)).ToList();

            if (!cartItems.Any()) return RedirectToAction("Index", "Cart");

            // Bắt đầu giao dịch Database
            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var total = cartItems.Sum(ci => ci.quantity * ci.medicine.unit_price);

                // 1. Tạo đơn hàng RetailOrder
                var order = new RetailOrder
                {
                    user_id = userId.Value,
                    total_amount = total,
                    status = "PENDING",
                    created_at = DateTime.Now,
                    pickup_slot = form.PickupSlot,
                    pickup_date = form.PickupDate,
                    note = form.Note
                };

                _context.RetailOrders.Add(order);
                await _context.SaveChangesAsync();

                // 2. Tạo chi tiết đơn hàng
                foreach (var ci in cartItems)
                {
                    _context.OrderDetails.Add(new OrderDetail
                    {
                        order_id = order.id,
                        medicine_id = ci.medicine_id,
                        quantity = ci.quantity,
                        price_at_purchase = ci.medicine.unit_price
                    });
                }

                // 3. Xóa các mục đã mua khỏi giỏ hàng
                _context.CartItemsMedicin.RemoveRange(cartItems);
                
                // MỚI: Luôn trừ kho ngay để giữ chỗ (reserve), và đặt trạng thái "Đã tiếp nhận"
                order.status_order = "Đã tiếp nhận";
                foreach (var ci in cartItems)
                {
                    var med = await _context.Medications.FindAsync(ci.medicine_id);
                    if (med != null)
                    {
                        med.stock_quantity = Math.Max(0, (med.stock_quantity ?? 0) - ci.quantity);
                    }
                }
                
                await _context.SaveChangesAsync();

                // 4. Nếu là thanh toán Online thì gọi PayOS, ngược lại thì bỏ qua
                if (form.PaymentMethod == "ONLINE")
                {
                    var baseUrl = $"{Request.Scheme}://{Request.Host}";
                    var request = new CreatePaymentLinkRequest
                    {
                        OrderCode = order.id,
                        Amount = (int)total,
                        Description = $"PET{order.id}",
                        CancelUrl = $"{baseUrl}/api/payment/cancel?orderId={order.id}",
                        ReturnUrl = $"{baseUrl}/api/payment/success?orderId={order.id}"
                    };

                    var result = await _payOSClient.PaymentRequests.CreateAsync(request);

                    // 5. CHỈ COMMIT TẠI ĐÂY (Khi cả DB và PayOS đều ok)
                    await tx.CommitAsync();

                    // Điều hướng khách sang trang thanh toán của ngân hàng
                    return Redirect(result.CheckoutUrl);
                }
                else
                {
                    // Thanh toán tại quầy
                    await tx.CommitAsync();
                    
                    TempData["SuccessMessage"] = "Đặt đơn thành công! Vui lòng đến cửa hàng để thanh toán và nhận thuốc.";
                    return RedirectToAction("Index", "Home", new { payment = "success" });
                }
            }
            catch (Exception ex)
            {
                // Nếu có bất kỳ lỗi nào ở trên, Rollback sẽ thu hồi lại dữ liệu trong DB
                // Vì lệnh Commit chưa được thực hiện, nên sẽ không bị lỗi Zombie
                await tx.RollbackAsync();
                return Content("Lỗi thanh toán: " + ex.Message);
            }
        }

        private int? GetCurrentUserId()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return int.TryParse(raw, out var id) ? id : null;
        }
    }
}