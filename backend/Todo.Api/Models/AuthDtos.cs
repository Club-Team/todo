namespace Todo.Api.Models;

public class RegisterRequest
{
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}

public class LoginRequest
{
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class RefreshRequest
{
    public string RefreshToken { get; set; } = null!;
}

public class TokenResponse
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public int ExpiresIn { get; set; }
    public string TokenType { get; set; } = null!;
}