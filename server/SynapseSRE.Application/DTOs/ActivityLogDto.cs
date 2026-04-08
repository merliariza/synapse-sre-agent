namespace SynapseSRE.Application.DTOs;

public class ActivityLogDto
{
    public Guid Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Details { get; set; } = "{}";
    public DateTime CreatedAt { get; set; }
}