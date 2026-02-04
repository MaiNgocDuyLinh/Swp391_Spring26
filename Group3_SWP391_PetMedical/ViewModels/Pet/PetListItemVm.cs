namespace Group3_SWP391_PetMedical.ViewModels.Pet
{
    public class PetListItemVm
    {
        public int PetId { get; set; }
        public string Name { get; set; } = "";
        public string? Species { get; set; }
        public string? Breed { get; set; }
        public int? Age { get; set; }
        public double? Weight { get; set; }
        public string? PetImg { get; set; } // lưu filename hoặc path
    }
}
