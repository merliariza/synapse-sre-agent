namespace SynapseSRE.Domain.Interfaces;

public interface IAgentService
{
    Task<string> AnalyzeIncidentAsync(string title, string description, string? logContent);
}