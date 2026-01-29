using System.Security.Claims;
using Group3_SWP391_PetMedical.Models.Common;
using Group3_SWP391_PetMedical.Services.Interfaces;
using Group3_SWP391_PetMedical.ViewModels;
using Group3_SWP391_PetMedical.ViewModels.Pet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Group3_SWP391_PetMedical.Controllers
{
    [Authorize(Roles ="Customer")]
    
    public class PetController : Controller
    {
        private readonly IPetService _petService;

        public PetController(IPetService petService)
        {
            _petService = petService;
        }

        public async Task<IActionResult> MyPets([FromQuery] PagingQuery query)
        {
            int ownerId = GetCurrentUserId();

            var result = await _petService.GetMyPetsAsync(ownerId, query);

            var vm = new MyPetsVm
            {
                Query = query,
                Result = result
            };

            return View(vm);
        }

        // ====== CREATE ======
        [HttpGet]
        public IActionResult Create()
        {
            return View("AddPet", new CreatePetVm());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePetVm vm)
        {
            int ownerId = GetCurrentUserId();

            if (!ModelState.IsValid)
                return View("AddPet", vm);

            try
            {
                await _petService.CreatePetAsync(ownerId, vm);
                return RedirectToAction(nameof(MyPets));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("AddPet", vm);
            }
        }

        // ====== EDIT ======
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            int ownerId = GetCurrentUserId();

            var vm = await _petService.GetEditPetAsync(ownerId, id);
            if (vm == null) return NotFound();

            return View("EditPet", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditPetVm vm)
        {
            int ownerId = GetCurrentUserId();

            if (!ModelState.IsValid)
                return View("EditPet", vm);

            try
            {
                var ok = await _petService.UpdatePetAsync(ownerId, vm);
                if (!ok) return NotFound();

                return RedirectToAction(nameof(MyPets));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("EditPet", vm);
            }
        }

        // ====== DELETE ======
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            int ownerId = GetCurrentUserId();

            var ok = await _petService.DeletePetAsync(ownerId, id);
            if (!ok) return NotFound();

            return RedirectToAction(nameof(MyPets));
        }

        private int GetCurrentUserId()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            idStr ??= User.FindFirstValue("user_id");

            if (string.IsNullOrWhiteSpace(idStr) || !int.TryParse(idStr, out int id))
                throw new Exception("Không lấy được user_id từ Claims. Hãy kiểm tra Login tạo Claims.");

            return id;
        }
    }
}
