namespace SynapseSRE.Application.DTOs;

public class IncidentDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; }
    
    public TriageAnalysisDto? TriageAnalysis { get; set; }
}