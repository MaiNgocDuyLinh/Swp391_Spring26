using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Group3_SWP391_PetMedical.ViewModels.Pet
{
    public class EditPetVm
    {
        public int PetId { get; set; }

        public string? CurrentPetImg { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên thú cưng")]
        [StringLength(100, ErrorMessage = "Tên thú cưng tối đa 100 ký tự")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập loài")]
        [StringLength(50, ErrorMessage = "Loài tối đa 50 ký tự")]
        public string Species { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập giống")]
        [StringLength(50, ErrorMessage = "Giống tối đa 50 ký tự")]
        public string Breed { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập cân nặng")]
        [Range(0.01, 500, ErrorMessage = "Cân nặng phải lớn hơn 0 và nhỏ hơn hoặc bằng 500kg")]
        public decimal? Weight { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tuổi")]
        [Range(0, 200, ErrorMessage = "Tuổi phải từ 0 đến 200")]
        public int? Age { get; set; }

        // Ảnh mới KHÔNG bắt buộc
        public IFormFile? NewPetImage { get; set; }
    }
}
