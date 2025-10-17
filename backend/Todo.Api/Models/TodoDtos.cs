using System.ComponentModel.DataAnnotations;

namespace Todo.Api.Models;

public record CreateTodoDto([Required]string Title, string Description);


public record UpdateTodoDto([Required]string Id, [Required]string Title, string Description, [Required]bool IsCompleted);

 