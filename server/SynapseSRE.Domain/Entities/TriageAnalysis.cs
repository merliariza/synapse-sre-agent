namespace SynapseSRE.Domain.Entities;

public class TriageAnalysis
{
    public Guid Id { get; set; }
    public Guid IncidentId { get; set; }
    public string AiSummary { get; set; } = string.Empty;
    public int SeverityScore { get; set; }
    public string? SuggestedFix { get; set; }
    public string ModelUsed { get; set; } = "phi4-mini";
    
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    public Incident Incident { get; set; } = null!;
}