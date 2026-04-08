namespace SynapseSRE.Application.DTOs;

public class TriageAnalysisDto
{
    public Guid Id { get; set; }
    public string AiSummary { get; set; } = string.Empty;
    public int SeverityScore { get; set; }
    public string? SuggestedFix { get; set; }
    public string ModelUsed { get; set; } = "phi4-mini";
    public DateTime ProcessedAt { get; set; }
}