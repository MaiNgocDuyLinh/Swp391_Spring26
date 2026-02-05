using System.ComponentModel.DataAnnotations;
using Group3_SWP391_PetMedical.Models;

namespace Group3_SWP391_PetMedical.ViewModels.Account
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

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập.")]
        [StringLength(50, ErrorMessage = "Tên đăng nhập không được vượt quá 50 ký tự.")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
        [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự.")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự.")]
        [StringLength(255, ErrorMessage = "Mật khẩu không được vượt quá 255 ký tự.")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu.")]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        public string RePassword { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng đồng ý với điều khoản.")]
        public bool AgreeTerm { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại.")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
        [MinLength(6, ErrorMessage = "Mật khẩu mới tối thiểu 6 ký tự.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu mới.")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = null!;

        /// <summary>User info for sidebar (avatar, email, ...). Optional when only rendering form.</summary>
        public User? User { get; set; }
    }
}
