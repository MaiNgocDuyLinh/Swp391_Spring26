using System.Security.Claims;
using Group3_SWP391_PetMedical.Services.Interfaces;
using Group3_SWP391_PetMedical.ViewModels.Shopping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Group3_SWP391_PetMedical.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CusShoppingController : Controller
    {
        private readonly ICusShoppingService _cusShoppingService;

        public CusShoppingController(ICusShoppingService cusShoppingService)
        {
            _cusShoppingService = cusShoppingService;
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] CusShoppingQuery query)
        {
            query ??= new CusShoppingQuery();

            var vm = new CusShoppingIndexVM
            {
                Query = query,
                Categories = await _cusShoppingService.GetCategoriesAsync(),
                Result = await _cusShoppingService.GetProductsAsync(query)
            };

            return View("~/Views/Shopping/CusIndex.cshtml", vm);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id, int? variantId)
        {
            var vm = await _cusShoppingService.GetProductDetailAsync(id);

            if (vm == null) return NotFound();

            if (vm.Variants != null && vm.Variants.Any())
            {
                vm.SelectedVariantId = variantId ?? vm.Variants.First().VariantId;
            }

            return View("~/Views/Shopping/CusDetail.cshtml", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(CusAddToCartVM vm)
        {
            try
            {
                int customerId = GetCurrentUserId();
                await _cusShoppingService.AddToCartAsync(customerId, vm.ProductId, vm.VariantId, vm.Quantity);
                TempData["success"] = "Đã thêm sản phẩm vào giỏ hàng.";
                return RedirectToAction(nameof(Cart));
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                return RedirectToAction(nameof(Detail), new { id = vm.ProductId, variantId = vm.VariantId });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Cart()
        {
            int customerId = GetCurrentUserId();
            var vm = await _cusShoppingService.GetCartAsync(customerId);
            return View("~/Views/Shopping/CusCart.cshtml", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCartItem(int cartItemId, int quantity)
        {
            try
            {
                int customerId = GetCurrentUserId();
                await _cusShoppingService.UpdateCartItemAsync(customerId, cartItemId, quantity);
                TempData["success"] = "Cập nhật giỏ hàng thành công.";
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }

            return RedirectToAction(nameof(Cart));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveCartItem(int cartItemId)
        {
            int customerId = GetCurrentUserId();
            await _cusShoppingService.RemoveCartItemAsync(customerId, cartItemId);
            TempData["success"] = "Đã xóa sản phẩm khỏi giỏ hàng.";
            return RedirectToAction(nameof(Cart));
        }

        [HttpGet]
        public async Task<IActionResult> Checkout(string? selectedCartItemIds)
        {
            int customerId = GetCurrentUserId();

            var selectedIds = ParseSelectedCartItemIds(selectedCartItemIds);
            if (!selectedIds.Any())
            {
                TempData["error"] = "Vui lòng chọn ít nhất 1 sản phẩm.";
                return RedirectToAction(nameof(Cart));
            }

            var vm = await _cusShoppingService.GetCheckoutAsync(customerId, selectedIds);

            if (!vm.Items.Any())
            {
                TempData["error"] = "Không tìm thấy sản phẩm đã chọn trong giỏ hàng.";
                return RedirectToAction(nameof(Cart));
            }

            return View("~/Views/Shopping/CusCheckout.cshtml", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CusCheckoutSubmitVM submitVm)
        {
            int customerId = GetCurrentUserId();

            var selectedIds = submitVm.SelectedCartItemIds?.Distinct().ToList() ?? new List<int>();
            if (!selectedIds.Any())
            {
                TempData["error"] = "Vui lòng chọn ít nhất 1 sản phẩm.";
                return RedirectToAction(nameof(Cart));
            }

            try
            {
                int orderId = await _cusShoppingService.PlaceOrderAsync(
                    customerId,
                    selectedIds,
                    submitVm.PickupNote,
                    submitVm.PickupDate,
                    submitVm.PaymentMethod);

                TempData["success"] = "Đặt hàng thành công.";
                return RedirectToAction(nameof(OrderDetail), new { id = orderId });
            }
            catch (Exception ex)
            {
                var vm = await _cusShoppingService.GetCheckoutAsync(customerId, selectedIds);
                vm.PickupDate = submitVm.PickupDate;
                vm.PickupNote = submitVm.PickupNote;
                vm.PaymentMethod = submitVm.PaymentMethod;

                TempData["error"] = ex.Message;
                return View("~/Views/Shopping/CusCheckout.cshtml", vm);
            }
        }

        private List<int> ParseSelectedCartItemIds(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return new List<int>();

            return raw
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => int.TryParse(x, out _))
                .Select(int.Parse)
                .Distinct()
                .ToList();
        }

        [HttpGet]
        public async Task<IActionResult> MyOrders([FromQuery] CusShoppingOrderQuery query)
        {
            int customerId = GetCurrentUserId();
            query ??= new CusShoppingOrderQuery();

            var vm = await _cusShoppingService.GetMyOrdersAsync(customerId, query);
            return View("~/Views/Shopping/CusMyOrders.cshtml", vm);
        }

        [HttpGet]
        public async Task<IActionResult> OrderDetail(int id)
        {
            int customerId = GetCurrentUserId();
            var vm = await _cusShoppingService.GetOrderDetailAsync(customerId, id);
            if (vm == null) return NotFound();

            return View("~/Views/Shopping/CusOrderDetail.cshtml", vm);
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? User.FindFirstValue("user_id");

            if (!int.TryParse(claim, out int userId))
            {
                throw new UnauthorizedAccessException("Không xác định được người dùng.");
            }

            return userId;
        }
    }
}