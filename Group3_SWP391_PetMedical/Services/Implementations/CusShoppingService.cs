using Group3_SWP391_PetMedical.Models.Common;
using Group3_SWP391_PetMedical.Repository.Interfaces;
using Group3_SWP391_PetMedical.Services.Interfaces;
using Group3_SWP391_PetMedical.ViewModels.Shopping;

namespace Group3_SWP391_PetMedical.Services.Implementations
{
    public class CusShoppingService : ICusShoppingService
    {
        private readonly ICusShoppingRepository _repository;
        private const string FixedPaymentMethod = "Thanh toán tại quầy";

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

        public async Task<CusCheckoutVM> GetCheckoutAsync(int customerId, List<int> selectedCartItemIds)
        {
            var vm = await _repository.GetCheckoutAsync(customerId, selectedCartItemIds);
            vm.PaymentMethod = FixedPaymentMethod;
            return vm;
        }

        public async Task<int> PlaceOrderAsync(
            int customerId,
            List<int> selectedCartItemIds,
            string? pickupNote,
            DateTime? pickupDate,
            string? paymentMethod)
        {
            ValidateCheckout(selectedCartItemIds, pickupDate);

            return await _repository.PlaceOrderAsync(
                customerId,
                selectedCartItemIds,
                pickupNote,
                pickupDate?.Date,
                FixedPaymentMethod);
        }

        public Task<CusShoppingMyOrdersVM> GetMyOrdersAsync(int customerId, CusShoppingOrderQuery query)
            => _repository.GetMyOrdersAsync(customerId, query);

        public Task<CusShoppingOrderDetailVM?> GetOrderDetailAsync(int customerId, int orderId)
            => _repository.GetOrderDetailAsync(customerId, orderId);

        public Task CancelOrderAsync(int customerId, int orderId)
            => _repository.CancelOrderAsync(customerId, orderId);

        //auto huy lich
        public Task<int> AutoCancelExpiredOrdersAsync()
            => _repository.AutoCancelExpiredOrdersAsync();

        private static void ValidateCheckout(List<int> selectedCartItemIds, DateTime? pickupDate)
        {
            var selectedIds = selectedCartItemIds?.Distinct().ToList() ?? new List<int>();
            if (!selectedIds.Any())
            {
                throw new Exception("Vui lòng chọn ít nhất 1 sản phẩm.");
            }

            if (!pickupDate.HasValue)
            {
                throw new Exception("Vui lòng chọn ngày nhận.");
            }

            var selectedDate = pickupDate.Value.Date;
            var minDate = DateTime.Today.AddDays(1);

            if (selectedDate < minDate)
            {
                throw new Exception("Ngày nhận phải từ ngày mai trở đi.");
            }
        }
    }
}