namespace Group3_SWP391_PetMedical.ViewModels.Shopping
{
    public class StaffShoppingQuery
    {
        public string? Keyword { get; set; }
        public int? CategoryId { get; set; }
        public string? Status { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 6;
    }
}