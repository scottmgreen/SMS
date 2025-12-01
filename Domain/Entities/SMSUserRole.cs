using System;
using System.Collections.Generic;
using SMS_Domain.Common;
using SMS_Domain.Entities;

namespace SMS_Domain.Entities;

public sealed class SMSUserRole : BaseAuditableEntity
{
        
    public SMSUserRole(SMSUserRoleID id) : base(id, "SYSTEM", DateTime.UtcNow) 
    {
        Code = id.Value;
        Permissions = new List<SMSUserRolePermission>();
    }

    public string? Code { get; set; }
    public string? Name { get; set; }
    public List<SMSUserRolePermission> Permissions { get; set; }
}