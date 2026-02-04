using Microsoft.AspNetCore.Mvc;
using Group3_SWP391_PetMedical.Models.Common;
using Group3_SWP391_PetMedical.ViewModels.Common;

namespace Group3_SWP391_PetMedical.Controllers
{
    public class ServiceController : Controller
    {
        private readonly IServiceService _serviceService;
        public ServiceController(IServiceService serviceService) => _serviceService = serviceService;

        public async Task<IActionResult> Service(string? q, int page = 1, int pageSize = 6)
        {
            var data = await _serviceService.GetServiceListAsync(new PagingQuery
            {
                Q = q,
                Page = page,
                PageSize = pageSize
            });

            var vm = new ListPageVM<Group3_SWP391_PetMedical.Models.Service>
            {
                Q = q,
                Data = data
            };

            return View(vm);
        }
    }
}
