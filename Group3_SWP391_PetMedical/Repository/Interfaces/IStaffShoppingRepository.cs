using Group3_SWP391_PetMedical.Models.Common;
using Group3_SWP391_PetMedical.ViewModels.Shopping;

namespace Group3_SWP391_PetMedical.Repository.Interfaces
{
    public interface IStaffShoppingRepository
    {
        Task<List<CusShoppingCategoryVM>> GetCategoriesAsync();

        Task<PagedResult<StaffShoppingProductRowVM>> GetProductsAsync(StaffShoppingQuery query);
        Task<StaffShoppingUpsertVM?> GetProductForEditAsync(int productId);
        Task<int> CreateProductAsync(StaffShoppingUpsertVM vm);
        Task UpdateProductAsync(StaffShoppingUpsertVM vm);
        Task StopSellingProductAsync(int productId);

        Task<PagedResult<StaffShoppingOrderRowVM>> GetOrdersAsync(StaffShoppingOrderQuery query);
        Task<StaffShoppingOrderDetailVM?> GetOrderDetailAsync(int orderId);
        Task UpdateOrderStatusAsync(StaffShoppingUpdateOrderStatusVM vm);
        Task<int> AutoCancelExpiredOrdersAsync();
    }
}