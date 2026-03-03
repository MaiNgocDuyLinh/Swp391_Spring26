using System.ComponentModel.DataAnnotations;

namespace Group3_SWP391_PetMedical.ViewModels.ForgetPassword
{
    public class SendResetOtpVm
    {
        [Required(ErrorMessage = "Vui lòng nhập tài khoản.")]
        public string Username { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string Email { get; set; } = "";
    }
}

