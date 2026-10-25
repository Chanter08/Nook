namespace Nook.Api.Models;

public class Chore
{
    public int ChoreId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? AssignedTo { get; set; }
    public DateOnly? DueDate { get; set; }
    public string Recurrence { get; set; } = "None";
    public bool IsCompleted { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastCompletedDate { get; set; }
}
