using System;
using System.Collections.Generic;

namespace CRM.EFModels.EFModels;

public partial class FileStorage
{
    public Guid FileId { get; set; }

    public Guid? ItemId { get; set; }

    public string FileName { get; set; } = null!;

    public string Extension { get; set; } = null!;

    public long? Bytes { get; set; }

    public byte[]? Value { get; set; }

    public DateTime UploadDate { get; set; }

    public string? UploadedBy { get; set; }

    public Guid? UserId { get; set; }

    public string? SourceFileId { get; set; }

    public Guid TenantId { get; set; }

    public DateTime LastModified { get; set; }

    public string? LastModifiedBy { get; set; }

    public bool Deleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual User? User { get; set; }
}
