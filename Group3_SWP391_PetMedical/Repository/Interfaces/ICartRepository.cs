using Group3_SWP391_PetMedical.ViewModels.Retail;

namespace Group3_SWP391_PetMedical.Repository.Interfaces;

public interface ICartRepository
{
    Task<CartVm> GetOrCreateActiveCartAsync(int userId);
    Task<CartVm> AddOrUpdateItemAsync(int userId, int medicineId, int quantity);
    Task<CartVm> UpdateQuantityAsync(int userId, int medicineId, int quantity);
    Task<CartVm> RemoveItemAsync(int userId, int medicineId);
}

