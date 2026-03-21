using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.Repository.Interfaces;
using Group3_SWP391_PetMedical.ViewModels.Retail;
using Microsoft.EntityFrameworkCore;

namespace Group3_SWP391_PetMedical.Repository.Implementations;

public class CartItemRepository : ICartItemRepository
{
    private readonly PetClinicContext _context;

    public CartItemRepository(PetClinicContext context)
    {
        _context = context;
    }

    public async Task<CartVm> GetOrCreateActiveCartAsync(int userId)
    {
        var cart = await _context.CartsMedicin
            .Include(c => c.CartItemsMedicin)
            .ThenInclude(ci => ci.medicine)
            .FirstOrDefaultAsync(c => c.user_id == userId && c.status == "ACTIVE");

        if (cart == null)
        {
            cart = new CartMedicin
            {
                user_id = userId,
                status = "ACTIVE",
                created_at = DateTime.UtcNow
            };
            _context.CartsMedicin.Add(cart);
            await _context.SaveChangesAsync();

            // re-load with items
            cart = await _context.CartsMedicin
                .Include(c => c.CartItemsMedicin)
                .ThenInclude(ci => ci.medicine)
                .FirstAsync(c => c.id == cart.id);
        }

        return Map(cart);
    }

    public async Task<CartVm> AddOrUpdateItemAsync(int userId, int medicineId, int quantity)
    {
        if (quantity <= 0) quantity = 1;

        var cart = await _context.CartsMedicin
            .Include(c => c.CartItemsMedicin)
            .FirstOrDefaultAsync(c => c.user_id == userId && c.status == "ACTIVE");

        if (cart == null)
        {
            cart = new CartMedicin
            {
                user_id = userId,
                status = "ACTIVE",
                created_at = DateTime.UtcNow
            };
            _context.CartsMedicin.Add(cart);
            await _context.SaveChangesAsync();
        }

        var item = await _context.CartItemsMedicin
            .FirstOrDefaultAsync(i => i.cart_id == cart.id && i.medicine_id == medicineId);

        if (item == null)
        {
            item = new CartItemMedicin
            {
                cart_id = cart.id,
                medicine_id = medicineId,
                quantity = quantity
            };
            _context.CartItemsMedicin.Add(item);
        }
        else
        {
            item.quantity += quantity;
        }

        await _context.SaveChangesAsync();
        return await LoadVm(cart.id);
    }

    public async Task<CartVm> UpdateQuantityAsync(int userId, int medicineId, int quantity)
    {
        var cart = await _context.CartsMedicin
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.user_id == userId && c.status == "ACTIVE");

        if (cart == null)
            return await GetOrCreateActiveCartAsync(userId);

        var item = await _context.CartItemsMedicin
            .FirstOrDefaultAsync(i => i.cart_id == cart.id && i.medicine_id == medicineId);

        if (item == null)
            return await LoadVm(cart.id);

        if (quantity <= 0)
        {
            _context.CartItemsMedicin.Remove(item);
        }
        else
        {
            item.quantity = quantity;
        }

        await _context.SaveChangesAsync();
        return await LoadVm(cart.id);
    }

    public async Task<CartVm> RemoveItemAsync(int userId, int medicineId)
    {
        var cart = await _context.CartsMedicin
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.user_id == userId && c.status == "ACTIVE");

        if (cart == null)
            return await GetOrCreateActiveCartAsync(userId);

        var item = await _context.CartItemsMedicin
            .FirstOrDefaultAsync(i => i.cart_id == cart.id && i.medicine_id == medicineId);

        if (item != null)
        {
            _context.CartItemsMedicin.Remove(item);
            await _context.SaveChangesAsync();
        }

        return await LoadVm(cart.id);
    }

    private async Task<CartVm> LoadVm(int cartId)
    {
        var cart = await _context.CartsMedicin
            .Include(c => c.CartItemsMedicin)
            .ThenInclude(ci => ci.medicine)
            .FirstAsync(c => c.id == cartId);

        return Map(cart);
    }

    private static CartVm Map(CartMedicin cart)
    {
        return new CartVm
        {
            cart_id = cart.id,
            user_id = cart.user_id,
            status = cart.status,
            items = cart.CartItemsMedicin
                .OrderBy(i => i.medicine.name)
                .Select(i => new CartItemVm
                {
                    medicine_id = i.medicine_id,
                    medicine_name = i.medicine.name,
                    unit_price = i.medicine.unit_price,
                    quantity = i.quantity,
                    stock_quantity = i.medicine.stock_quantity ?? 0
                })
                .ToList()
        };
    }
}

