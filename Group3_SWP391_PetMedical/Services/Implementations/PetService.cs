using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.Models.Common;
using Group3_SWP391_PetMedical.Repository.Interfaces;
using Group3_SWP391_PetMedical.Services.Interfaces;
using Group3_SWP391_PetMedical.ViewModels.Pet;
using Microsoft.AspNetCore.Hosting;
using System.Text.RegularExpressions;

namespace Group3_SWP391_PetMedical.Services.Implementations
{
    public class PetService : IPetService
    {
        private readonly IPetRepository _repo;
        private readonly IWebHostEnvironment _env;

        private const int DefaultPageSize = 6;
        private const int MaxPageSize = 50;

        public PetService(IPetRepository repo, IWebHostEnvironment env)
        {
            _repo = repo;
            _env = env;
        }

        public async Task<PagedResult<PetListItemVm>> GetMyPetsAsync(int ownerId, PagingQuery query)
        {
            // Normalize/validate ở Service
            var normalizedQuery = NormalizePagingQuery(query);

            var paged = await _repo.GetPetsByOwnerAsync(ownerId, normalizedQuery);

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
                    Age = p.age,
                    Weight = p.weight,
                    PetImg = p.PetImg,

                    // ADD: giới tính + ngày sinh + tuổi tự tăng (hiển thị năm-tháng-ngày)
                    PetGender = p.pet_gender,
                    PetBirthdate = p.pet_birthdate,
                    RealAgeText = BuildRealAgeText(p.pet_birthdate)

                }).ToList()
            };
        }

        //  CREATE 
        public async Task CreatePetAsync(int ownerId, CreatePetVm vm)
        {
            var pet = new Pet
            {
                owner_id = ownerId,
                name = (vm.Name ?? "").Trim(),
                species = (vm.Species ?? "").Trim(),
                breed = (vm.Breed ?? "").Trim(),
                age = vm.Age,
                weight = vm.Weight.HasValue ? (double)vm.Weight.Value : (double?)null,
                PetImg = null
            };

            //  lưu giới tính + ngày sinh (tuổi tự tăng xử lý khi hiển thị)
            pet.pet_gender = NormalizeGender(vm.PetGender);
            pet.pet_birthdate = vm.PetBirthdate?.Date;

            ValidateBirthdate(pet.pet_birthdate);

            if (pet.pet_birthdate.HasValue)
            {
                pet.age = CalculateAgeYmd(pet.pet_birthdate.Value.Date, DateTime.Today).years;
            }

            //  Validate trùng tên pet theo owner (ADD) 
            await EnsureUniquePetNameAsync(ownerId, pet.name, excludePetId: null);

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

        // edit -get
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
                Age = pet.age,
                Weight = pet.weight.HasValue ? (decimal)pet.weight.Value : (decimal?)null,
                CurrentPetImg = pet.PetImg,

                // ADD: giới tính + ngày sinh + tuổi tự tăng
                PetGender = pet.pet_gender,
                PetBirthdate = pet.pet_birthdate,
                RealAgeText = BuildRealAgeText(pet.pet_birthdate)
            };
        }

        //edit post
        public async Task<bool> UpdatePetAsync(int ownerId, EditPetVm vm)
        {
            var pet = await _repo.GetByIdAndOwnerAsync(vm.PetId, ownerId);
            if (pet == null) return false;

            pet.name = (vm.Name ?? "").Trim();
            pet.species = (vm.Species ?? "").Trim();
            pet.breed = (vm.Breed ?? "").Trim();
            pet.age = vm.Age;
            pet.weight = vm.Weight.HasValue ? (double)vm.Weight.Value : (double?)null;

            //   update giới tính + ngày sinh
            pet.pet_gender = NormalizeGender(vm.PetGender);
            pet.pet_birthdate = vm.PetBirthdate?.Date;

            ValidateBirthdate(pet.pet_birthdate);

            //  đồng bộ age theo ngày sinh (để không bị null/sai)
            if (pet.pet_birthdate.HasValue)
            {
                pet.age = CalculateAgeYmd(pet.pet_birthdate.Value.Date, DateTime.Today).years;
            }

            //Validate trùng tên pet theo owner (EDIT) - loại trừ chính nó
            await EnsureUniquePetNameAsync(ownerId, pet.name, excludePetId: vm.PetId);

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

        //delete
        public async Task<bool> DeletePetAsync(int ownerId, int petId)
        {
            var pet = await _repo.GetByIdAndOwnerAsync(petId, ownerId);
            if (pet == null) return false;

            DeleteOldPetImageIfAny(pet.PetImg);

            await _repo.DeleteAsync(pet);
            return true;
        }

        // VALIDATE / NORMALIZE

        private static PagingQuery NormalizePagingQuery(PagingQuery query)
        {
            // q: trim + gộp nhiều khoảng trắng thành 1 + rỗng => null
            var q = query.Q;
            if (string.IsNullOrWhiteSpace(q))
            {
                q = null;
            }
            else
            {
                q = Regex.Replace(q.Trim(), @"\s+", " ");
                if (q.Length == 0) q = null;
            }

            // page/pageSize: default + clamp
            var page = query.Page <= 0 ? 1 : query.Page;

            var pageSize = query.PageSize <= 0 ? DefaultPageSize : query.PageSize;
            pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

            // tạo object mới để không làm side-effect lên query gốc
            return new PagingQuery
            {
                Q = q,
                Page = page,
                PageSize = pageSize
            };
        }

        private static void ValidateImage(Microsoft.AspNetCore.Http.IFormFile file)
        {
            if (!file.ContentType.StartsWith("image/"))
                throw new Exception("Tệp tải lên không phải là hình ảnh.");

            const long maxBytes = 2 * 1024 * 1024;
            if (file.Length > maxBytes)
                throw new Exception("Ảnh quá lớn (tối đa 2MB).");
        }

        private void DeleteOldPetImageIfAny(string? petImg)
        {
            if (string.IsNullOrWhiteSpace(petImg)) return;

            if (petImg.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return;
            if (petImg.StartsWith("/")) return;

            var path = Path.Combine(_env.WebRootPath, "uploads", "pets", petImg);
            if (File.Exists(path))
                File.Delete(path);
        }

        // validate  không được có 2 pet trùng tên (không phân biệt hoa/thường, trim, gộp khoảng trắng)

        private static string NormalizePetName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "";
            var trimmed = name.Trim();
            trimmed = Regex.Replace(trimmed, @"\s+", " ");
            return trimmed.ToLowerInvariant();
        }

        private async Task EnsureUniquePetNameAsync(int ownerId, string? petName, int? excludePetId)
        {
            var normalized = NormalizePetName(petName);
            if (string.IsNullOrWhiteSpace(normalized))
                throw new Exception("Tên thú cưng không được để trống.");

            // Duyệt theo trang để lấy toàn bộ pets của owner  
            var page = 1;
            while (true)
            {
                var paged = await _repo.GetPetsByOwnerAsync(ownerId, new PagingQuery
                {
                    Q = null,                 // lấy tất cả để so sánh chính xác
                    Page = page,
                    PageSize = MaxPageSize
                });

                foreach (var p in paged.Items)
                {
                    if (excludePetId.HasValue && p.pet_id == excludePetId.Value) continue;

                    var existing = NormalizePetName(p.name);
                    if (existing == normalized)
                        throw new Exception("Tên thú cưng đã tồn tại. Vui lòng đặt tên khác.");
                }

                if (!paged.HasNext) break;
                page++;
            }
        }

       
        //   Gender + Real Age
       

        //  bắt buộc giới tính + chỉ cho phép giá trị hợp lệ
        private static string NormalizeGender(string? gender)
        {
            if (string.IsNullOrWhiteSpace(gender))
                throw new Exception("Vui lòng chọn giới tính.");

            var g = gender.Trim();

            if (g != "Đực" && g != "Cái" && g != "Không rõ")
                throw new Exception("Giới tính không hợp lệ.");

            return g;
        }

        // bắt buộc ngày sinh (tức tuổi không được để trống) + không được lớn hơn hôm nay
        private static void ValidateBirthdate(DateTime? birthdate)
        {
            if (!birthdate.HasValue)
                throw new Exception("Vui lòng nhập ngày sinh.");

            if (birthdate.Value.Date > DateTime.Today)
                throw new Exception("Ngày sinh không hợp lệ (không được lớn hơn ngày hiện tại).");
        }

        private static string? BuildRealAgeText(DateTime? birthdate)
        {
            if (!birthdate.HasValue) return null;

            var (y, m, d) = CalculateAgeYmd(birthdate.Value.Date, DateTime.Today);
            return $"{y} năm {m} tháng {d} ngày";
        }

        private static (int years, int months, int days) CalculateAgeYmd(DateTime birthDate, DateTime today)
        {
            birthDate = birthDate.Date;
            today = today.Date;

            if (birthDate > today) return (0, 0, 0);

            var years = today.Year - birthDate.Year;
            var lastBirthday = birthDate.AddYears(years);
            if (lastBirthday > today)
            {
                years--;
                lastBirthday = birthDate.AddYears(years);
            }

            var months = 0;
            var cursor = lastBirthday;
            while (cursor.AddMonths(1) <= today && months < 11)
            {
                cursor = cursor.AddMonths(1);
                months++;
            }

            var days = (today - cursor).Days;
            return (years, months, days);
        }

    }
}
