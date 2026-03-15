using Group3_SWP391_PetMedical.Models.Common;
using Group3_SWP391_PetMedical.Repository.Interfaces;
using Group3_SWP391_PetMedical.Services.Interfaces;
using Group3_SWP391_PetMedical.ViewModels.Shopping;

namespace Group3_SWP391_PetMedical.Services.Implementations
{
    public class CusShoppingService : ICusShoppingService
    {
        private readonly ICusShoppingRepository _repository;

        public CusShoppingService(ICusShoppingRepository repository)
        {
            _repository = repository;
        }

        public Task<List<CusShoppingCategoryVM>> GetCategoriesAsync()
            => _repository.GetCategoriesAsync();

        public Task<PagedResult<CusShoppingProductCardVM>> GetProductsAsync(CusShoppingQuery query)
            => _repository.GetProductsAsync(query);

        public Task<CusShoppingDetailVM?> GetProductDetailAsync(int productId)
            => _repository.GetProductDetailAsync(productId);

        public Task<CusCartVM> GetCartAsync(int customerId)
            => _repository.GetCartAsync(customerId);

        public Task AddToCartAsync(int customerId, int productId, int? variantId, int quantity)
            => _repository.AddToCartAsync(customerId, productId, variantId, quantity);

        public Task UpdateCartItemAsync(int customerId, int cartItemId, int quantity)
            => _repository.UpdateCartItemAsync(customerId, cartItemId, quantity);

        public Task RemoveCartItemAsync(int customerId, int cartItemId)
            => _repository.RemoveCartItemAsync(customerId, cartItemId);

        public Task<CusCheckoutVM> GetCheckoutAsync(int customerId)
            => _repository.GetCheckoutAsync(customerId);

        public Task<int> PlaceOrderAsync(int customerId, string? pickupNote, DateTime? pickupDate, string? paymentMethod)
            => _repository.PlaceOrderAsync(customerId, pickupNote, pickupDate, paymentMethod);

        public Task<CusShoppingMyOrdersVM> GetMyOrdersAsync(int customerId, CusShoppingOrderQuery query)
            => _repository.GetMyOrdersAsync(customerId, query);

        public Task<CusShoppingOrderDetailVM?> GetOrderDetailAsync(int customerId, int orderId)
            => _repository.GetOrderDetailAsync(customerId, orderId);
    }
}