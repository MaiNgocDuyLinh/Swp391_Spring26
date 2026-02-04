using Group3_SWP391_PetMedical.Models;
using Group3_SWP391_PetMedical.Models.Common;

namespace Group3_SWP391_PetMedical.Repository.Interfaces
{
    public interface IPetRepository
    {
        Task<PagedResult<Pet>> GetPetsByOwnerAsync(int ownerId, PagingQuery query);

        Task AddAsync(Pet pet);
        Task<Pet?> GetByIdAndOwnerAsync(int petId, int ownerId);
        Task UpdateAsync(Pet pet);
        Task DeleteAsync(Pet pet);
    }
}
