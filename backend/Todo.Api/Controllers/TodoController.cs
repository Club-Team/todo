using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Todo.Api.Data;
using Todo.Api.Models;

namespace Todo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // 🔒 Require JWT

public class TodoController : ControllerBase
{
    private readonly AppDbContext _context;

    public TodoController(AppDbContext context)
    {
        _context = context;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? 
                                  User.FindFirstValue("sub")!; // Keycloak uses "sub"


    // GET: api/todo
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TodoItem>>> GetAll()
    {
        var userId = GetUserId();
        var todos = await _context.TodoItems
            .Where(t => t.UserId == userId).ToListAsync();
        return Ok(todos);
    }

    // GET: api/todo/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TodoItem>> GetById(Guid id)
    {
        var userId = GetUserId();
        var todo = await _context.TodoItems
            .Where(t => t.UserId == userId
            && t.Id == id).FirstOrDefaultAsync();
        if (todo == null)
            return NotFound();

        return Ok(todo);
    }

    [HttpPost]
    public async Task<ActionResult<TodoItem>> Create(TodoItem todo)
    {
        var userId = GetUserId();
        todo.UserId = userId;
        todo.CreatedAt = DateTimeOffset.UtcNow;

        _context.TodoItems.Add(todo);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = todo.Id }, todo);
    }

    // PUT: api/todo/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, TodoItem updated)
    {
        var userId = GetUserId();
        var todo = await _context.TodoItems
            .Where(t => t.UserId == userId
                        && t.Id == id).FirstOrDefaultAsync();
        if (todo == null)
            return NotFound();

        todo.Title = updated.Title;
        todo.Description = updated.Description;
        todo.IsCompleted = updated.IsCompleted;
        todo.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: api/todo/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetUserId();
        var todo = await _context.TodoItems
            .Where(t => t.UserId == userId
                        && t.Id == id).FirstOrDefaultAsync();
        if (todo == null)
            return NotFound();

        _context.TodoItems.Remove(todo);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
