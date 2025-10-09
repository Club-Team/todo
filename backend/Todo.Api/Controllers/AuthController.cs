using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Todo.Api.Models;

namespace Todo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _clientFactory;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IConfiguration config, IHttpClientFactory clientFactory, ILogger<AuthController> logger)
    {
        _config = config;
        _clientFactory = clientFactory;
        _logger = logger;
    }

    private string Realm => _config["Keycloak:Realm"]!;
    private string Authority => _config["Keycloak:Authority"]!;
    private string ClientId => _config["Keycloak:ClientId"]!;
    private string ClientSecret => _config["Keycloak:ClientSecret"]!;
    private string AdminClientId => _config["Keycloak:AdminClientId"]!;
    private string AdminUsername => _config["Keycloak:AdminUsername"]!;
    private string AdminPassword => _config["Keycloak:AdminPassword"]!;

    private HttpClient CreateClient()
    {
        var client = _clientFactory.CreateClient();
        client.BaseAddress = new Uri(Authority);
        return client;
    }

    // 🔹 REGISTER
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        if (_config.GetValue<bool>("UseFakeAuth"))
        {
            return Ok(new { message = "User registered successfully" });

        }
        var client = CreateClient();

        // 1️⃣ Get admin access token
        var adminTokenResponse = await client.PostAsync(
            $"/realms/{Realm}/protocol/openid-connect/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["client_id"] = AdminClientId,
                ["username"] = AdminUsername,
                ["password"] = AdminPassword
            })
        );

        if (!adminTokenResponse.IsSuccessStatusCode)
            return StatusCode((int)adminTokenResponse.StatusCode, "Failed to get admin token");

        var adminJson = await adminTokenResponse.Content.ReadAsStringAsync();
        var adminData = JsonSerializer.Deserialize<JsonElement>(adminJson);
        var adminToken = adminData.GetProperty("access_token").GetString();

        // 2️⃣ Create user in Keycloak
        var userPayload = new
        {
            username = request.Username,
            email = request.Email,
            firstName = request.FirstName,
            lastName = request.LastName,
            enabled = true,
            credentials = new[]
            {
                new { type = "password", value = request.Password, temporary = false }
            }
        };

        var jsonContent = new StringContent(JsonSerializer.Serialize(userPayload), Encoding.UTF8, "application/json");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var createUserResponse = await client.PostAsync($"/admin/realms/{Realm}/users", jsonContent);

        if (!createUserResponse.IsSuccessStatusCode)
        {
            var error = await createUserResponse.Content.ReadAsStringAsync();
            return StatusCode((int)createUserResponse.StatusCode, error);
        }

        return Ok(new { message = "User registered successfully" });
    }

    // 🔹 LOGIN
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        if (_config.GetValue<bool>("UseFakeAuth"))
        {
            return Ok(new TokenResponse
            {
                AccessToken = "FAKE_TOKEN",
                RefreshToken = "FAKE_REFRESH",
                ExpiresIn = 3600,
                TokenType = "bearer"
            });
        }
        
        var client = CreateClient();

        var response = await client.PostAsync(
            $"/realms/{Realm}/protocol/openid-connect/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["client_id"] = ClientId,
                ["client_secret"] = ClientSecret,
                ["username"] = request.Username,
                ["password"] = request.Password
            })
        );

        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            return StatusCode((int)response.StatusCode, body);

        var data = JsonSerializer.Deserialize<JsonElement>(body);
        var tokenResponse = new TokenResponse
        {
            AccessToken = data.GetProperty("access_token").GetString()!,
            RefreshToken = data.GetProperty("refresh_token").GetString()!,
            ExpiresIn = data.GetProperty("expires_in").GetInt32(),
            TokenType = data.GetProperty("token_type").GetString()!
        };

        return Ok(tokenResponse);
    }

    // 🔹 REFRESH TOKEN
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshRequest request)
    {
        if (_config.GetValue<bool>("UseFakeAuth"))
        {
            return Ok(new TokenResponse
            {
                AccessToken = "FAKE_TOKEN",
                RefreshToken = "FAKE_REFRESH",
                ExpiresIn = 3600,
                TokenType = "bearer"
            });
        }
        var client = CreateClient();

        var response = await client.PostAsync(
            $"/realms/{Realm}/protocol/openid-connect/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["client_id"] = ClientId,
                ["client_secret"] = ClientSecret,
                ["refresh_token"] = request.RefreshToken
            })
        );

        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            return StatusCode((int)response.StatusCode, body);

        var data = JsonSerializer.Deserialize<JsonElement>(body);
        var tokenResponse = new TokenResponse
        {
            AccessToken = data.GetProperty("access_token").GetString()!,
            RefreshToken = data.GetProperty("refresh_token").GetString()!,
            ExpiresIn = data.GetProperty("expires_in").GetInt32(),
            TokenType = data.GetProperty("token_type").GetString()!
        };

        return Ok(tokenResponse);
    }
}
