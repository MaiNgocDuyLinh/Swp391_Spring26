using Group3_SWP391_PetMedical.Attributes;
using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.ViewModels.Account;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Net;
using System.Net.Mail;

namespace Group3_SWP391_PetMedical.Controllers
{
    public class AccountController : Controller
    {

        private readonly ILogger<HomeController> _logger;

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AccessDenied(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }
    }
}
