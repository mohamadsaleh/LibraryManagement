using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models;

public class Right
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public ICollection<RoleHasRight> RoleHasRights { get; set; } = new List<RoleHasRight>();
}