using Group3_SWP391_PetMedical.Repository.Interfaces;
using Group3_SWP391_PetMedical.Services.Interfaces;
using Group3_SWP391_PetMedical.ViewModels.Retail;

namespace Group3_SWP391_PetMedical.Services.Implementations;

public class CartItemService : ICartItemService
{
    private readonly ICartItemRepository _repo;
    private readonly IMedicinService _medicinService;

    public CartItemService(ICartItemRepository repo, IMedicinService medicinService)
    {
        _repo = repo;
        _medicinService = medicinService;
    }

    public Task<CartVm> GetOrCreateActiveCartAsync(int userId)
        => _repo.GetOrCreateActiveCartAsync(userId);

    public async Task<CartVm> AddToCartAsync(int userId, int medicineId, int quantity)
    {
        if (quantity <= 0) quantity = 1;

        var med = await _medicinService.GetByIdAsync(medicineId);
        if (med == null || (med.stock_quantity ?? 0) <= 0)
            return await _repo.GetOrCreateActiveCartAsync(userId);

        return await _repo.AddOrUpdateItemAsync(userId, medicineId, quantity);
    }

    public async Task<CartVm> UpdateQuantityAsync(int userId, int medicineId, int quantity)
    {
        if (quantity <= 0)
            return await _repo.RemoveItemAsync(userId, medicineId);

        var med = await _medicinService.GetByIdAsync(medicineId);
        var stock = med?.stock_quantity ?? 0;
        if (stock <= 0)
            return await _repo.RemoveItemAsync(userId, medicineId);

        if (quantity > stock)
            quantity = stock;

        return await _repo.UpdateQuantityAsync(userId, medicineId, quantity);
    }

    public Task<CartVm> RemoveItemAsync(int userId, int medicineId)
        => _repo.RemoveItemAsync(userId, medicineId);
}

