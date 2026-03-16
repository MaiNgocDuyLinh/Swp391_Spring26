using Group3_SWP391_PetMedical.Models.Common;
using Group3_SWP391_PetMedical.Repository.Interfaces;
using Group3_SWP391_PetMedical.Services.Interfaces;
using Group3_SWP391_PetMedical.ViewModels.Shopping;

namespace Group3_SWP391_PetMedical.Services.Implementations
{
    public class StaffShoppingService : IStaffShoppingService
    {
        private readonly IStaffShoppingRepository _repository;

        public StaffShoppingService(IStaffShoppingRepository repository)
        {
            _repository = repository;
        }

        public Task<List<CusShoppingCategoryVM>> GetCategoriesAsync()
            => _repository.GetCategoriesAsync();

        public Task<PagedResult<StaffShoppingProductRowVM>> GetProductsAsync(StaffShoppingQuery query)
            => _repository.GetProductsAsync(query);

        public Task<StaffShoppingUpsertVM?> GetProductForEditAsync(int productId)
            => _repository.GetProductForEditAsync(productId);

        public Task<int> CreateProductAsync(StaffShoppingUpsertVM vm)
            => _repository.CreateProductAsync(vm);

        public Task UpdateProductAsync(StaffShoppingUpsertVM vm)
            => _repository.UpdateProductAsync(vm);

        public Task StopSellingProductAsync(int productId)
            => _repository.StopSellingProductAsync(productId);

        public Task<PagedResult<StaffShoppingOrderRowVM>> GetOrdersAsync(StaffShoppingOrderQuery query)
            => _repository.GetOrdersAsync(query);

        public Task<StaffShoppingOrderDetailVM?> GetOrderDetailAsync(int orderId)
            => _repository.GetOrderDetailAsync(orderId);

        public Task UpdateOrderStatusAsync(StaffShoppingUpdateOrderStatusVM vm)
            => _repository.UpdateOrderStatusAsync(vm);
    }
}