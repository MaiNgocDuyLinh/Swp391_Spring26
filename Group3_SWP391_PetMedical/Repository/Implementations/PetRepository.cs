using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.Models.Common;
using Group3_SWP391_PetMedical.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Group3_SWP391_PetMedical.Repository.Implementations
{
    public class PetRepository : IPetRepository
    {
        private readonly PetClinicContext _context;

        public PetRepository(PetClinicContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Pet pet)
        {
            _context.Pets.Add(pet);
            await _context.SaveChangesAsync();
        }

        public async Task<PagedResult<Pet>> GetPetsByOwnerAsync(int ownerId, PagingQuery query)
        {
            var q = query.Q?.Trim();
            int page = query.Page <= 0 ? 1 : query.Page;
            int pageSize = query.PageSize <= 0 ? 9 : query.PageSize;

            IQueryable<Pet> pets = _context.Pets
                .AsNoTracking()
                .Where(p => p.owner_id == ownerId);

            if (!string.IsNullOrWhiteSpace(q))
            {
                var keyword = q.ToLower();
                pets = pets.Where(p =>
                    p.name.ToLower().Contains(keyword) ||
                    (p.species ?? "").ToLower().Contains(keyword) ||
                    (p.breed ?? "").ToLower().Contains(keyword)
                );
            }

            pets = pets.OrderByDescending(p => p.created_at).ThenByDescending(p => p.pet_id);

            int totalItems = await pets.CountAsync();
            var items = await pets
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Pet>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems
            };
        }

        // ==========================
        // NEW: Get pet by id + owner (for Edit/Delete security)
        // ==========================
        public async Task<Pet?> GetByIdAndOwnerAsync(int petId, int ownerId)
        {
            return await _context.Pets
                .FirstOrDefaultAsync(p => p.pet_id == petId && p.owner_id == ownerId);
        }

        // ==========================
        // NEW: Update pet
        // ==========================
        public async Task UpdateAsync(Pet pet)
        {
            _context.Pets.Update(pet);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(Pet pet)
        {
            _context.Pets.Remove(pet);
            await _context.SaveChangesAsync();
        }

    }
}
