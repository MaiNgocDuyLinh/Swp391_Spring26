using Group3_SWP391_PetMedical.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Group3_SWP391_PetMedical.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CartController : Controller
    {
        private readonly ICartItemService _cartService;

        public CartController(ICartItemService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Forbid();

            var vm = await _cartService.GetOrCreateActiveCartAsync(userId.Value);
            return View("~/Views/Retail/CartMedicin.cshtml", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int medicineId, int quantity = 1, string? returnUrl = null)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Forbid();

            await _cartService.AddToCartAsync(userId.Value, medicineId, quantity);

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int medicineId, int quantity)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Forbid();

            await _cartService.UpdateQuantityAsync(userId.Value, medicineId, quantity);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int medicineId)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Forbid();

            await _cartService.RemoveItemAsync(userId.Value, medicineId);
            return RedirectToAction(nameof(Index));
        }

        // -------- AJAX endpoints (Bootstrap 5 cart UI) --------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAjax([FromForm] int medicineId, [FromForm] int quantity)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Forbid();

            var vm = await _cartService.UpdateQuantityAsync(userId.Value, medicineId, quantity);
            var item = vm.items.FirstOrDefault(i => i.medicine_id == medicineId);

            return Json(new
            {
                ok = true,
                isEmpty = vm.items.Count == 0,
                item = item == null ? null : new
                {
                    medicineId = item.medicine_id,
                    quantity = item.quantity,
                    unitPrice = item.unit_price,
                    lineTotal = item.line_total,
                    stock = item.stock_quantity
                },
                cart = new
                {
                    totalQuantity = vm.total_quantity,
                    totalAmount = vm.total_amount
                }
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveAjax([FromForm] int medicineId)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Forbid();

            var vm = await _cartService.RemoveItemAsync(userId.Value, medicineId);

            return Json(new
            {
                ok = true,
                removedMedicineId = medicineId,
                isEmpty = vm.items.Count == 0,
                cart = new
                {
                    totalQuantity = vm.total_quantity,
                    totalAmount = vm.total_amount
                }
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearSelectedAjax([FromForm] int[] medicineIds)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Forbid();

            if (medicineIds == null || medicineIds.Length == 0)
                return Json(new { ok = true, cleared = Array.Empty<int>() });

            foreach (var id in medicineIds.Distinct())
            {
                await _cartService.RemoveItemAsync(userId.Value, id);
            }

            var vm = await _cartService.GetOrCreateActiveCartAsync(userId.Value);

            return Json(new
            {
                ok = true,
                cleared = medicineIds.Distinct().ToArray(),
                isEmpty = vm.items.Count == 0,
                cart = new
                {
                    totalQuantity = vm.total_quantity,
                    totalAmount = vm.total_amount
                }
            });
        }

        private int? GetCurrentUserId()
        {
            // LoginController currently sets ClaimTypes.NameIdentifier = user.user_id
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? User.FindFirstValue("user_id");

            return int.TryParse(raw, out var id) ? id : null;
        }
    }
}

