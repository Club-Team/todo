namespace Todo.Api.Models;

public class AppUser {
    public Guid Id { get; set; }
    public string KeycloakId { get; set; }
    public string Username { get; set; }
    public DateTime CreatedAt { get; set; }
}
