using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Todo.Api.Data;
using Todo.Api.Models;

namespace Todo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TodoController : ControllerBase
{
    private readonly AppDbContext _context;

    public TodoController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/todo
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TodoItem>>> GetAll()
    {
        var todos = await _context.TodoItems.ToListAsync();
        return Ok(todos);
    }

    // GET: api/todo/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TodoItem>> GetById(Guid id)
    {
        var todo = await _context.TodoItems.FindAsync(id);
        if (todo == null)
            return NotFound();

        return Ok(todo);
    }

    // POST: api/todo
    [HttpPost]
    public async Task<ActionResult<TodoItem>> Create(TodoItem todo)
    {
        todo.CreatedAt = DateTimeOffset.UtcNow;
        _context.TodoItems.Add(todo);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = todo.Id }, todo);
    }

    // PUT: api/todo/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, TodoItem updated)
    {
        var todo = await _context.TodoItems.FindAsync(id);
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
        var todo = await _context.TodoItems.FindAsync(id);
        if (todo == null)
            return NotFound();

        _context.TodoItems.Remove(todo);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
