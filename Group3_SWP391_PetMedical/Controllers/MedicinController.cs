using Microsoft.AspNetCore.Mvc;
using Group3_SWP391_PetMedical.Models.Common;
using Group3_SWP391_PetMedical.Services.Interfaces;
using Group3_SWP391_PetMedical.ViewModels.Medicin;

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

        public async Task<IActionResult> MedicinList(string? search, int page = 1, int pageSize = 10)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var data = await _medicinService.GetMedicinListAsync(new PagingQuery
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

        public async Task<IActionResult> MedicinDetails(int id)
        {
            var medicin = await _medicinService.GetByIdAsync(id);
            if (medicin == null)
            {
                return NotFound();
            }

            return View(medicin);
        }

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
                description = medicin.description
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MedicinEditForm(EditMedicinVm vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var updated = await _medicinService.UpdateAsync(
                vm.medicine_id,
                vm.name.Trim(),
                vm.unit_price,
                vm.stock_quantity,
                vm.description?.Trim());

            if (!updated) return NotFound();

            TempData["SuccessMessage"] = "Cập nhật thuốc thành công!";
            return RedirectToAction(nameof(MedicinDetails), new { id = vm.medicine_id });
        }
    }
}
