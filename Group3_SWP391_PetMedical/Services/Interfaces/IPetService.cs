using Group3_SWP391_PetMedical.Models.Common;
using Group3_SWP391_PetMedical.ViewModels;
using Group3_SWP391_PetMedical.ViewModels.Pet;

namespace Group3_SWP391_PetMedical.Services.Interfaces
{
    public interface IPetService
    {
        Task<PagedResult<PetListItemVm>> GetMyPetsAsync(int ownerId, PagingQuery query);

        // Create
        Task CreatePetAsync(int ownerId, CreatePetVm vm);

        // Edit
        Task<EditPetVm?> GetEditPetAsync(int ownerId, int petId);
        Task<bool> UpdatePetAsync(int ownerId, EditPetVm vm);

        // Delete
        Task<bool> DeletePetAsync(int ownerId, int petId);
    }
}
