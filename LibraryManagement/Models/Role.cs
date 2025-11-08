using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models;

public class Role
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    public ICollection<UserHasRole> UserHasRoles { get; set; } = new List<UserHasRole>();
    public ICollection<RoleHasRight> RoleHasRights { get; set; } = new List<RoleHasRight>();

}