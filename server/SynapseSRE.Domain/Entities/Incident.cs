namespace SynapseSRE.Domain.Entities;

public class Incident
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


    public User? User { get; set; }
    public TriageAnalysis? TriageAnalysis { get; set; }
    public ICollection<IncidentAttachment> Attachments { get; set; } = new List<IncidentAttachment>();
}