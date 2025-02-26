namespace Atoz.EFCore;

public abstract class BaseEntity : BaseEntity<int>
{
}

public abstract class BaseEntity<T> : IEntity<T>, ISoftDelete
{
    public required T Id { get; set; }
    public bool IsDeleted { get; set; }
}
