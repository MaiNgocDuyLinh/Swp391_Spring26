using Microsoft.AspNetCore.Mvc;

namespace Group3_SWP391_PetMedical.Controllers
{
    public class ServiceController : Controller
    {
        private readonly ILogger<ServiceController> _logger;

        public ServiceController(ILogger<ServiceController> logger)
        {
            _logger = logger;
        }

        public IActionResult Service()
        {
            return View();
        }
    }
}
