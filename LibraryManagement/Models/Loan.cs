using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models;

public class Loan
{
    public int Id { get; set; }

    [Required]
    public int BookId { get; set; }
    public Book Book { get; set; } = null!;

    [Required]
    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;

    [Required]
    public DateTime BorrowDate { get; set; }

    public DateTime? ReturnDate { get; set; }

    public bool IsReturned => ReturnDate.HasValue;
}