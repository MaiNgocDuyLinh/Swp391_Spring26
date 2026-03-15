using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Group3_SWP391_PetMedical.Controllers
{
    [Authorize(Roles = "Manager")]
    public class ManShoppingController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View("~/Views/Shopping/ManIndex.cshtml");
        }
    }
}