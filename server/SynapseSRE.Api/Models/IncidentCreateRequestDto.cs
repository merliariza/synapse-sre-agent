namespace SynapseSRE.Api.Models;
public class IncidentCreateRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IFormFile? LogFile { get; set; } 
}