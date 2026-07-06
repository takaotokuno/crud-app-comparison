using System.ComponentModel.DataAnnotations;

namespace AspNetNextApp.Api.Entities;

public sealed class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(512)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.Viewer;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<StockTransaction> StockTransactions { get; set; } = [];
}
