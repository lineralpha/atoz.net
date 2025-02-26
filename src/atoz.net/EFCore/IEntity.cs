using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Atoz.EFCore;

public interface IEntity : IEntity<int>
{
}

public interface IEntity<T>
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    T Id { get; set; }
}
