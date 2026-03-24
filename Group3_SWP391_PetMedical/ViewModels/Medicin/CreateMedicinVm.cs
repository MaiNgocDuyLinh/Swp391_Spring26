using System.ComponentModel.DataAnnotations;

namespace Group3_SWP391_PetMedical.ViewModels.Medicin
{
    public class CreateMedicinVm
    {
        [Required(ErrorMessage = "Tên thuốc không được để trống.")]
        [StringLength(150, ErrorMessage = "Tên thuốc tối đa 150 ký tự.")]
        public string name { get; set; } = string.Empty;

        [Range(typeof(decimal), "0.01", "999999999", ErrorMessage = "Đơn giá phải lớn hơn 0.")]
        public decimal unit_price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Số lượng tồn kho phải từ 0 trở lên.")]
        public int stock_quantity { get; set; }

        [StringLength(1000, ErrorMessage = "Mô tả tối đa 1000 ký tự.")]
        public string? description { get; set; }
        public string status { get; set; } = "active";
    }
}
