namespace Group3_SWP391_PetMedical.Models.Common
{
    public class PagingQuery
    {
        public string? Q { get; set; }          // keyword
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 9;  // default
    }
}
