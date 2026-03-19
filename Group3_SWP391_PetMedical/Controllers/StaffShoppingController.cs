using Group3_SWP391_PetMedical.Services.Interfaces;
using Group3_SWP391_PetMedical.ViewModels.Shopping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Group3_SWP391_PetMedical.Controllers
{
    [Authorize(Roles = "Staff")]
    public class StaffShoppingController : Controller
    {
        private readonly IStaffShoppingService _staffShoppingService;

        public StaffShoppingController(IStaffShoppingService staffShoppingService)
        {
            _staffShoppingService = staffShoppingService;
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] StaffShoppingQuery query)
        {
            query ??= new StaffShoppingQuery();

            var vm = new StaffShoppingIndexVM
            {
                Query = query,
                Categories = await _staffShoppingService.GetCategoriesAsync(),
                Result = await _staffShoppingService.GetProductsAsync(query)
            };

            return View("~/Views/Shopping/StaffIndex.cshtml", vm);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var vm = new StaffShoppingUpsertVM
            {
                Status = "Đang bán",
                Categories = await _staffShoppingService.GetCategoriesAsync(),
                Variants = new List<StaffShoppingVariantInputVM>()
            };

            return View("~/Views/Shopping/StaffCreate.cshtml", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StaffShoppingUpsertVM vm)
        {
            vm.Categories = await _staffShoppingService.GetCategoriesAsync();

            SyncProductStockFromVariants(vm);
            ValidateProductStatusWithStock(vm);

            if (!ModelState.IsValid)
                return View("~/Views/Shopping/StaffCreate.cshtml", vm);

            try
            {
                await _staffShoppingService.CreateProductAsync(vm);
                TempData["success"] = "Thêm sản phẩm mới thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                AddErrorsToModelState(ex.Message);
                return View("~/Views/Shopping/StaffCreate.cshtml", vm);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var vm = await _staffShoppingService.GetProductForEditAsync(id);
            if (vm == null) return NotFound();

            vm.Categories = await _staffShoppingService.GetCategoriesAsync();
            SyncProductStockFromVariants(vm);

            return View("~/Views/Shopping/StaffEdit.cshtml", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(StaffShoppingUpsertVM vm)
        {
            vm.Categories = await _staffShoppingService.GetCategoriesAsync();

            SyncProductStockFromVariants(vm);
            ValidateProductStatusWithStock(vm);

            if (!ModelState.IsValid)
                return View("~/Views/Shopping/StaffEdit.cshtml", vm);

            try
            {
                await _staffShoppingService.UpdateProductAsync(vm);
                TempData["success"] = "Cập nhật sản phẩm thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                AddErrorsToModelState(ex.Message);
                return View("~/Views/Shopping/StaffEdit.cshtml", vm);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StopSelling(int id)
        {
            try
            {
                await _staffShoppingService.StopSellingProductAsync(id);
                TempData["success"] = "Đã chuyển sản phẩm sang trạng thái dừng bán.";
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Orders([FromQuery] StaffShoppingOrderQuery query)
        {
            await _staffShoppingService.AutoCancelExpiredOrdersAsync();

            query ??= new StaffShoppingOrderQuery();

            var vm = new StaffShoppingOrdersVM
            {
                Query = query,
                Result = await _staffShoppingService.GetOrdersAsync(query)
            };

            return View("~/Views/Shopping/StaffOrders.cshtml", vm);
        }

        [HttpGet]
        public async Task<IActionResult> OrderDetail(int id)
        {
            await _staffShoppingService.AutoCancelExpiredOrdersAsync();

            var vm = await _staffShoppingService.GetOrderDetailAsync(id);
            if (vm == null) return NotFound();

            return View("~/Views/Shopping/StaffOrderDetail.cshtml", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(StaffShoppingUpdateOrderStatusVM vm)
        {
            try
            {
                await _staffShoppingService.UpdateOrderStatusAsync(vm);
                TempData["success"] = "Cập nhật trạng thái đơn hàng thành công.";
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }

            return RedirectToAction(nameof(OrderDetail), new { id = vm.OrderId });
        }

        private void SyncProductStockFromVariants(StaffShoppingUpsertVM vm)
        {
            if (vm.Variants == null || !vm.Variants.Any())
                return;

            vm.StockQuantity = vm.Variants.Sum(x => x.StockQuantity);
        }

        private void ValidateProductStatusWithStock(StaffShoppingUpsertVM vm)
        {
            var productStock = vm.StockQuantity;
            var productStatus = (vm.Status ?? string.Empty).Trim();

            if (productStock <= 0 && productStatus == "Đang bán")
            {
                ModelState.AddModelError(nameof(vm.Status), "Sản phẩm đã hết hàng nên không thể để trạng thái Đang bán.");
            }

            if (productStock > 0 && productStatus == "Hết hàng")
            {
                ModelState.AddModelError(nameof(vm.Status), "Sản phẩm vẫn còn hàng, vui lòng cập nhật lại trạng thái.");
            }

            if (vm.Variants == null || !vm.Variants.Any())
                return;

            for (int i = 0; i < vm.Variants.Count; i++)
            {
                var variant = vm.Variants[i];
                var variantStock = variant.StockQuantity;
                var variantStatus = (variant.Status ?? string.Empty).Trim();

                if (variantStock <= 0 && variantStatus == "Đang bán")
                {
                    ModelState.AddModelError($"Variants[{i}].Status", "Phân loại đã hết hàng nên không thể để trạng thái Đang bán.");
                }

                if (variantStock > 0 && variantStatus == "Hết hàng")
                {
                    ModelState.AddModelError($"Variants[{i}].Status", "Phân loại vẫn còn hàng, vui lòng cập nhật lại trạng thái.");
                }
            }
        }

        private void AddErrorsToModelState(string? message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                TempData["error"] = "Có lỗi xảy ra.";
                return;
            }

            var items = message.Split(new[] { "||" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var item in items)
            {
                var pair = item.Split(new[] { "##" }, 2, StringSplitOptions.None);

                if (pair.Length == 2)
                {
                    ModelState.AddModelError(pair[0], pair[1]);
                }
                else
                {
                    TempData["error"] = item;
                }
            }
        }
    }
}