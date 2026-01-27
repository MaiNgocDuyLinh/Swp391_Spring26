using System.ComponentModel.DataAnnotations;

namespace Group3_SWP391_PetMedical.ViewModels
{
    public class AccountViewModel
    {
        public int UserId { get; set; }
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? Phone { get; set; }
        public string RoleName { get; set; } = null!;
        public int RoleId { get; set; }
        public string Status { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }
    }

    public class AccountListViewModel
    {
        public List<AccountViewModel> Accounts { get; set; } = new();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public string? RoleFilter { get; set; }
        public string? StatusFilter { get; set; }
        public List<RoleFilterOption> AvailableRoles { get; set; } = new();
    }

    public class RoleFilterOption
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = null!;
    }

    public class CreateAccountViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự.")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
        [StringLength(100)]
        public string FullName { get; set; } = null!;

        [StringLength(20)]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn vai trò.")]
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn vai trò.")]
        public int RoleId { get; set; }
        public List<RoleOption> AvailableRoles { get; set; } = new();
    }

    public class EditAccountViewModel
    {
        public int UserId { get; set; }
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
        [StringLength(100)]
        public string FullName { get; set; } = null!;

        [StringLength(20)]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn vai trò.")]
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn vai trò.")]
        public int RoleId { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = null!;
        public List<RoleOption> AvailableRoles { get; set; } = new();
    }

    public class RoleOption
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = null!;
    }
}
