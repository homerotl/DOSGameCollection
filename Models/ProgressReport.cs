namespace DOSGameCollection.Models;

public class ProgressReport
{
    public int CurrentStep { get; set; }
    public int TotalSteps { get; set; }
    public required string Message { get; set; }
    public bool IsComplete { get; set; }
}
