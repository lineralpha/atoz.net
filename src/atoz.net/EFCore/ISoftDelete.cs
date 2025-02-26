namespace Atoz.EFCore;

public interface ISoftDelete
{
    bool IsDeleted { get; set; }
}
