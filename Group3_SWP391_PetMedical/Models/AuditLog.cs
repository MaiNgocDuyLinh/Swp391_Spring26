using System;
using System.Collections.Generic;

namespace Group3_SWP391_PetMedical.Models;

public class AuditLog
{
    public int AuditLogId { get; set; }
    public int? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string? Action { get; set; }
    public string? EntityName { get; set; }
    public string? EntityId { get; set; }
    public string? Description { get; set; }
    public string? IpAddress { get; set; }
    public DateTime? CreatedAt { get; set; }

    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
}
