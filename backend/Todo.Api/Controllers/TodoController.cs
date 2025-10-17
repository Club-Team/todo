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

    private string GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;


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
    public async Task<ActionResult<TodoItem>> Create(CreateTodoDto todo)
    {
        var userId = GetUserId();
        var createTodo = new TodoItem()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = todo.Title,
            Description = todo.Description,
            CreatedAt = DateTimeOffset.UtcNow,
                
        };
        _context.TodoItems.Add(createTodo);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = createTodo.Id }, todo);
    }

    // PUT: api/todo/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateTodoDto updated)
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
