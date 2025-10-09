using System.ComponentModel.DataAnnotations;

namespace Todo.Api.Models;

public class UserProfile
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string KeycloakUserId { get; set; } = null!; // sub from Keycloak token

    [MaxLength(100)]
    public string? FirstName { get; set; }

    [MaxLength(100)]
    public string? LastName { get; set; }

    [MaxLength(200)]
    public string? Email { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? UpdatedAt { get; set; }
}