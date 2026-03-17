using Group3_SWP391_PetMedical.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Group3_SWP391_PetMedical.Controllers
{
    [Authorize(Roles = "Customer")]
    public class RetailController : Controller
    {
        private readonly IMedicinService _medicinService;

        public RetailController(IMedicinService medicinService)
        {
            _medicinService = medicinService;
        }

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
    }
}
