using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.Models.Common;
using Group3_SWP391_PetMedical.ViewModels.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Group3_SWP391_PetMedical.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly PetClinicContext _context;

        private readonly IServiceService _serviceService;

        public HomeController(
            ILogger<HomeController> logger,
            PetClinicContext context,
            IServiceService serviceService   
        )
        {
            _logger = logger;
            _context = context;
            _serviceService = serviceService;  
        }

        public async Task<IActionResult> Index(string? q, int page = 1, int pageSize = 6)
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

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}