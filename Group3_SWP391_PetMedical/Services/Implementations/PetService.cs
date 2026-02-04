using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.Models.Common;
using Group3_SWP391_PetMedical.Repository.Interfaces;
using Group3_SWP391_PetMedical.Services.Interfaces;
using Group3_SWP391_PetMedical.ViewModels;
using Group3_SWP391_PetMedical.ViewModels.Pet;
using Microsoft.AspNetCore.Hosting;

namespace Group3_SWP391_PetMedical.Services.Implementations
{
    public class PetService : IPetService
    {
        private readonly IPetRepository _repo;
        private readonly IWebHostEnvironment _env;

        public PetService(IPetRepository repo, IWebHostEnvironment env)
        {
            _repo = repo;
            _env = env;
        }

        public async Task<PagedResult<PetListItemVm>> GetMyPetsAsync(int ownerId, PagingQuery query)
        {
            var paged = await _repo.GetPetsByOwnerAsync(ownerId, query);

            return new PagedResult<PetListItemVm>
            {
                Page = paged.Page,
                PageSize = paged.PageSize,
                TotalItems = paged.TotalItems,
                Items = paged.Items.Select(p => new PetListItemVm
                {
                    PetId = p.pet_id,
                    Name = p.name,
                    Species = p.species,
                    Breed = p.breed,
                    Age = p.age,              // int?
                    Weight = p.weight,        // double?
                    PetImg = p.PetImg
                }).ToList()
            };
        }

        // ===== CREATE =====
        public async Task CreatePetAsync(int ownerId, CreatePetVm vm)
        {
            var pet = new Pet
            {
                owner_id = ownerId,
                name = (vm.Name ?? "").Trim(),
                species = (vm.Species ?? "").Trim(),
                breed = (vm.Breed ?? "").Trim(),
                age = vm.Age, // int? (năm chẵn)
                weight = vm.Weight.HasValue ? (double)vm.Weight.Value : (double?)null,
                PetImg = null
            };

            // Ảnh KHÔNG bắt buộc: có thì upload, không có thì thôi
            if (vm.PetImage != null && vm.PetImage.Length > 0)
            {
                ValidateImage(vm.PetImage);

                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "pets");
                Directory.CreateDirectory(uploadsFolder);

                var ext = Path.GetExtension(vm.PetImage.FileName);
                var newFileName = $"{Guid.NewGuid()}{ext}";
                var fullPath = Path.Combine(uploadsFolder, newFileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await vm.PetImage.CopyToAsync(stream);
                }

                pet.PetImg = newFileName;
            }

            await _repo.AddAsync(pet);
        }

        // ===== EDIT (GET) =====
        public async Task<EditPetVm?> GetEditPetAsync(int ownerId, int petId)
        {
            var pet = await _repo.GetByIdAndOwnerAsync(petId, ownerId);
            if (pet == null) return null;

            return new EditPetVm
            {
                PetId = pet.pet_id,
                Name = pet.name,
                Species = pet.species ?? "",
                Breed = pet.breed ?? "",
                Age = pet.age, // int?
                Weight = pet.weight.HasValue ? (decimal)pet.weight.Value : (decimal?)null,
                CurrentPetImg = pet.PetImg
            };
        }

        // ===== EDIT (POST) =====
        public async Task<bool> UpdatePetAsync(int ownerId, EditPetVm vm)
        {
            var pet = await _repo.GetByIdAndOwnerAsync(vm.PetId, ownerId);
            if (pet == null) return false;

            pet.name = (vm.Name ?? "").Trim();
            pet.species = (vm.Species ?? "").Trim();
            pet.breed = (vm.Breed ?? "").Trim();
            pet.age = vm.Age; // int?
            pet.weight = vm.Weight.HasValue ? (double)vm.Weight.Value : (double?)null;

            if (vm.NewPetImage != null && vm.NewPetImage.Length > 0)
            {
                ValidateImage(vm.NewPetImage);

                DeleteOldPetImageIfAny(pet.PetImg);

                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "pets");
                Directory.CreateDirectory(uploadsFolder);

                var ext = Path.GetExtension(vm.NewPetImage.FileName);
                var newFileName = $"{Guid.NewGuid()}{ext}";
                var fullPath = Path.Combine(uploadsFolder, newFileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await vm.NewPetImage.CopyToAsync(stream);
                }

                pet.PetImg = newFileName;
            }

            await _repo.UpdateAsync(pet);
            return true;
        }

        // ===== DELETE =====
        public async Task<bool> DeletePetAsync(int ownerId, int petId)
        {
            var pet = await _repo.GetByIdAndOwnerAsync(petId, ownerId);
            if (pet == null) return false;

            DeleteOldPetImageIfAny(pet.PetImg);

            await _repo.DeleteAsync(pet);
            return true;
        }

        // ===== HELPERS =====

        private static void ValidateImage(Microsoft.AspNetCore.Http.IFormFile file)
        {
            if (!file.ContentType.StartsWith("image/"))
                throw new Exception("Tệp tải lên không phải là hình ảnh.");

            const long maxBytes = 2 * 1024 * 1024; // 2MB
            if (file.Length > maxBytes)
                throw new Exception("Ảnh quá lớn (tối đa 2MB).");
        }

        private void DeleteOldPetImageIfAny(string? petImg)
        {
            if (string.IsNullOrWhiteSpace(petImg)) return;

            // nếu lưu URL hoặc path tuyệt đối thì không xóa
            if (petImg.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return;
            if (petImg.StartsWith("/")) return;

            var path = Path.Combine(_env.WebRootPath, "uploads", "pets", petImg);
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}
