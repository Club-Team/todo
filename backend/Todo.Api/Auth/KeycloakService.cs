using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public class KeycloakService
{
    private readonly HttpClient _client;
    private readonly IConfiguration _cfg;
    public string BaseUrl => _cfg["Keycloak:BaseUrl"];
    public string Realm => _cfg["Keycloak:Realm"];

    public KeycloakService(HttpClient client, IConfiguration cfg) {
        _client = client;
        _cfg = cfg;
    }
    public async Task<JsonElement> GetTokenForUserAsync(string username, string password, CancellationToken ct = default)
    {
        var values = new[]
        {
            new KeyValuePair<string, string>("client_id",
                _cfg["Keycloak:ClientId"]),
            new KeyValuePair<string, string>("client_secret",
                _cfg["Keycloak:ClientSecret"]),
            new KeyValuePair<string, string>("grant_type",
                "password"),
            new KeyValuePair<string, string>("username",
                username),
            new KeyValuePair<string, string>("password",
                password)
        };
        var res = await _client.PostAsync($"{BaseUrl}/realms/{Realm}/protocol/openid-connect/token",
            new FormUrlEncodedContent(values),
            ct);
        var resultStr = await res.Content.ReadAsStringAsync(ct);
        Console.WriteLine(resultStr);
        res.EnsureSuccessStatusCode();
        using var doc = JsonDocument.Parse(await res.Content.ReadAsStreamAsync(ct));
        return doc.RootElement.Clone();
    }
    // 1) client_credentials -> admin token
    public async Task<string> GetAdminAccessTokenAsync(CancellationToken ct = default) {
        var values = new[]
        {
            new KeyValuePair<string,string>("grant_type","client_credentials"),
            new KeyValuePair<string,string>("client_id", _cfg["Keycloak:AdminClientId"]),
            new KeyValuePair<string,string>("client_secret", _cfg["Keycloak:AdminClientSecret"])
        };
        var res = await _client.PostAsync($"{BaseUrl}/realms/{Realm}/protocol/openid-connect/token", new FormUrlEncodedContent(values), ct);
        res.EnsureSuccessStatusCode();
        using var doc = JsonDocument.Parse(await res.Content.ReadAsStreamAsync(ct));
        return doc.RootElement.GetProperty("access_token").GetString();
    }

    // 2) find user by username (username == email or mobile)
    public async Task<string> FindUserIdByUsernameAsync(string username, string adminToken, CancellationToken ct = default) {
        var req = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/admin/realms/{Realm}/users?username={Uri.EscapeDataString(username)}&exact=true");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var res = await _client.SendAsync(req, ct);
        res.EnsureSuccessStatusCode();
        var s = await res.Content.ReadAsStringAsync(ct);
        using var j = JsonDocument.Parse(s);
        var arr = j.RootElement;
        if (arr.GetArrayLength() == 0) return null;
        return arr[0].GetProperty("id").GetString();
    }

    // 3) create user
    public async Task<string> CreateUserAsync(string username, string email, string mobile, string adminToken, CancellationToken ct = default) {
        var payload = new {
            username,
            email = email,
            enabled = true,
            attributes = new { mobile = new[] { mobile } }
        };
        var req = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/admin/realms/{Realm}/users");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        req.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var res = await _client.SendAsync(req, ct);
        Console.WriteLine(await res.Content.ReadAsStringAsync(ct));
        res.EnsureSuccessStatusCode(); // 201 expected
        var loc = res.Headers.Location?.ToString();
        if (string.IsNullOrEmpty(loc)) throw new Exception("Keycloak create user response missing Location header");
        var id = loc.TrimEnd('/').Split('/').Last();
        return id;
    }

    // 4) set/reset password
    public async Task SetUserPasswordAsync(string userId, string newPassword, string adminToken, CancellationToken ct = default) {
        var payload = new { type = "password", temporary = false, value = newPassword };
        var req = new HttpRequestMessage(HttpMethod.Put, $"{BaseUrl}/admin/realms/{Realm}/users/{userId}/reset-password");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        req.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var res = await _client.SendAsync(req, ct);
        res.EnsureSuccessStatusCode();
    }

    // 5) password grant (get token for user)
    public async Task SetUserPasswordAsync(
        string userId,
        string newPassword,
        string adminToken,
        bool temporary = false,
        CancellationToken ct = default)
    {
        var payload = new { type = "password", temporary, value = newPassword };
        var req = new HttpRequestMessage(
            HttpMethod.Put,
            $"{BaseUrl}/admin/realms/{Realm}/users/{userId}/reset-password"
        );
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        req.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var res = await _client.SendAsync(req, ct);
        res.EnsureSuccessStatusCode();
    }
    public async Task ClearRequiredActionsAsync(string userId, string adminToken, CancellationToken ct = default)
    {
        var req = new HttpRequestMessage(HttpMethod.Put, $"{BaseUrl}/admin/realms/{Realm}/users/{userId}");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        req.Content = new StringContent(JsonSerializer.Serialize(new {
            requiredActions = Array.Empty<string>() // clear any pending actions
        }), Encoding.UTF8, "application/json");

        var res = await _client.SendAsync(req, ct);
        res.EnsureSuccessStatusCode();
    }
    public async Task ForceEnableUserAsync(string userId, string adminToken, CancellationToken ct = default)
    {
        var req = new HttpRequestMessage(HttpMethod.Put, $"{BaseUrl}/admin/realms/{Realm}/users/{userId}");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        req.Content = new StringContent(JsonSerializer.Serialize(new {
            enabled = true,
            requiredActions = Array.Empty<string>(),
            emailVerified = true
        }), Encoding.UTF8, "application/json");
        var res = await _client.SendAsync(req, ct);
        res.EnsureSuccessStatusCode();
    }
    public async Task ActivateUserAsync(string userId, string adminToken, CancellationToken ct = default)
    {
        var req = new HttpRequestMessage(HttpMethod.Put, $"{BaseUrl}/admin/realms/{Realm}/users/{userId}");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        req.Content = new StringContent(JsonSerializer.Serialize(new {
            enabled = true,
            emailVerified = true,
            requiredActions = Array.Empty<string>()
        }), Encoding.UTF8, "application/json");

        var res = await _client.SendAsync(req, ct);
        var body = await res.Content.ReadAsStringAsync(ct);
        Console.WriteLine("ActivateUser Response: " + body);
        res.EnsureSuccessStatusCode();
    }
    // 6) refresh tokens
    public async Task<JsonElement> RefreshTokenAsync(string refreshToken, CancellationToken ct = default) {
        var values = new[]
        {
            new KeyValuePair<string,string>("client_id", _cfg["Keycloak:ClientId"]),
            new KeyValuePair<string,string>("client_secret", _cfg["Keycloak:ClientSecret"]),
            new KeyValuePair<string,string>("grant_type","refresh_token"),
            new KeyValuePair<string,string>("refresh_token", refreshToken)
        };
        var res = await _client.PostAsync($"{BaseUrl}/realms/{Realm}/protocol/openid-connect/token", new FormUrlEncodedContent(values), ct);
        res.EnsureSuccessStatusCode();
        using var doc = JsonDocument.Parse(await res.Content.ReadAsStreamAsync(ct));
        return doc.RootElement.Clone();
    }
}
