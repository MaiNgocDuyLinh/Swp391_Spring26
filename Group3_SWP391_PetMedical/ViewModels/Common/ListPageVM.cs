using Group3_SWP391_PetMedical.Models.Common;

namespace Group3_SWP391_PetMedical.ViewModels.Common
{
    public class ListPageVM<T>
    {
        public PagedResult<T> Data { get; set; } = new();
        public string? Q { get; set; }
    }
}
