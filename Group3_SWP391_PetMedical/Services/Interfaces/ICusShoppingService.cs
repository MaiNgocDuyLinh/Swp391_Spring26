using Group3_SWP391_PetMedical.Models.Common;
using Group3_SWP391_PetMedical.ViewModels.Shopping;

namespace Group3_SWP391_PetMedical.Services.Interfaces
{
    public interface ICusShoppingService
    {
        Task<List<CusShoppingCategoryVM>> GetCategoriesAsync();
        Task<PagedResult<CusShoppingProductCardVM>> GetProductsAsync(CusShoppingQuery query);
        Task<CusShoppingDetailVM?> GetProductDetailAsync(int productId);

        Task<CusCartVM> GetCartAsync(int customerId);
        Task AddToCartAsync(int customerId, int productId, int? variantId, int quantity);
        Task UpdateCartItemAsync(int customerId, int cartItemId, int quantity);
        Task RemoveCartItemAsync(int customerId, int cartItemId);

        Task<CusCheckoutVM> GetCheckoutAsync(int customerId, List<int> selectedCartItemIds);
        Task<int> PlaceOrderAsync(int customerId, List<int> selectedCartItemIds, string? pickupNote, DateTime? pickupDate, string? paymentMethod);

        Task<CusShoppingMyOrdersVM> GetMyOrdersAsync(int customerId, CusShoppingOrderQuery query);
        Task<CusShoppingOrderDetailVM?> GetOrderDetailAsync(int customerId, int orderId);

        Task CancelOrderAsync(int customerId, int orderId);
        Task<int> AutoCancelExpiredOrdersAsync();
    }
}