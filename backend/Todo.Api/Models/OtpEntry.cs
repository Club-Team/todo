namespace Todo.Api.Models;

public class OtpEntry {
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string OtpHash { get; set; }
    public DateTime ExpiresAt { get; set; }
    public int Attempts { get; set; }
}