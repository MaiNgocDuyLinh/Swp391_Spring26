using Group3_SWP391_PetMedical.ViewModels.Retail;

namespace Group3_SWP391_PetMedical.Services.Interfaces;

public interface ICartService
{
    Task<CartVm> GetOrCreateActiveCartAsync(int userId);
    Task<CartVm> AddToCartAsync(int userId, int medicineId, int quantity);
    Task<CartVm> UpdateQuantityAsync(int userId, int medicineId, int quantity);
    Task<CartVm> RemoveItemAsync(int userId, int medicineId);
}

