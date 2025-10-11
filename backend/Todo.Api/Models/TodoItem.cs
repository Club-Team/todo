using System.ComponentModel.DataAnnotations;

public class TodoItem
{
    [Key]
    public Guid Id { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsCompleted { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? UpdatedAt { get; set; }

    // Relation to user
    [Required]
    public string UserId { get; set; } = null!; // Keycloak `sub`
}