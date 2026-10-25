namespace Nook.Api.DTOs.Chores;

public sealed class ChoreResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? AssignedTo { get; set; }
    public DateOnly? DueDate { get; set; }
    public string Recurrence { get; set; } = "None";
    public bool IsCompleted { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastCompletedDate { get; set; }
}

public sealed class CreateChoreRequest
{
    public string? Title { get; set; }
    public string? AssignedTo { get; set; }
    public DateOnly? DueDate { get; set; }
    public string? Recurrence { get; set; }
}

public sealed class UpdateChoreRequest
{
    public string? Title { get; set; }
    public string? AssignedTo { get; set; }
    public DateOnly? DueDate { get; set; }
    public string? Recurrence { get; set; }
}
