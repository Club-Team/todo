using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Todo.Api.Data;
using Todo.Api.Models;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProfileController(AppDbContext context)
    {
        _context = context;
    }

    private string GetUserId() => User.FindFirstValue("sub")!;

    [HttpGet]
    public async Task<ActionResult<UserProfile>> GetProfile()
    {
        var userId = GetUserId();
        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.KeycloakUserId == userId);

        if (profile == null)
            return NotFound();

        return Ok(profile);
    }

    [HttpPost]
    public async Task<ActionResult<UserProfile>> CreateOrUpdate(UserProfile input)
    {
        var userId = GetUserId();
        var existing = await _context.UserProfiles.FirstOrDefaultAsync(p => p.KeycloakUserId == userId);

        if (existing == null)
        {
            input.KeycloakUserId = userId;
            input.CreatedAt = DateTimeOffset.UtcNow;
            _context.UserProfiles.Add(input);
        }
        else
        {
            existing.FirstName = input.FirstName;
            existing.LastName = input.LastName;
            existing.Email = input.Email;
            existing.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await _context.SaveChangesAsync();
        return Ok(existing ?? input);
    }
}