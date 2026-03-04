using System;

namespace Group3_SWP391_PetMedical.ViewModels.Admin
{
    public class AuditLogListItemVM
    {
        public int? UserId { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? RoleName { get; set; }

        public string? Action { get; set; }
        public string? EntityName { get; set; }
        public string? EntityId { get; set; }

        public string? OldValues { get; set; }
        public string? NewValues { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}
