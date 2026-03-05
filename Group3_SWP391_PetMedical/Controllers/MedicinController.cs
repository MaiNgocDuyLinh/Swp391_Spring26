using Microsoft.AspNetCore.Mvc;
using Group3_SWP391_PetMedical.Models.Common;
using Group3_SWP391_PetMedical.Services.Interfaces;

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
            ViewBag.TotalPages = Math.Max(data.TotalPages, 1);
            ViewBag.TotalItems = data.TotalItems;

            return View(data.Items);
        }
    }
}
