using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.Repository.Interfaces;
using Group3_SWP391_PetMedical.ViewModels.Retail;
using Microsoft.EntityFrameworkCore;

namespace Group3_SWP391_PetMedical.Repository.Implementations;

public class CartRepository : ICartRepository
{
    private readonly PetClinicContext _context;

    public CartRepository(PetClinicContext context)
    {
        _context = context;
    }

    public async Task<CartVm> GetOrCreateActiveCartAsync(int userId)
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .ThenInclude(ci => ci.medicine)
            .FirstOrDefaultAsync(c => c.user_id == userId && c.status == "ACTIVE");

        if (cart == null)
        {
            cart = new Cart
            {
                user_id = userId,
                status = "ACTIVE",
                created_at = DateTime.UtcNow
            };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();

            // re-load with items
            cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.medicine)
                .FirstAsync(c => c.id == cart.id);
        }

        return Map(cart);
    }

    public async Task<CartVm> AddOrUpdateItemAsync(int userId, int medicineId, int quantity)
    {
        if (quantity <= 0) quantity = 1;

        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.user_id == userId && c.status == "ACTIVE");

        if (cart == null)
        {
            cart = new Cart
            {
                user_id = userId,
                status = "ACTIVE",
                created_at = DateTime.UtcNow
            };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
        }

        var item = await _context.CartItems
            .FirstOrDefaultAsync(i => i.cart_id == cart.id && i.medicine_id == medicineId);

        if (item == null)
        {
            item = new CartItem
            {
                cart_id = cart.id,
                medicine_id = medicineId,
                quantity = quantity
            };
            _context.CartItems.Add(item);
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
        var cart = await _context.Carts
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.user_id == userId && c.status == "ACTIVE");

        if (cart == null)
            return await GetOrCreateActiveCartAsync(userId);

        var item = await _context.CartItems
            .FirstOrDefaultAsync(i => i.cart_id == cart.id && i.medicine_id == medicineId);

        if (item == null)
            return await LoadVm(cart.id);

        if (quantity <= 0)
        {
            _context.CartItems.Remove(item);
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
        var cart = await _context.Carts
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.user_id == userId && c.status == "ACTIVE");

        if (cart == null)
            return await GetOrCreateActiveCartAsync(userId);

        var item = await _context.CartItems
            .FirstOrDefaultAsync(i => i.cart_id == cart.id && i.medicine_id == medicineId);

        if (item != null)
        {
            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();
        }

        return await LoadVm(cart.id);
    }

    private async Task<CartVm> LoadVm(int cartId)
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .ThenInclude(ci => ci.medicine)
            .FirstAsync(c => c.id == cartId);

        return Map(cart);
    }

    private static CartVm Map(Cart cart)
    {
        return new CartVm
        {
            cart_id = cart.id,
            user_id = cart.user_id,
            status = cart.status,
            items = cart.CartItems
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

