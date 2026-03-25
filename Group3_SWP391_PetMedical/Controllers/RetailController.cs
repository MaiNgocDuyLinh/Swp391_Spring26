using Group3_SWP391_PetMedical.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Group3_SWP391_PetMedical.Controllers
{
    public class RetailController : Controller
    {
        private readonly IMedicinService _medicinService;
        private readonly IRetailOrderService _retailOrderService;

        public RetailController(IMedicinService medicinService, IRetailOrderService retailOrderService)
        {
            _medicinService = medicinService;
            _retailOrderService = retailOrderService;
        }

        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> RetailViewList(string? search, int page = 1, int pageSize = 12)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 12;

            var data = await _medicinService.GetMedicinListAsync(new Group3_SWP391_PetMedical.Models.Common.PagingQuery
            {
                Q = search,
                Page = page,
                PageSize = pageSize
            });

            ViewBag.Search = search;
            ViewBag.CurrentPage = data.Page;
            ViewBag.PageSize = data.PageSize;
            ViewBag.TotalPages = Math.Max(data.TotalPages, 1);
            ViewBag.TotalItems = data.TotalItems;

            return View(data.Items);
        }

        [Authorize(Roles = "Customer")]
        [HttpGet]
        public async Task<IActionResult> RetailOrderedViewList()
        {
            var userIdRaw = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            if (!int.TryParse(userIdRaw, out int userId))
            {
                return Forbid();
            }

            var orders = await _retailOrderService.GetOrdersByUserIdAsync(userId);
            
            // Lọc ra những đơn hàng có status là 'PAID'
            var paidOrders = orders.Where(o => (o.status ?? "").ToUpper() == "PAID").ToList();
            
            return View(paidOrders);
        }

        [Authorize(Roles = "Staff,Manager")]
        [HttpGet]
        public async Task<IActionResult> StaffViewRetailList(DateTime? date, string? search, string? status, int page = 1)
        {
            if (page < 1) page = 1;
            int pageSize = 10;
            var queryDate = date ?? DateTime.Today;

            var allOrders = await _retailOrderService.GetAllOrdersAsync(queryDate, search, status);

            int totalItems = allOrders.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            
            var pagedOrders = allOrders.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.SelectedDate = queryDate;
            ViewBag.Search = search;
            ViewBag.Status = status;

            return View(pagedOrders);
        }

        [Authorize(Roles = "Staff,Manager")]
        [HttpGet]
        public async Task<IActionResult> StaffViewRetailDetail(int id)
        {
            var order = await _retailOrderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

        [Authorize(Roles = "Staff,Manager")]
        [HttpPost]
        public async Task<IActionResult> StaffUpdateStatusOrder(int id, string status_order)
        {
            if (string.IsNullOrEmpty(status_order))
            {
                return BadRequest("Trạng thái không hợp lệ.");
            }

            var order = await _retailOrderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            await _retailOrderService.UpdateStatusOrderAsync(id, status_order);
            TempData["SuccessMessage"] = "Cập nhật trạng thái đơn hàng thành công.";
            return RedirectToAction("StaffViewRetailDetail", new { id = id });
        }

        [Authorize(Roles = "Customer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var userIdRaw = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            if (!int.TryParse(userIdRaw, out int userId)) return Forbid();

            var success = await _retailOrderService.CancelOrderAsync(id, userId);
            if (success)
            {
                TempData["SuccessMessage"] = "Hủy đơn hàng và hoàn kho thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể hủy đơn hàng này (Đơn không tồn tại hoặc đã được xử lý).";
            }

            return RedirectToAction(nameof(RetailOrderedViewList));
        }
    }
}
