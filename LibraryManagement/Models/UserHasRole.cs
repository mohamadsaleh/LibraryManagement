using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models;

public class UserHasRole
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    [Required]
    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;
}