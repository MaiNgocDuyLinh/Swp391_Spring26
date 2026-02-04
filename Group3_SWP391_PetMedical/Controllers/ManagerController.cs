using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Group3_SWP391_PetMedical.Services.Interfaces;

namespace Group3_SWP391_PetMedical.Controllers
{
    [Authorize(Roles = "Manager")]
    public class ManagerController : Controller
    {
        private readonly IManagerService _managerService;
        private const int PageSize = 5;

        public ManagerController(IManagerService managerService)
        {
            _managerService = managerService;
        }

        // ========== 1. VIEW LIST SERVICE ==========
        public async Task<IActionResult> ListServices(string? search, int page = 1)
        {
            var result = await _managerService.GetServicesPagedAsync(search, page, PageSize);
            
            ViewBag.CurrentPage = result.Page;
            ViewBag.TotalPages = result.TotalPages;
            ViewBag.TotalItems = result.TotalItems;
            ViewBag.Search = search;
            
            return View(result.Items.ToList());
        }

        // ========== 2. EDIT SERVICE (GET) ==========
        public async Task<IActionResult> EditService(int id)
        {
            var service = await _managerService.GetServiceByIdAsync(id);
            if (service == null)
            {
                return NotFound();
            }
            return View(service);
        }

        // ========== 2. EDIT SERVICE (POST) ==========
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditService(int id, decimal base_price, string? description)
        {
            var success = await _managerService.UpdateServiceAsync(id, base_price, description);
            if (!success)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] = "Cập nhật dịch vụ thành công!";
            return RedirectToAction("ListServices");
        }
    }
}
