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

            if (!ModelState.IsValid)
                return View("~/Views/Shopping/StaffCreate.cshtml", vm);

            try
            {
                var id = await _staffShoppingService.CreateProductAsync(vm);
                TempData["success"] = "Thêm sản phẩm mới thành công.";
                return RedirectToAction(nameof(Edit), new { id });
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                return View("~/Views/Shopping/StaffCreate.cshtml", vm);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var vm = await _staffShoppingService.GetProductForEditAsync(id);
            if (vm == null) return NotFound();

            return View("~/Views/Shopping/StaffEdit.cshtml", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(StaffShoppingUpsertVM vm)
        {
            vm.Categories = await _staffShoppingService.GetCategoriesAsync();

            if (!ModelState.IsValid)
                return View("~/Views/Shopping/StaffEdit.cshtml", vm);

            try
            {
                await _staffShoppingService.UpdateProductAsync(vm);
                TempData["success"] = "Cập nhật sản phẩm thành công.";
                return RedirectToAction(nameof(Edit), new { id = vm.ProductId });
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
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
    }
}