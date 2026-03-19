using Group3_SWP391_PetMedical.Models.Common;
using Group3_SWP391_PetMedical.Repository.Interfaces;
using Group3_SWP391_PetMedical.Services.Interfaces;
using Group3_SWP391_PetMedical.ViewModels.Shopping;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Group3_SWP391_PetMedical.Services.Implementations
{
    public class StaffShoppingService : IStaffShoppingService
    {
        private readonly IStaffShoppingRepository _repository;
        private readonly IWebHostEnvironment _environment;

        private static readonly string[] ValidProductStatuses =
        {
            "Đang bán",
            "Dừng bán",
            "Hết hàng"
        };

        private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".webp"
        };

        private const long MaxImageSizeBytes = 5 * 1024 * 1024;

        public StaffShoppingService(
            IStaffShoppingRepository repository,
            IWebHostEnvironment environment)
        {
            _repository = repository;
            _environment = environment;
        }

        public Task<List<CusShoppingCategoryVM>> GetCategoriesAsync()
            => _repository.GetCategoriesAsync();

        public Task<PagedResult<StaffShoppingProductRowVM>> GetProductsAsync(StaffShoppingQuery query)
            => _repository.GetProductsAsync(query);

        public Task<StaffShoppingUpsertVM?> GetProductForEditAsync(int productId)
            => _repository.GetProductForEditAsync(productId);

        public async Task<int> CreateProductAsync(StaffShoppingUpsertVM vm)
        {
            NormalizeVm(vm);

            var errors = ValidateProductVm(vm, isEdit: false, currentImageUrl: null);
            if (errors.Any())
                throw new Exception(BuildValidationMessage(errors));

            vm.ImageUrl = await SaveProductImageAsync(vm.ImageFile, null);

            return await _repository.CreateProductAsync(vm);
        }

        public async Task UpdateProductAsync(StaffShoppingUpsertVM vm)
        {
            NormalizeVm(vm);

            if (!vm.ProductId.HasValue || vm.ProductId.Value <= 0)
                throw new Exception("ProductId##Thiếu mã sản phẩm.");

            var current = await _repository.GetProductForEditAsync(vm.ProductId.Value);
            if (current == null)
                throw new Exception("ProductId##Sản phẩm không tồn tại.");

            var errors = ValidateProductVm(vm, isEdit: true, currentImageUrl: current.ImageUrl);
            if (errors.Any())
                throw new Exception(BuildValidationMessage(errors));

            vm.ImageUrl = await SaveProductImageAsync(vm.ImageFile, current.ImageUrl);

            await _repository.UpdateProductAsync(vm);
        }

        public Task StopSellingProductAsync(int productId)
            => _repository.StopSellingProductAsync(productId);

        public Task<PagedResult<StaffShoppingOrderRowVM>> GetOrdersAsync(StaffShoppingOrderQuery query)
            => _repository.GetOrdersAsync(query);

        public Task<StaffShoppingOrderDetailVM?> GetOrderDetailAsync(int orderId)
            => _repository.GetOrderDetailAsync(orderId);

        public Task UpdateOrderStatusAsync(StaffShoppingUpdateOrderStatusVM vm)
            => _repository.UpdateOrderStatusAsync(vm);

        public Task<int> AutoCancelExpiredOrdersAsync()
            => _repository.AutoCancelExpiredOrdersAsync();

        private void NormalizeVm(StaffShoppingUpsertVM vm)
        {
            vm.Name = string.IsNullOrWhiteSpace(vm.Name) ? null : vm.Name.Trim();
            vm.SKU = string.IsNullOrWhiteSpace(vm.SKU) ? null : vm.SKU.Trim();
            vm.Status = string.IsNullOrWhiteSpace(vm.Status) ? "Đang bán" : vm.Status.Trim();
            vm.Description = string.IsNullOrWhiteSpace(vm.Description) ? null : vm.Description.Trim();
            vm.ImageUrl = string.IsNullOrWhiteSpace(vm.ImageUrl) ? null : vm.ImageUrl.Trim();

            vm.Variants ??= new List<StaffShoppingVariantInputVM>();

            vm.Variants = vm.Variants
                .Where(HasVariantContent)
                .Select(x =>
                {
                    x.VariantName = string.IsNullOrWhiteSpace(x.VariantName) ? null : x.VariantName.Trim();
                    x.Color = string.IsNullOrWhiteSpace(x.Color) ? null : x.Color.Trim();
                    x.Size = string.IsNullOrWhiteSpace(x.Size) ? null : x.Size.Trim();
                    x.Material = string.IsNullOrWhiteSpace(x.Material) ? null : x.Material.Trim();
                    x.SKU = string.IsNullOrWhiteSpace(x.SKU) ? null : x.SKU.Trim();
                    x.Status = string.IsNullOrWhiteSpace(x.Status) ? "Đang bán" : x.Status.Trim();
                    x.ImageUrl = null;
                    return x;
                })
                .ToList();
        }

        private static bool HasVariantContent(StaffShoppingVariantInputVM x)
        {
            return x.VariantId.HasValue
                   || !string.IsNullOrWhiteSpace(x.VariantName)
                   || !string.IsNullOrWhiteSpace(x.Color)
                   || !string.IsNullOrWhiteSpace(x.Size)
                   || !string.IsNullOrWhiteSpace(x.Material)
                   || !string.IsNullOrWhiteSpace(x.SKU)
                   || x.PriceOverride.HasValue
                   || x.StockQuantity > 0;
        }

        private List<(string Key, string Message)> ValidateProductVm(
            StaffShoppingUpsertVM vm,
            bool isEdit,
            string? currentImageUrl)
        {
            var errors = new List<(string Key, string Message)>();

            if (vm.CategoryId <= 0)
                errors.Add(("CategoryId", "Vui lòng chọn danh mục."));

            if (string.IsNullOrWhiteSpace(vm.Name))
                errors.Add(("Name", "Vui lòng nhập tên sản phẩm."));
            else if (vm.Name.Length > 200)
                errors.Add(("Name", "Tên sản phẩm không được vượt quá 200 ký tự."));

            if (string.IsNullOrWhiteSpace(vm.SKU))
                errors.Add(("SKU", "Vui lòng nhập mã sản phẩm."));
            else if (vm.SKU.Length > 50)
                errors.Add(("SKU", "Mã sản phẩm không được vượt quá 50 ký tự."));

            if (vm.Price <= 0)
                errors.Add(("Price", "Giá sản phẩm phải lớn hơn 0."));

            if (vm.StockQuantity < 0)
                errors.Add(("StockQuantity", "Tồn kho không được nhỏ hơn 0."));

            if (string.IsNullOrWhiteSpace(vm.Status) || !ValidProductStatuses.Contains(vm.Status))
                errors.Add(("Status", "Trạng thái sản phẩm không hợp lệ."));

            if (!string.IsNullOrWhiteSpace(vm.Description) && vm.Description.Length > 4000)
                errors.Add(("Description", "Mô tả sản phẩm không được vượt quá 4000 ký tự."));

            if (!isEdit && vm.ImageFile == null)
                errors.Add(("ImageFile", "Vui lòng chọn ảnh sản phẩm."));

            if (isEdit && vm.ImageFile == null && string.IsNullOrWhiteSpace(currentImageUrl))
                errors.Add(("ImageFile", "Vui lòng chọn ảnh sản phẩm."));

            ValidateImage(vm.ImageFile, errors);

            var duplicateVariantSkus = vm.Variants
                .Where(x => !string.IsNullOrWhiteSpace(x.SKU))
                .GroupBy(x => x.SKU!, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            foreach (var duplicateSku in duplicateVariantSkus)
            {
                errors.Add(("SKU", $"Mã phân loại '{duplicateSku}' đang bị trùng."));
            }

            for (int i = 0; i < vm.Variants.Count; i++)
            {
                var item = vm.Variants[i];
                var keyPrefix = $"Variants[{i}]";

                if (string.IsNullOrWhiteSpace(item.VariantName))
                    errors.Add(($"{keyPrefix}.VariantName", $"Vui lòng nhập tên phân loại {i + 1}."));

                if (!string.IsNullOrWhiteSpace(item.SKU) && item.SKU.Length > 50)
                    errors.Add(($"{keyPrefix}.SKU", $"Mã phân loại {i + 1} không được vượt quá 50 ký tự."));

                if (item.PriceOverride.HasValue && item.PriceOverride.Value < 0)
                    errors.Add(($"{keyPrefix}.PriceOverride", $"Giá phân loại {i + 1} không được nhỏ hơn 0."));

                if (item.StockQuantity < 0)
                    errors.Add(($"{keyPrefix}.StockQuantity", $"Tồn kho phân loại {i + 1} không được nhỏ hơn 0."));

                if (string.IsNullOrWhiteSpace(item.Status) || !ValidProductStatuses.Contains(item.Status))
                    errors.Add(($"{keyPrefix}.Status", $"Trạng thái phân loại {i + 1} không hợp lệ."));
            }

            return errors;
        }

        private void ValidateImage(IFormFile? imageFile, List<(string Key, string Message)> errors)
        {
            if (imageFile == null || imageFile.Length <= 0)
                return;

            var extension = Path.GetExtension(imageFile.FileName);
            if (string.IsNullOrWhiteSpace(extension) || !AllowedImageExtensions.Contains(extension))
                errors.Add(("ImageFile", "Ảnh sản phẩm chỉ chấp nhận định dạng jpg, jpeg, png hoặc webp."));

            if (imageFile.Length > MaxImageSizeBytes)
                errors.Add(("ImageFile", "Ảnh sản phẩm không được vượt quá 5MB."));
        }

        private async Task<string?> SaveProductImageAsync(IFormFile? imageFile, string? currentImageUrl)
        {
            if (imageFile == null || imageFile.Length <= 0)
                return currentImageUrl;

            var folder = Path.Combine(_environment.WebRootPath, "uploads", "Shopping");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
            var fileName = $"product_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid():N}{extension}";
            var fullPath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            DeleteOldLocalImage(currentImageUrl);

            return $"/uploads/Shopping/{fileName}";
        }

        private void DeleteOldLocalImage(string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                return;

            var normalized = imageUrl.Replace("~/", "/").Trim();

            if (!normalized.StartsWith("/uploads/Shopping/", StringComparison.OrdinalIgnoreCase))
                return;

            var fullPath = Path.Combine(
                _environment.WebRootPath,
                normalized.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

            if (File.Exists(fullPath))
            {
                try
                {
                    File.Delete(fullPath);
                }
                catch
                {
                }
            }
        }

        private string BuildValidationMessage(List<(string Key, string Message)> errors)
        {
            return string.Join("||", errors.Select(x => $"{x.Key}##{x.Message}"));
        }
    }
}