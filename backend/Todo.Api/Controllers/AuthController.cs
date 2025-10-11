using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Todo.Api.Auth;
using Todo.Api.Data;
using Todo.Api.Models;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly KeycloakService _kc;
    private readonly OtpService _otp;
    private readonly AppDbContext _db;
    public AuthController(KeycloakService kc, OtpService otp, AppDbContext db) { _kc = kc; _otp = otp; _db = db; }

    // POST /auth/send-otp
    [HttpPost("send-otp")]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpDto dto)
    {
        var otp = await _otp.SendOtpAsync(dto.Contact);
        // For local/dev we return OTP. Remove this in prod.
        return Ok(new { message = "OTP sent (dev)", otp });
    }

    // POST /auth/verify-otp  -> verifies OTP, creates Keycloak user if missing, sets temp password, returns tokens
    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto)
    {
        try
        {
            if (!await _otp.VerifyOtpAsync(dto.Contact, dto.Otp))
                return BadRequest("Invalid or expired OTP");

            var adminToken = await _kc.GetAdminAccessTokenAsync();
            var username = dto.Contact;
            var kcId = await _kc.FindUserIdByUsernameAsync(username, adminToken);

            if (kcId == null)
            {
                string email = dto.Contact.Contains("@") ? dto.Contact : null;
                string mobile = email == null ? dto.Contact : null;
                kcId = await _kc.CreateUserAsync(username, email, mobile, adminToken);

                var appUser = new AppUser
                {
                    Id = Guid.NewGuid(),
                    KeycloakId = kcId,
                    Username = mobile ?? email,
                    CreatedAt = DateTime.UtcNow
                };
                _db.Users.Add(appUser);
                await _db.SaveChangesAsync();
            }

            // 1. Clear any required actions
            await _kc.ClearRequiredActionsAsync(kcId, adminToken);

            // 2. Set a temporary password (use temporary=false if you want token immediately)
            var tempPassword = "P@" + Guid.NewGuid().ToString("N").Substring(0, 12);
            await _kc.SetUserPasswordAsync(kcId, tempPassword, adminToken, temporary: false);
            await _kc.ForceEnableUserAsync(kcId, adminToken);
            await _kc.ActivateUserAsync(kcId, adminToken); // <-- ensures account is fully active

            // 3. Get tokens
            var tokens = await _kc.GetTokenForUserAsync(username, tempPassword);

            return Ok(tokens.ToString());
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);

        }
    }


    // POST /auth/register -> register with password
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var adminToken = await _kc.GetAdminAccessTokenAsync();
        var username = dto.Email ?? dto.Mobile;
        var existing = await _kc.FindUserIdByUsernameAsync(username, adminToken);
        if (existing != null) return Conflict("User already exists");

        var kcId = await _kc.CreateUserAsync(username, dto.Email, dto.Mobile, adminToken);
        await _kc.SetUserPasswordAsync(kcId, dto.Password, adminToken, false);

        var appUser = new AppUser { Id = Guid.NewGuid(), KeycloakId = kcId, Username = dto.Mobile, CreatedAt = DateTime.UtcNow };
        _db.Users.Add(appUser);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Register), new { id = appUser.Id }, new { success = true });
    }

    // POST /auth/login -> username (email/mobile) + password
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var tokenJson = await _kc.GetTokenForUserAsync(dto.Username, dto.Password);
        return Ok(tokenJson.ToString());
    }

    // POST /auth/refresh
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshDto dto)
    {
        var j = await _kc.RefreshTokenAsync(dto.RefreshToken);
        return Ok(j.ToString());
    }

    // GET /auth/me (example protected endpoint)
    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var preferred_username = User.Identity.Name; // gives preferred_username
        var kcId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var localUser = _db.Users.SingleOrDefault(u => u.KeycloakId == kcId);
        return Ok(new { keycloakId = kcId, email  = preferred_username,  localUser });
    }
}
