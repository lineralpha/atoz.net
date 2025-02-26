namespace Atoz.EFCore;

public abstract class AuditableBaseEntity : AuditableBaseEntity<int>
{
}

public abstract class AuditableBaseEntity<T> : BaseEntity<T>, IAuditable
{
    public DateTimeOffset CreatedAt { get; set; }
    public required string CreatedBy { get; set; }
    public DateTimeOffset? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
