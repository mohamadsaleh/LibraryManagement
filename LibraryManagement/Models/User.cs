using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models;

public class User
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string DisplayName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public ICollection<UserHasRole> UserHasRoles { get; set; } = new List<UserHasRole>();
    public ICollection<Role> Roles { get; set; } = new List<Role>();

}