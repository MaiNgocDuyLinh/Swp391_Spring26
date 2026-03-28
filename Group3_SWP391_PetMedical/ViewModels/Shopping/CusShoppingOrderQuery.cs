namespace Group3_SWP391_PetMedical.ViewModels.Shopping
{
    public class CusShoppingOrderQuery
    {
        public string? Q { get; set; }
        public string? PaymentStatus { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 6;
    }
}