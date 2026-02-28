using System.Collections.Generic;

namespace Group3_SWP391_PetMedical.ViewModels.Admin
{
    /// <summary>
    /// Thông tin 1 role dùng cho sidebar chọn role trên màn phân quyền.
    /// </summary>
    public class RolePermissionRoleItem
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = null!;
    }

    /// <summary>
    /// Quyền trên 1 màn hình (screen) cho 1 role.
    /// </summary>
    public class ScreenPermissionItem
    {
        public string ScreenKey { get; set; } = null!;
        public string ScreenName { get; set; } = null!;

        public bool CanView { get; set; }
        public bool CanCreate { get; set; }
        public bool CanUpdate { get; set; }
        public bool CanBan { get; set; }
    }

    /// <summary>
    /// ViewModel chính cho màn hình "Phân quyền theo màn hình".
    /// </summary>
    public class RolePermissionScreenViewModel
    {
        public int SelectedRoleId { get; set; }
        public string SelectedRoleName { get; set; } = null!;

        public List<RolePermissionRoleItem> Roles { get; set; } = new();
        public List<ScreenPermissionItem> Screens { get; set; } = new();
    }
}

