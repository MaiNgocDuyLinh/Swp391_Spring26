using Microsoft.AspNetCore.Mvc;
using Group3_SWP391_PetMedical.Models.Common;
using Group3_SWP391_PetMedical.Services.Interfaces;
using Group3_SWP391_PetMedical.ViewModels.Medicin;
using Microsoft.AspNetCore.Authorization;

namespace Group3_SWP391_PetMedical.Controllers
{
    
    public class MedicinController : Controller
    {
        private readonly IMedicinService _medicinService;

        public MedicinController(IMedicinService medicinService)
        {
            _medicinService = medicinService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Doctor,Staff,Manager")]
        public async Task<IActionResult> MedicinList(string? search, int page = 1, int pageSize = 10)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var data = await _medicinService.GetMedicinListAsync(new PagingQuery
            {
                Q = search,
                Status = "active", // Chỉ lấy thuốc đang bán
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

        [Authorize(Roles = "Doctor,Staff,Manager")]
        public async Task<IActionResult> MedicinInactiveList(string? search, int page = 1, int pageSize = 10)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            // Lấy tất cả trừ active (inactive, discontinued, v.v.)
            var data = await _medicinService.GetMedicinListAsync(new PagingQuery
            {
                Q = search,
                Status = "inactive",
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

        [Authorize(Roles = "Doctor,Staff,Manager")]
        public async Task<IActionResult> MedicinDetails(int id)
        {
            var medicin = await _medicinService.GetByIdAsync(id);
            if (medicin == null)
            {
                return NotFound();
            }

            return View(medicin);
        }

        [Authorize(Roles = "Staff,Manager")]
        [HttpGet]
        public IActionResult MedicinAddForm()
        {
            return View(new CreateMedicinVm());
        }

        [Authorize(Roles = "Staff,Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MedicinAddForm(CreateMedicinVm vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            // Check duplicate name
            var existing = await _medicinService.GetByNameAsync(vm.name.Trim());
            if (existing != null && existing.status.ToLower() == "active")
            {
                ModelState.AddModelError("name", "Thuốc đã có và đang bán, vui lòng kiểm tra lại.");
                return View(vm);
            }

            var created = await _medicinService.AddAsync(
                vm.name.Trim(),
                vm.unit_price,
                vm.stock_quantity,
                vm.description?.Trim(),
                vm.status);

            TempData["SuccessMessage"] = "Thêm thuốc thành công!";
            return RedirectToAction(nameof(MedicinDetails), new { id = created.medicine_id });
        }


        [Authorize(Roles = "Staff,Manager")]
        [HttpGet]
        public async Task<IActionResult> MedicinEditForm(int id)
        {
            var medicin = await _medicinService.GetByIdAsync(id);
            if (medicin == null) return NotFound();

            var vm = new EditMedicinVm
            {
                medicine_id = medicin.medicine_id,
                name = medicin.name,
                unit_price = medicin.unit_price,
                stock_quantity = medicin.stock_quantity ?? 0,
                description = medicin.description,
                status = medicin.status
            };

            return View(vm);
        }


        [Authorize(Roles = "Staff,Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MedicinEditForm(EditMedicinVm vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            // Check duplicate name
            var existing = await _medicinService.GetByNameAsync(vm.name.Trim());
            if (existing != null && existing.medicine_id != vm.medicine_id && existing.status.ToLower() == "active")
            {
                ModelState.AddModelError("name", "Tên thuốc đã tồn tại và đang ở trạng thái kinh doanh.");
                return View(vm);
            }

            var updated = await _medicinService.UpdateAsync(
                vm.medicine_id,
                vm.name.Trim(),
                vm.unit_price,
                vm.stock_quantity,
                vm.description?.Trim(),
                vm.status);

            if (!updated) return NotFound();

            TempData["SuccessMessage"] = "Cập nhật thuốc thành công!";
            return RedirectToAction(nameof(MedicinDetails), new { id = vm.medicine_id });
        }
    }
}
