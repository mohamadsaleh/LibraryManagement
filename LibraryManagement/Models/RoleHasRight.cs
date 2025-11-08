using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models;

public class RoleHasRight
{
    public int Id { get; set; }

    [Required]
    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;

    [Required]
    public int RightId { get; set; }
    public Right Right { get; set; } = null!;
}